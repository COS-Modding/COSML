#pragma warning disable IDE0052, IDE0044, CS0649, CS0414

using MonoMod;

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
        }
    }
}
