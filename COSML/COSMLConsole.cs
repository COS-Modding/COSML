using COSML.Log;
using COSML.Modding;
using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace COSML
{
    internal class COSMLConsole : MonoBehaviour
    {
        private static GameObject overlayCanvasGo;
        private static GameObject textPanelGo;
        private static Font font;


        private readonly List<string> messages = new(25);


        private static readonly string[] OSFonts =
        {
            // Windows
            "Consolas",
            // Mac
            "Menlo",
            // Linux
            "Courier New",
            "DejaVu Mono"
        };

        private new bool enabled = true;

        private KeyCode toggleKey = KeyCode.F10;
        private int maxMessageCount = 25;
        private int fontSize = 6;

        public void Start()
        {
            LoadFont();
            LoadSettings();

            if (overlayCanvasGo != null) return;

            DontDestroyOnLoad(gameObject);

            overlayCanvasGo = new GameObject("ConsoleCanvas");
            overlayCanvasGo.transform.SetParent(transform, false);
            Canvas canvas = overlayCanvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            overlayCanvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

            textPanelGo = new("ConsoleText");
            textPanelGo.transform.SetParent(overlayCanvasGo.transform, false);
            Text text = textPanelGo.AddComponent<Text>();
            text.font = font;
            text.fontSize = fontSize;
            text.fontStyle = FontStyle.Normal;
            text.supportRichText = true;
            text.alignment = TextAnchor.LowerLeft;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            textPanelGo.GetComponent<RectTransform>().sizeDelta = new Vector2(800, 450);
        }

        private void LoadFont()
        {
            font = Resources.FindObjectsOfTypeAll<Font>().Where(f => f != null && f.name == "Geomanist-Medium").First();
            if (font == null) font = Font.CreateDynamicFontFromOSFont(OSFonts, fontSize);
            if (font == null) font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
        }

        private void LoadSettings()
        {
            ConsoleSettings settings = ModHooks.GlobalSettings.ConsoleSettings;

            toggleKey = settings.ToggleHotkey;

            if (toggleKey == KeyCode.Escape)
            {
                Logging.API.Error("Esc cannot be used as hotkey for console togging");

                toggleKey = settings.ToggleHotkey = KeyCode.F10;
            }

            maxMessageCount = settings.MaxMessageCount;

            if (maxMessageCount <= 0)
            {
                Logging.API.Error($"Specified max console message count {maxMessageCount} is invalid");

                maxMessageCount = settings.MaxMessageCount = 24;
            }

            fontSize = settings.FontSize;

            if (fontSize <= 0)
            {
                Logging.API.Error($"Specified console font size {fontSize} is invalid");

                fontSize = settings.FontSize = 12;
            }

            string userFont = settings.Font;

            if (string.IsNullOrEmpty(userFont)) return;

            font = Font.CreateDynamicFontFromOSFont(userFont, fontSize);

            if (font == null) Logging.API.Error($"Specified font {userFont} not found.");
        }

        [PublicAPI]
        public void Update()
        {
            if (!Input.GetKeyDown(toggleKey)) return;

            enabled = !enabled;
            textPanelGo.SetActive(enabled);
        }

        public void AddText(string message, LogLevel level)
        {
            string color = $"<color={ModHooks.GlobalSettings.ConsoleSettings.DefaultColor}>";

            if (ModHooks.GlobalSettings.ConsoleSettings.UseLogColors)
            {
                color = level switch
                {
                    LogLevel.Info => $"<color={ModHooks.GlobalSettings.ConsoleSettings.InfoColor}>",
                    LogLevel.Debug => $"<color={ModHooks.GlobalSettings.ConsoleSettings.DebugColor}>",
                    LogLevel.Warn => $"<color={ModHooks.GlobalSettings.ConsoleSettings.WarningColor}>",
                    LogLevel.Error => $"<color={ModHooks.GlobalSettings.ConsoleSettings.ErrorColor}>",
                    _ => color
                };
            }

            messages.Add(color + message + "</color>");

            while (messages.Count > maxMessageCount)
            {
                messages.RemoveAt(0);
            }


            if (textPanelGo != null)
            {
                textPanelGo.GetComponent<Text>().text = string.Join(string.Empty, messages.ToArray());
            }
        }
    }
}