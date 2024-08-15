using COSML.Components.Keyboard;
using COSML.Log;
using System;
using System.Collections.Generic;
using UnityEngine;
using static COSML.MainMenu.MenuUtils;

namespace COSML.MainMenu
{
    public class ModMenu : IMainMenu
    {
        public MainMenuPagination pagination;
        public AbstractMainMenu parentMenu;
        public Action onBack;

        private Dictionary<int, MonoBehaviour> optionsMono;
        private Dictionary<int, MenuOption> menuOptions;
        private Dictionary<int, ModMenu> menus;
        private List<I18nText> i18nTexts;

        public override void Init() { }

        public void Init(IList<MenuOption> options)
        {
            optionsMono = [];
            menuOptions = [];
            menus = [];
            i18nTexts = [];

            pagination = gameObject.AddComponent<MainMenuPagination>();
            pagination.menu = this;

            try
            {
                AddModOptions(options);
            }
            catch (Exception ex)
            {
                Logging.API.Error($"Error adding menu options for {name}:\n" + ex);
            }
        }

        private void AddModOptions(IList<MenuOption> options)
        {
            optionsMono.Clear();
            menuOptions.Clear();
            menus.Clear();
            i18nTexts.Clear();

            int optionIndex = 0;
            foreach (MenuOption modOption in options)
            {
                modOption.ResetEvents();
                MonoBehaviour option;
                if (modOption is MenuButton buttonData)
                {
                    option = CreateButton(new ButtonData
                    {
                        parent = transform,
                        menu = this,
                        buttonId = optionIndex,
                        label = buttonData.Label,
                        arrow = buttonData.Menu != null,
                        visible = buttonData.Visible
                    });
                }
                else if (modOption is MenuText textData)
                {
                    option = CreateText(new TextData
                    {
                        parent = transform,
                        menu = this,
                        label = textData.Label,
                        visible = textData.Visible
                    });
                }
                else if (modOption is MenuSelect selectData)
                {
                    option = CreateSelect(new SelectData
                    {
                        parent = transform,
                        menu = this,
                        buttonId = optionIndex,
                        label = selectData.Label,
                        values = selectData.Values,
                        value = selectData.Value,
                        visible = selectData.Visible
                    });
                }
                else if (modOption is MenuToggle toggleData)
                {
                    option = CreateToggle(new ToggleData
                    {
                        parent = transform,
                        menu = this,
                        buttonId = optionIndex,
                        label = toggleData.Label,
                        on = toggleData.On,
                        off = toggleData.Off,
                        value = toggleData.Value,
                        visible = toggleData.Visible
                    });
                }
                else if (modOption is MenuSlider sliderData)
                {
                    option = CreateSlider(new SliderData
                    {
                        parent = transform,
                        menu = this,
                        buttonId = optionIndex,
                        label = sliderData.Label,
                        steps = sliderData.Steps,
                        value = sliderData.Value,
                        visible = sliderData.Visible
                    });
                }
                else if (modOption is MenuTextInput inputData)
                {
                    MainMenuInputText inputText = CreateInputText(new InputTextData
                    {
                        parent = transform,
                        menu = this,
                        buttonId = optionIndex,
                        label = inputData.Label,
                        text = inputData.Text,
                        max = inputData.Max,
                        visible = inputData.Visible
                    });
                    option = inputText;
                    inputText.ValueChangedHook += v => inputData.OnInput?.Invoke(v);
                }
                else
                {
                    continue;
                }

                optionsMono.Add(optionIndex, option);
                menuOptions.Add(optionIndex, modOption);
                optionIndex++;

                modOption.UpdateHook += (data) =>
                {
                    if (data is MenuSelect selectData)
                    {
                        UpdateSelect((Patches.MainMenuSelector)option, new SelectData
                        {
                            label = data.Label,
                            values = selectData.Values,
                            value = selectData.Value
                        });
                    }
                    else if (data is MenuToggle toggleData)
                    {
                        UpdateToggle((Patches.MainMenuSelector)option, new ToggleData
                        {
                            label = data.Label,
                            on = toggleData.On,
                            off = toggleData.Off,
                            value = toggleData.Value
                        });
                    }
                    else if (data is MenuSlider sliderData)
                    {
                        UpdateSlider((Patches.MainMenuSlider)option, new SliderData
                        {
                            label = data.Label,
                            steps = sliderData.Steps,
                            value = sliderData.Value
                        });
                    }
                    else if (data is MenuTextInput inputTextData)
                    {
                        UpdateInputText((MainMenuInputText)option, new InputTextData
                        {
                            label = data.Label,
                            text = inputTextData.Text,
                            max = inputTextData.Max,
                        });
                    }
                };

                option.gameObject.SetActive(modOption.Visible);
                modOption.VisibleHook += visible =>
                {
                    option.gameObject.SetActive(visible);
                    Refresh();
                };
            }

            pagination.Init([.. optionsMono.Values]);
        }

        public override void Loop()
        {
            if (gameObject.activeSelf)
            {
                backButton.Loop();
                pagination.Loop();
                foreach (MonoBehaviour option in optionsMono.Values)
                {
                    option.GetComponent<Patches.MainMenuButton>()?.Loop();
                    option.GetComponent<MainMenuText>()?.Loop();
                    option.GetComponent<MainMenuSelector>()?.Loop();
                    option.GetComponent<MainMenuSlider>()?.Loop();
                    option.GetComponent<MainMenuInputText>()?.Loop();
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
                foreach (MonoBehaviour option in optionsMono.Values)
                {
                    option.GetComponent<Patches.MainMenuButton>()?.InitRoll();
                    option.GetComponent<MainMenuText>()?.InitRoll();
                    option.GetComponent<MainMenuSelector>()?.InitRoll();
                    option.GetComponent<MainMenuSlider>()?.InitRoll();
                    option.GetComponent<MainMenuInputText>()?.InitRoll();
                }
                Refresh();
            }
        }

        public override void ForceExit()
        {
            backButton.ForceExit();
            pagination.ForceExit();
            foreach (MonoBehaviour option in optionsMono.Values)
            {
                option.GetComponent<Patches.MainMenuButton>()?.ForceExit();
                option.GetComponent<MainMenuText>()?.ForceExit();
                option.GetComponent<MainMenuSelector>()?.ForceExit();
                option.GetComponent<MainMenuSlider>()?.ForceExit();
                option.GetComponent<MainMenuInputText>()?.ForceExit();
            }
        }

        public override void OnClic(int buttonId)
        {
            GameController instance = GameController.GetInstance();
            if (UIKeyboard.GetInstance()?.IsOpen() ?? false) return;

            if (buttonId >= 0)
            {
                MonoBehaviour modOption = optionsMono[buttonId];
                MenuOption optionData = menuOptions[buttonId];

                if (modOption is Patches.MainMenuButton)
                {
                    MenuButton buttonData = (MenuButton)optionData;
                    instance.PlayGlobalSound("Play_menu_clic", false);
                    buttonData.OnClick?.Invoke();
                    MenuMain menu = buttonData.Menu;
                    if (menu != null)
                    {
                        if (menus.ContainsKey(buttonId)) DestroyImmediate(menus[buttonId].gameObject);
                        ModMenu modMenu = CreateMenu<ModMenu>(new MenuData
                        {
                            name = $"{name}_{menu.Id}",
                            label = menu.Label,
                            parent = this,
                            options = menu.Options,
                            onBack = menu.OnBack
                        });
                        menus[buttonId] = modMenu;
                        instance.GetUIController().mainMenu.Swap(modMenu, true);
                    }
                }
                else if (modOption is MainMenuSelector modSelect)
                {
                    if (optionData is MenuSelect selectData)
                    {
                        selectData.OnChange?.Invoke(modSelect.GetCurrentValue());
                    }
                    else if (optionData is MenuToggle toggleData)
                    {
                        toggleData.OnChange?.Invoke(modSelect.GetCurrentValue() == 1);
                    }
                }
                else if (modOption is Patches.MainMenuSlider modSlider)
                {
                    if (modSlider.HasUpdated())
                    {
                        MenuSlider sliderData = (MenuSlider)optionData;
                        sliderData.OnChange?.Invoke(modSlider.GetCurrentValue());
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
            OverableUI[][] overableUIs = pagination.GetOverableUI();
            browser = new Patches.UIBrowser(GetBrowserId(), overableUIs, GetBrowserIndex(overableUIs.Length), 0);
            return browser;
        }

        public void SetIndex(int i, int j)
        {
            browser?.ResetPosition(i, j);
        }

        public override void GoToPreviousMenu()
        {
            GameController instance = GameController.GetInstance();
            UIController uIController = instance.GetUIController();
            instance.PlayGlobalSound("Play_menu_return", false);
            uIController.mainMenu.Swap(parentMenu, false);
            onBack?.Invoke();
        }

        public override void Translate(I18nType i18n, I18nPlateformType i18nPlateformType) { }

        public void OnDestroy()
        {
            foreach (MenuOption modOption in menuOptions.Values) modOption.ResetEvents();
        }
    }
}
