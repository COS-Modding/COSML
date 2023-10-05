using COSML.Log;
using System;
using System.Linq;
using UnityEngine.UI;

namespace COSML.Menu
{
    public class InputButton : MainMenuText
    {
        public InputField input;
        public Text valueText;
        public bool isFocused;

        public void Start()
        {
            input.onValueChanged.AddListener(OnValueChanged);
        }

        public static event Action<string> ValueChangedHook;
        internal static void OnValueChanged(string text)
        {
            if (ValueChangedHook == null) return;

            Delegate[] invocationList = ValueChangedHook.GetInvocationList();
            foreach (Action<string> toInvoke in invocationList.Cast<Action<string>>())
            {
                try
                {
                    toInvoke.Invoke(text);
                }
                catch (Exception ex)
                {
                    Logging.API.Error(ex);
                }
            }
        }
    }
}
