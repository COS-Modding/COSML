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
using UnityEngine.SceneManagement;
using UObject = UnityEngine.Object;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace COSML
{
    /// <summary>
    ///     Handles loading of mods.
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

        public const int Version = 1;

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
                Logging.API.Error(e);
            }

            Logging.API.Info($"Mod loader: {ModHooks.ModVersion}");
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
            Logging.ClearOldModlogs();

            Logging.API.Debug($"Loading assemblies and constructing mods");

            string mods = Path.Combine(managed_path, "Mods");
            string[] files = Directory.GetDirectories(mods)
                .Except(new string[] { Path.Combine(mods, "Disabled") })
                .SelectMany(d => Directory.GetFiles(d, "*.dll"))
                .ToArray();

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
                catch (FileLoadException e)
                {
                    Logging.API.Error($"Unable to load assembly - {e}");
                }
                catch (BadImageFormatException e)
                {
                    Logging.API.Error($"Assembly is bad image. {e}");
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
                catch (ReflectionTypeLoadException e)
                {
                    asmTypes = e.Types.Where(x => x is not null);
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
                        catch (Exception e)
                        {
                            Logging.API.Error(e);

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
                catch (Exception e)
                {
                    Logging.API.Error(e);
                }

                if (!foundMod)
                {
                    AssemblyName info = asm.GetName();
                    Logging.API.Info($"Assembly {info.Name} ({info.Version}) loaded with 0 mods");
                }
            }

            var scenes = new List<string>();
            for (int i = 0; i < USceneManager.sceneCountInBuildSettings; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                scenes.Add(Path.GetFileNameWithoutExtension(scenePath));
            }

            ModInstance[] orderedMods = ModInstanceTypeMap.Values
                .OrderBy(x => x.Mod?.LoadPriority() ?? 0)
                .ToArray();

            // dict<scene name, list<(mod, list<objectNames>)>
            var toPreload = new Dictionary<string, List<(ModInstance, List<string> objectNames)>>();
            // dict<mod, dict<scene, dict<objName, object>>>
            var preloadedObjects = new Dictionary<ModInstance, Dictionary<string, Dictionary<string, GameObject>>>();
            // scene -> respective hooks
            var sceneHooks = new Dictionary<string, List<Func<IEnumerator>>>();

            Logging.API.Info("Creating mod preloads");

            // Setup dict of scene preloads
            GetPreloads(orderedMods, scenes, toPreload, sceneHooks);

            if (toPreload.Count > 0 || sceneHooks.Count > 0)
            {
                Preloader pld = coroutineHolder.GetComponent<Preloader>() ?? coroutineHolder.AddComponent<Preloader>();
                yield return pld.Preload(toPreload, preloadedObjects, sceneHooks);
            }

            foreach (ModInstance mod in orderedMods)
            {
                if (mod.Error is not null)
                {
                    Logging.API.Warn($"Not loading mod {mod.Name}: error state {mod.Error}");
                    continue;
                }

                try
                {
                    preloadedObjects.TryGetValue(mod, out Dictionary<string, Dictionary<string, GameObject>> preloads);
                    LoadMod(mod, false, preloads);
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

        private static void GetPreloads
        (
            ModInstance[] orderedMods,
            List<string> scenes,
            Dictionary<string, List<(ModInstance, List<string> objectNames)>> toPreload,
            Dictionary<string, List<Func<IEnumerator>>> sceneHooks
        )
        {
            foreach (var mod in orderedMods)
            {
                if (mod.Error != null)
                {
                    continue;
                }

                Logging.API.Debug($"Checking preloads for mod \"{mod.Mod.GetName()}\"");

                List<(string, string)> preloadNames = null;
                try
                {
                    preloadNames = mod.Mod.GetPreloadNames();
                }
                catch (Exception ex)
                {
                    Logging.API.Error($"Error getting preload names for mod {mod.Name}:\n" + ex);
                }

                try
                {
                    foreach (var (scene, hook) in mod.Mod.PreloadSceneHooks())
                    {
                        if (!sceneHooks.TryGetValue(scene, out var hooks))
                            sceneHooks[scene] = hooks = new List<Func<IEnumerator>>();

                        hooks.Add(hook);
                    }
                }
                catch (Exception ex)
                {
                    Logging.API.Error($"Error getting preload hooks for mod {mod.Name}:\n" + ex);
                }

                if (preloadNames == null)
                    continue;

                // dict<scene, list<objects>>
                Dictionary<string, List<string>> modPreloads = new();

                foreach ((string scene, string obj) in preloadNames)
                {
                    if (string.IsNullOrEmpty(scene) || string.IsNullOrEmpty(obj))
                    {
                        Logging.API.Warn($"Mod `{mod.Mod.GetName()}` passed null values to preload");
                        continue;
                    }

                    if (!scenes.Contains(scene))
                    {
                        Logging.API.Warn(
                            $"Mod `{mod.Mod.GetName()}` attempted preload from non-existent scene `{scene}`"
                        );
                        continue;
                    }

                    if (!modPreloads.TryGetValue(scene, out List<string> objects))
                    {
                        objects = new List<string>();
                        modPreloads[scene] = objects;
                    }

                    Logging.API.Debug($"Found object `{scene}.{obj}`");

                    objects.Add(obj);
                }

                foreach ((string scene, List<string> objects) in modPreloads)
                {
                    if (!toPreload.TryGetValue(scene, out List<(ModInstance, List<string>)> scenePreloads))
                    {
                        scenePreloads = new List<(ModInstance, List<string>)>();
                        toPreload[scene] = scenePreloads;
                    }

                    Logging.API.Debug($"`{mod.Name}` preloads {objects.Count} objects in the `{scene}` scene");

                    scenePreloads.Add((mod, objects));
                    toPreload[scene] = scenePreloads;
                }
            }
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
                        case ModErrorState.Initialize:
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
            bool updateModText = true,
            Dictionary<string, Dictionary<string, GameObject>> preloadedObjects = null
        )
        {
            try
            {
                if (mod is { Enabled: false, Error: null })
                {
                    mod.Enabled = true;
                    mod.Mod.Initialize(preloadedObjects);
                }
            }
            catch (Exception e)
            {
                mod.Error = ModErrorState.Initialize;
                Logging.API.Error($"Failed to load mod `{mod.Mod.GetName()}`\n{e}");
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
            Initialize,
            Unload
        }
    }
}