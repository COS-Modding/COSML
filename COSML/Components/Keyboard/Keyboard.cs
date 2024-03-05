using System;

namespace COSML.Components.Keyboard
{
    public static class Keyboard
    {

        /// <summary>
        /// Open controller keyboard.
        /// </summary>
        /// <param name="value">Input text value.</param>
        /// <param name="maxChar">Max character value.</param>
        /// <param name="onChange">Change action.</param>
        /// <returns></returns>
        public static void Open(string value, int maxChar = 0, Action<string> onChange = null)
        {
            if (GameController.GetInstance().GetInputsController() is not PadController) return;

            UIKeyboard keyboard = UIKeyboard.GetInstance();
            keyboard.maxChar = maxChar;
            keyboard.text.UpdateMaxChar(maxChar);
            keyboard.OnChangeHook += onChange;
            keyboard.Show(value);
        }
    }
}
