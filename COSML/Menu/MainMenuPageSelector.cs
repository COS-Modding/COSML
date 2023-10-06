using UnityEngine;
using UnityEngine.UI;

namespace COSML.Menu
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

            InputsController inputsController = GameController.GetInstance().GetInputsController();
            InputsController.ControllerType controllerType = inputsController.GetControllerType();
            padIndics.gameObject.SetActive(controllerType != InputsController.ControllerType.MOUSE);
            if (padIndics.gameObject.activeSelf)
            {
                prevPagePicto.gameObject.SetActive(CanLeftClic());
                nextPagePicto.gameObject.SetActive(CanRightClic());
                prevPagePicto.sprite = MenuResources.LBButtonSprites[(int)controllerType];
                nextPagePicto.sprite = MenuResources.RBButtonSprites[(int)controllerType];
            }

            // Buttons controller
            GameController instance = GameController.GetInstance();
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

        private void ChangePage()
        {
            pagination.ChangePage(currentPage);

            // Force update browser
            InputsController inputsController = GameController.GetInstance().GetInputsController();
            if (inputsController is not PadController) return;
            Patches.PadController padController = (Patches.PadController)inputsController;
            padController.ForceUpdateUIBrowser(menu);
        }

        public override void OnLeftClic()
        {
            currentPage = Mathf.Clamp(currentPage - 1, 0, pages.Length - 1);
            left.UpdateImage();
            right.UpdateImage();
            valueText.text = pages[currentPage].ToString().ToUpper();
            ChangePage();
        }

        public override void OnRightClic()
        {
            currentPage = Mathf.Clamp(currentPage + 1, 0, pages.Length - 1);
            left.UpdateImage();
            right.UpdateImage();
            valueText.text = pages[currentPage].ToString().ToUpper();
            ChangePage();
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
