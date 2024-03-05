using MonoMod;
using UnityEngine;
using static COSML.MainMenu.MenuUtils;

namespace COSML.Patches
{
    [MonoModPatch("global::MainMenuSelector")]
    internal class MainMenuSelector : global::MainMenuSelector
    {
        private I18nKey[] i18nValues;

        [MonoModIgnore]
        private string[] values;
        [MonoModIgnore]
        private int currentValue;
        [MonoModIgnore]
        private int initialSize;

        public new void Init(int defaultValue)
        {
            currentValue = defaultValue;
            left.Init(this);
            right.Init(this);
            over.Init(this);

            if (i18nValues?.Length > 0)
            {
                currentValue = Mathf.Clamp(currentValue, 0, i18nValues.Length - 1);
                valueText.text = I18n.Get(i18nValues[currentValue]).ToUpper();
            }
            else if (values?.Length > 0)
            {
                currentValue = Mathf.Clamp(currentValue, 0, values.Length - 1);
                valueText.text = values[currentValue].ToUpper();
            }
        }

        public extern void orig_SetValues(string[] newValues, I18nType type, bool init);
        public new void SetValues(string[] newValues, I18nType type, bool init)
        {
            i18nValues = [];

            orig_SetValues(newValues, type, init);
        }

        public void SetValues(I18nKey[] newValues, I18nType type, bool init)
        {
            values = [];
            i18nValues = newValues;
            SetCurrentValue(currentValue);

            if (init) initialSize = valueText.fontSize;
            valueText.font = fontDefinition.GetFont(type);
            valueText.fontSize = (uint)(type - I18nType.CHINESE_SIMPLIFIED) <= 3u ? asiaticSize : initialSize;
        }

        public new void SetCurrentValue(int newCurrentValue)
        {
            if (i18nValues?.Length > 0)
            {
                currentValue = Mathf.Clamp(newCurrentValue, 0, i18nValues.Length - 1);
                SetPosition(i18nValues);
                left.UpdateImage();
                right.UpdateImage();
                valueText.text = I18n.Get(i18nValues[currentValue]).ToUpper();
            }
            else if (values?.Length > 0)
            {
                currentValue = Mathf.Clamp(newCurrentValue, 0, values.Length - 1);
                left.UpdateImage();
                right.UpdateImage();
                valueText.text = values[currentValue].ToUpper();
            }
        }

        public new void OnLeftClic()
        {
            string label = "";
            if (i18nValues?.Length > 0)
            {
                currentValue = Mathf.Clamp(currentValue - 1, 0, i18nValues.Length - 1);
                label = I18n.Get(i18nValues[currentValue]);
            }
            else if (values?.Length > 0)
            {
                currentValue = Mathf.Clamp(currentValue - 1, 0, values.Length - 1);
                label = values[currentValue];
            }
            valueText.text = label.ToUpper();

            left.UpdateImage();
            right.UpdateImage();
            menu.OnClic(buttonId);
        }

        public new void OnRightClic()
        {

            string label = "";
            if (i18nValues?.Length > 0)
            {
                currentValue = Mathf.Clamp(currentValue + 1, 0, i18nValues.Length - 1);
                label = I18n.Get(i18nValues[currentValue]);
            }
            else if (values?.Length > 0)
            {
                currentValue = Mathf.Clamp(currentValue + 1, 0, values.Length - 1);
                label = values[currentValue];
            }
            valueText.text = label.ToUpper();

            left.UpdateImage();
            right.UpdateImage();
            menu.OnClic(buttonId);
        }

        public new bool CanLeftClic()
        {
            int length = i18nValues?.Length > 0 ? i18nValues.Length : values?.Length ?? 0;
            return currentValue > 0 && length > 1;
        }

        public new bool CanRightClic()
        {
            int length = i18nValues?.Length > 0 ? i18nValues.Length : values?.Length ?? 0;
            return currentValue < length - 1 && length > 1;
        }

        public void Translate()
        {
            SetCurrentValue(currentValue);
        }

        public void SetPosition(object[] array)
        {
            float textWidth = UnityUtils.FindGreatestWidth(valueText, array);
            float chevronNextX = right.transform.localPosition.x;
            valueText.transform.localPosition = new Vector3(chevronNextX - (CHEVRON_MARGIN + textWidth / 2), transform.Find("Text_Libellé").localPosition.y, 0);
            left.transform.localPosition = new Vector3(chevronNextX - (2 * CHEVRON_MARGIN + textWidth), left.transform.localPosition.y, 0);
        }
    }
}
