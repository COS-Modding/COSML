using COSML.Log;
using COSML.Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static COSML.COSML;
using static COSML.Menu.MenuUtils;

namespace COSML.Menu
{
    public class ModsMainMenu : IMainMenu
    {
        public MainMenuPagination pagination;

        private HashSet<MonoBehaviour> modButtons;
        private Dictionary<int, ModInstance> modInstances;
        private Dictionary<IModMenu, ModMenu> modMenus;

        public override void Init()
        {
            try
            {
                modButtons = new HashSet<MonoBehaviour>();
                modInstances = new Dictionary<int, ModInstance>();
                modMenus = new Dictionary<IModMenu, ModMenu>();

                pagination = gameObject.AddComponent<MainMenuPagination>();
                pagination.menu = this;
                pagination.Init();

                AddOptions();
            }
            catch (Exception ex)
            {
                Logging.API.Error($"Error adding mod menu options:\n" + ex);
            }

            Hide();
        }
        private void AddOptions()
        {
            ModInstance[] ModInstancesArr = ModInstances.Where(m => m.Error == null).ToArray();
            if (ModInstancesArr.Length <= 0) return;

            // Add mod buttons
            int modOrder = 1;
            foreach (ModInstance modInst in ModInstancesArr)
            {
                AddModButton(modInst, modOrder);
                AddModMenu(modInst);
                modOrder++;
            }
        }

        private void AddModButton(ModInstance modInst, int modOrder)
        {
            int buttonId = (modOrder - 1) % OPTION_MENU_MAX_PER_PAGE;
            MonoBehaviour modButton;
            string label = $"{modInst.Name} - {modInst.Mod.GetVersionSafe("???")}";
            try
            {
                if (modInst.Mod is IModTogglable && modInst.Mod is not IModMenu)
                {
                    modButton = CreateToggle(new InternalToggleData
                    {
                        parent = transform,
                        menu = this,
                        buttonId = buttonId,
                        position = buttonId,
                        label = label,
                        value = modInst.Enabled,
                    });
                }
                else if (modInst.Mod is IModMenu)
                {
                    modButton = CreateButton(new InternalButtonData
                    {
                        parent = transform,
                        menu = this,
                        buttonId = buttonId,
                        position = buttonId,
                        label = label
                    });
                }
                else
                {
                    modButton = CreateText(new InternalTextData
                    {
                        parent = transform,
                        menu = this,
                        position = buttonId,
                        label = label
                    });
                }

                modButtons.Add(modButton);
                modInstances.Add(buttonId, modInst);

                pagination?.AddElement(modButton.gameObject);
            }
            catch (Exception ex)
            {
                Logging.API.Error($"Error creating menu for {nameof(IModMenu)} {modInst.Name}\n" + ex);
            }
        }

        private void AddModMenu(ModInstance modInst)
        {
            if (modInst.Mod is IModMenu mod)
            {
                List<IOptionData> menuOptions = mod.GetMenu();
                if (modInst.Mod is IModTogglable)
                {
                    menuOptions.Insert(0, new ToggleData(
                        "Enable",
                        modInst.Enabled,
                        (value) =>
                        {
                            if (modInst.Enabled) UnloadMod(modInst);
                            else LoadMod(modInst);
                        }
                    ));
                }
                ModMenu modMenu = CreateMenu<ModMenu>(new InternalMenuData
                {
                    label = modInst.Name,
                    parent = this,
                    options = menuOptions
                });
                modMenus.Add(mod, modMenu);
            }
        }

        public override void Loop()
        {
            if (gameObject.activeSelf)
            {
                if (!backButton.over.enabled)
                {
                    backButton.over.enabled = true;
                    backButton.over.SetTrigger("Show");
                }
                backButton.Loop();
                pagination?.Loop();
                foreach (MonoBehaviour btn in modButtons)
                {
                    btn.GetComponent<MainMenuButton>()?.Loop();
                    btn.GetComponent<MainMenuText>()?.Loop();
                    btn.GetComponent<MainMenuSelector>()?.Loop();
                }
            }
        }

        public override void Show(AbstractMainMenu previousMenu)
        {
            if (!gameObject.activeSelf)
            {
                Patches.MainMenu mainMenu = (Patches.MainMenu)GameController.GetInstance().GetUIController().mainMenu;
                gameObject.SetActive(true);
                backButton.InitRoll();
                backButton.Loop();
                pagination?.InitRoll();
                if (backButton.gameObject.activeSelf && (previousMenu == null || previousMenu.GetType().Equals(typeof(RootMainMenu))))
                {
                    backButton.over.enabled = false;
                }
                foreach (MonoBehaviour btn in modButtons)
                {
                    btn.GetComponent<MainMenuButton>()?.InitRoll();
                    btn.GetComponent<MainMenuText>()?.InitRoll();
                    btn.GetComponent<MainMenuSelector>()?.InitRoll();
                }

                mainMenu.modMenu?.ResetBrowser();
            }
        }

        public override void ForceExit()
        {
            backButton.ForceExit();
            pagination?.ForceExit();
            foreach (MonoBehaviour btn in modButtons)
            {
                btn.GetComponent<MainMenuButton>()?.ForceExit();
                btn.GetComponent<MainMenuText>()?.ForceExit();
                btn.GetComponent<MainMenuSelector>()?.ForceExit();
            }
        }

        public override void OnClic(int buttonId)
        {
            GameController instance = GameController.GetInstance();
            UIController uicontroller = instance.GetUIController();

            if (buttonId < OPTION_MENU_MAX_PER_PAGE)
            {
                ModInstance modInst = modInstances[buttonId];
                if (modInst.Mod is IModMenu)
                {
                    instance.PlayGlobalSound("Play_menu_clic", false);
                    uicontroller.mainMenu.Swap(modMenus[modInst.Mod as IModMenu], true);
                }
                else if (modInst.Mod is IModTogglable)
                {
                    if (modInst.Enabled) UnloadMod(modInst);
                    else LoadMod(modInst);
                }
            }
            else if (buttonId == Constants.BACK_BUTTON_ID)
            {
                GoToPreviousMenu();
            }
        }

        public override bool IsOpen() => gameObject.activeSelf;

        public override AbstractUIBrowser GetBrowser()
        {
            OverableUI[][] overableUI = pagination?.GetOverableUI();

            if (overableUI == null)
            {
                int index = 0;
                overableUI = new OverableUI[modButtons.Count][];
                foreach (MonoBehaviour btn in modButtons)
                {
                    OverableUI btnOver = btn.GetComponent<MainMenuButton>();
                    btnOver ??= btn.GetComponent<MainMenuText>()?.over;
                    btnOver ??= btn.GetComponent<MainMenuSelector>()?.over;

                    overableUI[index] = new OverableUI[1] { btnOver };
                    index++;
                }
            }

            return new UIBrowser(GetBrowserId(), overableUI, 0, 0);
        }

        public override void GoToPreviousMenu()
        {
            GameController instance = GameController.GetInstance();
            UIController uicontroller = instance.GetUIController();
            instance.PlayGlobalSound("Play_menu_return", false);
            if (instance.GetPauseController().InPause())
            {
                uicontroller.mainMenu.Swap(uicontroller.mainMenu.pauseMenu, false);
                return;
            }
            uicontroller.mainMenu.Swap(uicontroller.mainMenu.rootMenu, false);
            backButton.over.SetTrigger("Hide");
        }

        public void ResetBrowser() { }

        public override void Translate(I18nType i18n, I18nPlateformType i18nPlateformType) { }
    }
}