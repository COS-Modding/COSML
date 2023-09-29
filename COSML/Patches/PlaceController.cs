using COSML.Log;
using COSML.Modding;
using MonoMod;
using Newtonsoft.Json;
using System;
using System.IO;
using UnityEngine;

namespace COSML.Patches
{
    [MonoModPatch("global::PlaceController")]
    public class PlaceController : global::PlaceController
    {
        private static string ModdedSavePath(int slot) => Path.Combine(Application.persistentDataPath, $"save{slot}.modded.json");
        private ModSavegameData moddedData;

        private int currentGameSaveId = -1;
        private Place currentPlace = null;
        private Place lastPlace = null;

        public extern void orig_SetPlace(Place newPlace, bool save);
        public new void SetPlace(Place newPlace, bool save)
        {
            PlaceChanged(newPlace);

            lastPlace = currentPlace;
            orig_SetPlace(newPlace, save);

        }

        private void PlaceChanged(Place place)
        {
            Logging.API.Info($"Changed place from {currentPlace?.gameObject.scene.name ?? "None"} to {place.gameObject.scene.name}");
            ModHooks.OnPlaceChanged(currentPlace, place);

            if (currentGameSaveId > -1 && lastPlace != null && currentPlace == null && place.gameObject.scene.name == Constants.TITLE_SCENE_NAME)
            {
                moddedData ??= new ModSavegameData();
                ModHooks.OnSaveLocalSettings(moddedData);
                SaveLocalData(currentGameSaveId);
            }
            else if (currentPlace?.gameObject.scene.name == Constants.TITLE_SCENE_NAME && lastPlace == null)
            {
                LoadLocalData(currentGameSaveId);
                ModHooks.OnLoadLocalSettings(moddedData);
            }

        }

        private void SaveLocalData(int saveSlot)
        {
            Logging.API.Debug($"Saving local data {saveSlot}");
            try
            {
                var path = ModdedSavePath(saveSlot);
                string modded = JsonConvert.SerializeObject(
                    moddedData,
                    Formatting.Indented,
                    new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Auto
                    }
                );
                if (File.Exists(path + ".bak")) File.Delete(path + ".bak");
                if (File.Exists(path)) File.Move(path, path + ".bak");
                using FileStream fileStream = File.Create(path);
                using var writer = new StreamWriter(fileStream);
                writer.Write(modded);
            }
            catch (Exception ex)
            {
                Logging.API.Error("Error saving local save data" + ex);
            }
        }

        private void LoadLocalData(int saveSlot)
        {
            Logging.API.Debug($"Loading local data {saveSlot}");
            try
            {
                var path = ModdedSavePath(saveSlot);
                if (File.Exists(path))
                {
                    using FileStream fileStream = File.OpenRead(path);
                    using var reader = new StreamReader(fileStream);
                    string json = reader.ReadToEnd();
                    moddedData = JsonConvert.DeserializeObject<ModSavegameData>(
                        json,
                        new JsonSerializerSettings()
                        {
                            TypeNameHandling = TypeNameHandling.Auto,
                            ObjectCreationHandling = ObjectCreationHandling.Replace
                        }
                    );
                    if (moddedData == null)
                    {
                        Logging.API.Error($"Loaded mod local data deserialized to null: {json}");
                        moddedData = new ModSavegameData();
                    }
                }
                else
                {
                    moddedData = new ModSavegameData();
                }
            }
            catch (Exception ex)
            {
                Logging.API.Error(ex);
                moddedData = new ModSavegameData();
            }
        }
    }
}
