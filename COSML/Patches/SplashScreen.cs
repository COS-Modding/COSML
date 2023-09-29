#pragma warning disable 414, 649

using COSML.Log;
using MonoMod;
using System.Threading;
using UnityEngine;

namespace COSML.Patches
{
    [MonoModPatch("global::SplashScreen")]
    public class SplashScreen : global::SplashScreen
    {
        [MonoModIgnore]
        private float waitToLaunchFirstVideoTimeLeft;
        [MonoModIgnore]
        private bool isVideoStarted;
        [MonoModIgnore]
        private bool waitVideoPublisherEnd;
        [MonoModIgnore]
        private bool waitVideoDevelopperEnd;
        [MonoModIgnore]
        private AbstractPlateform plateformManager;

        public extern void orig_Start();
        public new void Start()
        {
            orig_Start();
            waitToLaunchFirstVideoTimeLeft = 0f;
            isVideoStarted = true;
            waitVideoPublisherEnd = false;
            splashScreenPublisher.Stop();
            waitVideoDevelopperEnd = false;
            splashScreenDevelopper.Stop();
            loader.gameObject.SetActive(true);
            loader.SetTrigger("Loop");
            plateformManager.GetSaveManager().PreLoadData();

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
                Logging.API.Debug($"MainMenu: Already begun mod loading (state {COSML.LoadState})");
            }
        }
    }
}
