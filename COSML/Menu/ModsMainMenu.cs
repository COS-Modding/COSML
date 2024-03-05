using COSML.Log;
using COSML.Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static COSML.COSML;
using static COSML.MainMenu.MenuUtils;

namespace COSML.MainMenu
{
    public class ModsMainMenu : IMainMenu
    {
        public MainMenuPagination pagination;

        private List<MonoBehaviour> modButtons;
        private Dictionary<int, ModInstance> modInstances;
        private Dictionary<IModMenu, ModMenu> modMenus;

        public override void Init()
        {
            modButtons = [];
            modInstances = [];
            modMenus = [];

            pagination = gameObject.AddComponent<MainMenuPagination>();
            pagination.menu = this;

            try
            {
                AddMenuOptions();
            }
            catch (Exception ex)
            {
                Logging.API.Error($"Error adding mod menu options:\n" + ex);
            }

            Hide();
        }
        private void AddMenuOptions()
        {
            ModInstance[] ModInstancesArr = [.. ModInstances.Where(m => m.Error == null).OrderBy(m => m.Name)];
            if (ModInstancesArr.Length <= 0) return;

            // Add mod buttons
            int modOrder = 0;
            foreach (ModInstance modInst in ModInstancesArr)
            {
                try
                {
                    AddModMenu(modInst);
                    AddModButton(modInst, modOrder);
                }
                catch (Exception ex)
                {
                    Logging.API.Error($"Error creating menu for {modInst.Name}\n" + ex);
                    AddModButton(modInst, modOrder, true);
                }
                modOrder++;
            }

            pagination.Init(modButtons);
        }

        private MonoBehaviour AddModButton(ModInstance modInst, int modOrder, bool error = false)
        {
            MonoBehaviour modButton;
            string label = $"{modInst.Name} - {modInst.Mod.GetVersionSafe("???")}";

            int buttonId = modOrder % OPTION_MENU_MAX_PER_PAGE;

            if (!error && modInst.Mod is IModTogglable && modInst.Mod is not IModMenu)
            {
                modButton = CreateToggle(new ToggleData
                {
                    parent = transform,
                    menu = this,
                    buttonId = buttonId,
                    label = label,
                    value = modInst.Enabled,
                    on = new I18nKey("cosml.menu.mods.enabled.on"),
                    off = new I18nKey("cosml.menu.mods.enabled.off")
                });
            }
            else if (!error && modInst.Mod is IModMenu)
            {
                modButton = CreateButton(new ButtonData
                {
                    parent = transform,
                    menu = this,
                    buttonId = buttonId,
                    label = label
                });
            }
            else
            {
                modButton = CreateText(new TextData
                {
                    parent = transform,
                    menu = this,
                    label = label
                });

                if (error)
                {
                    UpdateOption(modButton, new I18nKey("cosml.menu.mods.error", label));
                    modButton.transform.Find("Text_Libellé").GetComponent<Text>().color = new Color(1, 1, 1, 0.3f);
                }
            }

            modButtons.Add(modButton);
            modInstances.Add(buttonId, modInst);

            return modButton;
        }

        private void AddModMenu(ModInstance modInst)
        {
            if (modInst.Mod is IModMenu mod)
            {
                IList<MenuOption> options = mod.GetMenu();
                if (modInst.Mod is IModTogglable)
                {
                    options.Insert(0, new MenuToggle(
                        new I18nKey("cosml.menu.mods.enabled"),
                        modInst.Enabled,
                        new I18nKey("cosml.menu.mods.enabled.on"),
                        new I18nKey("cosml.menu.mods.enabled.off"),
                        _ =>
                        {
                            if (modInst.Enabled) UnloadMod(modInst);
                            else LoadMod(modInst);
                        }
                    ));
                }
                ModMenu modMenu = CreateMenu<ModMenu>(new MenuData
                {
                    name = $"Menu_Mods_{modInst.Name.Replace(" ", "_")}",
                    label = modInst.Name,
                    parent = this,
                    options = options
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
                pagination.Loop();
                foreach (MonoBehaviour btn in modButtons)
                {
                    btn.GetComponent<Patches.MainMenuButton>()?.Loop();
                    btn.GetComponent<MainMenuText>()?.Loop();
                    btn.GetComponent<MainMenuSelector>()?.Loop();
                }
            }
        }

        public override void Show(AbstractMainMenu previousMenu)
        {
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
                backButton.InitRoll();
                backButton.Loop();
                pagination.InitRoll();
                if (backButton.gameObject.activeSelf && (previousMenu == null || previousMenu.GetType().Equals(typeof(RootMainMenu))))
                {
                    backButton.over.enabled = false;
                }
                foreach (MonoBehaviour btn in modButtons)
                {
                    btn.GetComponent<Patches.MainMenuButton>()?.InitRoll();
                    btn.GetComponent<MainMenuText>()?.InitRoll();
                    btn.GetComponent<MainMenuSelector>()?.InitRoll();
                }
                ResetBrowser();
                Refresh();
            }
        }

        public override void ForceExit()
        {
            backButton.ForceExit();
            pagination.ForceExit();
            foreach (MonoBehaviour btn in modButtons)
            {
                btn.GetComponent<Patches.MainMenuButton>()?.ForceExit();
                btn.GetComponent<MainMenuText>()?.ForceExit();
                btn.GetComponent<MainMenuSelector>()?.ForceExit();
            }
        }

        public override void OnClic(int buttonId)
        {
            if (buttonId >= 0)
            {
                GameController instance = GameController.GetInstance();
                UIController uiController = instance.GetUIController();
                ModInstance modInst = modInstances[buttonId];
                if (modInst.Mod is IModMenu)
                {
                    instance.PlayGlobalSound("Play_menu_clic", false);
                    ModMenu menu = modMenus[modInst.Mod as IModMenu];
                    uiController.mainMenu.Swap(menu, true);
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
            OverableUI[][] overableUIs = pagination.GetOverableUI();
            browser = new Patches.UIBrowser(GetBrowserId(), overableUIs, GetBrowserIndex(overableUIs.Length), 0);
            return browser;
        }

        public override void GoToPreviousMenu()
        {
            GameController instance = GameController.GetInstance();
            UIController uiController = instance.GetUIController();
            instance.PlayGlobalSound("Play_menu_return", false);
            if (instance.GetPauseController().InPause())
            {
                uiController.mainMenu.Swap(uiController.mainMenu.pauseMenu, false);
                return;
            }
            uiController.mainMenu.Swap(uiController.mainMenu.rootMenu, false);
            backButton.over.SetTrigger("Hide");
        }

        public void ResetBrowser() { }

        public void SetIndex(int i, int j)
        {
            browser?.ResetPosition(i, j);
        }

        public override void Translate(I18nType i18n, I18nPlateformType i18nPlateformType) { }
    }
}