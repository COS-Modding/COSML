#pragma warning disable CS0109

using MonoMod;
using System;
using System.Linq;

namespace COSML.MainMenu
{
    [MonoModPatch("global::OptionsMainMenu")]
    public class OptionsMainMenu : global::OptionsMainMenu
    {
        private int OriginalLanguageCount => Enum.GetNames(typeof(I18nType)).Length;

        public extern void orig_Init();
        public new void Init()
        {
            orig_Init();

            if (I18n.CurrentLang != null)
            {
                int value = (int)I18n.CurrentI18nType;
                if (!I18n.DefaultI18nCodes.Contains(I18n.CurrentLang)) value = OriginalLanguageCount + I18n.Languages.FindIndex(l => l == I18n.CurrentLang);
                languageSelector.SetCurrentValue(value);
            }
        }

        private extern void orig_SetOptionsValues(AbstractOptions options, bool init);
        private new void SetOptionsValues(AbstractOptions options, bool init)
        {
            if (!init) return;

            string[] values = [
                .. Enum.GetValues(typeof(I18nType)).Cast<I18nType>().Select(((Patches.I18nEntry)languageI18n).orig_GetTranslation),
                .. I18n.Languages.Select(l => I18n.Get(l, new I18nKey("menu.langue")))
            ];
            languageSelector.SetValues(values, options.i18n, init);
        }

        public extern void orig_OnClic(int buttonId);
        public new void OnClic(int buttonId)
        {
            if (buttonId == 0)
            {
                int currentValue = languageSelector.GetCurrentValue();
                I18n.CurrentLang = I18n.Languages.ElementAtOrDefault(currentValue - OriginalLanguageCount) ?? I18n.GetLang((I18nType)currentValue);

                if (I18n.DefaultI18nCodes.Contains(I18n.CurrentLang))
                {
                    AbstractPlateform plateformController = GameController.GetInstance().GetPlateformController();
                    AbstractOptions options = plateformController.GetOptions();
                    options.i18n = (I18nType)currentValue;
                    options.SetNeedSave();
                    options.TrySaveOptions(plateformController.GetSaveManager());
                }
            }
            orig_OnClic(buttonId);
        }
    }
}
