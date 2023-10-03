using MonoMod;
using UnityEngine;
using static COSML.Menu.MenuUtils;

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
            modButton = CreateButton(new InternalButtonData
            {
                parent = pauseMenuGo.transform,
                menu = this,
                label = "MODS",
                buttonId = 1,
                position = 1
            });

            // Edit help button
            GameObject helpButtonGo = pauseMenuGo.transform.Find($"MenuBarre_Aide").gameObject;
            helpButtonGo.GetComponent<MainMenuButton>().buttonId = 2;
            helpButtonGo.transform.localPosition = GetOptionButtonLocalPosition(2);
            // Edit quit button
            GameObject quitButtonGo = pauseMenuGo.transform.Find($"MenuBarre_Quitter").gameObject;
            quitButtonGo.transform.localPosition = GetOptionButtonLocalPosition(3);
            quitButtonGo.GetComponent<MainMenuButton>().buttonId = 3;
        }

        public extern void orig_Loop();
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

        public extern void orig_Show(AbstractMainMenu previousMenu);
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

        public extern void orig_ForceExit();
        public new void ForceExit()
        {
            optionButton.ForceExit();
            modButton?.ForceExit();
            aideButton.ForceExit();
            quitterButton.ForceExit();
            backButton.ForceExit();
        }

        public extern void orig_OnClic(int buttonId);
        public new void OnClic(int buttonId)
        {
            GameController instance = (GameController)GameController.GetInstance();
            UIController uIController = (UIController)instance.GetUIController();
            switch (buttonId)
            {
                case 0:
                    instance.PlayGlobalSound("Play_menu_clic", false);
                    uIController.mainMenu.Swap(uIController.mainMenu.optionMenu, true);
                    break;
                case 1:
                    instance.PlayGlobalSound("Play_menu_clic", false);
                    uIController.mainMenu.Swap(uIController.mainMenu.modMenu, true);
                    break;
                case 2:
                    instance.PlayGlobalSound("Play_menu_clic", false);
                    uIController.mainMenu.Swap(uIController.mainMenu.helpMenu, true);
                    break;
                case 3:
                    instance.PlayGlobalSound("Play_menu_clic", false);
                    uIController.mainMenu.Swap(uIController.mainMenu.confirmQuitMenu, true);
                    break;
                case 4:
                    GoToPreviousMenu();
                    break;
            }
        }

        public extern AbstractUIBrowser orig_GetBrowser();
        public new AbstractUIBrowser GetBrowser()
        {
            if (browser != null) return browser;

            browser = new UIBrowser(GetBrowserId(), new OverableUI[4][]
                {
                new OverableUI[1] { optionButton },
                new OverableUI[1] { modButton },
                new OverableUI[1] { aideButton },
                new OverableUI[1] { quitterButton }
                }, 0, 0);

            return browser;
        }
    }
}
