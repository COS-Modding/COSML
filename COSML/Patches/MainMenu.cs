using COSML.Log;
using COSML.Menu;
using MonoMod;
using System;
using UnityEngine;
using static COSML.Menu.MenuUtils;

namespace COSML.Patches
{
    [MonoModPatch("global::MainMenu")]
    public class MainMenu : global::MainMenu
    {
        public ModsMainMenu modMenu;

        private bool edited = false;

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

        public extern void orig_Init();
        public new void Init()
        {
            orig_Init();

            copyright.text += $" (COSML-{COSML.Version})";
        }

        public extern void orig_Hide();
        public new void Hide()
        {
            orig_Hide();
            modMenu?.Hide();
        }

        public void Update()
        {
            // For some reason mod button/menu are found in the scene but don't appear in-game
            // so we edit the main menu only when the scene has finished loading
            if (edited || GameObject.Find($"{Constants.MAIN_MENU_PATH}/Menu_Mods") != null) return;
            edited = true;

            try
            {
                SaveElements();

                modMenu = CreateMenu<ModsMainMenu>(new InternalMenuData
                {
                    label = "Mods",
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

        public new void Loop()
        {
            if (gameObject.activeSelf)
            {
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
                    InputsController inputsController = global::GameController.GetInstance().GetInputsController();
                    InputsController.ControllerType controllerType = inputsController.GetControllerType();
                    if (currentMenu != null)
                    {
                        currentMenu.Loop();
                    }
                    padIndics.gameObject.SetActive(currentMenu != null && !currentMenu.GetType().Equals(typeof(RootMainMenu)) && !currentMenu.GetType().Equals(typeof(EulaMenu)) && controllerType > InputsController.ControllerType.MOUSE);
                    if (padIndics.gameObject.activeSelf)
                    {
                        OverableUI overUI = inputsController.GetOverUI();
                        bool active = overUI != null && !overUI.GetType().Equals(typeof(MainMenuSelectorOver)) && !overUI.GetType().Equals(typeof(MainMenuTextOver));
                        validLabel.gameObject.SetActive(active);
                        validPicto.gameObject.SetActive(active);
                        validPicto.sprite = validSprites[(int)controllerType];
                        cancelPicto.sprite = cancelSprites[(int)controllerType];
                    }
                    if (controllerType == InputsController.ControllerType.MOUSE)
                    {
                        if (clicMask.gameObject.activeSelf)
                        {
                            clicMaskSkipTimeLeft -= Time.deltaTime;
                            if (clicMaskSkipTimeLeft <= 0f)
                            {
                                clicMask.gameObject.SetActive(false);
                            }
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
    }
}