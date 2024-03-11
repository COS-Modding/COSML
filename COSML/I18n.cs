using COSML.Log;
using COSML.Modding;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace COSML
{
    public static class I18n
    {
        public static Dictionary<string, Dictionary<string, string>> Translations = [];
        public static List<string> Languages = [];
        public static readonly string[] DefaultI18nCodes = ["en", "fr", "es", "es-419", "pt", "de", "it", "zh", "zh-tw", "ja", "ko", "ru", "cs", "pl"];
        public static bool IsValidI18nFile(string f) => Path.GetExtension(f) == ".txt" && Path.GetFileName(f) != "template.txt";
        public static string I18nPath { get; private set; }
        public static I18nType CurrentI18nType => GameController.GetInstance().GetPlateformController().GetOptions().i18n;
        public static I18nPlateformType CurrentI18nPlateformType => GameController.GetInstance().GetPlateformController().GetI18nPlateformType();
        public static string CurrentLang
        {
            get => ModHooks.GlobalSettings.SelectedLanguage ?? GetLang(CurrentI18nType);
            internal set => ModHooks.GlobalSettings.SelectedLanguage = value ?? GetLang(CurrentI18nType);
        }

        private static FontDefinition fontDefinition;
        private static HashSet<FileSystemWatcher> filesWatchers;

        internal static void LoadI18n()
        {
            if (filesWatchers != null) return;
            filesWatchers = [];

            TrySetFontDefinition();

            I18nPath = Path.Combine(COSML.ManagedPath, "I18n");
            Directory.CreateDirectory(I18nPath);

            LoadLang(I18nPath, "global");

            LoadLang(Path.Combine(I18nPath, "COSML"), "COSML");

            foreach (COSML.ModInstance modInst in COSML.ModInstances)
            {
                LoadLang(Path.Combine(COSML.ModsPath, modInst.Name, "I18n"), $"\"{modInst.Name}\"");
            }
        }

        private static void LoadLang(string i18nPath, string name)
        {
            if (!Directory.Exists(i18nPath)) return;

            string[] filesPath = [.. new DirectoryInfo(i18nPath).GetFiles("*").Select(f => f.FullName).Where(IsValidI18nFile)];
            if (filesPath.Length <= 0) return;

            foreach (string file in filesPath) LoadLangTranslations(file);
            ReloadTranslations();

            FileSystemWatcher watcher = new(i18nPath)
            {
                Filter = "*.txt",
                NotifyFilter = NotifyFilters.LastWrite,
                EnableRaisingEvents = true
            };
            watcher.Changed += OnUpdateI18n;
            filesWatchers.Add(watcher);

            Logging.API.Info($"Loaded {name} i18n of {filesPath.Length} language{(filesPath.Length > 1 ? "s" : "")}: {string.Join(", ", filesPath.Select(Path.GetFileNameWithoutExtension))}");
        }

        private static void LoadLangTranslations(string file)
        {
            string lang = Path.GetFileNameWithoutExtension(file);
            foreach (string line in File.ReadLines(file))
            {
                if (line.Length == 0) continue;

                string[] split = line.Split(['='], 2);
                if (split.Length != 2) Logging.API.Warn($"I18n: Line \"{line}\" has no = sign!");
                if (!Translations.ContainsKey(lang)) Translations[lang] = [];
                if (!DefaultI18nCodes.Contains(lang) && !Languages.Contains(lang)) Languages.Add(lang);

                string key = split[0];
                string value = split[1]?.ToUpper();
                if (Translations[lang].ContainsKey(key)) Translations[lang][key] = value;
                else Translations[lang].Add(key, value);
            }
        }

        /// <summary>
        /// Get corresponding language code from i18n type
        /// </summary>
        /// <param name="type">I18n type.</param>
        /// <returns></returns>
        public static string GetLang(I18nType type)
        {
            return type switch
            {
                I18nType.ENGLISH => "en",
                I18nType.FRENCH => "fr",
                I18nType.SPANISH_EUROPEAN => "es",
                I18nType.SPANISH_LATIN_AMERICAN => "es-419",
                I18nType.PORTUGUESE => "pt",
                I18nType.GERMAN => "de",
                I18nType.ITALIAN => "it",
                I18nType.CHINESE_SIMPLIFIED => "zh",
                I18nType.CHINESE_TRADITIONAL => "zh-tw",
                I18nType.JAPANESE => "ja",
                I18nType.KOREAN => "ko",
                I18nType.RUSSIAN => "ru",
                I18nType.CZESH => "cs",
                I18nType.POLISH => "pl",
                _ => "en"
            };
        }

        private static void OnUpdateI18n(object sender, FileSystemEventArgs e)
        {
            if (!IsValidI18nFile(e.FullPath)) return;

            LoadLangTranslations(e.FullPath);

            string lang = Path.GetFileNameWithoutExtension(e.FullPath);
            if (CurrentLang != lang) return;

            ReloadTranslations();
            Logging.API.Info($"I18n: Reloaded {lang} translations");
        }

        private static void ReloadTranslations()
        {
            try
            {
                GameController instance = GameController.GetInstance();
                if (instance == null) return;

                instance.GetUIController().mainMenu.Translate(CurrentI18nType, CurrentI18nPlateformType);
            }
            catch (Exception ex)
            {
                Logging.API.Error($"An error occured while reloading translations\n{ex}");
            }
        }

        /// <summary>
        /// Set translation by key
        /// </summary>
        /// <param name="key">Translation key.</param>
        /// <param name="value">Translation value.</param>
        public static void Set(string key, string value) => Set(CurrentLang, key, value);
        /// <summary>
        /// Set translation by language and key
        /// </summary>
        /// <param name="type">Language type.</param>
        /// <param name="key">Translation key.</param>
        /// <param name="value">Translation value.</param>
        public static void Set(string type, string key, string value)
        {
            Translations[type][key] = value?.ToUpper();
            ReloadTranslations();
        }

        /// <summary>
        /// Get translation by i18n key
        /// </summary>
        /// <param name="key">I18n key.</param>
        public static string Get(I18nKey key) => Get(CurrentLang, key);
        /// <summary>
        /// Get translation by key
        /// </summary>
        /// <param name="key">Translation key.</param>
        public static string Get(string key) => Get(CurrentLang, key);
        /// <summary>
        /// Get translation by language and key
        /// </summary>
        /// <param name="type">Language type.</param>
        /// <param name="key">Translation key.</param>
        public static string Get(string type, string key) => Get(type, new I18nKey(key));
        /// <summary>
        /// Get translation by language and i18n key
        /// </summary>
        /// <param name="type">Language type.</param>
        /// <param name="key">I18n key.</param>
        public static string Get(string type, I18nKey key)
        {
            if (type == null || key?.key == null || !Translations.ContainsKey(type)) return key?.label ?? "";
            if (!Translations[type].ContainsKey(key.key)) return (key.key ?? key.label ?? "").ToUpper();
            return TryFormat(type, key.key, key.args);
        }

        /// <summary>
        /// Add I18nModdedText component to a gameobject.
        /// </summary>
        /// <param name="go">GameObject to add the component to.</param>
        /// <param name="key">I18nKey to set.</param>
        /// <returns></returns>
        /// <remarks>Also removes I18nText components.</remarks>
        public static I18nModdedText AddComponentI18nModdedText(GameObject go, I18nKey key)
        {
            if (go == null || key == null || key.key == null) return null;

            TrySetFontDefinition();

            I18nModdedText text = go.GetComponent<I18nModdedText>();
            if (text == null)
            {
                text = go.AddComponent<I18nModdedText>();
                text.fontDefinition = fontDefinition;
                text.asiaticSize = 52;
                text.consoleAlt18n = [];
                text.hasConsoleAltI18n = false;
                RemoveComponentI18nText(go);
            }

            text.i18n = key;
            text.Init(CurrentI18nType, CurrentI18nPlateformType);

            return text;
        }

        /// <summary>
        /// Remove I18nText component on a gameobject.
        /// </summary>
        /// <param name="go">GameObject to remove the component from.</param>
        internal static void RemoveComponentI18nText(GameObject go)
        {
            I18nText i18nText = go?.GetComponent<I18nText>();
            if (i18nText != null) UnityEngine.Object.Destroy(i18nText);
        }

        private static void TrySetFontDefinition()
        {
            if (fontDefinition != null) return;
            fontDefinition = UnityEngine.Object.FindObjectOfType<I18nText>().fontDefinition;
        }

        internal static string TryFormat(string lang, string key, object[] args)
        {
            try { return string.Format(Translations[lang][key], args ?? []).ToUpper(); }
            catch (Exception) { return string.Empty; }
        }
    }

    public class I18nKey : Patches.I18nEntry
    {
        /// <summary>
        /// Label of the i18n key.
        /// </summary>
        /// <remarks>Used if no key was provided, meaning it is not meant to be translated.</remarks>
        public string label;

        /// <summary>
        /// I18n key for auto translation.
        /// </summary>
        /// <param name="key">Key of the translation.</param>
        /// <param name="args">Arguments to pass to the formatter.</param>
        public I18nKey(string key, params object[] args) : base()
        {
            this.key = key;
            this.args = args;
        }

        public static implicit operator I18nKey(string value) => new(null) { label = value };

        public override int GetHashCode() => HashCode.Combine(key, label, args);
        public override bool Equals(object obj)
        {
            return obj is I18nKey i18nKey &&
                   key == i18nKey.key &&
                   label == i18nKey.label &&
                   string.Join(",", args).Equals(string.Join(",", i18nKey.args));
        }
    }

    /// <summary>
    /// I18n text component for mods.
    /// </summary>
    public class I18nModdedText : I18nText
    {
        public void Translate()
        {
            ModHooks.GlobalSettings.SelectedLanguage ??= I18n.GetLang(I18n.CurrentI18nType);
            Translate(I18n.CurrentI18nType, I18n.CurrentI18nPlateformType);
        }
    }
}
