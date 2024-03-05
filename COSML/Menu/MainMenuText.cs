using UnityEngine;

namespace COSML.MainMenu
{
    public class MainMenuText : MonoBehaviour
    {
        public AbstractMainMenu menu;
        public MainMenuTextOver over;
        public Animator overAnimator;

        protected bool onOver;

        public void Init()
        {
            over.Init(this);
        }

        public void InitRoll()
        {
            over.InitRoll();
            onOver = false;
        }

        public void ForceExit()
        {
            over.ForceExit();
        }

        public void Loop()
        {
            if (over.IsOver())
            {
                if (!onOver)
                {
                    overAnimator.ResetTrigger("RollOver");
                    overAnimator.ResetTrigger("RollOut");
                    overAnimator.SetTrigger("RollOver");
                    onOver = true;
                }
            }
            else if (onOver)
            {
                overAnimator.ResetTrigger("RollOver");
                overAnimator.ResetTrigger("RollOut");
                overAnimator.SetTrigger("RollOut");
                onOver = false;
            }
        }

        public bool IsOver()
        {
            return onOver;
        }
    }

    public class MainMenuTextOver : PointableUI, OverableUI, OverableInteractive
    {
        private MainMenuText selector;
        private bool isOver;

        public void Init(MainMenuText newSelector)
        {
            selector = newSelector;
            InitRoll();
        }

        public void InitRoll()
        {
            isOver = false;
        }

        public void ForceExit()
        {
            isOver = false;
            GameController.GetInstance().GetInputsController().ExitUI(this);
        }

        public InteractiveCursorType GetCursorType()
        {
            return InteractiveCursorType.MOVE;
        }

        public bool AutoExit(bool forceExit)
        {
            if (forceExit) ForceExit();
            return forceExit;
        }

        public void CheckClick() { }

        public bool IsDisplayed()
        {
            return true;
        }

        public bool TryOpenJournal()
        {
            return false;
        }

        public void OnOver()
        {
            OnPointerEnter();
        }

        public override void OnPointerEnter()
        {
            if (!isOver)
            {
                GameController instance = GameController.GetInstance();
                instance.GetInputsController().EnterUI(this);
                if (!selector.IsOver()) instance.PlayGlobalSound("Play_menu_hover", true);
            }

            isOver = true;
        }

        public override bool ForceCusorOn()
        {
            return false;
        }

        public override bool DisplayToolTip()
        {
            return false;
        }

        public bool IsOver()
        {
            return isOver;
        }
    }
}
