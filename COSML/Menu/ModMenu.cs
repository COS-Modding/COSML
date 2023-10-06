using COSML.Log;
using System;
using System.Collections.Generic;
using UnityEngine;
using static COSML.Menu.MenuUtils;

namespace COSML.Menu
{
    public class ModMenu : IMainMenu
    {
        public MainMenuPagination pagination;
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
                pagination = gameObject.AddComponent<MainMenuPagination>();
                pagination.menu = this;
                pagination.Init();

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
                else if (modOption is TextData textData)
                {
                    option = CreateText(new InternalTextData
                    {
                        parent = transform,
                        menu = this,
                        position = modIndex,
                        label = textData.label
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
                else if (modOption is InputTextData inputData)
                {
                    MainMenuInputButton inputText = CreateInputText(new InternalInputTextData
                    {
                        parent = transform,
                        menu = this,
                        buttonId = modIndex,
                        position = modIndex,
                        label = inputData.label,
                        text = inputData.text
                    });
                    option = inputText;
                    MainMenuInputButton.ValueChangedHook += (string value) =>
                    {
                        inputData.onInput?.Invoke(value);
                    };
                }
                else
                {
                    continue;
                }

                options.Add(modIndex, option);
                optionsData.Add(modIndex, modOption);
                modIndex++;

                pagination?.AddElement(option.gameObject);
            }
        }

        public override void Loop()
        {
            if (gameObject.activeSelf)
            {
                backButton.Loop();
                pagination?.Loop();
                foreach (MonoBehaviour option in options.Values)
                {
                    option.GetComponent<MainMenuButton>()?.Loop();
                    option.GetComponent<MainMenuText>()?.Loop();
                    option.GetComponent<MainMenuSelector>()?.Loop();
                    option.GetComponent<MainMenuSlider>()?.Loop();
                    option.GetComponent<MainMenuInputButton>()?.Loop();
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
                pagination?.InitRoll();
                foreach (MonoBehaviour option in options.Values)
                {
                    option.GetComponent<MainMenuButton>()?.InitRoll();
                    option.GetComponent<MainMenuText>()?.InitRoll();
                    option.GetComponent<MainMenuSelector>()?.InitRoll();
                    option.GetComponent<MainMenuSlider>()?.InitRoll();
                    option.GetComponent<MainMenuInputButton>()?.InitRoll();
                }
            }
        }

        public override void ForceExit()
        {
            backButton.ForceExit();
            pagination?.ForceExit();
            foreach (MonoBehaviour option in options.Values)
            {
                option.GetComponent<MainMenuButton>()?.ForceExit();
                option.GetComponent<MainMenuText>()?.ForceExit();
                option.GetComponent<MainMenuSelector>()?.ForceExit();
                option.GetComponent<MainMenuSlider>()?.ForceExit();
                option.GetComponent<MainMenuInputButton>()?.ForceExit();
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
                IOptionData optionData = optionsData[buttonId];

                if (modOption is MainMenuButton)
                {
                    ButtonData buttonData = (ButtonData)optionData;
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
                    if (optionData is SelectData selectData)
                    {
                        selectData.onChange(modSelect.GetCurrentValue());
                    }
                    else if (optionData is ToggleData toggleData)
                    {
                        toggleData.onChange(modSelect.GetCurrentValue() == 1);
                    }
                }
                else if (modOption is Patches.MainMenuSlider modSlider)
                {
                    if (modSlider.HasUpdated())
                    {
                        SliderData sliderData = (SliderData)optionData;
                        sliderData.onChange(modSlider.GetCurrentValue());
                    }
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
                overableUI = new OverableUI[options.Count][];
                foreach (MonoBehaviour option in options.Values)
                {
                    OverableUI optionOver = option.GetComponent<MainMenuButton>();
                    optionOver ??= option.GetComponent<MainMenuText>()?.over;
                    optionOver ??= option.GetComponent<MainMenuSelector>()?.over;
                    optionOver ??= option.GetComponent<MainMenuSlider>()?.over;
                    optionOver ??= option.GetComponent<MainMenuInputButton>()?.over;

                    overableUI[index] = new OverableUI[1] { optionOver };
                    index++;
                }
            }

            return new UIBrowser(GetBrowserId(), overableUI, 0, 0);
        }

        public override void GoToPreviousMenu()
        {
            GameController instance = GameController.GetInstance();
            UIController uIController = instance.GetUIController();
            instance.PlayGlobalSound("Play_menu_return", false);
            uIController.mainMenu.Swap(parentMenu, false);
            onBack?.Invoke();
        }

        public void ResetBrowser() { }

        public override void Translate(I18nType i18n, I18nPlateformType i18nPlateformType) { }
    }
}
