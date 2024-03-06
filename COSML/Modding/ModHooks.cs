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
    /// Class to hook into various events for the game.
    /// </summary>
    public class ModHooks
    {
        private static readonly string SettingsPath = Path.Combine(Application.persistentDataPath, "COSML.GlobalSettings.json");
        private static COSMLConsole _console;

        /// <summary>
        /// Version of the game.
        /// </summary>
        public static string GameVersion = Application.version;

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
                if (!File.Exists(SettingsPath)) return;

                using FileStream fileStream = File.OpenRead(SettingsPath);
                using var reader = new StreamReader(fileStream);
                string json = reader.ReadToEnd();

                var settings = JsonConvert.DeserializeObject<COSMLGlobalSettings>(
                    json,
                    new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Auto,
                        ObjectCreationHandling = ObjectCreationHandling.Replace
                    }
                );
                if (settings != null)
                {
                    GlobalSettings = settings;
                    Logging.SetLogLevel(GlobalSettings.LoggingLevel);
                    Logging.SetIncludeTimestampt(GlobalSettings.IncludeTimestamps);
                    Logging.API.Info("Loaded Global Settings");
                }
            }
            catch (Exception ex)
            {
                Logging.API.Error(ex);
            }
        }

        internal static void SaveGlobalSettings()
        {
            try
            {
                var settings = GlobalSettings;
                if (settings is null) return;

                settings.ModEnabledSettings = [];

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

                Logging.API.Info("Saved Global Settings");
            }
            catch (Exception ex)
            {
                Logging.API.Error(ex);
            }
        }

        internal static void LogConsole(string message, LogLevel level)
        {
            try
            {
                _console ??= new GameObject("COSMLConsole").AddComponent<COSMLConsole>();
                _console.AddText(message, level);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        /// <summary>
        /// Called when the game is fully closed
        /// </summary>
        public static event Action ApplicationQuitHook;

        /// <summary>
        /// Called when the game is fully closed
        /// </summary>
        internal static void OnApplicationQuit()
        {
            Logging.API.Debug("Quitting game");
            ApplicationQuitHook?.Invoke();
        }

        internal static event Action<ModSavegameData> SaveLocalSettings;
        internal static event Action<ModSavegameData> LoadLocalSettings;

        internal static void OnSaveLocalSettings(ModSavegameData data) => SaveLocalSettings?.Invoke(data);
        internal static void OnLoadLocalSettings(ModSavegameData data) => LoadLocalSettings?.Invoke(data);


        #region PlaceHandling

        /// <summary>
        /// Called after a the place has been changed
        /// </summary>
        public static event Action<Place, Place> PlaceChanged;

        /// <summary>
        /// Called after the place has been changed
        /// </summary>
        internal static void OnPlaceChanged(Place from, Place to)
        {
            PlaceChanged?.Invoke(from, to);
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
        public static IMod GetMod(string name, bool onlyEnabled = false, bool allowLoadError = false)
        {
            return COSML.ModInstanceNameMap.TryGetValue(name, out var mod) && (!onlyEnabled || mod.Enabled) && (allowLoadError || mod.Error is null) ? mod.Mod : null;
        }

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
        public static bool ModEnabled(IModTogglable mod) => ModEnabled(mod.GetType());

        /// <summary>
        /// Gets if a mod is currently enabled.
        /// </summary>
        /// <param name="name">The name of the mod to check.</param>
        /// <returns></returns>
        public static bool ModEnabled(string name) => !COSML.ModInstanceNameMap.TryGetValue(name, out var instance) || instance.Enabled;

        /// <summary>
        /// Gets if a mod is currently enabled.
        /// </summary>
        /// <param name="type">The type of the mod to check.</param>
        /// <returns></returns>
        public static bool ModEnabled(Type type) => !COSML.ModInstanceTypeMap.TryGetValue(type, out var instance) || instance.Enabled;

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

        private static event Action InternalFinishedLoadingModsHook;

        /// <summary>
        /// Event invoked when mods have finished loading. If modloading has already finished, subscribers will be invoked immediately.
        /// </summary>
        public static event Action FinishedLoadingModsHook
        {
            add
            {
                InternalFinishedLoadingModsHook += value;

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
            remove => InternalFinishedLoadingModsHook -= value;
        }

        internal static void OnFinishedLoadingMods()
        {
            InternalFinishedLoadingModsHook?.Invoke();
        }
    }
}
