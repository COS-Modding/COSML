#pragma warning disable IDE0044, IDE0060

using MonoMod;
using UnityEngine;
using static COSML.MainMenu.MenuUtils;

namespace COSML.Patches
{
    [MonoModPatch("global::RootMainMenu")]
    public class RootMainMenu : global::RootMainMenu
    {
        public MainMenuButton modButton;

        [MonoModIgnore]
        private bool canQuitApplication = true;
        [MonoModIgnore]
        private AbstractUIBrowser browser;

        public void EditMenu()
        {
            // Add mods button
            GameObject mainMenuGo = GameObject.Find($"{Constants.MAIN_MENU_PATH}/Menu_Principal");
            modButton = CreateRootButton(new ButtonData
            {
                parent = mainMenuGo.transform,
                menu = this,
                label = new I18nKey("cosml.menu.mods"),
                buttonId = 2
            });
            modButton.transform.localPosition = GetRootButtonLocalPosition(2);

            // Edit quit button
            GameObject quitButtonGo = mainMenuGo.transform.Find($"Bouton Quit").gameObject;
            quitButtonGo.transform.localPosition = GetRootButtonLocalPosition(3);
            MainMenuButton quitMainMenuButton = quitButtonGo.GetComponent<MainMenuButton>();
            quitMainMenuButton.buttonId = 3;
        }

        public new void Loop()
        {
            if (gameObject.activeSelf)
            {
                playButton.Loop();
                optionButton.Loop();
                modButton?.Loop();
                if (canQuitApplication)
                {
                    quitButton.Loop();
                }
            }
        }

        public new void Show(AbstractMainMenu previousMenu)
        {
            if (!gameObject.activeSelf)
            {
                MainMenu mainMenu = (MainMenu)GameController.GetInstance().GetUIController().mainMenu;
                gameObject.SetActive(true);
                playButton.InitRoll();
                optionButton.InitRoll();
                modButton?.InitRoll();
                if (canQuitApplication)
                {
                    quitButton.InitRoll();
                }

                mainMenu.selectSaveMenu.ResetBrowser();
                mainMenu.optionMenu.ResetBrowser();
                mainMenu.modMenu.ResetBrowser();
            }
        }

        public new void ForceExit()
        {
            playButton.ForceExit();
            optionButton.ForceExit();
            modButton?.ForceExit();
            if (canQuitApplication)
            {
                quitButton.ForceExit();
            }
        }

        public new void OnClic(int buttonId)
        {
            GameController instance = (GameController)GameController.GetInstance();
            InputsController inputsController = instance.GetInputsController();
            Patches.MainMenu mainMenu = (Patches.MainMenu)instance.GetUIController().mainMenu;
            switch (buttonId)
            {
                case 0:
                    instance.PlayGlobalSound("Play_menu_clic", false);
                    mainMenu.Swap(mainMenu.selectSaveMenu, true);
                    break;
                case 1:
                    instance.PlayGlobalSound("Play_menu_clic", false);
                    mainMenu.Swap(mainMenu.optionMenu, true);
                    break;
                case 2:
                    instance.PlayGlobalSound("Play_menu_clic", false);
                    mainMenu.Swap(mainMenu.modMenu, true);
                    break;
                case 3:
                    inputsController.DisplayCursor(false);
                    instance.PlayGlobalSound("Play_menu_clic", false);
                    instance.GetUIController().mainMenu.Disable();
                    Application.Quit();
                    break;
            }
        }

        public new AbstractUIBrowser GetBrowser()
        {
            if (browser != null) return browser;

            if (canQuitApplication)
            {
                browser = new UIBrowser(GetBrowserId(),
                [
                    [playButton],
                    [optionButton],
                    [modButton],
                    [quitButton]
                ], 0, 0);
            }
            else
            {
                browser = new UIBrowser(GetBrowserId(),
                [
                    [playButton],
                    [optionButton],
                    [modButton]
                ], 0, 0);
            }

            return browser;
        }
    }
}
