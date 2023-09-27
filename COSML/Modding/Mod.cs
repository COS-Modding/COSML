using COSML.Log;
using MonoMod.Utils;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace COSML.Modding
{
    /// <inheritdoc cref="Loggable" />
    /// <inheritdoc cref="IMod" />
    /// <summary>
    ///     Base mod class.
    /// </summary>
    public abstract class Mod : Loggable, IMod
    {
        private readonly string _globalSettingsPath;

        private readonly Type globalSettingsType = null;
        private readonly FastReflectionDelegate onLoadGlobalSettings;
        private readonly FastReflectionDelegate onSaveGlobalSettings;
        private readonly Type saveSettingsType = null;
        private readonly FastReflectionDelegate onLoadSaveSettings;
        private readonly FastReflectionDelegate onSaveSaveSettings;

        /// <summary>
        ///     The Mods Name
        /// </summary>
        public readonly string Name;

        /// <inheritdoc />
        /// <summary>
        ///     Constructs the mod, assigns the instance and sets the name.
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

            _globalSettingsPath ??= GetGlobalSettingsPath();

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
        ///     Get's the Mod's Name
        /// </summary>
        /// <returns></returns>
        public string GetName()
        {
            return Name;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Returns the objects to preload in order for the mod to work.
        /// </summary>
        /// <returns>A List of tuples containing scene name, object name</returns>
        public virtual List<(string, string)> GetPreloadNames()
        {
            return null;
        }

        /// <summary>
        /// A list of requested scenes to be preloaded and actions to execute on loading of those scenes
        /// </summary>
        /// <returns>List of tuples containg scene names and the respective actions.</returns>
        public virtual (string, Func<IEnumerator>)[] PreloadSceneHooks() => Array.Empty<(string, Func<IEnumerator>)>();

        /// <inheritdoc />
        /// <summary>
        ///     Called after preloading of all mods.
        /// </summary>
        /// <param name="preloadedObjects">The preloaded objects relevant to this <see cref="Mod" /></param>
        public virtual void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects) => Initialize();

        /// <inheritdoc />
        /// <summary>
        ///     Returns version of Mod
        /// </summary>
        /// <returns>Mod Version</returns>
        public virtual string GetVersion()
        {
            return "UNKNOWN";
        }

        /// <summary>
        ///     Controls when this mod should load compared to other mods.  Defaults to ordered by name.
        /// </summary>
        /// <returns></returns>
        public virtual int LoadPriority()
        {
            return 1;
        }

        /// <summary>
        ///     Called after preloading of all mods.
        /// </summary>
        public virtual void Initialize() { }

        /// <summary>
        ///     If this mod defines a menu via the <see cref="IModMenu"/> or <see cref="ICustomMenuMod"/> interfaces, override this method to 
        ///     change the text of the button to jump to this mod's menu.
        /// </summary>
        /// <returns></returns>
        //public virtual string GetMenuButtonText() => $"{GetName()} {Language.Language.Get("MAIN_OPTIONS", "MainMenu")}";

        private void HookSaveMethods()
        {
            ModHooks.ApplicationQuitHook += SaveGlobalSettings;
            ModHooks.SaveLocalSettings += SaveLocalSettings;
            ModHooks.LoadLocalSettings += LoadLocalSettings;
            SceneManager.activeSceneChanged += SceneChanged;
        }

        private void SceneChanged(Scene arg0, Scene arg1)
        {
            Info($"Changed scene from {arg0.name} to {arg1.name}");
            if (arg1.name != Constants.MAIN_MENU_SCENE_NAME) return;

            Info("Save load settings (in theory)");
            //if (saveSettingsType is Type saveType)
            //{
            //    onLoadSaveSettings(this, Activator.CreateInstance(saveType));
            //}
        }

        private void LoadGlobalSettings()
        {
            try
            {
                // test to see if we can load global settings from this mod
                if (globalSettingsType is Type saveType)
                {
                    if (!File.Exists(_globalSettingsPath))
                        return;
                    Info("Loading Global Settings");

                    if (TryLoadGlobalSettings(_globalSettingsPath, saveType))
                        return;

                    Error($"Null global settings passed to {GetName()}");

                    string globalSettingsBackup = _globalSettingsPath + ".bak";
                    if (!File.Exists(globalSettingsBackup))
                        return;

                    if (TryLoadGlobalSettings(globalSettingsBackup, saveType))
                    {
                        Info("Successfully loaded global settings from backup");
                        File.Delete(_globalSettingsPath);
                        File.Copy(globalSettingsBackup, _globalSettingsPath);
                    }
                    Error("Failed to load global settings from backup");

                }
            }
            catch (Exception e)
            {
                Error(e);
            }
        }

        /// <summary>
        /// Try to load the global settings from the given path. Returns true if the global settings were successfully loaded.
        /// </summary>
        private bool TryLoadGlobalSettings(string path, Type saveType)
        {
            using FileStream fileStream = File.OpenRead(_globalSettingsPath);
            using var reader = new StreamReader(fileStream);
            string json = reader.ReadToEnd();

            object obj = JsonConvert.DeserializeObject(
                json,
                saveType,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    ObjectCreationHandling = ObjectCreationHandling.Replace
                }
            );

            if (obj is null) return false;

            onLoadGlobalSettings(this, obj);
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
                    object obj = onSaveGlobalSettings(this);
                    if (obj is null) return;

                    if (File.Exists(_globalSettingsPath + ".bak")) File.Delete(_globalSettingsPath + ".bak");
                    if (File.Exists(_globalSettingsPath)) File.Move(_globalSettingsPath, _globalSettingsPath + ".bak");

                    using FileStream fileStream = File.Create(_globalSettingsPath);
                    using var writer = new StreamWriter(fileStream);
                    writer.Write(JsonConvert.SerializeObject(
                        obj,
                        Formatting.Indented,
                        new JsonSerializerSettings
                        {
                            TypeNameHandling = TypeNameHandling.Auto
                        }
                    ));
                }
            }
            catch (Exception e)
            {
                Error(e);
            }
        }

        private void LoadLocalSettings(ModSavegameData data)
        {
            try
            {
                if (saveSettingsType is not Type saveType) return;
                if (!data.modData.TryGetValue(GetName(), out var obj)) return;

                onLoadSaveSettings
                (
                    this,
                    obj.ToObject
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
            catch (Exception e)
            {
                Error(e);
            }
        }

        private void SaveLocalSettings(ModSavegameData data)
        {
            try
            {
                if (saveSettingsType is not Type saveType)
                    return;

                //var settings = this.onSaveSaveSettings(this);

                //if (settings is null)
                //    return;

                //data.modData[this.GetName()] = JToken.FromObject
                //(
                //    settings,
                //    JsonSerializer.Create
                //    (
                //        new JsonSerializerSettings
                //        {
                //            ContractResolver = ShouldSerializeContractResolver.Instance,
                //            TypeNameHandling = TypeNameHandling.Auto,
                //            Converters = JsonConverterTypes.ConverterTypes
                //        }
                //    )
                //);
            }
            catch (Exception e)
            {
                Error(e);
            }
        }
    }
}