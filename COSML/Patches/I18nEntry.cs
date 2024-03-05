using MonoMod;
using System;

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
            if (I18n.CurrentLang == null) return orig_GetTranslation(type);
            if (I18n.Translations.ContainsKey(I18n.CurrentLang) && I18n.Translations[I18n.CurrentLang].ContainsKey(key))
            {
                return TryFormat(I18n.Translations[I18n.CurrentLang][key]);
            }
            return orig_GetTranslation(type);
        }

        private string TryFormat(string str)
        {
            try { return string.Format(str, args ?? []); }
            catch (Exception) { return string.Empty; }
        }
    }
}
