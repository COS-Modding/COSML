#pragma warning disable CS0649

using COSML.Components.Keyboard;
using COSML.Components.Toast;
using COSML.Log;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Object;

namespace COSML.MainMenu
{
    public class MenuUtils
    {
        internal const float ROOT_MENU_MIN_Y = -72;
        internal const float ROOT_MENU_HEIGHT = 160;
        internal const int OPTION_MENU_MAX_PER_PAGE = 10;
        internal const float OPTION_MENU_HEIGHT = 120;
        internal const float OPTION_MENU_MIN_Y = 404;
        internal const float CHEVRON_MARGIN = 96;
        internal const float CHEVRON_MARGIN_FACTOR = 1.2f;

        internal static GameObject MenuTemplate { get; private set; }
        internal static GameObject RootButtonTemplate { get; private set; }
        internal static GameObject ButtonTemplate { get; private set; }
        internal static GameObject SelectTemplate { get; private set; }
        internal static GameObject SliderTemplate { get; private set; }
        internal static GameObject InputTextTemplate { get; private set; }

        private static bool cloned = false;

        /// <summary>
        /// Backup useful common elements.
        /// </summary>
        internal static void SaveElements()
        {
            if (cloned) return;

            GameObject templates = new("COSMLTemplates");
            templates.SetActive(false);
            DontDestroyOnLoad(templates);
            // Menu
            MenuTemplate = Instantiate(GameObject.Find($"{Constants.MAIN_MENU_PATH}/Menu_Options"), templates.transform, false);
            MenuTemplate.name = "Menu";
            // Root button
            RootButtonTemplate = Instantiate(GameObject.Find($"{Constants.MAIN_MENU_PATH}/Menu_Principal/Bouton Quit"), templates.transform, false);
            RootButtonTemplate.name = "RootButton";
            // Button
            ButtonTemplate = Instantiate(GameObject.Find($"{Constants.MAIN_MENU_PATH}/Menu_OptionsAudio/MenuBarre_Reset"), templates.transform, false);
            ButtonTemplate.name = "Button";
            // Select
            SelectTemplate = Instantiate(GameObject.Find($"{Constants.MAIN_MENU_PATH}/Menu_Options/MenuSelecteur_Langue"), templates.transform, false);
            SelectTemplate.name = "Select";
            // Slider
            SliderTemplate = Instantiate(GameObject.Find($"{Constants.MAIN_MENU_PATH}/Menu_OptionsAudio/MenuSlider_General"), templates.transform, false);
            SliderTemplate.name = "Slider";
            // Input text
            InputTextTemplate = Instantiate(GameObject.Find($"{Constants.GLOBAL_CANVAS_PATH}/AnnotationUI/popup/InputField/"), templates.transform, false);
            InputTextTemplate.name = "InputText";

            cloned = true;
        }

        internal static void AddKeyboard()
        {
            Transform globalCanvasTransform = GameObject.Find(Constants.GLOBAL_CANVAS_PATH).transform;
            if (globalCanvasTransform.Find("Keyboard") != null) return;

            // Keyboard
            GameObject keyboardGo = Instantiate(globalCanvasTransform.Find("AnnotationUI").gameObject, globalCanvasTransform, false);
            keyboardGo.name = "Keyboard";
            Destroy(keyboardGo.transform.Find("popup/Rune").gameObject);
            JournalAnnotationUI annotationUI = keyboardGo.GetComponent<JournalAnnotationUI>();
            UIKeyboard keyboard = keyboardGo.AddComponent<UIKeyboard>();
            // Keyboard text
            JournalAnnotationText annotationText = annotationUI.text;
            UIKeyboardText keyboardText = annotationText.gameObject.AddComponent<UIKeyboardText>();
            keyboardText.input = annotationText.input;
            keyboardText.charCounter = annotationText.charCounter;
            Destroy(annotationText);

            // Setup keys
            JournalAnnotationKeyLineUI[] keys = annotationUI.keys;
            List<UIKeyboardKeyLine> keyLineList = [];
            foreach (JournalAnnotationKeyLineUI line in keys)
            {
                UIKeyboardKeyLine kbLine = line.gameObject.AddComponent<UIKeyboardKeyLine>();
                keyLineList.Add(kbLine);
                List<UIKeyboardKey> keyList = [];
                foreach (JournalAnnotationKeyUI key in line.keys)
                {
                    if (key == null)
                    {
                        keyList.Add(null);
                        continue;
                    }
                    UIKeyboardKey kbKey = key.gameObject.AddComponent<UIKeyboardKey>();
                    kbKey.backImg = key.backImg;
                    kbKey.charText = key.charText;
                    kbKey.pushImg = key.pushImg;
                    kbKey.pictoImg = key.pictoImg;
                    kbKey.skipOnQwertyUS = key.skipOnQwertyUS;
                    kbKey.keyCodeQWERTY = key.keyCodeQWERTY;
                    kbKey.keyCodeQWERTY_US = key.keyCodeQWERTY_US;
                    kbKey.keyCodeAZERTY = key.keyCodeAZERTY;
                    kbKey.keyCodeQWERTS = key.keyCodeQWERTS;
                    kbKey.keyType = (UIKeyboardKey.KeyType)key.keyType;
                    kbKey.padElements = key.padElements;
                    kbKey.tipsImg = key.tipsImg;
                    kbKey.tipsSprites = key.tipsSprites;
                    kbKey.over = key.over;
                    keyList.Add(kbKey);
                    kbKey.Init();
                    Destroy(key.gameObject.GetComponent<JournalAnnotationKeyUI>());
                }
                kbLine.keys = [.. keyList];
                Destroy(line.gameObject.GetComponent<JournalAnnotationKeyLineUI>());
            }

            // Assign values
            keyboard.closeButton = annotationUI.closeButton;
            keyboard.validButton = annotationUI.validButton;
            keyboard.clearButton = annotationUI.clearButton;
            keyboard.keys = [.. keyLineList];
            keyboard.text = keyboardText;
            keyboard.englishKeyboardConfig = annotationUI.englishKeyboardConfig;
            keyboard.frenchKeyboardConfig = annotationUI.frenchKeyboardConfig;
            keyboard.spanishEuropeanKeyboardConfig = annotationUI.spanishEuropeanKeyboardConfig;
            keyboard.spanishLatinAmericanKeyboardConfig = annotationUI.spanishLatinAmericanKeyboardConfig;
            keyboard.portugueseKeyboardConfig = annotationUI.portugueseKeyboardConfig;
            keyboard.germanKeyboardConfig = annotationUI.germanKeyboardConfig;
            keyboard.italianKeyboardConfig = annotationUI.italianKeyboardConfig;
            keyboard.chineseSimplifiedKeyboardConfig = annotationUI.chineseSimplifiedKeyboardConfig;
            keyboard.chineseTraditionalKeyboardConfig = annotationUI.chineseTraditionalKeyboardConfig;
            keyboard.japaneseKeyboardConfig = annotationUI.japaneseKeyboardConfig;
            keyboard.koreanKeyboardConfig = annotationUI.koreanKeyboardConfig;
            keyboard.russianKeyboardConfig = annotationUI.russianKeyboardConfig;
            keyboard.czeshKeyboardConfig = annotationUI.czeshKeyboardConfig;
            keyboard.polishKeyboardConfig = annotationUI.polishKeyboardConfig;
            keyboard.picotPrevKey = annotationUI.picotPrevKey;
            keyboard.pictoNextKey = annotationUI.pictoNextKey;
            keyboard.offKeyColor = annotationUI.offKeyColor;
            keyboard.keyboardChangeBack = annotationUI.keyboardChangeBack;
            keyboard.shiftImg = annotationUI.shiftImg;
            keyboard.shiftOnSprite = annotationUI.shiftOnSprite;
            keyboard.shiftOffSprite = annotationUI.shiftOffSprite;
            Destroy(annotationUI);

            Patches.UIController uiController = (Patches.UIController)GameController.GetInstance().GetUIController();
            uiController.keyboard = keyboard;
            keyboard.Init();
        }

        internal static void AddToast()
        {
            Transform globalCanvasTransform = GameObject.Find(Constants.GLOBAL_CANVAS_PATH).transform;
            if (globalCanvasTransform.Find("Toast") != null) return;

            // Base
            GameObject toastGo = new("Toast");
            toastGo.transform.SetParent(globalCanvasTransform, false);

            // Canvas
            GameObject canvasGo = new("Canvas");
            canvasGo.SetActive(false);
            canvasGo.transform.SetParent(toastGo.transform, false);
            UIToast toast = canvasGo.AddComponent<UIToast>();
            toast.canvas = canvasGo.AddComponent<CanvasGroup>();
            canvasGo.AddComponent<RectTransform>();
            toast.animator = canvasGo.AddComponent<UIToastAnimator>();

            // Background
            GameObject bgGo = new("Background");
            bgGo.transform.SetParent(canvasGo.transform, false);
            toast.background = bgGo.AddComponent<Image>();
            toast.background.color = Color.black;

            // Text
            GameObject toastTextGo = new("Text");
            toastTextGo.transform.SetParent(canvasGo.transform, false);
            toast.text = toastTextGo.AddComponent<Text>();
            toast.text.font = MenuResources.GetFontByName("Geomanist-Medium");
            toast.text.fontSize = (uint)(I18n.CurrentI18nType - I18nType.CHINESE_SIMPLIFIED) <= 3u ? 52 : 40;
            toast.text.alignment = TextAnchor.MiddleRight;
            toast.text.horizontalOverflow = HorizontalWrapMode.Overflow;
            toast.text.verticalOverflow = VerticalWrapMode.Overflow;

            Patches.UIController uiController = (Patches.UIController)GameController.GetInstance().GetUIController();
            uiController.toast = toast;
            toast.Init();
        }

        internal static Patches.MainMenuButton CreateRootButton(ButtonData data)
        {
            GameObject buttonGo = Instantiate(RootButtonTemplate, data.parent, false);
            buttonGo.name = data.name ?? "RootButton";
            Patches.MainMenuButton buttonMenu = buttonGo.GetComponent<Patches.MainMenuButton>();
            buttonMenu.menu = data.menu;
            buttonMenu.buttonId = data.buttonId;

            // Label
            GameObject labelGo = buttonGo.transform.Find("Text").gameObject;
            Text labelText = labelGo.GetComponent<Text>();
            labelText.text = data.label.label?.ToUpper() ?? "MODS";
            I18n.AddComponentI18nModdedText(labelText.gameObject, data.label, true);

            return buttonMenu;
        }

        internal static T CreateMenu<T>(MenuData data) where T : AbstractMainMenu
        {
            Transform mainMenuTransform = GameObject.Find(Constants.MAIN_MENU_PATH).transform;
            GameObject menuGo = Instantiate(MenuTemplate, mainMenuTransform, false);
            menuGo.name = data.name ?? $"Menu_{data.label.label.Replace(" ", "_")}";
            Text titleText = menuGo.transform.Find("Text_Titre").GetComponent<Text>();
            titleText.text = data.label.label?.ToUpper() ?? "MENU";
            I18n.AddComponentI18nModdedText(titleText.gameObject, data.label, true);

            Destroy(menuGo.GetComponent<global::OptionsMainMenu>());
            T menu = menuGo.AddComponent<T>();

            // Back button
            if (menu is IMainMenu mainMenu)
            {
                mainMenu.backButton = menuGo.transform.Find("BoutonBack").GetComponent<Patches.MainMenuButton>();
                mainMenu.backButton.buttonId = Constants.BACK_BUTTON_ID;
                mainMenu.backButton.menu = menu;
            }

            menuGo.ClearChildren(go => go.name.StartsWith("Menu"));

            // Reset Swapper/PadIndics layer position
            Transform padIndicsTransform = mainMenuTransform.Find("PadIndics");
            padIndicsTransform.SetParent(null, false);
            padIndicsTransform.SetParent(mainMenuTransform, false);
            Transform swapperTransform = mainMenuTransform.Find("Swapper");
            swapperTransform.SetParent(null, false);
            swapperTransform.SetParent(mainMenuTransform, false);

            if (menu is ModMenu modMenu)
            {
                modMenu.Init(data.options);
                modMenu.parentMenu = data.parent;
                modMenu.onBack = data.onBack;
            }
            else
            {
                menu.Init();
            }

            return menu;
        }

        internal static Patches.MainMenuButton CreateButton(ButtonData data)
        {
            GameObject buttonGo = Instantiate(ButtonTemplate, data.parent, false);
            buttonGo.name = data.name ?? "Button";
            Patches.MainMenuButton button = buttonGo.GetComponent<Patches.MainMenuButton>();
            button.menu = data.menu;
            button.buttonId = data.buttonId;

            // Arrow
            buttonGo.transform.Find("fleche")?.gameObject.SetActive(data.arrow ?? true);

            UpdateOption(button, data.label);

            return button;
        }

        internal static MainMenuText CreateText(TextData data)
        {
            // Base
            GameObject textGo = Instantiate(SelectTemplate, data.parent, false);
            textGo.name = data.name ?? "Text";
            MainMenuSelector selector = textGo.GetComponent<MainMenuSelector>();
            MainMenuText text = textGo.AddComponent<MainMenuText>();
            text.menu = data.menu;
            text.overAnimator = selector.overAnimator;
            Destroy(selector);

            // Over
            GameObject overGo = textGo.transform.Find("Collider").gameObject;
            Destroy(overGo.GetComponent<MainMenuSelectorOver>());
            MainMenuTextOver textButtonOver = overGo.AddComponent<MainMenuTextOver>();
            textButtonOver.Init(text);
            text.over = textButtonOver;

            // Clear unused
            Destroy(textGo.transform.Find("Text_Valeur").gameObject);
            Destroy(textGo.transform.Find("ChevronPrev").gameObject);
            Destroy(textGo.transform.Find("ChevronNext").gameObject);

            UpdateOption(text, data.label);

            return text;
        }

        internal static T UpdateOption<T>(T option, I18nKey label) where T : MonoBehaviour
        {
            Text text = option.transform.Find("Text_Libellé")?.GetComponent<Text>();
            text ??= option.transform.Find("Text")?.GetComponent<Text>();
            if (text != null)
            {
                if (label.key != null)
                {
                    I18n.AddComponentI18nModdedText(text.gameObject, label, true);
                }
                else
                {
                    text.text = label.label.ToUpper();
                    I18n.RemoveComponentI18nText(text.gameObject);
                }
            }
            else
            {
                Logging.API.Warn($"Could not find Text component to update for {option.name}");
            }


            return option;
        }

        internal static Patches.MainMenuSelector CreateSelect(SelectData data)
        {
            // Base
            GameObject selectGo = Instantiate(SelectTemplate, data.parent, false);
            selectGo.name = data.name ?? "Select";
            Patches.MainMenuSelector select = (Patches.MainMenuSelector)selectGo.GetComponent<MainMenuSelector>();
            select.menu = data.menu;
            select.buttonId = data.buttonId;
            data.values ??= [];

            // Chevrons
            GameObject chevronPrevGo = selectGo.transform.Find("ChevronPrev").gameObject;
            GameObject chevronNextGo = selectGo.transform.Find("ChevronNext").gameObject;
            MainMenuSelectorButton chevronPrevButton = chevronPrevGo.GetComponent<MainMenuSelectorButton>();
            MainMenuSelectorButton chevronNextButton = chevronNextGo.GetComponent<MainMenuSelectorButton>();
            chevronPrevButton.img = chevronPrevGo.GetComponent<Image>();
            chevronNextButton.img = chevronNextGo.GetComponent<Image>();
            chevronPrevGo.SetActive(false);

            // Collider
            GameObject colliderGo = selectGo.transform.Find("Collider").gameObject;
            MainMenuSelectorOver colliderOver = colliderGo.GetComponent<MainMenuSelectorOver>();

            select.Init(data.value);
            select.SetValues(data.values, I18n.CurrentI18nType, true);

            UpdateSelect(select, data);

            return select;
        }

        internal static Patches.MainMenuSelector UpdateSelect(Patches.MainMenuSelector select, SelectData data)
        {
            data.values ??= [""];
            select.SetValues(data.values, I18n.CurrentI18nType, true);
            select.SetCurrentValue(data.value);

            UpdateOption(select, data.label);

            return select;
        }

        internal static MainMenuSelector CreateToggle(ToggleData data)
        {
            MainMenuSelector toggle = CreateSelect(new SelectData
            {
                name = data.name,
                parent = data.parent,
                menu = data.menu,
                buttonId = data.buttonId,
                label = data.label,
                values = [data.off, data.on],
                value = data.value ? 1 : 0
            });
            toggle.name = data.name ?? "Toggle";

            return toggle;
        }

        internal static Patches.MainMenuSelector UpdateToggle(Patches.MainMenuSelector toggle, ToggleData data)
        {
            return UpdateSelect(toggle, new SelectData
            {
                values = [data.off, data.on],
                value = data.value ? 1 : 0
            });
        }

        internal static Patches.MainMenuSlider CreateSlider(SliderData data)
        {
            // Base
            GameObject sliderGo = Instantiate(SliderTemplate, data.parent, false);
            sliderGo.name = data.name ?? "Slider";
            Patches.MainMenuSlider slider = (Patches.MainMenuSlider)sliderGo.GetComponent<MainMenuSlider>();
            slider.menu = data.menu;
            slider.buttonId = data.buttonId;

            // Chevrons
            GameObject chevronPrevGo = sliderGo.transform.Find("ChevronPrev").gameObject;
            GameObject chevronNextGo = sliderGo.transform.Find("ChevronNext").gameObject;
            MainMenuSelectorButton chevronPrevButton = chevronPrevGo.GetComponent<MainMenuSelectorButton>();
            MainMenuSelectorButton chevronNextButton = chevronNextGo.GetComponent<MainMenuSelectorButton>();
            chevronPrevButton.img = chevronPrevGo.GetComponent<Image>();
            chevronNextButton.img = chevronNextGo.GetComponent<Image>();
            chevronPrevGo.SetActive(false);

            // Value
            GameObject sliderValueGo = Instantiate(SelectTemplate.transform.Find("Text_Valeur").gameObject, sliderGo.transform, false);
            sliderValueGo.name = "Text_Valeur";

            UpdateSlider(slider, data);

            return slider;
        }

        internal static Patches.MainMenuSlider UpdateSlider(Patches.MainMenuSlider slider, SliderData data)
        {
            data.steps ??= [];

            // Label/value
            Text sliderValueText = slider.transform.Find("Text_Valeur").GetComponent<Text>();
            sliderValueText.alignment = TextAnchor.MiddleRight;
            sliderValueText.transform.localPosition = new Vector3(-CHEVRON_MARGIN - 32, sliderValueText.transform.localPosition.y, 0);
            slider.valueText = sliderValueText;

            // Slider steps
            GameObject sliderBarGo = slider.transform.Find("Slider").gameObject;
            GameObject sliderFondGo = sliderBarGo.transform.Find("SliderFond").gameObject;
            sliderFondGo.ClearChildren(go => go.name != "Step0");
            MainMenuSliderPart[] parts = new MainMenuSliderPart[data.steps.Length];
            GameObject step0 = sliderFondGo.transform.GetChild(0).gameObject;
            parts[0] = step0.GetComponent<MainMenuSliderPart>();
            float sliderMinWidth = sliderBarGo.transform.Find("CurseurMinPos").localPosition.x;
            float sliderMaxWidth = sliderBarGo.transform.Find("CurseurMaxPos").localPosition.x;
            float sliderStepWidth = (sliderMaxWidth - sliderMinWidth) / (data.steps.Length - 1);
            const float sliderStepMargin = 10;
            for (int i = 1; i < data.steps.Length; i++)
            {
                GameObject step = Instantiate(step0, sliderFondGo.transform, false);
                step.name = $"Step{i}";
                step.transform.localPosition = new Vector3(i * sliderStepWidth + (i == data.steps.Length - 1 ? sliderStepMargin : 0), step.transform.localPosition.y, 0);
                MainMenuSliderPart stepPart = step.GetComponent<MainMenuSliderPart>();
                stepPart.value = i;
                parts[i] = stepPart;
            }
            slider.slideParts = parts;

            slider.Init(data.value);
            slider.SetValues(data.steps);

            object value = data.steps[Mathf.Clamp(data.value, 0, data.steps.Length - 1)];
            if (value is I18nKey i18nKey && i18nKey.key != null)
            {
                I18nModdedText i18nModText = I18n.AddComponentI18nModdedText(sliderValueText.gameObject, i18nKey);
                i18nModText?.Translate();
            }
            else
            {
                sliderValueText.text = value.ToString().ToUpper();
                I18n.RemoveComponentI18nText(sliderValueText.gameObject);
            }

            UpdateOption(slider, data.label);

            return slider;
        }

        internal static MainMenuInputText CreateInputText(InputTextData data)
        {
            // Button
            MainMenuText textInput = CreateText(new TextData
            {
                name = data.name,
                label = data.label ?? "INPUT",
                parent = data.parent,
                menu = data.menu
            });
            textInput.name = data.name ?? "InputText";
            MainMenuInputText input = textInput.gameObject.AddComponent<MainMenuInputText>();
            input.menu = textInput.menu;
            input.over = input.gameObject.AddComponent<MainMenuInputTextOver>();
            input.overAnimator = textInput.overAnimator;
            Destroy(textInput);

            // Input field
            GameObject inputFieldGo = Instantiate(InputTextTemplate, input.transform, false);
            Destroy(inputFieldGo.GetComponent<JournalAnnotationText>());
            inputFieldGo.name = "InputField";
            inputFieldGo.transform.localPosition = new Vector3(-30, -64, 0);
            InputField inputField = inputFieldGo.GetComponent<InputField>();
            inputField.text = data.text ?? "";
            inputField.characterLimit = data.max;
            Text inputText = inputFieldGo.transform.Find("Text").GetComponent<Text>();
            inputText.alignment = TextAnchor.MiddleLeft;
            input.valueText = inputText;
            input.input = inputField;
            input.maxChar = data.max;

            // Collider
            GameObject colliderGo = input.transform.Find("Collider").gameObject;
            input.inputOver = colliderGo.AddComponent<MainMenuInputTextOver>();
            Destroy(colliderGo.GetComponent<MainMenuTextOver>());

            // Background
            GameObject background = new("Background");
            background.transform.SetParent(input.transform, false);
            Image image = background.AddComponent<Image>();
            image.sprite = MenuResources.InputTextBackground;
            RectTransform rectImage = image.GetComponent<RectTransform>();
            rectImage.sizeDelta = new Vector2(image.sprite.textureRect.width, image.sprite.textureRect.height);
            RectTransform rectBackground = image.GetComponent<RectTransform>();
            rectBackground.sizeDelta = rectImage.sizeDelta;
            background.transform.localPosition = new Vector3(320, background.transform.localPosition.y, 0);
            inputFieldGo.transform.SetParent(null, false);
            inputFieldGo.transform.SetParent(input.transform, false);

            // Char counter
            GameObject counter = new("CharCounter");
            counter.transform.SetParent(input.transform, false);
            Text counterText = counter.AddComponent<Text>();
            counterText.color = new Color(0.6392f, 0.6392f, 0.6392f);
            counterText.font = MenuResources.GetFontByName("Geomanist-Medium");
            counterText.fontSize = 34;
            counterText.alignment = TextAnchor.MiddleRight;
            counterText.horizontalOverflow = HorizontalWrapMode.Overflow;
            counterText.verticalOverflow = VerticalWrapMode.Overflow;
            counter.transform.localPosition = new Vector3(620, -62, 0);
            input.counterText = counterText;

            input.Init();

            UpdateInputText(input, data);

            return input;
        }

        internal static MainMenuInputText UpdateInputText(MainMenuInputText input, InputTextData data)
        {
            InputField inputField = input.transform.Find("InputField").GetComponent<InputField>();
            inputField.text = data.text;
            input.maxChar = data.max;
            input.counterText.gameObject.SetActive(data.max > 0);
            if (data.max > 0)
            {
                input.input.characterLimit = data.max;
                input.input.text = input.valueText.text[..Math.Min(data.max, input.valueText.text.Length)];
                input.counterText.text = $"{input.input.text.Length}/{data.max}";
            }
            Text inputText = inputField.transform.Find("Text").GetComponent<Text>();
            inputText.GetComponent<RectTransform>().sizeDelta = new Vector2(data.max > 0 ? 590 : 700, 100);

            UpdateOption(input, data.label);

            return input;
        }

        internal static Vector3 GetRootButtonLocalPosition(int position)
        {
            return new Vector3(0, ROOT_MENU_MIN_Y - (position * ROOT_MENU_HEIGHT), 0);
        }

        internal static Vector3 GetOptionButtonLocalPosition(int position)
        {
            return new Vector3(0, OPTION_MENU_MIN_Y - (position % OPTION_MENU_MAX_PER_PAGE * OPTION_MENU_HEIGHT), 0);
        }

        /// <summary>
        /// Swap to a menu.
        /// <param name="menuId">Id of the menu.</param>
        /// </summary>
        public static void GoTo(string menuId)
        {
            AbstractMainMenu menu = FindMenuById(menuId);
            if (menu == null)
            {
                Logging.API.Warn($"Menu {menuId} has not been found!");
                return;
            }

            GameController.GetInstance()?.GetUIController().mainMenu?.Swap(menu, true);
        }

        /// <summary>
        /// Swap to the previous menu.
        /// </summary>
        public static void Back()
        {
            GameController.GetInstance()?.GetUIController().mainMenu?.GoToPreviousMenu();
        }

        /// <summary>
        /// Find a menu in the main menu tree.
        /// <param name="menuId">Id of the menu.</param>
        /// </summary>
        /// <returns>The found menu.</returns>
        public static AbstractMainMenu FindMenuById(string menuId)
        {
            return GameObject.Find($"{Constants.MAIN_MENU_PATH}/{menuId}")?.GetComponent<AbstractMainMenu>();
        }

        /// <summary>
        /// Refresh current menu.
        /// </summary>
        public static void Refresh()
        {
            GameController instance = GameController.GetInstance();
            UIController uiController = GameController.GetInstance().GetUIController();
            AbstractMainMenu currentMenu = ((Patches.MainMenu)uiController.mainMenu).CurrentMenu;
            if (currentMenu is ModMenu || currentMenu is ModsMainMenu)
            {
                currentMenu.ForceExit();
                AbstractUIBrowser browser = currentMenu.GetBrowser();
                if (instance.GetInputsController() is Patches.PadController padController)
                {
                    padController.SetUIBrowser(browser);
                }
            }
        }

        internal struct MenuData
        {
            internal string name;
            internal I18nKey label;
            internal AbstractMainMenu parent;
            internal IList<MenuOption> options;
            internal Action onBack;
        }

        internal struct ButtonData
        {
            internal string name;
            internal Transform parent;
            internal AbstractMainMenu menu;
            internal int buttonId;
            internal I18nKey label;
            internal bool? arrow;
            internal bool visible;
        }

        internal struct TextData
        {
            internal string name;
            internal Transform parent;
            internal AbstractMainMenu menu;
            internal I18nKey label;
            internal bool visible;
        }

        internal struct SelectData
        {
            internal string name;
            internal Transform parent;
            internal AbstractMainMenu menu;
            internal int buttonId;
            internal I18nKey label;
            internal I18nKey[] values;
            internal int value;
            internal bool visible;
        }

        internal struct ToggleData
        {
            internal string name;
            internal Transform parent;
            internal AbstractMainMenu menu;
            internal int buttonId;
            internal I18nKey label;
            internal I18nKey on;
            internal I18nKey off;
            internal bool value;
            internal bool visible;
        }

        internal struct SliderData
        {
            internal string name;
            internal Transform parent;
            internal AbstractMainMenu menu;
            internal int buttonId;
            internal I18nKey label;
            internal object[] steps;
            internal int value;
            internal bool visible;
        }

        internal struct InputTextData
        {
            internal string name;
            internal Transform parent;
            internal AbstractMainMenu menu;
            internal int buttonId;
            internal I18nKey label;
            internal string text;
            internal int max;
            internal bool visible;
        }

        /// <summary>
        /// A class representing a menu.
        /// </summary>
        public class MenuMain
        {
            /// <summary>
            /// Menu id.
            /// </summary>
            public string Id { get; private set; }
            /// <summary>
            /// Menu label.
            /// </summary>
            public I18nKey Label { get; private set; }
            /// <summary>
            /// Menu options list.
            /// </summary>
            public IList<MenuOption> Options { get; private set; }
            /// <summary>
            /// Menu back action.
            /// </summary>
            public Action OnBack { get; private set; }

            /// <summary>
            /// Create a menu.
            /// </summary>
            /// <param name="id">Menu id.</param>
            /// <param name="label">Menu label.</param>
            /// <param name="options">Menu options list.</param>
            /// <param name="onBack">Menu back action.</param>
            public MenuMain(string id, I18nKey label, IList<MenuOption> options, Action onBack = null)
            {
                Id = id;
                Label = label;
                Options = options;
                OnBack = onBack;
            }
        }

        /// <summary>
        /// A class for a menu option.
        /// </summary>
        public abstract class MenuOption
        {
            /// <summary>
            /// Option label.
            /// </summary>
            public I18nKey Label { get; protected set; }
            /// <summary>
            /// Option visibility.
            /// </summary>
            public bool Visible { get; protected set; }

            internal event Action<MenuOption> UpdateHook;
            internal event Action<bool> VisibleHook;

            /// <summary>
            /// Change menu option label.
            /// </summary>
            /// <param name="label"></param>
            public void SetLabel(I18nKey label)
            {
                Label = label;
                UpdateHook?.Invoke(this);
            }

            /// <summary>
            /// Change menu option visibility.
            /// </summary>
            /// <param name="visible">Option visibility value.</param>
            public void SetVisible(bool visible)
            {
                Visible = visible;
                VisibleHook?.Invoke(visible);
            }

            protected void Update() => UpdateHook?.Invoke(this);

            internal void ResetEvents()
            {
                UpdateHook = null;
                VisibleHook = null;
            }
        }

        /// <summary>
        /// A class representing a menu button option.
        /// </summary>
        public class MenuButton : MenuOption
        {
            /// <summary>
            /// Button click action.
            /// </summary>
            public Action OnClick { get; private set; }
            /// <summary>
            /// Button menu.
            /// </summary>
            public MenuMain Menu { get; private set; }

            /// <summary>
            /// Create a menu button.
            /// </summary>
            /// <param name="label">Button label.</param>
            /// <param name="menu">Button menu.</param>
            /// <param name="onClick">Button click action.</param>
            /// <param name="visible">Button visibility.</param>
            public MenuButton(I18nKey label, MenuMain menu = null, Action onClick = null, bool visible = true)
            {
                Label = label;
                Menu = menu;
                OnClick = onClick;
                Visible = visible;
            }
        }

        /// <summary>
        /// A class representing a menu text option.
        /// </summary>
        public class MenuText : MenuOption
        {
            /// <summary>
            /// Create a menu text.
            /// </summary>
            /// <param name="label">Text label.</param>
            /// <param name="visible">Text visibility.</param>
            public MenuText(I18nKey label, bool visible = true)
            {
                Label = label;
                Visible = visible;
            }
        }

        /// <summary>
        /// A class representing a menu select option.
        /// </summary>
        public class MenuSelect : MenuOption
        {
            /// <summary>
            /// Select values.
            /// </summary>
            public I18nKey[] Values { get; private set; }
            /// <summary>
            /// Select value.
            /// </summary>
            public int Value { get; private set; }
            /// <summary>
            /// Select change action.
            /// </summary>
            public Action<int> OnChange { get; private set; }

            /// <summary>
            /// Create a menu select.
            /// </summary>
            /// <param name="label">Select label.</param>
            /// <param name="values">Select values.</param>
            /// <param name="value">Select value.</param>
            /// <param name="onChange">Select change action.</param>
            /// <param name="visible">Select visibility.</param>
            public MenuSelect(I18nKey label, I18nKey[] values, int value = 0, Action<int> onChange = null, bool visible = true)
            {
                Label = label;
                Values = values;
                Value = value;
                OnChange = onChange;
                Visible = visible;
            }

            /// <summary>
            /// Change select values
            /// </summary>
            /// <param name="values"></param>
            public void SetValues(I18nKey[] values)
            {
                Values = values;
                Update();
            }

            /// <summary>
            /// Change select value
            /// </summary>
            /// <param name="value"></param>
            public void SetValue(int value)
            {
                Value = value;
                Update();
            }
        }

        /// <summary>
        /// A class representing a menu toggle option.
        /// </summary>
        public class MenuToggle : MenuOption
        {
            /// <summary>
            /// Toggle value.
            /// </summary>
            public bool Value { get; private set; }
            /// <summary>
            /// Toggle on label.
            /// </summary>
            public I18nKey On { get; private set; }
            /// <summary>
            /// Toggle off label.
            /// </summary>
            public I18nKey Off { get; private set; }
            /// <summary>
            /// Toggle change action.
            /// </summary>
            public Action<bool> OnChange { get; private set; }

            /// <summary>
            /// Create a menu toggle.
            /// </summary>
            /// <param name="label">Toggle label.</param>
            /// <param name="value">Toggle value.</param>
            /// <param name="on">Toggle on label.</param>
            /// <param name="off">Toggle off label.</param>
            /// <param name="onChange">Toggle change action.</param>
            /// <param name="visible">Toggle visibility.</param>
            public MenuToggle(I18nKey label, bool value = true, I18nKey on = null, I18nKey off = null, Action<bool> onChange = null, bool visible = true)
            {
                Label = label;
                Value = value;
                On = on;
                Off = off;
                OnChange = onChange;
                Visible = visible;
            }

            /// <summary>
            /// Change toggle value
            /// </summary>
            /// <param name="value"></param>
            public void SetValue(bool value)
            {
                Value = value;
                Update();
            }

            /// <summary>
            /// Change toggle on label
            /// </summary>
            /// <param name="on"></param>
            public void SetOn(I18nKey on)
            {
                On = on;
                Update();
            }

            /// <summary>
            /// Change toggle off label
            /// </summary>
            /// <param name="off"></param>
            public void SetOff(I18nKey off)
            {
                Off = off;
                Update();
            }
        }

        /// <summary>
        /// A class representing a menu slider option.
        /// </summary>
        public class MenuSlider : MenuOption
        {
            /// <summary>
            /// Slider steps.
            /// </summary>
            public object[] Steps { get; private set; }
            /// <summary>
            /// Slider value.
            /// </summary>
            public int Value { get; private set; }
            /// <summary>
            /// Slider change action.
            /// </summary>
            public Action<int> OnChange { get; private set; }

            /// <summary>
            /// Create a menu slider.
            /// </summary>
            /// <param name="label">Slider label.</param>
            /// <param name="steps">Slider steps.</param>
            /// <param name="value">Slider value.</param>
            /// <param name="onChange">Slider change action.</param>
            /// <param name="visible">Slider visibility.</param>
            public MenuSlider(I18nKey label, object[] steps, int value = 0, Action<int> onChange = null, bool visible = true)
            {
                Label = label;
                Steps = steps;
                Value = value;
                OnChange = onChange;
                Visible = visible;
            }

            /// <summary>
            /// Change slider steps.
            /// </summary>
            /// <param name="steps">Slider steps.</param>
            public void SetSteps(object[] steps)
            {
                Steps = steps;
                Update();
            }

            /// <summary>
            /// Change slider value.
            /// </summary>
            /// <param name="value">Slider value.</param>
            public void SetValue(int value)
            {
                Value = value;
                Update();
            }
        }

        /// <summary>
        /// A class representing a menu text input.
        /// </summary>
        public class MenuTextInput : MenuOption
        {
            /// <summary>
            /// Text input text.
            /// </summary>
            public string Text { get; private set; }
            /// <summary>
            /// Text input max characters value.
            /// </summary>
            public int Max { get; private set; }
            /// <summary>
            /// Text input action.
            /// </summary>
            public Action<string> OnInput { get; private set; }

            /// <summary>
            /// Create a menu text input.
            /// </summary>
            /// <param name="label">Text input label.</param>
            /// <param name="text">Text input text.</param>
            /// <param name="max">Text input max characters value.</param>
            /// <param name="onInput">Text input action.</param>
            /// <param name="visible">Text input visibility.</param>
            public MenuTextInput(I18nKey label, string text = "", int max = 0, Action<string> onInput = null, bool visible = true)
            {
                Label = label;
                Text = text;
                Max = max;
                OnInput = onInput;
                Visible = visible;
            }

            /// <summary>
            /// Change text input value.
            /// </summary>
            /// <param name="text">Text input value.</param>
            public void SetText(string text)
            {
                Text = text;
                Update();
            }

            /// <summary>
            /// Change text input max characters count.
            /// </summary>
            /// <param name="max">Max characters value.</param>
            public void SetMax(int max)
            {
                Max = max;
                Update();
            }
        }
    }
}
