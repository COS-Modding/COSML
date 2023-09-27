using COSML.Log;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace COSML.Modding
{
    /// <summary>
    ///     Class to hook into various events for the game.
    /// </summary>
    public class ModHooks
    {
        private static readonly string SettingsPath = Path.Combine(Application.persistentDataPath, "COSML.GlobalSettings.json");

        /// <summary>
        ///     Dictionary of mods and their version
        /// </summary>
        public static readonly Dictionary<string, string> LoadedModsWithVersions = new();

        private static COSMLConsole _console;

        /// <summary>
        ///     The Version of the API
        /// </summary>
        public static string ModVersion;

        /// <summary>
        ///     Version of the game
        /// </summary>
        public static string GameVersion;

        static ModHooks()
        {
            // Save global settings only if mods have finished loading
            FinishedLoadingModsHook += () => ApplicationQuitHook += SaveGlobalSettings;
        }

        /// <summary>
        /// The global ModHooks settings.
        /// </summary>
        public static COSMLGlobalSettings GlobalSettings { get; private set; } = new();

        internal static void LoadGlobalSettings()
        {
            try
            {
                if (!File.Exists(SettingsPath))
                    return;
                Logging.API.Info("Loading Global Settings");
                using FileStream fileStream = File.OpenRead(SettingsPath);
                using var reader = new StreamReader(fileStream);
                string json = reader.ReadToEnd();

                var de = JsonConvert.DeserializeObject<COSMLGlobalSettings>(
                    json,
                    new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Auto,
                        ObjectCreationHandling = ObjectCreationHandling.Replace
                    }
                );
                if (de != null)
                {
                    GlobalSettings = de;
                    Logging.SetLogLevel(GlobalSettings.LoggingLevel);
                    Logging.SetUseShortLogLevel(GlobalSettings.ShortLoggingLevel);
                    Logging.SetIncludeTimestampt(GlobalSettings.IncludeTimestamps);
                }
            }
            catch (Exception e)
            {
                Logging.API.Error(e);
            }
        }

        internal static void SaveGlobalSettings()
        {
            try
            {
                Logging.API.Info("Saving Global Settings");

                var settings = GlobalSettings;
                if (settings is null) return;

                settings.ModEnabledSettings = new Dictionary<string, bool>();

                foreach (var x in COSML.ModInstances)
                {
                    if (x.Mod is IModTogglable && x.Error is null)
                    {
                        settings.ModEnabledSettings.Add(x.Name, x.Enabled);
                    }
                }

                if (File.Exists(SettingsPath + ".bak")) File.Delete(SettingsPath + ".bak");
                if (File.Exists(SettingsPath)) File.Move(SettingsPath, SettingsPath + ".bak");

                using FileStream fileStream = File.Create(SettingsPath);
                using var writer = new StreamWriter(fileStream);

                writer.Write(JsonConvert.SerializeObject(
                    settings,
                    Formatting.Indented,
                    new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Auto
                    }
                ));
            }
            catch (Exception e)
            {
                Logging.API.Error(e);
            }
        }

        internal static void LogConsole(string message, LogLevel level)
        {
            try
            {
                if (!GlobalSettings.ShowDebugLogInGame) return;

                _console ??= new GameObject("COSMLConsole").AddComponent<COSMLConsole>();
                _console.AddText(message, level);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        internal static void SetCOSMLVersion(MainMenu mainMenu)
        {
            GameVersion = mainMenu.copyright.text.Split("v.").Last();
            ModVersion = $"COSML-{COSML.Version}";

            mainMenu.copyright.text += $" ({ModVersion})";
        }


        /// <summary>
        ///     Called whenever game tries to create a new gameobject.  This happens often, care should be taken.
        /// </summary>
        public static event Func<GameObject, GameObject> ObjectPoolSpawnHook;

        /// <summary>
        ///     Called whenever game tries to show cursor
        /// </summary>
        internal static GameObject OnObjectPoolSpawn(GameObject go)
        {
            if (ObjectPoolSpawnHook == null) return go;

            Delegate[] invocationList = ObjectPoolSpawnHook.GetInvocationList();

            foreach (Func<GameObject, GameObject> toInvoke in invocationList.Cast<Func<GameObject, GameObject>>())
            {
                try
                {
                    go = toInvoke.Invoke(go);
                }
                catch (Exception ex)
                {
                    Logging.API.Error(ex);
                }
            }

            return go;
        }

        /// <summary>
        ///     Called when the game is fully closed
        /// </summary>
        /// <remarks>ApplicationQuit.OnApplicationQuit</remarks>
        public static event Action ApplicationQuitHook;

        /// <summary>
        ///     Called when the game is fully closed
        /// </summary>
        /// <remarks>ApplicationQuit.OnApplicationQuit</remarks>
        internal static void OnApplicationQuit()
        {
            Logging.API.Debug("OnApplicationQuit Invoked");

            if (ApplicationQuitHook == null) return;

            Delegate[] invocationList = ApplicationQuitHook.GetInvocationList();
            foreach (Action toInvoke in invocationList.Cast<Action>())
            {
                try
                {
                    toInvoke.Invoke();
                }
                catch (Exception ex)
                {
                    Logging.API.Error(ex);
                }
            }
        }

        #region SaveHandling

        /// <summary>
        ///     Called directly after a save has been loaded
        /// </summary>
        /// <remarks>GameManager.LoadGame</remarks>
        public static event Action<int> SavegameLoadHook;

        /// <summary>
        ///     Called directly after a save has been loaded
        /// </summary>
        /// <remarks>GameManager.LoadGame</remarks>
        internal static void OnSavegameLoad(int id)
        {
            Logging.API.Debug("OnSavegameLoad Invoked");

            if (SavegameLoadHook == null)
            {
                return;
            }

            Delegate[] invocationList = SavegameLoadHook.GetInvocationList();

            foreach (Action<int> toInvoke in invocationList.Cast<Action<int>>())
            {
                try
                {
                    toInvoke.Invoke(id);
                }
                catch (Exception ex)
                {
                    Logging.API.Error(ex);
                }
            }
        }

        /// <summary>
        ///     Called directly after a save has been saved
        /// </summary>
        /// <remarks>GameManager.SaveGame</remarks>
        public static event Action<int> SavegameSaveHook;

        /// <summary>
        ///     Called directly after a save has been saved
        /// </summary>
        /// <remarks>GameManager.SaveGame</remarks>
        internal static void OnSavegameSave(int id)
        {
            Logging.API.Debug("OnSavegameSave Invoked");

            if (SavegameSaveHook == null) return;

            Delegate[] invocationList = SavegameSaveHook.GetInvocationList();

            foreach (Action<int> toInvoke in invocationList.Cast<Action<int>>())
            {
                try
                {
                    toInvoke.Invoke(id);
                }
                catch (Exception ex)
                {
                    Logging.API.Error(ex);
                }
            }
        }

        /// <summary>
        ///     Called whenever a new game is started
        /// </summary>
        /// <remarks>GameManager.LoadFirstScene</remarks>
        public static event Action NewGameHook;

        /// <summary>
        ///     Called whenever a new game is started
        /// </summary>
        /// <remarks>GameManager.LoadFirstScene</remarks>
        internal static void OnNewGame()
        {
            Logging.API.Debug("OnNewGame Invoked");

            if (NewGameHook == null) return;

            Delegate[] invocationList = NewGameHook.GetInvocationList();

            foreach (Action toInvoke in invocationList.Cast<Action>())
            {
                try
                {
                    toInvoke.Invoke();
                }
                catch (Exception ex)
                {
                    Logging.API.Error(ex);
                }
            }
        }

        /// <summary>
        ///     Called before a save file is deleted
        /// </summary>
        /// <remarks>GameManager.ClearSaveFile</remarks>
        public static event Action<int> SavegameClearHook;

        /// <summary>
        ///     Called before a save file is deleted
        /// </summary>
        /// <remarks>GameManager.ClearSaveFile</remarks>
        internal static void OnSavegameClear(int id)
        {
            Logging.API.Debug("OnSavegameClear Invoked");

            if (SavegameClearHook == null) return;

            Delegate[] invocationList = SavegameClearHook.GetInvocationList();

            foreach (Action<int> toInvoke in invocationList)
            {
                try
                {
                    toInvoke.Invoke(id);
                }

                catch (Exception ex)
                {
                    Logging.API.Error(ex);
                }
            }
        }

        /// <summary>
        ///     Overrides the filename to load for a given slot.  Return null to use vanilla names.
        /// </summary>
        public static event Func<int, string> GetSaveFileNameHook;

        /// <summary>
        ///     Overrides the filename to load for a given slot.  Return null to use vanilla names.
        /// </summary>
        internal static string GetSaveFileName(int saveSlot)
        {
            Logging.API.Debug("GetSaveFileName Invoked");

            if (GetSaveFileNameHook == null)
            {
                return null;
            }

            string ret = null;

            Delegate[] invocationList = GetSaveFileNameHook.GetInvocationList();

            foreach (Func<int, string> toInvoke in invocationList)
            {
                try
                {
                    ret = toInvoke.Invoke(saveSlot);
                }

                catch (Exception ex)
                {
                    Logging.API.Error(ex);
                }
            }

            return ret;
        }

        /// <summary>
        ///     Called after a game has been cleared from a slot.
        /// </summary>
        public static event Action<int> AfterSaveGameClearHook;

        /// <summary>
        ///     Called after a game has been cleared from a slot.
        /// </summary>
        internal static void OnAfterSaveGameClear(int saveSlot)
        {
            Logging.API.Debug("OnAfterSaveGameClear Invoked");

            if (AfterSaveGameClearHook == null)
            {
                return;
            }

            Delegate[] invocationList = AfterSaveGameClearHook.GetInvocationList();

            foreach (Action<int> toInvoke in invocationList)
            {
                try
                {
                    toInvoke.Invoke(saveSlot);
                }
                catch (Exception ex)
                {
                    Logging.API.Error(ex);
                }
            }
        }

        #endregion

        internal static event Action<ModSavegameData> SaveLocalSettings;
        internal static event Action<ModSavegameData> LoadLocalSettings;

        internal static void OnSaveLocalSettings(ModSavegameData data)
        {
            data.loadedMods = LoadedModsWithVersions;
            SaveLocalSettings?.Invoke(data);
        }

        internal static void OnLoadLocalSettings(ModSavegameData data) => LoadLocalSettings?.Invoke(data);


        #region SceneHandling

        /// <summary>
        ///     Called after a new Scene has been loaded
        /// </summary>
        /// <remarks>N/A</remarks>
        public static event Action<string> SceneChanged;

        /// <summary>
        ///     Called after a new Scene has been loaded
        /// </summary>
        /// <remarks>N/A</remarks>
        internal static void OnSceneChanged(string targetScene)
        {
            Logging.API.Debug("OnSceneChanged Invoked");

            if (SceneChanged == null)
            {
                return;
            }

            Delegate[] invocationList = SceneChanged.GetInvocationList();

            foreach (Action<string> toInvoke in invocationList.Cast<Action<string>>())
            {
                try
                {
                    toInvoke.Invoke(targetScene);
                }
                catch (Exception ex)
                {
                    Logging.API.Error(ex);
                }
            }
        }

        /// <summary>
        ///     Called right before a scene gets loaded, can change which scene gets loaded
        /// </summary>
        /// <remarks>N/A</remarks>
        public static event Func<string, string> BeforeSceneLoadHook;

        /// <summary>
        ///     Called right before a scene gets loaded, can change which scene gets loaded
        /// </summary>
        /// <remarks>N/A</remarks>
        internal static string BeforeSceneLoad(string sceneName)
        {
            Logging.API.Debug("BeforeSceneLoad Invoked");

            if (BeforeSceneLoadHook == null)
            {
                return sceneName;
            }

            Delegate[] invocationList = BeforeSceneLoadHook.GetInvocationList();

            foreach (Func<string, string> toInvoke in invocationList.Cast<Func<string, string>>())
            {
                try
                {
                    sceneName = toInvoke.Invoke(sceneName);
                }
                catch (Exception ex)
                {
                    Logging.API.Error(ex);
                }
            }

            return sceneName;
        }

        #endregion

        #region mod loading values

        /// <summary>
        /// Gets a mod instance by name.
        /// </summary>
        /// <param name="name">The name of the mod.</param>
        /// <param name="onlyEnabled">Should the method only return the mod if it is enabled.</param>
        /// <param name="allowLoadError">Should the method return the mod even if it had load errors.</param>
        /// <returns></returns>
        public static IMod GetMod(
            string name,
            bool onlyEnabled = false,
            bool allowLoadError = false
        ) => COSML.ModInstanceNameMap.TryGetValue(name, out var mod)
            && (!onlyEnabled || mod.Enabled)
            && (allowLoadError || mod.Error is null)
         ? mod.Mod : null;

        /// <summary>
        /// Gets a mod instance by type.
        /// </summary>
        /// <param name="type">The type of the mod.</param>
        /// <param name="onlyEnabled">Should the method only return the mod if it is enabled.</param>
        /// <param name="allowLoadError">Should the method return the mod even if it had load errors.</param>
        /// <returns></returns>
        public static IMod GetMod(
            Type type,
            bool onlyEnabled = false,
            bool allowLoadError = false
        ) => COSML.ModInstanceTypeMap.TryGetValue(type, out var mod)
            && (!onlyEnabled || mod.Enabled)
            && (allowLoadError || mod.Error is null)
         ? mod.Mod : null;

        /// <summary>
        /// Gets if the mod is currently enabled.
        /// </summary>
        /// <param name="mod">The togglable mod to check.</param>
        /// <returns></returns>
        public static bool ModEnabled(
            IModTogglable mod
        ) => ModEnabled(mod.GetType());

        /// <summary>
        /// Gets if a mod is currently enabled.
        /// </summary>
        /// <param name="name">The name of the mod to check.</param>
        /// <returns></returns>
        public static bool ModEnabled(
            string name
        ) => COSML.ModInstanceNameMap.TryGetValue(name, out var instance) ? instance.Enabled : true;

        /// <summary>
        /// Gets if a mod is currently enabled.
        /// </summary>
        /// <param name="type">The type of the mod to check.</param>
        /// <returns></returns>
        public static bool ModEnabled(
            Type type
        ) => COSML.ModInstanceTypeMap.TryGetValue(type, out var instance) ? instance.Enabled : true;

        /// <summary>
        /// Returns an iterator over all mods.
        /// </summary>
        /// <param name="onlyEnabled">Should the iterator only contain enabled mods.</param>
        /// <param name="allowLoadError">Should the iterator contain mods which have load errors.</param>
        /// <returns></returns>
        public static IEnumerable<IMod> GetAllMods(
            bool onlyEnabled = false,
            bool allowLoadError = false
        ) => COSML.ModInstances
            .Where(x => (!onlyEnabled || x.Enabled) && (allowLoadError || x.Error is null))
            .Select(x => x.Mod);

        #endregion

        private static event Action _finishedLoadingModsHook;

        /// <summary>
        /// Event invoked when mods have finished loading. If modloading has already finished, subscribers will be invoked immediately.
        /// </summary>
        public static event Action FinishedLoadingModsHook
        {
            add
            {
                _finishedLoadingModsHook += value;

                if (!COSML.LoadState.HasFlag(COSML.ModLoadState.Loaded)) return;

                try
                {
                    value.Invoke();
                }
                catch (Exception ex)
                {
                    Logging.API.Error(ex);
                }
            }
            remove => _finishedLoadingModsHook -= value;
        }

        internal static void OnFinishedLoadingMods()
        {
            if (_finishedLoadingModsHook == null) return;

            Delegate[] invocationList = _finishedLoadingModsHook.GetInvocationList();

            foreach (Action toInvoke in invocationList.Cast<Action>())
            {
                try
                {
                    toInvoke.Invoke();
                }
                catch (Exception ex)
                {
                    Logging.API.Error(ex);
                }
            }
        }
    }
}
