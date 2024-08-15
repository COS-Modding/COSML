using COSML.Log;
using COSML.Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace COSML
{
    /// <summary>
    /// Handles loading of mods.
    /// </summary>
    public static class COSML
    {
        [Flags]
        internal enum ModLoadState
        {
            NotStarted = 0,
            Started = 1,
            Preloaded = 2,
            Loaded = 4,
        }

        public const string Version = "1.0.4";

        public static string ManagedPath { get; private set; }
        public static string ModsPath { get; private set; }

        internal static ModLoadState LoadState = ModLoadState.NotStarted;
        internal static Dictionary<Type, ModInstance> ModInstanceTypeMap { get; private set; } = [];
        internal static Dictionary<string, ModInstance> ModInstanceNameMap { get; private set; } = [];
        internal static HashSet<ModInstance> ModInstances { get; private set; } = [];

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

        internal static IEnumerator LoadModsInit(GameObject coroutineHolder)
        {
            try
            {
                Logging.InitializeFileStream();
            }
            catch (Exception ex)
            {
                // We can still log to the console at least, if that's enabled.
                Debug.Log($"Error while initializing ModLog.txt\n{ex}");
            }

            ModHooks.LoadGlobalSettings();

            Logging.API.Info($"Mod loader: {Version}");
            Logging.API.Info("Starting mod loading");

            ManagedPath = SystemInfo.operatingSystemFamily switch
            {
                OperatingSystemFamily.Windows => Path.Combine(Application.dataPath, "Managed"),
                OperatingSystemFamily.MacOSX => Path.Combine(Application.dataPath, "Resources", "Data", "Managed"),
                OperatingSystemFamily.Linux => Path.Combine(Application.dataPath, "Managed"),
                OperatingSystemFamily.Other => null,
                _ => throw new ArgumentOutOfRangeException()
            };

            if (ManagedPath is null)
            {
                LoadState |= ModLoadState.Loaded;
                UnityEngine.Object.Destroy(coroutineHolder);
                yield break;
            }

            Logging.API.Debug($"Loading assemblies and constructing mods");

            ModsPath = Path.Combine(ManagedPath, "Mods");
            Directory.CreateDirectory(ModsPath);
            string[] filesPath = [.. new DirectoryInfo(ModsPath).GetFiles("*", SearchOption.AllDirectories).Where(f => f.Extension == ".dll").Select(f => f.FullName)];

            Logging.API.Debug($"DLL files: {string.Join(",\n", filesPath)}");

            Assembly Resolve(object sender, ResolveEventArgs args)
            {
                var asm_name = new AssemblyName(args.Name);
                if (filesPath.FirstOrDefault(x => x.EndsWith($"{asm_name.Name}.dll")) is string path)
                {
                    return Assembly.LoadFrom(path);
                }

                return null;
            }

            AppDomain.CurrentDomain.AssemblyResolve += Resolve;

            List<Assembly> asms = new(filesPath.Length);

            // Load all the assemblies first to avoid dependency issues
            // Dependencies are lazy-loaded, so we won't have attempted loads until the mod initialization.
            foreach (string path in filesPath)
            {
                Logging.API.Debug($"Loading assembly \"{path}\"");

                try
                {
                    asms.Add(Assembly.LoadFrom(path));
                }
                catch (FileLoadException ex)
                {
                    Logging.API.Error($"Unable to load assembly\n{ex}");
                }
                catch (BadImageFormatException ex)
                {
                    Logging.API.Error($"Assembly is bad image\n{ex}");
                }
                catch (PathTooLongException)
                {
                    Logging.API.Error("Unable to load, path to dll is too long!");
                }
            }

            foreach (Assembly asm in asms)
            {
                Logging.API.Debug($"Loading mods in assembly \"{asm.FullName}\"");

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

                        Logging.API.Debug($"Constructing mod \"{ty.FullName}\"");

                        try
                        {
                            if (ty.GetConstructor(Type.EmptyTypes)?.Invoke([]) is Mod mod)
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
                            Logging.API.Error($"Failed to instantiate assembly mod \"{ty.FullName}\"\n{ex}");

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

            ModInstance[] orderedMods = [.. ModInstanceTypeMap.Values.OrderBy(x => x.Mod?.LoadPriority() ?? 0)];

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

            Logging.API.Info("Finished loading mods:" + UpdateModText());

            ModHooks.OnFinishedLoadingMods();
            LoadState |= ModLoadState.Loaded;

            I18n.LoadI18n();

            UnityEngine.Object.Destroy(coroutineHolder.gameObject);
        }

        private static string UpdateModText()
        {
            string text = "";

            foreach (ModInstance mod in ModInstances)
            {
                if (mod.Error is not ModErrorState err)
                {
                    if (mod.Enabled) text += $"\n    {mod.Name} : {mod.Mod.GetVersionSafe("ERROR")}";
                }
                else
                {
                    text += err switch
                    {
                        ModErrorState.Construct => $"\n    {mod.Name} : Failed to call constructor! Check ModLog.txt",
                        ModErrorState.Duplicate => $"\n    {mod.Name} : Failed to load! Duplicate mod detected",
                        ModErrorState.Init => $"\n    {mod.Name} : Failed to initialize! Check ModLog.txt",
                        ModErrorState.Unload => $"\n    {mod.Name} : Failed to unload! Check ModLog.txt",
                        _ => throw new ArgumentOutOfRangeException(),
                    };
                }
            }

            return text;
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
                Logging.API.Error($"Failed to load mod \"{mod.Mod.GetName()}\"\n{ex}");
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
                Logging.API.Error($"Failed to unload mod \"{mod.Name}\"\n{ex}");
            }

            if (updateModText) UpdateModText();
        }

        // Essentially the state of a loaded **mod**. The assembly has nothing to do directly with mods.
        internal class ModInstance
        {
            // The constructed instance of the mod. If Error is \"Construct\" this will be null.
            // Generally if Error is anything this value should not be referred to.
            internal IMod Mod;

            internal string Name;

            internal ModErrorState? Error;

            // If the mod is "Enabled" (in the context of IModTogglable)
            internal bool Enabled;
        }

        internal enum ModErrorState
        {
            Construct,
            Duplicate,
            Init,
            Unload
        }
    }
}
