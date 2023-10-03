using MonoMod;
using UnityEngine;
using static COSML.Menu.MenuUtils;

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
            modButton = CreateRootButton(new InternalButtonData
            {
                parent = mainMenuGo.transform,
                menu = this,
                label = "MODS",
                buttonId = 2,
                position = 2
            });

            // Edit quit button
            GameObject quitButtonGo = mainMenuGo.transform.Find($"Bouton Quit").gameObject;
            quitButtonGo.transform.localPosition = GetRootButtonLocalPosition(3);
            MainMenuButton quitMainMenuButton = quitButtonGo.GetComponent<MainMenuButton>();
            quitMainMenuButton.buttonId = 3;
        }

        public extern void orig_Loop();
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

        public extern void orig_Show();
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

        public extern void orig_ForceExit();
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

        public extern void orig_OnClic(int buttonId);
        public new void OnClic(int buttonId)
        {
            GameController instance = (GameController)GameController.GetInstance();
            InputsController inputsController = instance.GetInputsController();
            UIController uIController = (UIController)instance.GetUIController();
            switch (buttonId)
            {
                case 0:
                    instance.PlayGlobalSound("Play_menu_clic", false);
                    uIController.mainMenu.Swap(uIController.mainMenu.selectSaveMenu, true);
                    break;
                case 1:
                    instance.PlayGlobalSound("Play_menu_clic", false);
                    uIController.mainMenu.Swap(uIController.mainMenu.optionMenu, true);
                    break;
                case 2:
                    instance.PlayGlobalSound("Play_menu_clic", false);
                    uIController.mainMenu.Swap(uIController.mainMenu.modMenu, true);
                    break;
                case 3:
                    inputsController.DisplayCursor(false);
                    instance.PlayGlobalSound("Play_menu_clic", false);
                    instance.GetUIController().mainMenu.Disable();
                    Application.Quit();
                    break;
            }
        }

        public extern AbstractUIBrowser orig_GetBrowser();
        public new AbstractUIBrowser GetBrowser()
        {
            if (browser != null) return browser;

            if (canQuitApplication)
            {
                browser = new UIBrowser(GetBrowserId(), new OverableUI[4][]
                {
                    new OverableUI[1] { playButton },
                    new OverableUI[1] { optionButton },
                    new OverableUI[1] { modButton },
                    new OverableUI[1] { quitButton }
                }, 0, 0);
            }
            else
            {
                browser = new UIBrowser(GetBrowserId(), new OverableUI[3][]
                {
                    new OverableUI[1] { playButton },
                    new OverableUI[1] { optionButton },
                    new OverableUI[1] { modButton }
                }, 0, 0);
            }

            return browser;
        }
    }
}
