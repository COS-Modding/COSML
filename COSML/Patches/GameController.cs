using COSML.Log;
using COSML.Modding;
using MonoMod;
using System.Threading;
using UnityEngine;

namespace COSML.Patches
{
    [MonoModPatch("global::GameController")]
    public class GameController : global::GameController
    {
        public void Awake()
        {
            // Mod Loading
            if (COSML.LoadState == COSML.ModLoadState.NotStarted)
            {
                COSML.LoadState = COSML.ModLoadState.Started;

                // Preload reflection
                new Thread(ReflectionHelper.PreloadCommonTypes).Start();

                GameObject obj = new();
                DontDestroyOnLoad(obj);
                obj.AddComponent<BakingIgnore>().StartCoroutine(COSML.LoadModsInit(obj));
            }
            else
            {
                Logging.API.Debug($"Already begun mod loading (state {COSML.LoadState})");
            }
        }

        public void OnApplicationQuit()
        {
            ModHooks.OnApplicationQuit();
        }
    }
}
