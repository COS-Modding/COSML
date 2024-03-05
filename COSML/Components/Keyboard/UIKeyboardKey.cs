using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace COSML.Components.Keyboard
{
    public class UIKeyboardKey : RollButtonUI
    {
        public enum KeyType
        {
            STD,
            SHIFT,
            SPACE,
            BACKSPACE,
            VALID,
            PREV_KB,
            NEXT_KB,
            LEFT_CARET,
            RIGHT_CARET
        }

        public enum ChineseKeyType
        {
            STD,
            PREV,
            NEXT
        }

        public Image backImg;
        public Text charText;
        public Image pushImg;
        public Image pictoImg;
        public bool skipOnQwertyUS;
        public byte keyCodeQWERTY;
        public byte keyCodeQWERTY_US;
        public byte keyCodeAZERTY;
        public byte keyCodeQWERTS;
        public KeyType keyType;
        public GameObject padElements;
        public Image tipsImg;
        public Sprite[] tipsSprites;

        private string currentValue;
        private KeyboardConfigDef.KeyDef keyConfig;
        private KeyboardType keyboardType;
        private bool canCombine;
        private bool isPressed;
        private float timeLeft;
        private bool hideKey;
        private string keyboardName;
        private ChineseKeyType chineseKeyType;
        private int prevKeyId;

        public UIController UIController { get; private set; }

        public void Init()
        {
            gameObject.SetActive(value: true);
            keyConfig = null;
            currentValue = "";
            if (charText != null && keyType == KeyType.STD) charText.text = currentValue;
            keyboardType = KeyboardType.QWERTY;
            chineseKeyType = ChineseKeyType.STD;
            canCombine = false;
            isPressed = false;
            timeLeft = 0f;
            prevKeyId = -1;
            InitRoll();
        }

        public void Loop(bool onShift, bool onAlt, int newKeyId)
        {
            GameController instance = GameController.GetInstance();
            if (!UIKeyboard.GetInstance().IsOpen()) return;

            if (hideKey)
            {
                if (backImg.enabled || charText != null && charText.enabled)
                {
                    backImg.enabled = false;
                    if (charText != null) charText.enabled = false;
                    ForceExit();
                    over.gameObject.SetActive(value: false);
                }
            }
            else
            {
                HideIfNotKeyboard(backImg);
            }

            padElements?.SetActive(backImg.enabled);
            if (tipsImg != null) tipsImg.sprite = tipsSprites[(int)instance.GetInputsController().GetControllerType()];
            if (charText != null)
            {
                charText.enabled = backImg.enabled;
                if (keyType == KeyType.STD)
                {
                    if (keyConfig == null)
                    {
                        currentValue = "";
                        canCombine = false;
                    }
                    else if (onShift && !keyConfig.valueShift.Equals(""))
                    {
                        canCombine = keyConfig.canCombine != null && keyConfig.canCombine.Length > 1 && keyConfig.canCombine[1];
                        currentValue = keyConfig.valueShift;
                    }
                    else if (onAlt && !keyConfig.valueAlt.Equals(""))
                    {
                        canCombine = keyConfig.canCombine != null && keyConfig.canCombine.Length > 2 && keyConfig.canCombine[2];
                        currentValue = keyConfig.valueAlt;
                    }
                    else
                    {
                        canCombine = keyConfig.canCombine != null && keyConfig.canCombine.Length != 0 && keyConfig.canCombine[0];
                        currentValue = keyConfig.value;
                    }
                    charText.text = currentValue;
                }
            }
            else
            {
                canCombine = false;
            }

            if (isPressed && (!isOver || !instance.GetInputsController().IsOnClic()))
            {
                isPressed = false;
                RefreshPressed();
            }
            else if (keyType != 0)
            {
                RefreshPressed();
            }

            LoopRoll();
            if (instance.GetInputsController().IsOnClic())
            {
                if (newKeyId != prevKeyId) prevKeyId = -1;
            }
            else prevKeyId = newKeyId;
        }

        public void Resume(Dictionary<byte, KeyboardConfigDef.KeyDef> keyboardConfig, KeyboardType newKeyboardType, string newKeyboardName, bool hasAdditionnalLanguage)
        {
            keyboardName = newKeyboardName;
            over.gameObject.SetActive(value: false);
            isOver = false;
            isPressed = false;
            timeLeft = 0f;
            prevKeyId = -1;
            chineseKeyType = ChineseKeyType.STD;
            if (pictoImg != null)
            {
                pictoImg.enabled = keyType != KeyType.STD;
            }

            RefreshPressed();
            UpdateLanguage(keyboardConfig, newKeyboardType, keyboardName, resetOver: true, hasAdditionnalLanguage);
        }

        public void UpdateLanguage(Dictionary<byte, KeyboardConfigDef.KeyDef> keyboardConfig, KeyboardType newKeyboardType, string newKeyboardName, bool resetOver, bool hasAdditionnalLanguage)
        {
            keyboardName = newKeyboardName;
            if (resetOver)
            {
                over.gameObject.SetActive(value: false);
                isOver = false;
            }

            keyboardType = newKeyboardType;
            chineseKeyType = ChineseKeyType.STD;
            hideKey = skipOnQwertyUS && (keyboardType == KeyboardType.QWERTY_US || keyboardName.Equals("ja") || keyboardName.Equals("zh")) || !hasAdditionnalLanguage && (keyType == KeyType.PREV_KB || keyType == KeyType.NEXT_KB);
            switch (keyboardType)
            {
                case KeyboardType.QWERTY_US:
                    if (!keyboardConfig.TryGetValue(keyCodeQWERTY_US, out keyConfig)) keyConfig = null;
                    break;

                case KeyboardType.AZERTY:
                    if (!keyboardConfig.TryGetValue(keyCodeAZERTY, out keyConfig)) keyConfig = null;
                    break;

                case KeyboardType.QWERTS:
                    if (!keyboardConfig.TryGetValue(keyCodeQWERTS, out keyConfig)) keyConfig = null;
                    break;

                default:
                    if (!keyboardConfig.TryGetValue(keyCodeQWERTY, out keyConfig)) keyConfig = null;
                    break;
            }

            if (pictoImg != null) pictoImg.enabled = keyType != KeyType.STD;
            if (charText != null && keyType == KeyType.STD)
            {
                if (keyConfig == null) currentValue = "";
                else currentValue = keyConfig.value;
                charText.text = currentValue;
            }
        }

        public override void CheckClick()
        {
            GameController instance = GameController.GetInstance();
            InputsController inputsController = instance.GetInputsController();
            bool flag;
            if (!inputsController.IsCursorOff() && inputsController.IsOnClic() && prevKeyId == GetInstanceId())
            {
                if (isPressed)
                {
                    if (keyType == KeyType.SHIFT)
                    {
                        flag = false;
                    }
                    else
                    {
                        timeLeft -= Time.deltaTime;
                        if (timeLeft <= 0f)
                        {
                            flag = true;
                            timeLeft = 0.1f;
                        }
                        else
                        {
                            flag = false;
                        }
                    }
                }
                else
                {
                    flag = true;
                    timeLeft = 0.3f;
                }
            }
            else
            {
                flag = false;
            }

            if (!flag) return;

            UIKeyboard keyboard = UIKeyboard.GetInstance();
            bool pressedAltKey = true;
            switch (chineseKeyType)
            {
                case ChineseKeyType.PREV:
                    keyboard.text.PrevPinyinLine();
                    break;

                case ChineseKeyType.NEXT:
                    keyboard.text.NextPinyinLine();
                    break;

                default:
                    {

                        if (charText != null && keyConfig != null && keyType == KeyType.STD)
                        {
                            if (canCombine) keyboard.text.AddChar(currentValue, keyConfig.combinedValues);
                            else keyboard.text.AddChar(currentValue, null);
                            break;
                        }

                        switch (keyType)
                        {
                            case KeyType.SHIFT:
                                keyboard.SwitchShift();
                                break;

                            case KeyType.SPACE:
                                keyboard.text.AddChar(" ", null);
                                break;

                            case KeyType.BACKSPACE:
                                keyboard.text.Backspace();
                                break;

                            case KeyType.VALID:
                                keyboard.Valid();
                                break;

                            case KeyType.PREV_KB:
                                if (keyboard.CanPrevAltKeyboard()) keyboard.PrevAltKeyboard();
                                else pressedAltKey = false;
                                break;

                            case KeyType.NEXT_KB:
                                if (keyboard.CanNextAltKeyboard()) keyboard.NextAltKeyboard();
                                else pressedAltKey = false;

                                break;
                            case KeyType.LEFT_CARET:
                                keyboard.text.PrevCaret();
                                break;

                            case KeyType.RIGHT_CARET:
                                keyboard.text.NextCaret();
                                break;
                        }
                        break;
                    }
            }

            isPressed = true;
            RefreshPressed();
            if (pressedAltKey) instance.PlayGlobalSound("Play_virtualKB_tap", isOver: false);
        }

        private void RefreshPressed()
        {
            if (pushImg == null)
            {
                if (isPressed)
                {
                    if (charText != null) charText.color = Color.black;
                    backImg.color = Color.white;
                }
                else
                {
                    if (charText != null) charText.color = Color.white;
                    backImg.color = Color.black;
                }

                if (pictoImg != null)
                {
                    if (isPressed) pictoImg.color = Color.black;
                    else pictoImg.color = Color.white;
                }
            }
            else
            {
                if (!isPressed)
                {
                    InputsController inputsController = GameController.GetInstance().GetInputsController();
                    switch (keyType)
                    {
                        case KeyType.SHIFT:
                            isPressed = inputsController.VirtualKeyboardShift() && !inputsController.IsOnClic();
                            break;
                        case KeyType.SPACE:
                            isPressed = inputsController.VirtualKeyboardSpace() && !inputsController.IsOnClic();
                            break;
                        case KeyType.BACKSPACE:
                            isPressed = inputsController.VirtualKeyboardBackspace() && !inputsController.IsOnClic();
                            break;
                        case KeyType.VALID:
                            isPressed = inputsController.IsShortcutValid() && !inputsController.IsOnClic();
                            break;
                    }
                }

                backImg.gameObject.SetActive(!isPressed);
                pushImg.gameObject.SetActive(isPressed);
                if (pictoImg != null)
                {
                    if (isPressed) pictoImg.color = Color.black;
                    else pictoImg.color = Color.white;
                }
            }

            overImg.enabled = !isPressed;
        }

        public override bool IsDisplayed()
        {
            return UIKeyboard.GetInstance().IsOpen() && !hideKey;
        }

        public override InteractiveCursorType GetCursorType()
        {
            return InteractiveCursorType.BUTTON;
        }

        public override AbstractRuneTip GetRuneTip()
        {
            return null;
        }

        public override bool CanOver()
        {
            return true;
        }

        public override bool ForceCusorOn()
        {
            return true;
        }

        public override string GetOverSoundEvent()
        {
            return "Play_virtualKB_hover";
        }

        public void SetChineseKeyType(ChineseKeyType newChineseKeyType)
        {
            chineseKeyType = newChineseKeyType;
        }

        public override bool DisplayToolTip()
        {
            return false;
        }
    }
}