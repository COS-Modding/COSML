using MonoMod;

namespace COSML.Patches
{
    [MonoModPatch("global::I18nEntry")]
    public class I18nEntry : global::I18nEntry
    {
        /// <summary>
        /// Array of arguments to be formated with the i18n key.
        /// </summary>
        public object[] args;

        /// <summary>
        /// Key of the i18n entry.
        /// </summary>
        public new string key;

        public extern string orig_GetTranslation(I18nType type);
        public new string GetTranslation(I18nType type)
        {
            string lang = I18n.CurrentLang;
            if (lang == null) return orig_GetTranslation(type);
            if (I18n.Translations.ContainsKey(lang) && I18n.Translations[lang].ContainsKey(key)) return I18n.TryFormat(lang, key, args);
            return orig_GetTranslation(type);
        }
    }
}
