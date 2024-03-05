using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace COSML.Components.Keyboard
{
    public class UIKeyboard : MonoBehaviour, AbstractPadUI
    {
        public JournalAnnotationCloseUI closeButton;
        public JournalAnnotationValidUI validButton;
        public JournalAnnotationClearUI clearButton;
        public UIKeyboardText text;
        public UIKeyboardKeyLine[] keys;
        public int maxChar;
        public event Action<string> OnChangeHook;

        public KeyboardConfigDef englishKeyboardConfig;
        public KeyboardConfigDef frenchKeyboardConfig;
        public KeyboardConfigDef spanishEuropeanKeyboardConfig;
        public KeyboardConfigDef spanishLatinAmericanKeyboardConfig;
        public KeyboardConfigDef portugueseKeyboardConfig;
        public KeyboardConfigDef germanKeyboardConfig;
        public KeyboardConfigDef italianKeyboardConfig;
        public KeyboardConfigDef chineseSimplifiedKeyboardConfig;
        public KeyboardConfigDef chineseTraditionalKeyboardConfig;
        public KeyboardConfigDef japaneseKeyboardConfig;
        public KeyboardConfigDef koreanKeyboardConfig;
        public KeyboardConfigDef russianKeyboardConfig;
        public KeyboardConfigDef czeshKeyboardConfig;
        public KeyboardConfigDef polishKeyboardConfig;

        public Image picotPrevKey;
        public Image pictoNextKey;
        public Color offKeyColor;
        public GameObject keyboardChangeBack;
        public Image shiftImg;
        public Sprite shiftOnSprite;
        public Sprite shiftOffSprite;

        private bool firstLoop;
        private int keyboardAltId;
        private int nbKeyboardAlt;
        private bool onShift;
        private UIKeyboardBrowser browser;
        private ShortcutTimed spaceCaret;
        private ShortcutTimed backspaceCaret;
        private ShortcutTimed shiftButton;
        private ShortcutTimed prevCaret;
        private ShortcutTimed nextCaret;
        private KeyboardConfigDef keyboardConfig;

        private static UIKeyboard staticInstance;

        public static UIKeyboard GetInstance() => staticInstance;

        public void Init()
        {
            staticInstance = this;
            gameObject.SetActive(true);
            closeButton.Init();
            validButton.Init();
            clearButton.Init();
            text.Init();
            foreach (UIKeyboardKeyLine line in keys)
            {
                foreach (UIKeyboardKey key in line.keys)
                {
                    key?.Init();
                }
            }
            gameObject.SetActive(false);
            keyboardConfig = englishKeyboardConfig;
            firstLoop = true;
            spaceCaret = new ShortcutTimed(true);
            backspaceCaret = new ShortcutTimed(true);
            shiftButton = new ShortcutTimed(false);
            prevCaret = new ShortcutTimed(true);
            nextCaret = new ShortcutTimed(true);
            keyboardAltId = 0;
            nbKeyboardAlt = 1;
            onShift = false;
            browser = null;
            shiftImg.sprite = shiftOffSprite;
            keyboardChangeBack.gameObject.SetActive(false);
        }

        public void Loop()
        {
            if (!gameObject.activeSelf) return;

            GameController instance = GameController.GetInstance();
            InputsController inputsController = instance.GetInputsController();
            int newKeyId = browser == null ? -1 : browser.GetSelectedId();
            Dictionary<byte, KeyboardConfigDef.KeyDef> dictionary = null;
            if (UpdateKeyboardConfig())
            {
                if (onShift != keyboardConfig.onShiftByDefault) SwitchShift();
                dictionary = keyboardConfig.GetKeyboardConfig();
                text.ChangeLanguage();
            }
            bool flag = keyboardConfig.configName.Equals("zh");
            closeButton.Loop();
            validButton.Loop();
            clearButton.Loop();
            text.Loop(keyboardConfig);
            for (int i = 0; i < keys.Length; i++)
            {
                for (int j = 0; j < keys[i].keys.Length; j++)
                {
                    UIKeyboardKey key = keys[i].keys[j];
                    if (key != null)
                    {
                        if (dictionary != null)
                        {
                            key.UpdateLanguage(dictionary, keyboardConfig.keyboardType, keyboardConfig.configName, false, keyboardChangeBack.gameObject.activeSelf);
                        }
                        key.Loop(onShift, false, newKeyId);
                        if (i == 0 && flag)
                        {
                            text.UpdateChineseChar(j, key, onShift);
                        }
                    }
                }
            }
            spaceCaret.Loop(inputsController.VirtualKeyboardSpace());
            backspaceCaret.Loop(inputsController.VirtualKeyboardBackspace());
            shiftButton.Loop(inputsController.VirtualKeyboardShift());
            prevCaret.Loop(inputsController.VirtualKeyboardPrevCaret());
            nextCaret.Loop(inputsController.VirtualKeyboardNextCaret());
            if (!firstLoop && !inputsController.IsOnClic())
            {
                if (inputsController.VirtualKeyboardValid()) Valid();
                else
                {
                    if (spaceCaret.IsDown())
                    {
                        text.AddChar(" ", null);
                        instance.PlayGlobalSound("Play_virtualKB_tap", false);
                    }
                    else if (backspaceCaret.IsDown())
                    {
                        text.Backspace();
                        instance.PlayGlobalSound("Play_virtualKB_tap", false);
                    }
                    if (shiftButton.IsDown())
                    {
                        SwitchShift();
                        instance.PlayGlobalSound("Play_virtualKB_tap", false);
                    }
                    if (prevCaret.IsDown())
                    {
                        text.PrevCaret();
                        instance.PlayGlobalSound("Play_virtualKB_tap", false);
                    }
                    else if (nextCaret.IsDown())
                    {
                        text.NextCaret();
                        instance.PlayGlobalSound("Play_virtualKB_tap", false);
                    }
                }
            }
            if (CanPrevAltKeyboard()) picotPrevKey.color = Color.white;
            else picotPrevKey.color = offKeyColor;
            if (CanNextAltKeyboard()) pictoNextKey.color = Color.white;
            else pictoNextKey.color = offKeyColor;
            if (inputsController.GetControllerType() <= InputsController.ControllerType.MOUSE) Hide();
            firstLoop = false;
        }

        public KeyboardConfigDef GetKeyboardConfigDef()
        {
            return keyboardConfig;
        }

        public void Show(string value)
        {
            UpdateKeyboardConfig();
            if (onShift != keyboardConfig.onShiftByDefault) SwitchShift();
            Dictionary<byte, KeyboardConfigDef.KeyDef> dictionary = keyboardConfig.GetKeyboardConfig();
            gameObject.SetActive(true);
            text.Show(value);
            closeButton.Resume();
            validButton.Resume();
            clearButton.Resume();
            foreach (UIKeyboardKeyLine line in keys)
            {
                foreach (UIKeyboardKey key in line.keys)
                {
                    key?.Resume(dictionary, keyboardConfig.keyboardType, keyboardConfig.configName, keyboardChangeBack.gameObject.activeSelf);
                }
            }
            firstLoop = true;
            spaceCaret.Resume();
            backspaceCaret.Resume();
            shiftButton.Resume();
            prevCaret.Resume();
            nextCaret.Resume();
        }

        private bool UpdateKeyboardConfig()
        {
            KeyboardConfigDef keyboardConfigDef = GameController.GetInstance().GetPlateformController().GetOptions().i18n switch
            {
                I18nType.FRENCH => frenchKeyboardConfig,
                I18nType.SPANISH_EUROPEAN => spanishEuropeanKeyboardConfig,
                I18nType.SPANISH_LATIN_AMERICAN => spanishLatinAmericanKeyboardConfig,
                I18nType.PORTUGUESE => portugueseKeyboardConfig,
                I18nType.GERMAN => germanKeyboardConfig,
                I18nType.ITALIAN => italianKeyboardConfig,
                I18nType.CHINESE_SIMPLIFIED => chineseSimplifiedKeyboardConfig,
                I18nType.CHINESE_TRADITIONAL => chineseTraditionalKeyboardConfig,
                I18nType.JAPANESE => japaneseKeyboardConfig,
                I18nType.KOREAN => koreanKeyboardConfig,
                I18nType.RUSSIAN => russianKeyboardConfig,
                I18nType.CZESH => czeshKeyboardConfig,
                I18nType.POLISH => polishKeyboardConfig,
                _ => englishKeyboardConfig,
            };
            keyboardChangeBack.gameObject.SetActive(keyboardConfigDef.altKeyboardDef != null && keyboardConfigDef.altKeyboardDef.Length != 0);
            nbKeyboardAlt = keyboardConfigDef.altKeyboardDef.Length + 1;
            if (keyboardAltId > 0 && keyboardConfigDef.altKeyboardDef != null && keyboardConfigDef.altKeyboardDef.Length != 0)
            {
                keyboardAltId = Mathf.Clamp(keyboardAltId, 0, keyboardConfigDef.altKeyboardDef.Length);
                keyboardConfigDef = keyboardConfigDef.altKeyboardDef[keyboardAltId - 1];
            }
            else
            {
                keyboardAltId = 0;
            }
            if (keyboardConfigDef.GetInstanceID().Equals(keyboardConfig.GetInstanceID())) return false;
            keyboardConfig = keyboardConfigDef;
            return true;
        }

        public void Valid()
        {
            GameController.GetInstance().PlayGlobalSound("Play_virtualKB_validate", false);
            Hide();
            OnChangeHook?.Invoke(text.input.text);
        }

        public void Clear()
        {
            text.Clear();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            closeButton.ForceExit();
            validButton.ForceExit();
            clearButton.ForceExit();
            foreach (UIKeyboardKeyLine line in keys)
            {
                foreach (UIKeyboardKey key in line.keys)
                {
                    key?.ForceExit();
                }
            }
            GameController.GetInstance().SkipNextSoundOver();
        }

        public bool IsOpen()
        {
            return gameObject.activeSelf;
        }

        public void LateLoop()
        {
            if (gameObject.activeSelf) text.LateLoop();
        }

        public AbstractUIBrowser GetBrowser()
        {
            OverableUI[][] overableUIs =
            [
                [null, null, null, null, null, null, null, null, null, null, clearButton],
                keys[0].keys,
                keys[1].keys,
                keys[2].keys,
                keys[3].keys,
                keys[4].keys,
            ];
            browser = new UIKeyboardBrowser(GetBrowserId(), overableUIs, 2, 1);
            return browser;
        }

        public int GetBrowserId()
        {
            return GetInstanceID();
        }

        public IEnumerable<BulleTradUI> GetBulles()
        {
            return null;
        }

        public void PrevAltKeyboard()
        {
            keyboardAltId--;
        }

        public void NextAltKeyboard()
        {
            keyboardAltId++;
        }

        public bool CanPrevAltKeyboard()
        {
            return keyboardAltId > 0;
        }

        public bool CanNextAltKeyboard()
        {
            return keyboardAltId < nbKeyboardAlt - 1;
        }

        public void SwitchShift()
        {
            onShift = !onShift;
            if (onShift)
            {
                shiftImg.sprite = shiftOnSprite;
                return;
            }
            shiftImg.sprite = shiftOffSprite;
        }

        private class ShortcutTimed
        {
            private bool pressed;
            private bool down;
            private float timeLeft;
            private readonly bool allowReinit;

            public ShortcutTimed(bool newAllowReinit)
            {
                allowReinit = newAllowReinit;
                Resume();
            }

            public void Resume()
            {
                pressed = false;
                down = false;
                timeLeft = 0f;
            }

            public void Loop(bool newPressed)
            {
                if (!newPressed)
                {
                    pressed = false;
                    down = false;
                    timeLeft = 0f;
                    return;
                }
                if (!pressed)
                {
                    pressed = true;
                    down = true;
                    timeLeft = 0.3f;
                    return;
                }
                if (!allowReinit)
                {
                    down = false;
                    return;
                }
                timeLeft -= Time.deltaTime;
                if (timeLeft <= 0f)
                {
                    down = true;
                    timeLeft = 0.1f;
                    return;
                }
                down = false;
            }

            public bool IsDown()
            {
                return down;
            }
        }
    }
}