using COSML.Log;
using COSML.Menu;
using MonoMod;
using System;
using UnityEngine;
using static COSML.Menu.MenuUtils;

namespace COSML.Patches
{
    [MonoModPatch("global::MainMenu")]
    public class MainMenu : global::MainMenu
    {
        public ModsMainMenu modMenu;

        private bool edited = false;

        [MonoModIgnore]
        public new RootMainMenu rootMenu;
        [MonoModIgnore]
        public new PauseMenu pauseMenu;

        public extern void orig_Init();
        public new void Init()
        {
            orig_Init();

            copyright.text += $" (COSML-{COSML.Version})";
        }

        public extern void orig_Hide();
        public new void Hide()
        {
            orig_Hide();
            modMenu?.Hide();
        }

        public void Update()
        {
            // For some reason mod button/menu are found in the scene but don't appear in-game
            // so we edit the main menu only when the scene has finished loading
            if (edited || GameObject.Find($"{Constants.MAIN_MENU_PATH}/Menu_Mods") != null) return;
            edited = true;

            try
            {
                SaveElements();

                modMenu = CreateMenu<ModsMainMenu>(new InternalMenuData
                {
                    label = "Mods",
                    parent = rootMenu
                });

                // Root main menu
                rootMenu.EditMenu();

                // Pause menu
                pauseMenu.EditMenu();
            }
            catch (Exception ex)
            {
                Logging.API.Error($"Error creating mod menu:\n" + ex);
            }
        }
    }
}