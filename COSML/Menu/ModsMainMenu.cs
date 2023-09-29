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

        private Dictionary<int, ModInstance> buttonsMod;

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
            buttonsMod = new Dictionary<int, ModInstance>();
            float optionYDelta = 0;
            int modOrder = 1;
            int modPage;

            // Add mod buttons
            foreach (ModInstance modInst in ModInstancesArr)
            {
                // First mod per page
                modPage = (int)Math.Ceiling((double)modOrder / MENU_OPTION_MAX_PER_PAGE);
                if ((modOrder - 1) % MENU_OPTION_MAX_PER_PAGE == 0)
                {
                    GameObject currentPageGo = new($"Mods_Page_{modPage}");
                    currentPageGo.transform.SetParent(transform, false);
                    currentPageGo.SetActive(modPage == 1);
                    pages.Add(modPage, currentPageGo);
                }

                // Menu
                int buttonId = (modOrder - 1) % MENU_OPTION_MAX_PER_PAGE;
                GameObject modButtonGo;
                try
                {
                    if (modInst.Mod is IModTogglable)
                    {
                        modButtonGo = CreateToggle(new ToggleOptions
                        {
                            name = $"Mods_Button_{modOrder}",
                            parent = pages[modPage].transform,
                            menu = this,
                            buttonId = buttonId,
                            label = $"{modInst.Name} - {modInst.Mod.GetVersionSafe("???")}",
                            value = modInst.Enabled
                        });
                    }
                    else
                    {
                        modButtonGo = CreateButton(new ButtonOptions
                        {
                            name = $"Mods_Button_{modOrder}",
                            parent = pages[modPage].transform,
                            menu = this,
                            buttonId = buttonId,
                            label = $"{modInst.Name} - {modInst.Mod.GetVersionSafe("???")}",
                            arrow = modInst.Mod is IModMenu
                        });
                    }

                    modButtonGo.transform.localPosition = new Vector3(0, MENU_OPTION_MIN_Y - (optionYDelta % (MENU_OPTION_HEIGHT * MENU_OPTION_MAX_PER_PAGE)), 0);
                    buttonsMod.Add(buttonId, modInst);
                }
                catch (Exception ex)
                {
                    Logging.API.Error($"Error creating menu for {nameof(IModMenu)} {modInst.Name}\n" + ex);
                }

                optionYDelta += MENU_OPTION_HEIGHT;
                totalPagesCount = modPage;
                modOrder++;
            }

            // Add pagination
            if (totalPagesCount > 1)
            {
                GameObject paginationGo = CreateSelect(new SelectOptions
                {
                    name = "Pagination_Button",
                    parent = transform,
                    menu = this,
                    label = "PAGE",
                    values = pages.Keys.Select(k => k.ToString()).ToArray(),
                    currentValue = 0
                });
                paginationGo.transform.localPosition = new Vector3(0, -916, 0);
                pageSelector = paginationGo.GetComponent<MainMenuSelector>();
                pageSelector.buttonId = MENU_OPTION_MAX_PER_PAGE;
                paginationGo.SetActive(totalPagesCount > 1);
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
                }
            }
        }

        public override void OnClic(int buttonId)
        {
            Logging.API.Debug($"buttonId: {buttonId}");
            if (buttonId < 0) return;

            GameController instance = GameController.GetInstance();
            //UIController uicontroller = instance.GetUIController();

            if (buttonId < MENU_OPTION_MAX_PER_PAGE)
            {
                ModInstance modInst = buttonsMod[buttonId];
                if (modInst.Mod is IModMenu)
                {
                    Logging.API.Debug("button is IModMenu");
                    instance.PlayGlobalSound("Play_menu_clic", false);
                    //uicontroller.mainMenu.Swap(uicontroller.mainMenu.optionControlsMenu, true);
                }
                else if (modInst.Mod is IModTogglable)
                {
                    if (modInst.Enabled) UnloadMod(modInst);
                    else LoadMod(modInst);
                }
            }
            else if (buttonId == MENU_OPTION_MAX_PER_PAGE && pageSelector != null)
            {
                ChangePage(pageSelector.GetCurrentValue() + 1);
            }
            else if (buttonId == MENU_OPTION_MAX_PER_PAGE + 1)
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

        public override bool IsOpen()
        {
            return gameObject.activeSelf;
        }

        public override AbstractUIBrowser GetBrowser()
        {
            int buttonsCount = pages[currentPageNumber].transform.childCount;
            int btnIndex = 0;

            OverableUI[][] overableUIs = new OverableUI[buttonsCount + 1][];
            foreach (Transform btn in pages[currentPageNumber].transform)
            {
                overableUIs[btnIndex] = new OverableUI[1] { btn.GetComponent<MainMenuButton>() };
                btnIndex++;
            }

            if (totalPagesCount > 1 && pageSelector != null)
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

        public void ResetBrowser()
        {
            browser = null;
        }

        public override void Translate(I18nType i18n, I18nPlateformType i18nPlateformType) { }
    }
}