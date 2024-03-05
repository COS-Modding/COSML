using COSML.Log;
using MonoMod.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace COSML.Modding
{
    /// <inheritdoc cref="Loggable" />
    /// <inheritdoc cref="IMod" />
    /// <summary>
    /// Base mod class.
    /// </summary>
    public abstract class Mod : Loggable, IMod
    {
        private readonly string globalSettingsPath;

        private readonly Type globalSettingsType = null;
        private readonly FastReflectionDelegate onLoadGlobalSettings;
        private readonly FastReflectionDelegate onSaveGlobalSettings;
        private readonly Type saveSettingsType = null;
        private readonly FastReflectionDelegate onLoadSaveSettings;
        private readonly FastReflectionDelegate onSaveSaveSettings;

        /// <summary>
        /// The Mods Name
        /// </summary>
        public readonly string Name;

        /// <inheritdoc />
        /// <summary>
        /// Constructs the mod, assigns the instance and sets the name.
        /// </summary>
        protected Mod(string name = null)
        {
            if (string.IsNullOrEmpty(name)) name = GetType().Name;

            if
            (
                GetType()
                    .GetInterfaces()
                    .FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IGlobalSettings<>))
                    is Type globalType
            )
            {
                globalSettingsType = globalType.GetGenericArguments()[0];
                foreach (var mi in globalType.GetMethods())
                {
                    switch (mi.Name)
                    {
                        case nameof(IGlobalSettings<object>.OnLoadGlobal):
                            onLoadGlobalSettings = mi.GetFastDelegate();
                            break;
                        case nameof(IGlobalSettings<object>.OnSaveGlobal):
                            onSaveGlobalSettings = mi.GetFastDelegate();
                            break;
                    }
                }
            }

            if
            (
                GetType()
                    .GetInterfaces()
                    .FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ILocalSettings<>))
                    is Type saveType
            )
            {
                saveSettingsType = saveType.GetGenericArguments()[0];

                foreach (var mi in saveType.GetMethods())
                {
                    switch (mi.Name)
                    {
                        case nameof(ILocalSettings<object>.OnLoadLocal):
                            onLoadSaveSettings = mi.GetFastDelegate();
                            break;
                        case nameof(ILocalSettings<object>.OnSaveLocal):
                            onSaveSaveSettings = mi.GetFastDelegate();
                            break;
                    }
                }
            }

            Name = name;

            Info("Initializing");

            globalSettingsPath ??= GetGlobalSettingsPath();

            LoadGlobalSettings();
            HookSaveMethods();
        }

        private string GetGlobalSettingsPath()
        {
            string globalSettingsFileName = $"{GetType().Name}.GlobalSettings.json";

            string location = GetType().Assembly.Location;
            string directory = Path.GetDirectoryName(location);
            string globalSettingsOverride = Path.Combine(directory, globalSettingsFileName);

            if (File.Exists(globalSettingsOverride))
            {
                Info("Overriding Global Settings path with Mod directory");
                return globalSettingsOverride;
            }

            return Path.Combine(Application.persistentDataPath, globalSettingsFileName);
        }

        /// <inheritdoc />
        /// <summary>
        /// Get's the mod's Name
        /// </summary>
        /// <returns></returns>
        public string GetName() => Name;

        /// <inheritdoc />
        /// <summary>
        /// Returns the objects to preload in order for the mod to work.
        /// </summary>
        /// <returns>A List of tuples containing scene name, object name</returns>
        public virtual List<(string, string)> GetPreloadNames() => null;

        /// <summary>
        /// A list of requested scenes to be preloaded and actions to execute on loading of those scenes
        /// </summary>
        /// <returns>List of tuples containg scene names and the respective actions.</returns>
        public virtual (string, Func<IEnumerator>)[] PreloadSceneHooks() => Array.Empty<(string, Func<IEnumerator>)>();

        /// <inheritdoc />
        /// <summary>
        /// Called after preloading of all mods.
        /// </summary>
        /// <param name="preloadedObjects">The preloaded objects relevant to this <see cref="Mod" /></param>
        public virtual void Init(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects) => Init();

        /// <inheritdoc />
        /// <summary>
        /// Returns version of Mod
        /// </summary>
        /// <returns>Mod Version</returns>
        public virtual string GetVersion() => "???";

        /// <summary>
        /// Controls when this mod should load compared to other mods.  Defaults to ordered by name.
        /// </summary>
        /// <returns></returns>
        public virtual int LoadPriority() => 1;

        /// <summary>
        /// Called after preloading of all mods.
        /// </summary>
        public virtual void Init() { }

        private void HookSaveMethods()
        {
            ModHooks.ApplicationQuitHook += SaveGlobalSettings;
            ModHooks.SaveLocalSettings += SaveLocalSettings;
            ModHooks.LoadLocalSettings += LoadLocalSettings;
        }

        private void PlaceChanged(Place from, Place to)
        {
            if (from.name == Constants.SPLASH_SCREEN_SCENE_NAME || to.name != Constants.MAIN_MENU_SCENE_NAME) return;

            if (saveSettingsType is Type saveType)
            {
                Debug($"onLoadSaveSettings: {saveType.Name}");
                onLoadSaveSettings(this, Activator.CreateInstance(saveType));
            }
        }

        private void LoadGlobalSettings()
        {
            try
            {
                // test to see if we can load global settings from this mod
                if (globalSettingsType is Type saveType)
                {
                    if (!File.Exists(globalSettingsPath)) return;
                    Info("Loading Global Settings");

                    if (TryLoadGlobalSettings(globalSettingsPath, saveType)) return;

                    Error($"Null global settings passed to {GetName()}");

                    string globalSettingsBackup = globalSettingsPath + ".bak";
                    if (!File.Exists(globalSettingsBackup)) return;

                    if (TryLoadGlobalSettings(globalSettingsBackup, saveType))
                    {
                        Info("Successfully loaded global settings from backup");
                        File.Delete(globalSettingsPath);
                        File.Copy(globalSettingsBackup, globalSettingsPath);
                    }
                    Error("Failed to load global settings from backup");

                }
            }
            catch (Exception ex)
            {
                Error(ex);
            }
        }

        /// <summary>
        /// Try to load the global settings from the given path. Returns true if the global settings were successfully loaded.
        /// </summary>
        private bool TryLoadGlobalSettings(string path, Type saveType)
        {
            using FileStream fileStream = File.OpenRead(path);
            using var reader = new StreamReader(fileStream);
            string json = reader.ReadToEnd();

            object settings = JsonConvert.DeserializeObject(
                json,
                saveType,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    ObjectCreationHandling = ObjectCreationHandling.Replace
                }
            );

            if (settings is null) return false;

            onLoadGlobalSettings(this, settings);
            return true;
        }

        /// <summary>
        /// Save global settings to saves folder.
        /// </summary>
        protected void SaveGlobalSettings()
        {
            try
            {
                if (globalSettingsType is Type saveType)
                {
                    Info("Saving Global Settings");
                    object settings = onSaveGlobalSettings(this);
                    if (settings is null) return;

                    if (File.Exists(globalSettingsPath + ".bak")) File.Delete(globalSettingsPath + ".bak");
                    if (File.Exists(globalSettingsPath)) File.Move(globalSettingsPath, globalSettingsPath + ".bak");

                    using FileStream fileStream = File.Create(globalSettingsPath);
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
            }
            catch (Exception ex)
            {
                Error(ex);
            }
        }

        private void LoadLocalSettings(ModSavegameData data)
        {
            try
            {
                if (saveSettingsType is not Type saveType) return;
                if (!data.modData.TryGetValue(GetName(), out var settings)) return;

                onLoadSaveSettings
                (
                    this,
                    settings.ToObject
                    (
                        saveType,
                        JsonSerializer.Create
                        (
                            new JsonSerializerSettings
                            {
                                TypeNameHandling = TypeNameHandling.Auto,
                                ObjectCreationHandling = ObjectCreationHandling.Replace
                            }
                        )
                    )
                );
            }
            catch (Exception ex)
            {
                Error(ex);
            }
        }

        private void SaveLocalSettings(ModSavegameData data)
        {
            try
            {
                if (saveSettingsType is not Type saveType) return;

                var settings = onSaveSaveSettings(this);
                if (settings is null) return;

                data.modData[GetName()] = JToken.FromObject
                (
                    settings,
                    JsonSerializer.Create
                    (
                        new JsonSerializerSettings
                        {
                            TypeNameHandling = TypeNameHandling.Auto
                        }
                    )
                );
            }
            catch (Exception ex)
            {
                Error(ex);
            }
        }
    }
}