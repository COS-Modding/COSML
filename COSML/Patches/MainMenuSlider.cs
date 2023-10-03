using MonoMod;
using UnityEngine;
using UnityEngine.UI;

namespace COSML.Patches
{
    [MonoModPatch("global::MainMenuSlider")]
    public class MainMenuSlider : global::MainMenuSlider
    {
        [MonoModIgnore]
        public new MainMenuSliderPart[] slideParts;

        [MonoModIgnore]
        private int currentValue;
        [MonoModIgnore]
        private bool onOver;

        public Text valueText;

        private string[] values;
        private bool updateValue;

        public void SetValues(string[] newValues)
        {
            values = newValues;
            SetCurrentValue(currentValue, false);
        }

        public new void SetCurrentValue(int newCurrentValue, bool launchEventMenu)
        {
            updateValue = false;
            if (newCurrentValue != GetCurrentValue())
            {
                updateValue = true;
                currentValue = Mathf.Clamp(newCurrentValue, 0, GetMaxValue());
            }

            UpdateCursor();
            left.UpdateImage();
            right.UpdateImage();

            if (launchEventMenu)
            {
                menu.OnClic(buttonId);
            }

            if (valueText != null && values != null)
            {
                valueText.text = values[currentValue].ToUpper();
            }
        }

        public bool HasUpdated()
        {
            return updateValue;
        }

        private void UpdateCursor()
        {
            sliderPosition.position = Vector3.Lerp(sliderPositionMin.position, sliderPositionMax.position, (float)GetCurrentValue() / GetMaxValue());
        }

        public new void OnLeftClic()
        {
            SetCurrentValue(currentValue - 1, false);
            left.UpdateImage();
            right.UpdateImage();
            menu.OnClic(buttonId);
        }

        public new void OnRightClic()
        {
            SetCurrentValue(currentValue + 1, false);
            left.UpdateImage();
            right.UpdateImage();
            menu.OnClic(buttonId);
        }

        public new bool CanRightClic()
        {
            return currentValue < GetMaxValue();
        }

        public int GetMaxValue()
        {
            return slideParts.Length - 1;
        }

        public new void Loop()
        {
            left.LoopRoll();
            right.LoopRoll();
            over.LoopRoll();
            bool flag = left.IsOver() || right.IsOver() || over.IsOver();
            if (!flag)
            {
                MainMenuSliderPart[] array = slideParts;
                for (int i = 0; i < array.Length; i++)
                {
                    if (array[i].IsOver())
                    {
                        flag = true;
                        break;
                    }
                }
            }

            if (flag)
            {
                if (!onOver)
                {
                    overAnimator.ResetTrigger("RollOver");
                    overAnimator.ResetTrigger("RollOut");
                    overAnimator.SetTrigger("RollOver");
                    if (valueText != null)
                    {
                        valueText.color = Color.black;
                    }
                    onOver = true;
                }
            }
            else if (onOver)
            {
                overAnimator.ResetTrigger("RollOver");
                overAnimator.ResetTrigger("RollOut");
                overAnimator.SetTrigger("RollOut");
                if (valueText != null)
                {
                    valueText.color = Color.white;
                }
                onOver = false;
            }

            UpdateCursor();
        }
    }
}
