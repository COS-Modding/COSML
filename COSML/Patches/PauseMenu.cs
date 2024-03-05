using MonoMod;
using UnityEngine;
using static COSML.MainMenu.MenuUtils;

namespace COSML.Patches
{
    [MonoModPatch("global::PauseMenu")]
    public class PauseMenu : global::PauseMenu
    {
        public MainMenuButton modButton;

        [MonoModIgnore]
        private AbstractUIBrowser browser;

        public void EditMenu()
        {
            // Add mods button
            GameObject pauseMenuGo = GameObject.Find($"{Constants.MAIN_MENU_PATH}/Menu_Pause");
            modButton = CreateButton(new ButtonData
            {
                parent = pauseMenuGo.transform,
                menu = this,
                label = new I18nKey("cosml.menu.mods"),
                buttonId = 1
            });
            modButton.transform.localPosition = GetOptionButtonLocalPosition(1);

            // Edit help button
            GameObject helpButtonGo = pauseMenuGo.transform.Find($"MenuBarre_Aide").gameObject;
            helpButtonGo.GetComponent<MainMenuButton>().buttonId = 2;
            helpButtonGo.transform.localPosition = GetOptionButtonLocalPosition(2);
            // Edit quit button
            GameObject quitButtonGo = pauseMenuGo.transform.Find($"MenuBarre_Quitter").gameObject;
            quitButtonGo.transform.localPosition = GetOptionButtonLocalPosition(3);
            quitButtonGo.GetComponent<MainMenuButton>().buttonId = 3;
        }

        public new void Loop()
        {
            if (gameObject.activeSelf)
            {
                if (!backButton.over.enabled)
                {
                    backButton.over.enabled = true;
                    backButton.over.SetTrigger("Show");
                }

                optionButton.Loop();
                modButton?.Loop();
                aideButton.Loop();
                quitterButton.Loop();
                backButton.Loop();
            }
        }

        public new void Show(AbstractMainMenu previousMenu)
        {
            if (!gameObject.activeSelf)
            {
                MainMenu mainMenu = (MainMenu)GameController.GetInstance().GetUIController().mainMenu;
                gameObject.SetActive(value: true);
                optionButton.InitRoll();
                modButton?.InitRoll();
                aideButton.InitRoll();
                quitterButton.InitRoll();
                backButton.InitRoll();
                backButton.Loop();
                if (backButton.gameObject.activeSelf && (previousMenu == null || previousMenu.GetType().Equals(typeof(RootMainMenu))))
                {
                    backButton.over.enabled = false;
                }

                mainMenu.optionMenu.ResetBrowser();
                mainMenu.modMenu.ResetBrowser();
                mainMenu.helpMenu.ResetBrowser();
                mainMenu.confirmQuitMenu.ResetBrowser();
            }
        }

        public new void ForceExit()
        {
            optionButton.ForceExit();
            modButton?.ForceExit();
            aideButton.ForceExit();
            quitterButton.ForceExit();
            backButton.ForceExit();
        }

        public new void OnClic(int buttonId)
        {
            GameController instance = (GameController)GameController.GetInstance();
            Patches.MainMenu mainMenu = (Patches.MainMenu)instance.GetUIController().mainMenu;
            switch (buttonId)
            {
                case 0:
                    instance.PlayGlobalSound("Play_menu_clic", false);
                    mainMenu.Swap(mainMenu.optionMenu, true);
                    break;
                case 1:
                    instance.PlayGlobalSound("Play_menu_clic", false);
                    mainMenu.Swap(mainMenu.modMenu, true);
                    break;
                case 2:
                    instance.PlayGlobalSound("Play_menu_clic", false);
                    mainMenu.Swap(mainMenu.helpMenu, true);
                    break;
                case 3:
                    instance.PlayGlobalSound("Play_menu_clic", false);
                    mainMenu.Swap(mainMenu.confirmQuitMenu, true);
                    break;
                case 4:
                    GoToPreviousMenu();
                    break;
            }
        }

        public new AbstractUIBrowser GetBrowser()
        {
            if (browser != null) return browser;

            browser = new UIBrowser(GetBrowserId(),
                [
                    [optionButton],
                    [modButton],
                    [aideButton],
                    [quitterButton]
                ], 0, 0);

            return browser;
        }
    }
}
