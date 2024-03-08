using System;
using UnityEngine;
using UnityEngine.UI;

namespace COSML.Components.Toast
{
    public class UIToast : MonoBehaviour
    {
        public UIToastAnimator animator;
        public CanvasGroup canvas;
        public Image background;
        public Text text;

        private static UIToast staticInstance;

        public static UIToast GetInstance() => staticInstance;

        public void Init()
        {
            staticInstance = this;
            gameObject.SetActive(true);
            animator.Init();
        }

        public void Loop()
        {
            if (!gameObject.activeSelf && Toast.queue.Count > 0) Show();
        }

        private void Show()
        {
            ToastData data = Toast.queue[0];
            text.text = data.text.label?.ToUpper();
            I18n.AddComponentI18nModdedText(text.gameObject, data.text);
            animator.durationTime = data.duration;
            SetPosition(data.position);

            Init();
            animator.Show(data.position);
        }

        private void SetPosition(ToastPosition position)
        {
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            RectTransform bgRect = background.GetComponent<RectTransform>();
            RectTransform textRect = text.GetComponent<RectTransform>();
            switch (position)
            {
                case ToastPosition.TopLeft:
                    transform.parent.localPosition = new Vector3(-1920 + Toast.TOAST_MARGIN, 1080 - Toast.TOAST_MARGIN, 0);
                    canvasRect.pivot = new Vector2(0.5f, 1f);
                    canvasRect.sizeDelta = new Vector2(text.preferredWidth + 2 * Toast.TOAST_PADDING, Toast.TOAST_HEIGHT);
                    bgRect.pivot = new Vector2(0, 0.5f);
                    bgRect.sizeDelta = new Vector2(text.preferredWidth + 2 * Toast.TOAST_PADDING, Toast.TOAST_HEIGHT);
                    textRect.pivot = new Vector2(0, 1f + Toast.TOAST_TEXT_PIVOT_FIX);
                    textRect.sizeDelta = new Vector2(text.preferredWidth, Toast.TOAST_HEIGHT);
                    text.transform.localPosition = new Vector3(Toast.TOAST_PADDING, 0, 0);
                    break;

                case ToastPosition.TopRight:
                    transform.parent.localPosition = new Vector3(1920 - Toast.TOAST_MARGIN, 1080 - Toast.TOAST_MARGIN, 0);
                    canvasRect.pivot = new Vector2(0.5f, 1f);
                    canvasRect.sizeDelta = new Vector2(text.preferredWidth + 2 * Toast.TOAST_PADDING, Toast.TOAST_HEIGHT);
                    bgRect.pivot = new Vector2(1f, 0.5f);
                    bgRect.sizeDelta = new Vector2(text.preferredWidth + 2 * Toast.TOAST_PADDING, Toast.TOAST_HEIGHT);
                    textRect.pivot = new Vector2(1f, 1 + Toast.TOAST_TEXT_PIVOT_FIX);
                    textRect.sizeDelta = new Vector2(text.preferredWidth, Toast.TOAST_HEIGHT);
                    text.transform.localPosition = new Vector3(-Toast.TOAST_PADDING, 0, 0);
                    break;

                case ToastPosition.BottomLeft:
                    transform.parent.localPosition = new Vector3(-1920 + Toast.TOAST_MARGIN, -1080 + Toast.TOAST_MARGIN, 0);
                    canvasRect.pivot = new Vector2(0.5f, 0);
                    canvasRect.sizeDelta = new Vector2(text.preferredWidth + 2 * Toast.TOAST_PADDING, Toast.TOAST_HEIGHT);
                    bgRect.pivot = new Vector2(0, 0.5f);
                    bgRect.sizeDelta = new Vector2(text.preferredWidth + 2 * Toast.TOAST_PADDING, Toast.TOAST_HEIGHT);
                    textRect.pivot = new Vector2(0, 0 + Toast.TOAST_TEXT_PIVOT_FIX);
                    textRect.sizeDelta = new Vector2(text.preferredWidth, Toast.TOAST_HEIGHT);
                    text.transform.localPosition = new Vector3(Toast.TOAST_PADDING, 0, 0);
                    break;

                case ToastPosition.BottomRight:
                    transform.parent.localPosition = new Vector3(1920 - Toast.TOAST_MARGIN, -1080 + Toast.TOAST_MARGIN, 0);
                    canvasRect.pivot = new Vector2(0.5f, 0);
                    canvasRect.sizeDelta = new Vector2(text.preferredWidth + 2 * Toast.TOAST_PADDING, Toast.TOAST_HEIGHT);
                    bgRect.pivot = new Vector2(1f, 0.5f);
                    bgRect.sizeDelta = new Vector2(text.preferredWidth + 2 * Toast.TOAST_PADDING, Toast.TOAST_HEIGHT);
                    textRect.pivot = new Vector2(1f, 0 + Toast.TOAST_TEXT_PIVOT_FIX);
                    textRect.sizeDelta = new Vector2(text.preferredWidth, Toast.TOAST_HEIGHT);
                    text.transform.localPosition = new Vector3(-Toast.TOAST_PADDING, 0, 0);
                    break;
            }
        }

        public void LateLoop()
        {
            if (!gameObject.activeSelf) return;
            if (!animator.IsInAnimation())
            {
                gameObject.SetActive(false);
                if (Toast.queue.Count > 0) Toast.queue.RemoveAt(0);
            }
        }
    }

    public struct ToastData
    {
        public I18nKey text;
        public float duration;
        public ToastPosition position;

        public override readonly int GetHashCode() => HashCode.Combine(text, duration, position);
        public override readonly bool Equals(object obj)
        {
            return obj is ToastData data &&
                   text.Equals(data.text) &&
                   duration == data.duration &&
                   position == data.position;
        }

    }

    public enum ToastPosition
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }
}
