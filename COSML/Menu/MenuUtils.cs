using System;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Object;

namespace COSML.Menu
{
    public class MenuUtils
    {
        public const int MENU_OPTION_MAX_PER_PAGE = 10;
        public const float MENU_OPTION_HEIGHT = 120;
        public const float MENU_OPTION_MIN_Y = 404;

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
        /// <param name="options">Menu options</param>
        /// <returns></returns>
        public static GameObject CreateMenu<T>(MenuOptions options) where T : AbstractMainMenu
        {
            Transform mainMenuTransform = GameObject.Find(Constants.MAIN_MENU_PATH).transform;
            GameObject menuGo = Instantiate(menuTemplate, mainMenuTransform, false);
            menuGo.name = options.name ?? "Menu";
            menuGo.transform.Find("Text_Titre").gameObject.GetComponent<Text>().text = options.label ?? "MENU";

            Destroy(menuGo.GetComponent<OptionsMainMenu>());
            T menu = menuGo.AddComponent<T>();

            // Back button
            if (menu is IMainMenu)
            {
                IMainMenu mainMenu = menu as IMainMenu;
                mainMenu.backButton = menuGo.transform.Find("BoutonBack").gameObject.GetComponent<MainMenuButton>();
                mainMenu.backButton.buttonId = MENU_OPTION_MAX_PER_PAGE + 1;
                mainMenu.backButton.menu = menu;
            }

            // Clear cloned options
            foreach (Transform child in menu.transform)
            {
                GameObject go = child.gameObject;
                if (go.name.StartsWith("Menu")) Destroy(go);
            }

            // Reset Swapper layer position
            Transform swapperTransform = mainMenuTransform.transform.Find("Swapper");
            swapperTransform.SetParent(null, false);
            swapperTransform.SetParent(mainMenuTransform, false);

            return menuGo;
        }

        /// <summary>
        /// Create a root menu button.
        /// </summary>
        /// <param name="options">Button options</param>
        /// <returns></returns>
        public static GameObject CreateRootButton(ButtonOptions options)
        {
            GameObject buttonGo = options.parent != null ? Instantiate(rootButtonTemplate, options.parent, false) : Instantiate(rootButtonTemplate);
            buttonGo.name = options.name ?? "Button";
            MainMenuButton modButtonMenu = buttonGo.GetComponent<MainMenuButton>();
            modButtonMenu.menu = options.menu;
            modButtonMenu.buttonId = options.buttonId;

            // Label
            GameObject labelGo = buttonGo.transform.Find("Text").gameObject;
            Text labelText = labelGo.GetComponent<Text>();
            labelText.text = options.label ?? "BUTTON";

            return buttonGo;
        }

        /// <summary>
        /// Create a button.
        /// </summary>
        /// <param name="options">Button options</param>
        /// <returns></returns>
        public static GameObject CreateButton(ButtonOptions options)
        {
            GameObject buttonGo = options.parent != null ? Instantiate(buttonTemplate, options.parent, false) : Instantiate(buttonTemplate);
            buttonGo.name = options.name ?? "Button";
            MainMenuButton modButtonMenu = buttonGo.GetComponent<MainMenuButton>();
            modButtonMenu.menu = options.menu;
            modButtonMenu.buttonId = options.buttonId;

            // Label
            GameObject labelGo = buttonGo.transform.Find("Text").gameObject;
            Text labelText = labelGo.GetComponent<Text>();
            labelText.text = options.label ?? "BUTTON";

            // Arrow
            if (options.arrow == false)
            {
                buttonGo.transform.Find("fleche").gameObject.SetActive(false);
            }

            return buttonGo;
        }

        /// <summary>
        /// Create a toggle.
        /// </summary>
        /// <param name="options">Toggle options</param>
        /// <returns></returns>
        public static GameObject CreateToggle(ToggleOptions options)
        {
            return CreateSelect(new SelectOptions
            {
                name = options.name,
                parent = options.parent,
                menu = options.menu,
                buttonId = options.buttonId,
                label = options.label,
                values = new string[2] { options.off ?? "OFF", options.off ?? "ON" },
                currentValue = options.value ? 1 : 0
            });
        }

        /// <summary>
        /// Create a select.
        /// </summary>
        /// <param name="options">Select options</param>
        /// <returns></returns>
        public static GameObject CreateSelect(SelectOptions options)
        {
            float chevronNextX = 694;
            float chevronMargin = 128;

            // Base
            GameObject selectGo = options.parent != null ? Instantiate(selectTemplate, options.parent, false) : Instantiate(selectTemplate);
            selectGo.name = options.name ?? "Select";
            MainMenuSelector selector = selectGo.GetComponent<MainMenuSelector>();
            selector.menu = options.menu;
            selector.buttonId = options.buttonId;
            options.values ??= new string[0];
            selector.SetValues(options.values, I18nType.ENGLISH, true);

            // Chevrons
            GameObject chevronPrevGo = selectGo.transform.Find("ChevronPrev").gameObject;
            GameObject chevronNextGo = selectGo.transform.Find("ChevronNext").gameObject;
            MainMenuSelectorButton chevronPrevButton = chevronPrevGo.GetComponent<MainMenuSelectorButton>();
            MainMenuSelectorButton chevronNextButton = chevronNextGo.GetComponent<MainMenuSelectorButton>();
            chevronPrevButton.img = chevronPrevGo.GetComponent<Image>();
            chevronNextButton.img = chevronNextGo.GetComponent<Image>();
            chevronPrevButton.Init(selector);
            chevronNextButton.Init(selector);

            // Collider
            GameObject colliderGo = selectGo.transform.Find("Collider").gameObject;
            MainMenuSelectorOver colliderOver = colliderGo.GetComponent<MainMenuSelectorOver>();
            colliderOver.Init(selector);

            // Label/value
            GameObject labelGo = selectGo.transform.Find("Text_Libellé").gameObject;
            Text labelText = labelGo.GetComponent<Text>();
            labelText.text = options.label ?? "SELECT";
            GameObject selectValueGo = selectGo.transform.Find("Text_Valeur").gameObject;
            float textWidth = FindGreatestWidth(selectValueGo.GetComponent<Text>(), options.values);
            selectValueGo.transform.localPosition = new Vector3(chevronNextX - (chevronMargin + textWidth / 2), labelGo.transform.localPosition.y, 0);
            chevronPrevGo.transform.localPosition = new Vector3(chevronNextX - (2 * chevronMargin + textWidth), chevronPrevGo.transform.localPosition.y, 0);
            chevronPrevGo.SetActive(false);
            selector.Init(options.currentValue);

            return selectGo;
        }

        private static float FindGreatestWidth(Text text, string[] values)
        {
            float width = 0;

            foreach (string val in values)
            {
                text.text = val;
                width = Math.Max(width, text.preferredWidth);
            }

            return width;
        }

        /// <summary>
        /// Create a slider.
        /// </summary>
        /// <param name="options">Slider options</param>
        /// <returns></returns>
        public static GameObject CreateSlider(SliderOptions options)
        {
            GameObject sliderGo = options.parent != null ? Instantiate(sliderTemplate, options.parent, false) : Instantiate(sliderTemplate);
            return sliderGo;
        }

        /// <summary>
        /// A struct representing a menu.
        /// </summary>
        public struct MenuOptions
        {
            public string name;
            public string label;
        }

        /// <summary>
        /// A struct representing a button.
        /// </summary>
        public struct ButtonOptions
        {
            public string name;
            public Transform parent;
            public AbstractMainMenu menu;
            public int buttonId;
            public string label;
            public bool? arrow;
        }

        /// <summary>
        /// A struct representing a toggle.
        /// </summary>
        public struct ToggleOptions
        {
            public string name;
            public Transform parent;
            public AbstractMainMenu menu;
            public int buttonId;
            public string label;
            public string on;
            public string off;
            public bool value;
        }

        /// <summary>
        /// A struct representing a select.
        /// </summary>
        public struct SelectOptions
        {
            public string name;
            public Transform parent;
            public AbstractMainMenu menu;
            public int buttonId;
            public string label;
            public string[] values;
            public int currentValue;
        }

        /// <summary>
        /// A struct representing a slider.
        /// </summary>
        public struct SliderOptions
        {
            public string name;
            public Transform parent;
            public AbstractMainMenu menu;
            public string label;
            public string[] values;
            public int currentValue;
        }
    }
}
