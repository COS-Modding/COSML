using COSML.Log;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace COSML.Menu
{
    public class MainMenuInputButton : MonoBehaviour
    {
        public AbstractMainMenu menu;
        public InputField input;
        public Text valueText;
        public MainMenuInputButtonOver over;
        public MainMenuInputButtonOver inputOver;
        public Animator overAnimator;

        protected bool onOver;

        public void Init()
        {
            over.Init(this);
            inputOver.Init(this);
            input.onValueChanged.AddListener(OnValueChanged);
        }

        public void InitRoll()
        {
            over.InitRoll();
            inputOver.InitRoll();
            onOver = false;
        }

        public void ForceExit()
        {
            over.ForceExit();
            inputOver.ForceExit();
        }

        public void Loop()
        {
            if (over.IsOver() || inputOver.IsOver())
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

        public static event Action<string> ValueChangedHook;
        internal static void OnValueChanged(string text)
        {
            if (ValueChangedHook == null) return;

            Delegate[] invocationList = ValueChangedHook.GetInvocationList();
            foreach (Action<string> toInvoke in invocationList.Cast<Action<string>>())
            {
                try
                {
                    toInvoke.Invoke(text);
                }
                catch (Exception ex)
                {
                    Logging.API.Error(ex);
                }
            }
        }

    }

    public class MainMenuInputButtonOver : PointableUI, OverableUI, OverableInteractive
    {
        private MainMenuInputButton selector;
        private bool isOver;

        public void Init(MainMenuInputButton newSelector)
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
            if (forceExit)
            {
                ForceExit();
            }

            return forceExit;
        }

        public void CheckClick()
        {
        }

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
                if (!selector.IsOver())
                {
                    instance.PlayGlobalSound("Play_menu_hover", true);
                }
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
