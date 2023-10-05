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
        private AbstractUIBrowser browser;

        public MainMenuSelector pageSelector;
        private Dictionary<int, GameObject> pages;
        private int currentPageNumber = 1;
        private int totalPagesCount;

        private Dictionary<int, ModInstance> modButtons;
        private Dictionary<IModMenu, ModMenu> modMenus;

        public override void Init()
        {
            try
            {
                AddOptions();
            }
            catch (Exception ex)
            {
                Logging.API.Error($"Error adding mod menu options:\n" + ex);
            }

            Hide();
            browser = null;
        }
        private void AddOptions()
        {
            ModInstance[] ModInstancesArr = ModInstances.Where(m => m.Error == null).ToArray();
            if (ModInstancesArr.Length <= 0) return;

            pages = new Dictionary<int, GameObject>();
            modButtons = new Dictionary<int, ModInstance>();
            modMenus = new Dictionary<IModMenu, ModMenu>();
            float optionYDelta = 0;
            int modOrder = 1;
            int modPage;

            // Add mod buttons
            foreach (ModInstance modInst in ModInstancesArr)
            {
                // First mod per page
                modPage = (int)Math.Ceiling((double)modOrder / OPTION_MENU_MAX_PER_PAGE);
                if ((modOrder - 1) % OPTION_MENU_MAX_PER_PAGE == 0)
                {
                    GameObject currentPageGo = new($"Mods_Page_{modPage}");
                    currentPageGo.transform.SetParent(transform, false);
                    currentPageGo.SetActive(modPage == 1);
                    pages.Add(modPage, currentPageGo);
                }

                float yPos = OPTION_MENU_MIN_Y - (optionYDelta % (OPTION_MENU_HEIGHT * OPTION_MENU_MAX_PER_PAGE));
                AddModButton(modInst, pages[modPage].transform, modOrder);
                AddModMenu(modInst);

                optionYDelta += OPTION_MENU_HEIGHT;
                totalPagesCount = modPage;
                modOrder++;
            }

            // Add pagination
            if (totalPagesCount > 1)
            {
                pageSelector = CreateSelect(new InternalSelectData
                {
                    parent = transform,
                    menu = this,
                    label = "PAGE",
                    values = pages.Keys.Select(k => k.ToString()).ToArray(),
                    value = 0,
                    buttonId = OPTION_MENU_MAX_PER_PAGE,
                    position = OPTION_MENU_MAX_PER_PAGE + 1
                });
                pageSelector.gameObject.SetActive(totalPagesCount > 1);
            }
        }

        private void AddModButton(ModInstance modInst, Transform parent, int modOrder)
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
                        parent = parent,
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
                        parent = parent,
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
                        parent = parent,
                        menu = this,
                        position = buttonId,
                        label = label
                    });
                }

                modButtons.Add(buttonId, modInst);
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
                pageSelector?.Loop();
                if (pages != null)
                {
                    foreach (Transform mod in pages[currentPageNumber].transform)
                    {
                        mod.GetComponent<MainMenuButton>()?.Loop();
                        mod.GetComponent<MainMenuText>()?.Loop();
                        mod.GetComponent<MainMenuSelector>()?.Loop();
                    }
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
                if (backButton.gameObject.activeSelf && (previousMenu == null || previousMenu.GetType().Equals(typeof(RootMainMenu))))
                {
                    backButton.over.enabled = false;
                }
                pageSelector?.InitRoll();
                if (pages != null)
                {
                    foreach (Transform mod in pages[currentPageNumber].transform)
                    {
                        mod.GetComponent<MainMenuButton>()?.InitRoll();
                        mod.GetComponent<MainMenuText>()?.InitRoll();
                        mod.GetComponent<MainMenuSelector>()?.InitRoll();
                    }
                }

                mainMenu.modMenu?.ResetBrowser();
            }
        }

        public override void ForceExit()
        {
            backButton.ForceExit();
            pageSelector?.ForceExit();
            if (pages != null)
            {
                foreach (Transform mod in pages[currentPageNumber].transform)
                {
                    mod.GetComponent<MainMenuButton>()?.ForceExit();
                    mod.GetComponent<MainMenuText>()?.ForceExit();
                    mod.GetComponent<MainMenuSelector>()?.ForceExit();
                }
            }
        }

        public override void OnClic(int buttonId)
        {
            GameController instance = GameController.GetInstance();
            UIController uicontroller = instance.GetUIController();

            if (buttonId < OPTION_MENU_MAX_PER_PAGE)
            {
                ModInstance modInst = modButtons[buttonId];
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
            else if (buttonId == Constants.PAGINATION_BUTTON_ID && pageSelector != null)
            {
                ChangePage(pageSelector.GetCurrentValue() + 1);
            }
            else if (buttonId == Constants.BACK_BUTTON_ID)
            {
                GoToPreviousMenu();
            }
        }

        public void ChangePage(int page)
        {
            if (totalPagesCount <= 1) return;

            pages[currentPageNumber].SetActive(false);
            currentPageNumber = Mathf.Clamp(page, 1, totalPagesCount);
            pages[currentPageNumber].SetActive(true);

            // Force update browser
            GameController instance = GameController.GetInstance();
            InputsController inputsController = instance.GetInputsController();
            if (inputsController is not PadController) return;
            Patches.PadController padController = (Patches.PadController)inputsController;
            padController.ForceUpdateUIBrowser(this);
        }

        public override bool IsOpen() => gameObject.activeSelf;

        public override AbstractUIBrowser GetBrowser()
        {
            if (browser != null) return browser;

            int buttonsCount = pages[currentPageNumber].transform.childCount;
            int btnIndex = 0;
            bool hasPagination = totalPagesCount > 1 && pageSelector != null;

            OverableUI[][] overableUIs = new OverableUI[buttonsCount + (hasPagination ? 1 : 0)][];
            foreach (Transform btn in pages[currentPageNumber].transform)
            {
                OverableUI btnOver = btn.GetComponent<MainMenuButton>();
                btnOver ??= btn.GetComponent<MainMenuText>()?.over;
                btnOver ??= btn.GetComponent<MainMenuSelector>()?.over;
                overableUIs[btnIndex] = new OverableUI[1] { btnOver };
                btnIndex++;
            }

            if (hasPagination)
            {
                overableUIs[buttonsCount] = new OverableUI[1] { pageSelector.over };
            }

            browser = new UIBrowser(GetBrowserId(), overableUIs, 0, 0);

            return browser;
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

        public void ResetBrowser() => browser = null;

        public override void Translate(I18nType i18n, I18nPlateformType i18nPlateformType) { }
    }
}