using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Object;

namespace COSML.Menu
{
    public class MenuUtils
    {
        public const float ROOT_MENU_MIN_Y = -72;
        public const float ROOT_MENU_HEIGHT = 160;

        public const int OPTION_MENU_MAX_PER_PAGE = 10;
        public const float OPTION_MENU_HEIGHT = 120;
        public const float OPTION_MENU_MIN_Y = 404;

        public const float CHEVRON_MARGIN = 128;

        public static GameObject menuTemplate;
        public static GameObject rootButtonTemplate;
        public static GameObject buttonTemplate;
        public static GameObject selectTemplate;
        public static GameObject sliderTemplate;

        private static bool cloned = false;

        /// <summary>
        /// Save options
        /// </summary>
        public static void SaveElements()
        {
            if (cloned) return;

            GameObject templates = new("COSMLTemplates");
            templates.SetActive(false);
            DontDestroyOnLoad(templates);

            // Menu
            menuTemplate = Instantiate(GameObject.Find($"{Constants.MAIN_MENU_PATH}/Menu_Options"), templates.transform, false);
            menuTemplate.name = "Menu";

            // Root button
            rootButtonTemplate = Instantiate(GameObject.Find($"{Constants.MAIN_MENU_PATH}/Menu_Principal/Bouton Quit"), templates.transform, false);
            rootButtonTemplate.name = "RootButton";

            // Button
            buttonTemplate = Instantiate(GameObject.Find($"{Constants.MAIN_MENU_PATH}/Menu_OptionsAudio/MenuBarre_Reset"), templates.transform, false);
            buttonTemplate.name = "Button";

            // Select
            selectTemplate = Instantiate(GameObject.Find($"{Constants.MAIN_MENU_PATH}/Menu_Options/MenuSelecteur_Langue"), templates.transform, false);
            selectTemplate.name = "Select";

            // Slider
            sliderTemplate = Instantiate(GameObject.Find($"{Constants.MAIN_MENU_PATH}/Menu_OptionsAudio/MenuSlider_General"), templates.transform, false);
            sliderTemplate.name = "Slider";

            cloned = true;
        }

        /// <summary>
        /// Create a menu.
        /// </summary>
        /// <param name="data">Menu data</param>
        /// <returns></returns>
        internal static T CreateMenu<T>(InternalMenuData data) where T : AbstractMainMenu
        {
            Transform mainMenuTransform = GameObject.Find(Constants.MAIN_MENU_PATH).transform;
            GameObject menuGo = Instantiate(menuTemplate, mainMenuTransform, false);
            menuGo.name = $"Menu_{data.label?.Replace(" ", "") ?? ""}";
            menuGo.transform.Find("Text_Titre").gameObject.GetComponent<Text>().text = data.label?.ToUpper() ?? "MENU";

            Destroy(menuGo.GetComponent<OptionsMainMenu>());
            T menu = menuGo.AddComponent<T>();

            // Back button
            if (menu is IMainMenu mainMenu)
            {
                mainMenu.backButton = menuGo.transform.Find("BoutonBack").gameObject.GetComponent<MainMenuButton>();
                mainMenu.backButton.buttonId = OPTION_MENU_MAX_PER_PAGE + 1;
                mainMenu.backButton.menu = menu;
            }

            ClearChildren(menuGo, (GameObject go) => go.name.StartsWith("Menu"));

            // Reset Swapper/PadIndics layer position
            Transform padIndicsTransform = mainMenuTransform.Find("PadIndics");
            padIndicsTransform.SetParent(null, false);
            padIndicsTransform.SetParent(mainMenuTransform, false);
            Transform swapperTransform = mainMenuTransform.Find("Swapper");
            swapperTransform.SetParent(null, false);
            swapperTransform.SetParent(mainMenuTransform, false);

            if (menu is ModMenu modMenu)
            {
                modMenu.Init(data.options ?? new List<IOptionData>());
                modMenu.parentMenu = data.parent;
                modMenu.onBack = data.onBack;
            }
            else
            {
                menu.Init();
            }

            return menu;
        }

        /// <summary>
        /// Create a root menu button.
        /// </summary>
        /// <param name="data">Button data</param>
        /// <returns></returns>
        internal static MainMenuButton CreateRootButton(InternalButtonData data)
        {
            GameObject buttonGo = data.parent != null ? Instantiate(rootButtonTemplate, data.parent, false) : Instantiate(rootButtonTemplate);
            buttonGo.name = "Button";
            MainMenuButton buttonMenu = buttonGo.GetComponent<MainMenuButton>();
            buttonMenu.menu = data.menu;
            buttonMenu.buttonId = data.buttonId;
            buttonMenu.transform.localPosition = GetRootButtonLocalPosition(data.position);

            // Label
            GameObject labelGo = buttonGo.transform.Find("Text").gameObject;
            Text labelText = labelGo.GetComponent<Text>();
            labelText.text = data.label?.ToUpper() ?? "BUTTON";

            return buttonMenu;
        }

        /// <summary>
        /// Create a button.
        /// </summary>
        /// <param name="data">Button data</param>
        /// <returns></returns>
        internal static MainMenuButton CreateButton(InternalButtonData data)
        {
            GameObject buttonGo = data.parent != null ? Instantiate(buttonTemplate, data.parent, false) : Instantiate(buttonTemplate);
            buttonGo.name = "Button";
            MainMenuButton buttonMenu = buttonGo.GetComponent<MainMenuButton>();
            buttonMenu.menu = data.menu;
            buttonMenu.buttonId = data.buttonId;
            buttonMenu.transform.localPosition = GetOptionButtonLocalPosition(data.position);

            // Label
            GameObject labelGo = buttonGo.transform.Find("Text").gameObject;
            Text labelText = labelGo.GetComponent<Text>();
            labelText.text = data.label?.ToUpper() ?? "BUTTON";

            // Arrow
            if (data.arrow == false)
            {
                buttonGo.transform.Find("fleche").gameObject.SetActive(false);
            }

            return buttonMenu;
        }

        /// <summary>
        /// Create a select.
        /// </summary>
        /// <param name="data">Select data</param>
        /// <returns></returns>
        internal static MainMenuSelector CreateSelect(InternalSelectData data)
        {
            // Base
            GameObject selectGo = data.parent != null ? Instantiate(selectTemplate, data.parent, false) : Instantiate(selectTemplate);
            selectGo.name = "Select";
            MainMenuSelector select = selectGo.GetComponent<MainMenuSelector>();
            select.menu = data.menu;
            select.buttonId = data.buttonId;
            select.transform.localPosition = GetOptionButtonLocalPosition(data.position);
            data.values ??= new string[0];
            data.values = data.values.Select(v => v.ToUpper()).ToArray();
            select.SetValues(data.values, I18nType.ENGLISH, true);

            // Chevrons
            GameObject chevronPrevGo = selectGo.transform.Find("ChevronPrev").gameObject;
            GameObject chevronNextGo = selectGo.transform.Find("ChevronNext").gameObject;
            MainMenuSelectorButton chevronPrevButton = chevronPrevGo.GetComponent<MainMenuSelectorButton>();
            MainMenuSelectorButton chevronNextButton = chevronNextGo.GetComponent<MainMenuSelectorButton>();
            chevronPrevButton.img = chevronPrevGo.GetComponent<Image>();
            chevronNextButton.img = chevronNextGo.GetComponent<Image>();
            chevronPrevButton.Init(select);
            chevronNextButton.Init(select);

            // Collider
            GameObject colliderGo = selectGo.transform.Find("Collider").gameObject;
            MainMenuSelectorOver colliderOver = colliderGo.GetComponent<MainMenuSelectorOver>();
            colliderOver.Init(select);

            // Label/value
            GameObject labelGo = selectGo.transform.Find("Text_Libellé").gameObject;
            Text labelText = labelGo.GetComponent<Text>();
            labelText.text = data.label?.ToUpper() ?? "SELECT";
            GameObject selectValueGo = selectGo.transform.Find("Text_Valeur").gameObject;
            float textWidth = FindGreatestWidth(selectValueGo.GetComponent<Text>(), data.values);
            float chevronNextX = chevronNextGo.transform.localPosition.x;
            selectValueGo.transform.localPosition = new Vector3(chevronNextX - (CHEVRON_MARGIN + textWidth / 2), labelGo.transform.localPosition.y, 0);
            chevronPrevGo.transform.localPosition = new Vector3(chevronNextX - (2 * CHEVRON_MARGIN + textWidth), chevronPrevGo.transform.localPosition.y, 0);
            chevronPrevGo.SetActive(false);
            select.Init(data.value);

            return select;
        }

        /// <summary>
        /// Create a toggle.
        /// </summary>
        /// <param name="options">Toggle options</param>
        /// <returns></returns>
        internal static MainMenuSelector CreateToggle(InternalToggleData options)
        {
            MainMenuSelector toggle = CreateSelect(new InternalSelectData
            {
                parent = options.parent,
                menu = options.menu,
                buttonId = options.buttonId,
                position = options.position,
                label = options.label,
                values = new string[2] { options.off ?? "OFF", options.on ?? "ON" },
                value = options.value ? 1 : 0
            });
            toggle.name = "Toggle";

            return toggle;
        }

        /// <summary>
        /// Create a slider.
        /// </summary>
        /// <param name="data">Slider data</param>
        /// <returns></returns>
        internal static MainMenuSlider CreateSlider(InternalSliderData data)
        {
            // Base
            GameObject sliderGo = data.parent != null ? Instantiate(sliderTemplate, data.parent, false) : Instantiate(sliderTemplate);
            sliderGo.name = "Slider";
            Patches.MainMenuSlider slider = (Patches.MainMenuSlider)sliderGo.GetComponent<MainMenuSlider>();
            slider.menu = data.menu;
            slider.buttonId = data.buttonId;
            slider.transform.localPosition = GetOptionButtonLocalPosition(data.position);
            data.steps ??= new object[0];

            // Chevrons
            GameObject chevronPrevGo = sliderGo.transform.Find("ChevronPrev").gameObject;
            GameObject chevronNextGo = sliderGo.transform.Find("ChevronNext").gameObject;
            MainMenuSelectorButton chevronPrevButton = chevronPrevGo.GetComponent<MainMenuSelectorButton>();
            MainMenuSelectorButton chevronNextButton = chevronNextGo.GetComponent<MainMenuSelectorButton>();
            chevronPrevButton.img = chevronPrevGo.GetComponent<Image>();
            chevronNextButton.img = chevronNextGo.GetComponent<Image>();
            chevronPrevButton.Init(slider);
            chevronNextButton.Init(slider);
            chevronPrevGo.SetActive(false);

            // Collider
            GameObject colliderGo = sliderGo.transform.Find("Collider").gameObject;
            MainMenuSelectorOver colliderOver = colliderGo.GetComponent<MainMenuSelectorOver>();
            colliderOver.Init(slider);

            // Label/value
            GameObject labelGo = sliderGo.transform.Find("Text_Libellé").gameObject;
            Text labelText = labelGo.GetComponent<Text>();
            labelText.text = data.label?.ToUpper() ?? "SLIDER";
            GameObject sliderValueGo = Instantiate(selectTemplate.transform.Find("Text_Valeur").gameObject, sliderGo.transform, false);
            sliderValueGo.name = "Text_Valeur";
            Text sliderValueText = sliderValueGo.GetComponent<Text>();
            sliderValueText.alignment = TextAnchor.MiddleRight;
            float textWidth = FindGreatestWidth(sliderValueText, data.steps);
            float chevronPrevX = chevronPrevGo.transform.localPosition.x;
            sliderValueGo.transform.localPosition = new Vector3(chevronPrevX - textWidth / 2 - CHEVRON_MARGIN / 1.5f, sliderValueGo.transform.localPosition.y, 0);
            slider.valueText = sliderValueText;
            slider.Init(data.value);

            // Slider steps
            GameObject sliderBarGo = sliderGo.transform.Find("Slider").gameObject;
            GameObject sliderFondGo = sliderBarGo.transform.Find("SliderFond").gameObject;
            ClearChildren(sliderFondGo, (GameObject go) => go.name != "Step0");
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
                stepPart.Init(slider);
            }
            slider.slideParts = parts;
            slider.SetValues(data.steps.Select(s => s.ToString()).ToArray());

            return slider;
        }

        private static float FindGreatestWidth(Text text, object[] values)
        {
            float width = 0;

            foreach (object val in values)
            {
                text.text = val.ToString();
                width = Math.Max(width, text.preferredWidth);
            }

            return width;
        }

        /// <summary>
        /// Destroy a gameobject's children.
        /// </summary>
        /// <param name="parent">GameObject parent</param>
        /// <param name="condition">Condition for destroying children</param>
        public static void ClearChildren(GameObject parent, Func<GameObject, bool> condition)
        {
            foreach (Transform child in parent.transform)
            {
                if (condition(child.gameObject)) Destroy(child.gameObject);
            }
        }

        public static Vector3 GetRootButtonLocalPosition(int position)
        {
            return new Vector3(0, ROOT_MENU_MIN_Y - (position * ROOT_MENU_HEIGHT), 0);
        }

        public static Vector3 GetOptionButtonLocalPosition(int position)
        {
            return new Vector3(0, OPTION_MENU_MIN_Y - (position % OPTION_MENU_MAX_PER_PAGE * OPTION_MENU_HEIGHT), 0);
        }

        /// <summary>
        /// A struct representing a button.
        /// </summary>
        internal struct InternalButtonData
        {
            internal Transform parent;
            internal AbstractMainMenu menu;
            internal int buttonId;
            internal int position;
            internal string label;
            internal bool? arrow;
        }

        /// <summary>
        /// A struct representing a select.
        /// </summary>
        internal struct InternalSelectData
        {
            internal Transform parent;
            internal AbstractMainMenu menu;
            internal int buttonId;
            internal int position;
            internal string label;
            internal string[] values;
            internal int value;
        }

        /// <summary>
        /// A struct representing a toggle.
        /// </summary>
        internal struct InternalToggleData
        {
            internal Transform parent;
            internal AbstractMainMenu menu;
            internal int buttonId;
            internal int position;
            internal string label;
            internal string on;
            internal string off;
            internal bool value;
        }

        /// <summary>
        /// A struct representing a slider.
        /// </summary>
        internal struct InternalSliderData
        {
            internal Transform parent;
            internal AbstractMainMenu menu;
            internal int buttonId;
            internal int position;
            internal string label;
            internal object[] steps;
            internal int value;
        }

        /// <summary>
        /// A struct representing a mod menu.
        /// </summary>
        internal struct InternalMenuData
        {
            internal string label;
            internal AbstractMainMenu parent;
            internal List<IOptionData> options;
            internal Action onBack;
        }

        /// <summary>
        /// A struct representing a mod menu.
        /// </summary>
        public struct MenuData
        {
            public string label { get; }
            public List<IOptionData> options { get; }
            public Action onBack { get; }

            public MenuData(string label, List<IOptionData> options, Action onBack)
            {
                this.label = label;
                this.options = options;
                this.onBack = onBack;
            }
        }

        /// <summary>
        /// An interface for a menu option.
        /// </summary>
        public interface IOptionData { }

        /// <summary>
        /// A struct representing a mod button.
        /// </summary>
        public struct ButtonData : IOptionData
        {
            public string label { get; }
            public Action onClick { get; }
            public MenuData? menu { get; }

            public ButtonData(string label, Action onClick)
            {
                this.label = label;
                this.onClick = onClick;
            }

            public ButtonData(string label, MenuData menu, Action onClick) : this(label, onClick)
            {
                this.menu = menu;
            }
        }

        /// <summary>
        /// A struct representing a mod select.
        /// </summary>
        public struct SelectData : IOptionData
        {
            public SelectData(string label, string[] values, int value, Action<int> onChange)
            {
                this.label = label;
                this.values = values;
                this.value = value;
                this.onChange = onChange;
            }

            public string label { get; }
            public string[] values { get; }
            public int value { get; }
            public Action<int> onChange { get; }
        }

        /// <summary>
        /// A struct representing a mod toggle.
        /// </summary>
        public struct ToggleData : IOptionData
        {
            public string label { get; }
            public bool value { get; }
            public Action<bool> onChange { get; }
            public string on { get; }
            public string off { get; }

            public ToggleData(string label, bool value, Action<bool> onChange)
            {
                this.label = label;
                this.value = value;
                this.onChange = onChange;
            }

            public ToggleData(string label, bool value, string on, string off, Action<bool> onChange) : this(label, value, onChange)
            {
                this.on = on;
                this.off = off;
            }
        }

        /// <summary>
        /// A struct representing a mod slider.
        /// </summary>
        public struct SliderData : IOptionData
        {
            public string label { get; }
            public object[] steps { get; }
            public int value { get; }
            public Action<int> onChange { get; }

            public SliderData(string label, object[] steps, int value, Action<int> onChange)
            {
                this.label = label;
                this.steps = steps;
                this.value = value;
                this.onChange = onChange;
            }
        }
    }
}
