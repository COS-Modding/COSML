using COSML.Log;
using COSML.Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace COSML
{
    /// <summary>
    /// Handles loading of mods.
    /// </summary>
    internal static class COSML
    {
        [Flags]
        public enum ModLoadState
        {
            NotStarted = 0,
            Started = 1,
            Preloaded = 2,
            Loaded = 4,
        }

        public const int Version = 2;

        public static ModLoadState LoadState = ModLoadState.NotStarted;

        public static Dictionary<Type, ModInstance> ModInstanceTypeMap { get; private set; } = new();
        public static Dictionary<string, ModInstance> ModInstanceNameMap { get; private set; } = new();
        public static HashSet<ModInstance> ModInstances { get; private set; } = new();

        /// <summary>
        /// Try to add a ModInstance to the internal dictionaries.
        /// </summary>
        /// <param name="ty">The type of the mod.</param>
        /// <param name="mod">The ModInstance.</param>
        /// <returns>True if the ModInstance was successfully added; false otherwise.</returns>
        private static bool TryAddModInstance(Type ty, ModInstance mod)
        {
            if (ModInstanceNameMap.ContainsKey(mod.Name))
            {
                Logging.API.Warn($"Found multiple mods with name {mod.Name}.");
                mod.Error = ModErrorState.Duplicate;
                ModInstanceNameMap[mod.Name].Error = ModErrorState.Duplicate;
                ModInstanceTypeMap[ty] = mod;
                ModInstances.Add(mod);
                return false;
            }

            ModInstanceTypeMap[ty] = mod;
            ModInstanceNameMap[mod.Name] = mod;
            ModInstances.Add(mod);
            return true;
        }

        /// <summary>
        /// Starts the main loading of all mods.
        /// This loads assemblies, constructs and initializes mods, and creates the mod list menu.<br/>
        /// This method should only be called once in the lifetime of the game.
        /// </summary>
        /// <param name="coroutineHolder"></param>
        /// <returns></returns>
        public static IEnumerator LoadModsInit(GameObject coroutineHolder)
        {
            try
            {
                Logging.InitializeFileStream();
            }
            catch (Exception e)
            {
                // We can still log to the console at least, if that's enabled.
                Debug.Log($"Error while initializing ModLog.txt: {e}");
            }

            Logging.API.Info($"Mod loader: {Version}");
            Logging.API.Info("Starting mod loading");

            string managed_path = SystemInfo.operatingSystemFamily switch
            {
                OperatingSystemFamily.Windows => Path.Combine(Application.dataPath, "Managed"),
                OperatingSystemFamily.MacOSX => Path.Combine(Application.dataPath, "Resources", "Data", "Managed"),
                OperatingSystemFamily.Linux => Path.Combine(Application.dataPath, "Managed"),
                OperatingSystemFamily.Other => null,
                _ => throw new ArgumentOutOfRangeException()
            };

            if (managed_path is null)
            {
                LoadState |= ModLoadState.Loaded;
                UObject.Destroy(coroutineHolder);
                yield break;
            }

            ModHooks.LoadGlobalSettings();

            Logging.API.Debug($"Loading assemblies and constructing mods");

            string mods = Path.Combine(managed_path, "Mods");
            Directory.CreateDirectory(mods);
            DirectoryInfo modsDir = new DirectoryInfo(mods);
            string[] files = modsDir.GetFiles("*", SearchOption.AllDirectories).Where((f) => f.Extension == ".dll").Select((f) => f.FullName).ToArray();

            Logging.API.Debug($"DLL files: {string.Join(",\n", files)}");

            Assembly Resolve(object sender, ResolveEventArgs args)
            {
                var asm_name = new AssemblyName(args.Name);
                if (files.FirstOrDefault(x => x.EndsWith($"{asm_name.Name}.dll")) is string path)
                {
                    return Assembly.LoadFrom(path);
                }

                return null;
            }

            AppDomain.CurrentDomain.AssemblyResolve += Resolve;

            List<Assembly> asms = new(files.Length);

            // Load all the assemblies first to avoid dependency issues
            // Dependencies are lazy-loaded, so we won't have attempted loads until the mod initialization.
            foreach (string path in files)
            {
                Logging.API.Debug($"Loading assembly `{path}`");

                try
                {
                    asms.Add(Assembly.LoadFrom(path));
                }
                catch (FileLoadException ex)
                {
                    Logging.API.Error($"Unable to load assembly - {ex}");
                }
                catch (BadImageFormatException ex)
                {
                    Logging.API.Error($"Assembly is bad image. {ex}");
                }
                catch (PathTooLongException)
                {
                    Logging.API.Error("Unable to load, path to dll is too long!");
                }
            }

            foreach (Assembly asm in asms)
            {
                Logging.API.Debug($"Loading mods in assembly `{asm.FullName}`");

                bool foundMod = false;

                IEnumerable<Type> asmTypes;

                // If some types cannot be loaded (e.g. they derive from a type in an uninstalled mod),
                // then only the successfully loaded types are returned.
                try
                {
                    asmTypes = asm.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    asmTypes = ex.Types.Where(x => x is not null);
                }

                try
                {
                    foreach (Type ty in asmTypes)
                    {
                        if (!ty.IsClass || ty.IsAbstract || !ty.IsSubclassOf(typeof(Mod))) continue;

                        foundMod = true;

                        Logging.API.Debug($"Constructing mod `{ty.FullName}`");

                        try
                        {
                            if (ty.GetConstructor(Type.EmptyTypes)?.Invoke(Array.Empty<object>()) is Mod mod)
                            {
                                TryAddModInstance(
                                    ty,
                                    new ModInstance
                                    {
                                        Mod = mod,
                                        Enabled = false,
                                        Error = null,
                                        Name = mod.GetName()
                                    }
                                );
                            }
                        }
                        catch (Exception ex)
                        {
                            Logging.API.Error(ex);

                            TryAddModInstance(
                                ty,
                                new ModInstance
                                {
                                    Mod = null,
                                    Enabled = false,
                                    Error = ModErrorState.Construct,
                                    Name = ty.Name
                                }
                            );
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logging.API.Error(ex);
                }

                if (!foundMod)
                {
                    AssemblyName info = asm.GetName();
                    Logging.API.Info($"Assembly {info.Name} ({info.Version}) loaded with 0 mods");
                }
            }

            ModInstance[] orderedMods = ModInstanceTypeMap.Values
                .OrderBy(x => x.Mod?.LoadPriority() ?? 0)
                .ToArray();

            foreach (ModInstance mod in orderedMods)
            {
                if (mod.Error is not null)
                {
                    Logging.API.Warn($"Not loading mod {mod.Name}: error state {mod.Error}");
                    continue;
                }

                try
                {
                    LoadMod(mod, false);
                    if (!ModHooks.GlobalSettings.ModEnabledSettings.TryGetValue(mod.Name, out var enabled))
                    {
                        enabled = true;
                    }

                    if (mod.Error == null && mod.Mod is IModTogglable && !enabled)
                    {
                        UnloadMod(mod, false);
                    }
                }
                catch (Exception ex)
                {
                    Logging.API.Error("Error: " + ex);
                }
            }

            Logging.API.Info("Finished loading mods:\n" + UpdateModText());

            ModHooks.OnFinishedLoadingMods();
            LoadState |= ModLoadState.Loaded;

            UObject.Destroy(coroutineHolder.gameObject);
        }

        private static string UpdateModText()
        {
            StringBuilder builder = new();

            foreach (ModInstance mod in ModInstances)
            {
                if (mod.Error is not ModErrorState err)
                {
                    if (mod.Enabled) builder.AppendLine($"{mod.Name} : {mod.Mod.GetVersionSafe("ERROR")}");
                }
                else
                {
                    switch (err)
                    {
                        case ModErrorState.Construct:
                            builder.AppendLine($"{mod.Name} : Failed to call constructor! Check ModLog.txt");
                            break;
                        case ModErrorState.Duplicate:
                            builder.AppendLine($"{mod.Name} : Failed to load! Duplicate mod detected");
                            break;
                        case ModErrorState.Init:
                            builder.AppendLine($"{mod.Name} : Failed to initialize! Check ModLog.txt");
                            break;
                        case ModErrorState.Unload:
                            builder.AppendLine($"{mod.Name} : Failed to unload! Check ModLog.txt");
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            return builder.ToString();
        }

        internal static void LoadMod
        (
            ModInstance mod,
            bool updateModText = true
        )
        {
            try
            {
                if (mod is { Enabled: false, Error: null })
                {
                    mod.Enabled = true;
                    mod.Mod.Init();
                }
            }
            catch (Exception ex)
            {
                mod.Error = ModErrorState.Init;
                Logging.API.Error($"Failed to load mod `{mod.Mod.GetName()}`\n{ex}");
            }

            if (updateModText) UpdateModText();
        }

        internal static void UnloadMod(ModInstance mod, bool updateModText = true)
        {
            try
            {
                if (mod is { Mod: IModTogglable imodt, Enabled: true, Error: null })
                {
                    mod.Enabled = false;
                    imodt.Unload();
                }
            }
            catch (Exception ex)
            {
                mod.Error = ModErrorState.Unload;
                Logging.API.Error($"Failed to unload mod `{mod.Name}`\n{ex}");
            }

            if (updateModText) UpdateModText();
        }

        // Essentially the state of a loaded **mod**. The assembly has nothing to do directly with mods.
        public class ModInstance
        {
            // The constructed instance of the mod. If Error is `Construct` this will be null.
            // Generally if Error is anything this value should not be referred to.
            public IMod Mod;

            public string Name;

            public ModErrorState? Error;

            // If the mod is "Enabled" (in the context of IModTogglable)
            public bool Enabled;
        }

        public enum ModErrorState
        {
            Construct,
            Duplicate,
            Init,
            Unload
        }
    }
}
