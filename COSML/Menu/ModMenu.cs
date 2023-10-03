using COSML.Log;
using System;
using System.Collections.Generic;
using UnityEngine;
using static COSML.Menu.MenuUtils;

namespace COSML.Menu
{
    public class ModMenu : IMainMenu
    {
        private AbstractUIBrowser browser;

        public AbstractMainMenu parentMenu;
        public Action onBack;

        private Dictionary<int, MonoBehaviour> options;
        private Dictionary<int, IOptionData> optionsData;
        private Dictionary<int, ModMenu> optionsMenu;

        public override void Init() { }

        public void Init(List<IOptionData> menuOptions)
        {
            try
            {
                AddModOptions(menuOptions);
            }
            catch (Exception ex)
            {
                Logging.API.Error($"Error adding menu options for {name}:\n" + ex);
            }
        }

        private void AddModOptions(List<IOptionData> menuOptions)
        {
            options = new Dictionary<int, MonoBehaviour>();
            optionsData = new Dictionary<int, IOptionData>();
            optionsMenu = new Dictionary<int, ModMenu>();

            int modIndex = 0;
            foreach (IOptionData modOption in menuOptions)
            {
                MonoBehaviour option;
                if (modOption is ButtonData buttonData)
                {
                    bool hasSubMenu = buttonData.menu.HasValue;
                    if (hasSubMenu)
                    {
                        MenuData menuData = buttonData.menu.Value;
                        ModMenu modMenu = CreateMenu<ModMenu>(new InternalMenuData()
                        {
                            label = menuData.label,
                            parent = this,
                            options = menuData.options,
                            onBack = menuData.onBack
                        });
                        optionsMenu[modIndex] = modMenu;
                    }
                    option = CreateButton(new InternalButtonData
                    {
                        parent = transform,
                        menu = this,
                        buttonId = modIndex,
                        position = modIndex,
                        label = buttonData.label,
                        arrow = hasSubMenu
                    });
                }
                else if (modOption is SelectData selectData)
                {
                    option = CreateSelect(new InternalSelectData
                    {
                        parent = transform,
                        menu = this,
                        buttonId = modIndex,
                        position = modIndex,
                        label = selectData.label,
                        values = selectData.values,
                        value = selectData.value
                    });
                }
                else if (modOption is ToggleData toggleData)
                {
                    option = CreateToggle(new InternalToggleData
                    {
                        parent = transform,
                        menu = this,
                        buttonId = modIndex,
                        position = modIndex,
                        label = toggleData.label,
                        on = toggleData.on,
                        off = toggleData.off,
                        value = toggleData.value
                    });
                }
                else if (modOption is SliderData sliderData)
                {
                    option = CreateSlider(new InternalSliderData
                    {
                        parent = transform,
                        menu = this,
                        buttonId = modIndex,
                        position = modIndex,
                        label = sliderData.label,
                        steps = sliderData.steps,
                        value = sliderData.value
                    });
                }
                else
                {
                    continue;
                }

                options.Add(modIndex, option);
                optionsData.Add(modIndex, modOption);
                modIndex++;
            }
        }

        public override void Loop()
        {
            if (gameObject.activeSelf)
            {
                backButton.Loop();
                foreach (MonoBehaviour option in options.Values)
                {
                    option.GetComponent<MainMenuButton>()?.Loop();
                    option.GetComponent<MainMenuSelector>()?.Loop();
                    option.GetComponent<MainMenuSlider>()?.Loop();
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
                foreach (MonoBehaviour option in options.Values)
                {
                    option.GetComponent<MainMenuButton>()?.InitRoll();
                    option.GetComponent<MainMenuSelector>()?.InitRoll();
                    option.GetComponent<MainMenuSlider>()?.InitRoll();
                }
            }
        }

        public override void ForceExit()
        {
            backButton.ForceExit();
            foreach (MonoBehaviour option in options.Values)
            {
                option.GetComponent<MainMenuButton>()?.ForceExit();
                option.GetComponent<MainMenuSelector>()?.ForceExit();
                option.GetComponent<MainMenuSlider>()?.ForceExit();
            }
        }

        public override void OnClic(int buttonId)
        {
            if (buttonId < 0) return;

            if (buttonId < OPTION_MENU_MAX_PER_PAGE)
            {
                GameController instance = GameController.GetInstance();
                UIController uicontroller = instance.GetUIController();
                MonoBehaviour modOption = options[buttonId];

                if (modOption is MainMenuButton)
                {
                    ButtonData buttonData = (ButtonData)optionsData[buttonId];
                    instance.PlayGlobalSound("Play_menu_clic", false);
                    if (buttonData.menu.HasValue)
                    {
                        uicontroller.mainMenu.Swap(optionsMenu[buttonId], true);
                    }
                    else
                    {
                        buttonData.onClick();
                    }
                }
                else if (modOption is MainMenuSelector modSelect)
                {
                    if (optionsData[buttonId] is SelectData)
                    {
                        SelectData selectData = (SelectData)optionsData[buttonId];
                        selectData.onChange(modSelect.GetCurrentValue());
                    }
                    else if (optionsData[buttonId] is ToggleData)
                    {
                        ToggleData toggleData = (ToggleData)optionsData[buttonId];
                        toggleData.onChange(modSelect.GetCurrentValue() == 1);
                    }
                }
                else if (modOption is Patches.MainMenuSlider modSlider)
                {
                    if (modSlider.HasUpdated())
                    {
                        SliderData sliderData = (SliderData)optionsData[buttonId];
                        sliderData.onChange(modSlider.GetCurrentValue());
                    }
                }
            }
            //else if (buttonId == OPTION_MENU_MAX_PER_PAGE && pageSelector != null)
            //{
            //    ChangePage(pageSelector.GetCurrentValue() + 1);
            //}
            else if (buttonId == OPTION_MENU_MAX_PER_PAGE + 1)
            {
                GoToPreviousMenu();
            }
        }

        public override bool IsOpen() => gameObject.activeSelf;

        public override AbstractUIBrowser GetBrowser()
        {
            if (browser != null) return browser;

            //int buttonsCount = pages[currentPageNumber].transform.childCount;
            int optionIndex = 0;
            //bool hasPagination = totalPagesCount > 1 && pageSelector != null;

            //OverableUI[][] overableUIs = new OverableUI[buttonsCount + (hasPagination ? 1 : 0)][];
            OverableUI[][] overableUIs = new OverableUI[options.Count][];
            foreach (MonoBehaviour option in options.Values)
            {
                OverableUI optionOver = option.GetComponent<MainMenuButton>();
                optionOver ??= option.GetComponent<MainMenuSelector>()?.over;
                optionOver ??= option.GetComponent<MainMenuSlider>()?.over;
                overableUIs[optionIndex] = new OverableUI[1] { optionOver };
                optionIndex++;
            }

            //if (hasPagination)
            //{
            //    overableUIs[buttonsCount] = new OverableUI[1] { pageSelector.over };
            //}

            browser = new UIBrowser(GetBrowserId(), overableUIs, 0, 0);

            return browser;
        }

        public override void GoToPreviousMenu()
        {
            GameController instance = GameController.GetInstance();
            UIController uIController = instance.GetUIController();
            instance.PlayGlobalSound("Play_menu_return", false);
            uIController.mainMenu.Swap(parentMenu, false);
            onBack?.Invoke();
        }

        public void ResetBrowser() => browser = null;

        public override void Translate(I18nType i18n, I18nPlateformType i18nPlateformType) { }
    }
}
