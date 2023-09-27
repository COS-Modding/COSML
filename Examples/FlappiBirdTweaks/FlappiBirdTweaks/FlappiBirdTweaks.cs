using COSML.Modding;

namespace FlappiBirdTweaks
{
    public class FlappiBirdTweaks : Mod, IGlobalSettings<GlobalData>, ILocalSettings<LocalData>, IModTogglable
    {
        private GlobalData globalData = new GlobalData();
        private LocalData localData = new LocalData();

        public FlappiBirdTweaks() : base("FlappiBird Tweaks") { }
        public override string GetVersion() => "1.0.0";
        public void OnLoadGlobal(GlobalData data) => globalData = data;
        public GlobalData OnSaveGlobal() => globalData;
        public void OnLoadLocal(LocalData data) => localData = data;
        public LocalData OnSaveLocal() => localData;

        private bool loaded;
        private float orig_upSpeed;
        private float orig_downSpeed;
        private FlappiBirdPuzzle instance;

        public override void Initialize()
        {
            Info("Loaded FlappiBirdTweaks");

            loaded = false;
            On.FlappiBirdPuzzle.Loop += FlappiBirdLoop;
        }

        private void FlappiBirdLoop(On.FlappiBirdPuzzle.orig_Loop orig, FlappiBirdPuzzle self)
        {
            orig(self);

            if (loaded) return;
            loaded = true;

            Info("Backup original speed values");
            instance = self;
            orig_upSpeed = self.upSpeed;
            orig_downSpeed = self.downSpeed;

            Info("Set new speed values");
            self.upSpeed = globalData.upSpeed;
            self.downSpeed = globalData.downSpeed;
        }

        public void Unload()
        {
            Info("Unloaded FlappiBirdTweaks");

            On.FlappiBirdPuzzle.Loop -= FlappiBirdLoop;

            if (instance != null)
            {
                instance.upSpeed = orig_upSpeed;
                instance.downSpeed = orig_downSpeed;
            }
        }
    }
}
