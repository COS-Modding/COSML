using COSML.Components.Keyboard;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace COSML.MainMenu
{
    public class MainMenuInputText : MonoBehaviour
    {
        public AbstractMainMenu menu;
        public InputField input;
        public Text valueText;
        public Text counterText;
        public int maxChar = 0;
        public MainMenuInputTextOver over;
        public MainMenuInputTextOver inputOver;
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

        public void Show()
        {
            Keyboard.Open(input.text, maxChar, value => input.text = value);

            input.caretPosition = input.text.Length;
            input.selectionFocusPosition = input.caretPosition;
            input.selectionAnchorPosition = input.caretPosition;
        }

        public bool IsOver()
        {
            return onOver;
        }

        public event Action<string> ValueChangedHook;
        private void OnValueChanged(string text)
        {
            counterText.text = $"{text.Length}/{maxChar}";
            ValueChangedHook?.Invoke(text);
        }

    }

    public class MainMenuInputTextOver : PointableUI, OverableUI, OverableInteractive
    {
        private MainMenuInputText input;
        private bool isOver;

        public void Init(MainMenuInputText newInput)
        {
            input = newInput;
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

        public void CheckClick()
        {
            if (GameController.GetInstance().GetInputsController().OnUiClic()) input.Show();
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
                if (!input.IsOver())
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
