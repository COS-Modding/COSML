#pragma warning disable IDE0052, IDE0044, CS0649, CS0414

using COSML.Log;
using COSML.MainMenu;
using MonoMod;
using System;
using UnityEngine;
using UnityEngine.UI;
using static COSML.MainMenu.MenuUtils;

namespace COSML.Patches
{
    [MonoModPatch("global::MainMenu")]
    public class MainMenu : global::MainMenu
    {
        public ModsMainMenu modMenu;
        public Text cosmlVersion;

        [MonoModIgnore]
        public new RootMainMenu rootMenu;
        [MonoModIgnore]
        public new PauseMenu pauseMenu;

        [MonoModIgnore]
        private AbstractMainMenu currentMenu;
        [MonoModIgnore]
        private bool waitMaskHide;
        [MonoModIgnore]
        private bool isDisplayed;
        [MonoModIgnore]
        private Animator animator;
        [MonoModIgnore]
        private float clicMaskSkipTimeLeft;

        private bool edited = false;

        public extern void orig_Init();
        public new void Init()
        {
            orig_Init();

            // Add COSML version
            GameObject cosmlVersionGo = new("COSMLVersion");
            cosmlVersionGo.transform.SetParent(copyright.transform, false);
            cosmlVersion = cosmlVersionGo.AddComponent<Text>();
            cosmlVersion.color = Color.white;
            cosmlVersion.text = $"COSML v{COSML.Version}";
            cosmlVersion.font = MenuResources.GetFontByName("Geomanist-Medium");
            cosmlVersion.fontSize = 40;
            cosmlVersion.alignment = TextAnchor.MiddleCenter;
            cosmlVersion.horizontalOverflow = HorizontalWrapMode.Wrap;
            cosmlVersion.verticalOverflow = VerticalWrapMode.Overflow;
            cosmlVersionGo.transform.localPosition = Vector3.zero;
            cosmlVersionGo.GetComponent<RectTransform>().sizeDelta = new Vector2(2600, 100);
            cosmlVersionGo.SetActive(false);
        }

        public extern void orig_Hide();
        public new void Hide()
        {
            orig_Hide();
            modMenu?.Hide();
        }

        public new void Loop()
        {
            if (gameObject.activeSelf)
            {
                // For some reason mod button/menu are found in the scene but don't appear in-game
                // so we edit the main menu only when the scene has finished loading
                if (!edited && GameObject.Find($"{Constants.MAIN_MENU_PATH}/Menu_Mods") == null)
                {
                    edited = true;

                    try
                    {
                        SaveElements();
                        AddKeyboard();
                        AddToast();

                        modMenu = CreateMenu<ModsMainMenu>(new MenuData
                        {
                            name = "Menu_Mods",
                            label = new I18nKey("cosml.menu.mods"),
                            parent = rootMenu
                        });

                        // Root main menu
                        rootMenu.EditMenu();

                        // Pause menu
                        pauseMenu.EditMenu();
                    }
                    catch (Exception ex)
                    {
                        Logging.API.Error($"Error creating mod menu:\n" + ex);
                    }
                }

                if (waitMaskHide)
                {
                    if (!initMask.activeSelf && pressButton.activeSelf)
                    {
                        GameController instance = (GameController)GameController.GetInstance();
                        InputsController startInputsController = instance.GetInputsController().GetStartInputsController();
                        if (startInputsController != null)
                        {
                            animator.enabled = false;
                            startInputsController.DisplayCursor(true);
                            instance.ChangeInputController(startInputsController);
                            waitMaskHide = false;
                            isDisplayed = true;
                            copyright.color = new Color(copyright.color.r, copyright.color.g, copyright.color.b, 1f);
                            cosmlVersion.gameObject.SetActive(true);
                            pressButton.SetActive(false);
                            if (instance.GetPlateformController().IsEulaAccepted())
                            {
                                currentMenu.Show(null);
                            }
                            else
                            {
                                currentMenu = null;
                                Swap(eulaMenu, true);
                            }
                            instance.PlayGlobalSound("Play_menu_clic", false);
                        }
                    }
                }
                else
                {
                    GameController instance = (GameController)GameController.GetInstance();
                    InputsController inputsController = instance.GetInputsController();
                    InputsController.ControllerType controllerType = inputsController.GetControllerType();
                    currentMenu?.Loop();
                    padIndics.gameObject.SetActive(currentMenu != null && !currentMenu.GetType().Equals(typeof(RootMainMenu)) && !currentMenu.GetType().Equals(typeof(EulaMenu)) && controllerType > InputsController.ControllerType.MOUSE);
                    if (padIndics.gameObject.activeSelf)
                    {
                        OverableUI overUI = inputsController.GetOverUI();
                        bool validActive = overUI != null && !overUI.GetType().Equals(typeof(MainMenuSelectorOver)) && !overUI.GetType().Equals(typeof(MainMenuTextOver));
                        validLabel.gameObject.SetActive(validActive);
                        validPicto.gameObject.SetActive(validActive);
                        validPicto.sprite = validSprites[(int)controllerType];
                        cancelPicto.sprite = cancelSprites[(int)controllerType];
                    }
                    if (controllerType == InputsController.ControllerType.MOUSE)
                    {
                        if (clicMask.gameObject.activeSelf)
                        {
                            clicMaskSkipTimeLeft -= Time.deltaTime;
                            if (clicMaskSkipTimeLeft <= 0f) clicMask.gameObject.SetActive(false);
                        }
                    }
                    else if (!clicMask.gameObject.activeSelf)
                    {
                        clicMask.gameObject.SetActive(true);
                        clicMaskSkipTimeLeft = clicMaskSkipTime;
                    }
                }
                demoBar.Loop();
            }
        }

        public extern void orig_Translate(I18nType i18n, I18nPlateformType i18nPlateformType);
        public new void Translate(I18nType i18n, I18nPlateformType i18nPlateformType)
        {
            orig_Translate(i18n, i18nPlateformType);

            foreach (I18nModdedText i18nModText in Resources.FindObjectsOfTypeAll<I18nModdedText>())
            {
                i18nModText.Translate(i18n, i18nPlateformType);
            }
            foreach (MainMenuSelector selector in Resources.FindObjectsOfTypeAll<MainMenuSelector>())
            {
                if (selector.transform.parent.name != "MenuOption") continue;
                selector.Translate();
            }
            foreach (MainMenuSlider slider in Resources.FindObjectsOfTypeAll<MainMenuSlider>())
            {
                if (slider.transform.parent.name != "MenuOption") continue;
                slider.Translate();
            }
        }

        public new void Swap(AbstractMainMenu newCurrentMenu, bool next)
        {
            if (next) swapper.Next(newCurrentMenu);
            else
            {
                if (newCurrentMenu == null) background.SetTrigger("Hide");
                swapper.Prev(newCurrentMenu);
            }

            GameController.GetInstance().GetInputsController().SetCursorOff(true, true);
            newCurrentMenu?.ForceExit();
            newCurrentMenu?.GetBrowser();
        }

        public AbstractMainMenu CurrentMenu => currentMenu;
    }
}