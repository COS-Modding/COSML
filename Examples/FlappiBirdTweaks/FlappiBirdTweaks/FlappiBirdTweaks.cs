using COSML.Modding;
using System.Collections.Generic;
using static COSML.Menu.MenuUtils;

namespace FlappiBirdTweaks
{
    public class FlappiBirdTweaks : Mod, ILocalSettings<LocalData>, IModTogglable, IModMenu
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
        private float[] speedValues = new float[] { 0.25f, 0.5f, 0.75f, 1f, 2f, 4f, 8f };
        private object[] speedSteps = new object[] { "x¼", "x½", "x¾", "x1", "x2", "x4", "x8" };

        public override void Init()
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
            self.upSpeed *= speedValues[localData.upSpeed];
            self.downSpeed *= speedValues[localData.downSpeed];
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

        private void UpdateSpeed(bool up, int newIndex)
        {
            if (up)
            {
                localData.upSpeed = newIndex;
                if (instance != null)
                {
                    instance.upSpeed = orig_upSpeed * speedValues[newIndex];
                }
            }
            else
            {
                localData.downSpeed = newIndex;
                if (instance != null)
                {
                    instance.downSpeed = orig_downSpeed * speedValues[newIndex];
                }
            }
        }

        public List<IOptionData> GetMenu()
        {
            return new List<IOptionData> {
                new SliderData(
                    "Up speed",
                    speedSteps,
                    localData.upSpeed,
                    (newIndex) => UpdateSpeed(true, newIndex)
                ),
                new SliderData(
                    "Down speed",
                    speedSteps,
                    localData.downSpeed,
                    (newIndex) => UpdateSpeed(false, newIndex)
                )
            };
        }
    }
}
