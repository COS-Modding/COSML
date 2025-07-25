﻿using COSML.Modding;
using System.Collections.Generic;
using static COSML.MainMenu.MenuUtils;

namespace FlappiBirdTweaks
{
    public class FlappiBirdTweaks : Mod, ILocalSettings<LocalData>, IModTogglable, IModMenu
    {
        private LocalData localData = new();

        public FlappiBirdTweaks() : base("FlappiBird Tweaks") { }
        public override string GetVersion() => "1.0.0";
        public LocalData OnSaveLocal() => localData;

        // Instance of the puzzle to edit values
        private FlappiBirdPuzzle instance;


        // Copies of the orginal values of the bird speed, for unloading
        private float orig_upSpeed;
        private float orig_downSpeed;

        // Differents speeds of the bird
        private float[] speedValues = [0.25f, 0.5f, 0.75f, 1f, 2f, 4f, 8f];
        private object[] speedSteps = ["x¼", "x½", "x¾", "x1", "x2", "x4", "x8"];

        private bool loaded;

        // Menu options
        private MenuSlider upSpeedSlider;
        private MenuSlider downSpeedSlider;

        public override void Init()
        {
            Info("Loaded FlappiBirdTweaks");

            loaded = false;

            // Hook of the puzzle loop function
            On.FlappiBirdPuzzle.Loop += FlappiBirdLoop;
        }

        private void FlappiBirdLoop(On.FlappiBirdPuzzle.orig_Loop orig, FlappiBirdPuzzle self)
        {
            // Call the original function to keep the function logic
            orig(self);

            // As we are in the Loop function, we must edit values once
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

        public void OnLoadLocal(LocalData data)
        {
            localData = data;

            upSpeedSlider?.SetValue(localData.upSpeed);
            downSpeedSlider?.SetValue(localData.downSpeed);
        }

        // Mod menu to easily edit values
        public IList<MenuOption> GetMenu()
        {
            upSpeedSlider = new MenuSlider(
                "Up speed",
                speedSteps,
                localData.upSpeed,
                (newIndex) => UpdateSpeed(true, newIndex)
            );

            downSpeedSlider = new MenuSlider(
                "Down speed",
                speedSteps,
                localData.downSpeed,
                (newIndex) => UpdateSpeed(false, newIndex)
            );

            return [upSpeedSlider, downSpeedSlider];
        }

    }
}
