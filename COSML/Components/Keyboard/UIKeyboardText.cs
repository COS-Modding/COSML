using HPark.Hangul;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace COSML.Components.Keyboard
{

    public class UIKeyboardText : MonoBehaviour
    {
        public InputField input;
        public Text charCounter;

        private bool needRefreshFocus;
        private bool needUpdateCaret;
        private KeyboardConfigDef.KeyDef.KeyCombinedDef[] previousCombined;
        private int accentCarret;
        private string previousValue;
        private int pinyinLine;
        private int pinyinLineNbLine;
        private string[] propositions;
        private Dictionary<string, char> koreanChars;

        public void Init()
        {
            koreanChars = new Dictionary<string, char>
            {
                { "ㅗㅏ", 'ㅘ' },
                { "ㅗㅐ", 'ㅙ' },
                { "ㅗㅣ", 'ㅚ' },
                { "ㅜㅓ", 'ㅝ' },
                { "ㅜㅔ", 'ㅞ' },
                { "ㅜㅣ", 'ㅟ' },
                { "ㅡㅣ", 'ㅢ' },
                { "ㄱㅅ", 'ㄳ' },
                { "ㄴㅈ", 'ㄵ' },
                { "ㄴㅎ", 'ㄶ' },
                { "ㄹㄱ", 'ㄺ' },
                { "ㄹㅁ", 'ㄻ' },
                { "ㄹㅂ", 'ㄼ' },
                { "ㄹㅅ", 'ㄽ' },
                { "ㄹㅌ", 'ㄾ' },
                { "ㄹㅍ", 'ㄿ' },
                { "ㄹㅎ", 'ㅀ' },
                { "ㅂㅅ", 'ㅄ' }
            };
        }

        public void Show(string text)
        {
            input.text = text;
            needRefreshFocus = true;
            needUpdateCaret = true;
            accentCarret = -1;
            pinyinLine = 0;
            pinyinLineNbLine = 0;
            propositions = null;

            input.caretPosition = input.text.Length;
            input.selectionFocusPosition = input.caretPosition;
            input.selectionAnchorPosition = input.caretPosition;
        }

        private void RefreshFocus()
        {
            EventSystem.current?.SetSelectedGameObject(null, new PointerEventData(EventSystem.current));

            input.Select();
            if (GameController.GetInstance().GetInputsController().GetControllerType() != 0) needUpdateCaret = true;

            previousCombined = null;
            previousValue = "";
            accentCarret = -1;
            pinyinLine = 0;
            pinyinLineNbLine = 0;
            propositions = null;
        }

        public void Clear()
        {
            input.text = "";
            previousCombined = null;
            previousValue = "";
            accentCarret = -1;
            pinyinLine = 0;
            pinyinLineNbLine = 0;
            propositions = null;
        }

        public void ChangeLanguage()
        {
            previousCombined = null;
            previousValue = "";
            accentCarret = -1;
            TryInvertSelection();
            input.selectionFocusPosition = input.caretPosition;
            input.selectionAnchorPosition = input.caretPosition;
            pinyinLine = 0;
            pinyinLineNbLine = 0;
            propositions = null;
        }

        public void AddChar(string charValue, KeyboardConfigDef.KeyDef.KeyCombinedDef[] combined)
        {
            KeyboardConfigDef keyboardConfigDef = UIKeyboard.GetInstance().GetKeyboardConfigDef();
            bool flag = keyboardConfigDef.configName.Equals("ja");
            bool flag2 = keyboardConfigDef.configName.Equals("ko");
            bool flag3 = keyboardConfigDef.configName.Equals("zh");
            TryInvertSelection();
            if (previousCombined == null && combined != null && combined.Length != 0)
            {
                previousCombined = combined;
                previousValue = charValue;
                if (flag)
                {
                    char[] array = input.text.ToCharArray();
                    string text = "";
                    for (int i = 0; i < input.caretPosition; i++)
                    {
                        text += array[i];
                    }

                    text += charValue;
                    for (int j = input.caretPosition; j < input.text.Length; j++)
                    {
                        text += array[j];
                    }

                    input.text = text;
                    input.caretPosition += 1;
                }

                accentCarret = input.caretPosition;
            }
            else
            {
                bool flag4 = false;
                if (previousCombined != null)
                {
                    charValue = (previousValue + charValue).Trim();
                    for (int k = 0; k < previousCombined.Length; k++)
                    {
                        if (previousCombined[k].raw == charValue)
                        {
                            charValue = previousCombined[k].combined;
                            break;
                        }
                    }
                }

                if (flag3)
                {
                    if (propositions != null && charValue.Length > 0 && int.TryParse(charValue.Substring(charValue.Length - 1, 1), out var result) && result < propositions.Length)
                    {
                        string text2 = result != 0 ? propositions[result - 1] : propositions[9];
                        if (text2 != null)
                        {
                            charValue = text2;
                            flag4 = true;
                        }
                    }

                    if (!flag4) charValue = GetSelected() + charValue;
                }

                string text3 = GetPrev(flag, flag2, isZh: false);
                string next = GetNext();
                if (flag2)
                {
                    if (text3.Length == 0 || accentCarret == -1)
                    {
                        text3 += charValue;
                        accentCarret = text3.Length;
                    }
                    else if (charValue.Equals(" "))
                    {
                        accentCarret = -1;
                    }
                    else
                    {
                        string text4 = HangulString.SplitToPhonemes(text3.Substring(text3.Length - 1, 1)) + charValue;
                        if (text4.Length >= 2 && koreanChars.TryGetValue(text4.Substring(text4.Length - 2, 2), out var value))
                        {
                            text4 = text4.Substring(0, text4.Length - 2) + value;
                        }

                        text3 = !HangulChar.TryJoinToSyllable(text4.ToCharArray(), out value) ? text3 + charValue : text3.Substring(0, text3.Length - 1) + value;
                        accentCarret = text3.Length;
                    }
                }
                else if (flag3 && charValue.Equals(" ") && input.selectionAnchorPosition != input.selectionFocusPosition)
                {
                    accentCarret = -1;
                }
                else
                {
                    text3 += charValue;
                    accentCarret = -1;
                }

                int selectionAnchorPosition = input.selectionAnchorPosition;
                input.text = text3 + next;
                input.caretPosition = text3.Length;
                if (flag3)
                {
                    if (!charValue.Equals(" ") && !flag4)
                    {
                        input.selectionAnchorPosition = selectionAnchorPosition;
                        input.selectionFocusPosition = text3.Length;
                    }
                }
                else if (accentCarret != -1)
                {
                    input.selectionAnchorPosition = text3.Length - 1;
                    input.selectionFocusPosition = text3.Length;
                }

                input.ForceLabelUpdate();
                previousCombined = null;
                previousValue = "";
            }

            pinyinLine = 0;
        }

        private string GetPrev(bool isJa, bool isKo, bool isZh)
        {
            if (input.caretPosition == 0 || input.text.Length == 0) return "";
            if (previousCombined != null && isJa) return input.text.Substring(0, input.caretPosition - 1);
            if (isZh || isKo || input.selectionFocusPosition == input.selectionAnchorPosition) return input.text.Substring(0, input.caretPosition);
            if (input.selectionAnchorPosition == 0) return "";

            return input.text.Substring(0, input.selectionAnchorPosition);
        }

        private string GetNext()
        {
            if (input.caretPosition < 0 || input.caretPosition >= input.text.Length || input.text.Length == 0) return "";

            return input.text.Substring(input.caretPosition, input.text.Length - input.caretPosition);
        }

        private string GetSelected()
        {
            if (input.selectionFocusPosition > input.selectionAnchorPosition) return input.text.Substring(input.selectionAnchorPosition, input.selectionFocusPosition - input.selectionAnchorPosition);
            if (input.selectionFocusPosition > input.selectionAnchorPosition) return input.text.Substring(input.selectionFocusPosition, input.selectionAnchorPosition - input.selectionFocusPosition);

            return "";
        }

        public void Backspace()
        {
            TryInvertSelection();
            if (previousCombined == null)
            {

                KeyboardConfigDef keyboardConfigDef = UIKeyboard.GetInstance().GetKeyboardConfigDef();
                bool isJa = keyboardConfigDef.configName.Equals("ja");
                bool isKo = keyboardConfigDef.configName.Equals("ko");
                bool flag = keyboardConfigDef.configName.Equals("zh");
                string text = GetPrev(isJa, isKo, flag);
                string next = GetNext();
                int selectionAnchorPosition = input.selectionAnchorPosition;
                if ((input.selectionAnchorPosition == input.selectionFocusPosition || flag) && text.Length > 0)
                {
                    text = text.Substring(0, text.Length - 1);
                }

                input.text = text + next;
                input.caretPosition = text.Length;
                if (flag)
                {
                    input.selectionAnchorPosition = selectionAnchorPosition;
                    input.selectionFocusPosition = text.Length;
                }
            }
            else
            {
                previousCombined = null;
            }

            previousValue = "";
            accentCarret = -1;
            pinyinLine = 0;
            pinyinLineNbLine = 0;
            propositions = null;
        }

        public void PrevCaret()
        {
            TryInvertSelection();
            input.caretPosition = Mathf.Clamp(input.caretPosition - 1, 0, input.text.Length);
            input.ForceLabelUpdate();
            pinyinLine = 0;
            pinyinLineNbLine = 0;
            propositions = null;
        }

        public void NextCaret()
        {
            TryInvertSelection();
            input.caretPosition = Mathf.Clamp(input.caretPosition + 1, 0, input.text.Length);
            input.ForceLabelUpdate();
            pinyinLine = 0;
            pinyinLineNbLine = 0;
            propositions = null;
        }

        public void PrevPinyinLine()
        {
            pinyinLine--;
        }

        public void NextPinyinLine()
        {
            pinyinLine++;
        }

        private void TryInvertSelection()
        {
            if (input.selectionFocusPosition < input.selectionAnchorPosition)
            {
                int selectionAnchorPosition = input.selectionAnchorPosition;
                int selectionFocusPosition = input.selectionFocusPosition;
                input.caretPosition = selectionAnchorPosition;
                input.selectionAnchorPosition = selectionFocusPosition;
                input.selectionFocusPosition = selectionAnchorPosition;
            }
        }

        public void Loop(KeyboardConfigDef keyboardConfig)
        {
            if (keyboardConfig.configName.Equals("zh"))
            {
                KeyboardConfigDef.PinyinEndDef[] chineseChars = keyboardConfig.GetChineseChars(GetSelected().ToUpper());
                if (chineseChars == null)
                {
                    int num = Mathf.Abs(input.selectionFocusPosition - input.selectionAnchorPosition);
                    if (num > 0)
                    {
                        TryInvertSelection();
                        if (num == 1)
                        {
                            input.caretPosition = input.selectionFocusPosition;
                        }
                        else
                        {
                            input.selectionAnchorPosition = input.selectionFocusPosition - 1;
                            chineseChars = keyboardConfig.GetChineseChars(input.text.ToUpper().Substring(input.selectionAnchorPosition, 1));
                            if (chineseChars == null)
                            {
                                input.caretPosition = input.selectionFocusPosition;
                            }
                        }
                    }
                }

                if (chineseChars != null && chineseChars.Length != 0)
                {
                    pinyinLineNbLine = 1;
                    KeyboardConfigDef.PinyinEndDef[] array = chineseChars;
                    foreach (KeyboardConfigDef.PinyinEndDef pinyinEndDef in array)
                    {
                        if (pinyinEndDef != null && pinyinEndDef.chineseChars != null)
                        {
                            pinyinLineNbLine = Mathf.Max(pinyinLineNbLine, pinyinEndDef.chineseChars.Length);
                        }
                    }

                    pinyinLine = Mathf.Clamp(pinyinLine, 0, pinyinLineNbLine - 1);
                    propositions = new string[10];
                    for (int j = 1; j < chineseChars.Length; j++)
                    {
                        if (chineseChars[j] != null && chineseChars[j].chineseChars != null && chineseChars[j].chineseChars.Length > pinyinLine)
                        {
                            propositions[j - 1] = chineseChars[j].chineseChars[pinyinLine].ToString();
                        }
                        else
                        {
                            propositions[j - 1] = null;
                        }
                    }

                    if (chineseChars[0] != null && chineseChars[0].chineseChars != null && chineseChars[0].chineseChars.Length > pinyinLine)
                    {
                        propositions[9] = chineseChars[0].chineseChars[pinyinLine].ToString();
                    }
                    else
                    {
                        propositions[9] = null;
                    }
                }
                else
                {
                    propositions = null;
                }
            }
            else
            {
                pinyinLine = 0;
                pinyinLineNbLine = 0;
                propositions = null;
                if (accentCarret != input.caretPosition || keyboardConfig.configName.Equals("ja") && (input.caretPosition <= 0 || input.caretPosition - 1 >= input.text.Length || !input.text.ToCharArray()[input.caretPosition - 1].ToString().Equals(previousValue)))
                {
                    previousCombined = null;
                    previousValue = "";
                    accentCarret = -1;
                }
            }
        }

        public void LateLoop()
        {
            GameController instance = GameController.GetInstance();
            InputsController inputsController = instance.GetInputsController();

            UIKeyboard keyboard = UIKeyboard.GetInstance();
            input.text = FormatAnnotationText(input.text);
            if (!input.isFocused && (needRefreshFocus || !keyboard.validButton.IsOver()))
            {
                RefreshFocus();
                needRefreshFocus = false;
            }
            else if (inputsController.OnTripleClic())
            {
                input.selectionAnchorPosition = 0;
                input.selectionFocusPosition = input.text.Length;
                input.ForceLabelUpdate();
            }
            else if (inputsController.OnDoubleClic())
            {
                int num = input.caretPosition;
                int i = input.caretPosition;
                char[] array = input.text.ToCharArray();
                while (num > 0 && array[num - 1] != ' ')
                {
                    num--;
                }

                for (; i < array.Length - 1 && array[i + 1] != ' '; i++)
                {
                }

                input.selectionAnchorPosition = num;
                input.selectionFocusPosition = i + 1;
                input.ForceLabelUpdate();
            }
            else if (needUpdateCaret)
            {
                input.caretPosition = input.text.Length;
                needUpdateCaret = false;
            }
            UpdateMaxChar(keyboard.maxChar);
        }

        public static string FormatAnnotationText(string rawString)
        {
            if (rawString == null) return "";

            return Regex.Replace(rawString, "[½¼¾¶§±]", "");
        }

        public void UpdateChineseChar(int keyId, UIKeyboardKey key, bool onShift)
        {
            switch (keyId)
            {
                case 11:
                    if (!onShift && propositions != null && pinyinLineNbLine > 1)
                    {
                        key.charText.text = "";
                        key.pictoImg.enabled = true;
                        key.SetChineseKeyType(UIKeyboardKey.ChineseKeyType.PREV);
                    }
                    else
                    {
                        key.pictoImg.enabled = false;
                        key.SetChineseKeyType(UIKeyboardKey.ChineseKeyType.STD);
                    }

                    break;
                case 12:
                    if (!onShift && propositions != null && pinyinLineNbLine > 1)
                    {
                        key.charText.text = "";
                        key.pictoImg.enabled = true;
                        key.SetChineseKeyType(UIKeyboardKey.ChineseKeyType.NEXT);
                    }
                    else
                    {
                        key.pictoImg.enabled = false;
                        key.SetChineseKeyType(UIKeyboardKey.ChineseKeyType.STD);
                    }

                    break;
                default:
                    if (!onShift && propositions != null && propositions != null && keyId > 0 && keyId <= propositions.Length && propositions[keyId - 1] != null)
                    {
                        key.charText.text = propositions[keyId - 1];
                    }

                    break;
            }
        }

        public void UpdateMaxChar(int max)
        {
            input.characterLimit = max;
            charCounter.text = max <= 0 ? "" : Mathf.Clamp(input.text.Length, 0, max) + "/" + max;
        }
    }
}