using System.Collections.Generic;

namespace COSML.Components.Toast
{
    public static class Toast
    {
        internal const float TOAST_HEIGHT = 100f;
        internal const float TOAST_MARGIN = 50f;
        internal const float TOAST_PADDING = 30f;
        internal const float TOAST_TEXT_PIVOT_FIX = 0.05f;

        internal static List<ToastData> queue = [];

        /// <summary>
        /// Show a toast.
        /// </summary>
        /// <param name="text">Text of the toast.</param>
        /// <param name="duration">Duration of the toast (in seconds).</param>
        /// <param name="position">Position of the toast.</param>
        public static void Show(I18nKey text, float duration = 5f, ToastPosition position = ToastPosition.BottomRight)
        {
            ToastData data = new()
            {
                text = text,
                duration = duration,
                position = position
            };
            if (queue.Find(d => d.Equals(data)).text != null) return;
            queue.Add(data);
        }
    }
}
