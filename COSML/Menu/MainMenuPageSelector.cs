using COSML.Components.Keyboard;
using UnityEngine;
using UnityEngine.UI;

namespace COSML.MainMenu
{
    public class MainMenuPageSelector : AbstractMainMenuSelector
    {
        public Text valueText;
        public MainMenuPagination pagination;

        public GameObject padIndics;
        public Image prevPagePicto;
        public Image nextPagePicto;

        private int currentPage;
        private int[] pages;
        private bool onOver;

        public void Init(int defaultValue)
        {
            currentPage = defaultValue;
            left.Init(this);
            right.Init(this);
            if (pages != null)
            {
                currentPage = Mathf.Clamp(currentPage, 0, pages.Length - 1);
                valueText.text = pages[currentPage].ToString().ToUpper();
            }
        }

        public void SetValues(int[] newValues)
        {
            pages = newValues;
            currentPage = Mathf.Clamp(currentPage, 0, pages.Length - 1);
            valueText.text = pages[currentPage].ToString().ToUpper();
            left?.UpdateImage();
            right?.UpdateImage();
        }

        public void InitRoll()
        {
            left.InitRoll();
            right.InitRoll();
            onOver = false;
        }

        public void ForceExit()
        {
            left.ForceExit();
            right.ForceExit();
            if (onOver)
            {
                overAnimator.ResetTrigger("RollOver");
                overAnimator.ResetTrigger("RollOut");
                overAnimator.SetTrigger("RollOut");
                onOver = false;
            }
        }

        public void Loop()
        {
            left.LoopRoll();
            right.LoopRoll();

            GameController instance = GameController.GetInstance();
            InputsController inputsController = instance.GetInputsController();
            InputsController.ControllerType controllerType = inputsController.GetControllerType();
            padIndics.gameObject.SetActive(controllerType != InputsController.ControllerType.MOUSE);
            if (padIndics.gameObject.activeSelf)
            {
                prevPagePicto.gameObject.SetActive(CanLeftClic());
                nextPagePicto.gameObject.SetActive(CanRightClic());
                prevPagePicto.sprite = MenuResources.LBButtonSprites[(int)controllerType];
                nextPagePicto.sprite = MenuResources.RBButtonSprites[(int)controllerType];
            }

            if (UIKeyboard.GetInstance()?.IsOpen() ?? false) return;

            // Buttons controller
            if (inputsController.IsShortcutPrev() && CanLeftClic())
            {
                instance.PlayGlobalSound("Play_menu_clic", false);
                OnLeftClic();
            }
            else if (inputsController.IsShortcutNext() && CanRightClic())
            {
                instance.PlayGlobalSound("Play_menu_clic", false);
                OnRightClic();
            }
        }

        public override void OnLeftClic()
        {
            currentPage = Mathf.Clamp(currentPage - 1, 0, pages.Length - 1);
            left.UpdateImage();
            right.UpdateImage();
            valueText.text = pages[currentPage].ToString().ToUpper();
            pagination.ChangePage(currentPage);
        }

        public override void OnRightClic()
        {
            currentPage = Mathf.Clamp(currentPage + 1, 0, pages.Length - 1);
            left.UpdateImage();
            right.UpdateImage();
            valueText.text = pages[currentPage].ToString().ToUpper();
            pagination.ChangePage(currentPage);
        }

        public override bool CanLeftClic()
        {
            if (currentPage > 0)
            {
                return pages.Length > 1;
            }

            return false;
        }

        public override bool CanRightClic()
        {
            if (currentPage < pages.Length - 1)
            {
                return pages.Length > 1;
            }

            return false;
        }

        public override bool IsOver()
        {
            return onOver;
        }
    }
}
