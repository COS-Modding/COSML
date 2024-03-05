using System;
using System.Collections;
using UnityEngine;

namespace COSML.Components.Toast
{
    public class UIToastAnimator : MonoBehaviour
    {
        private const float DEFAULT_DURATION_TIME = 5f;

        public CanvasGroup canvas;
        public float durationTime = DEFAULT_DURATION_TIME;

        private const float MOVE_TIME = 0.3f;
        private const float MOVE_DISTANCE = 200f;

        private bool isPlaying;
        private float moveDistance = MOVE_DISTANCE;
        public bool IsInAnimation() => isPlaying;

        public void Init()
        {
            if (canvas == null) canvas = gameObject.GetComponent<CanvasGroup>();
            canvas.transform.localPosition = new Vector3(0, -MOVE_DISTANCE, 0);
        }

        public void Show(ToastPosition position = ToastPosition.BottomRight)
        {
            moveDistance = Math.Abs(moveDistance);
            if ((byte)position < 2) moveDistance *= -1;
            StartCoroutine(Play());
        }

        private IEnumerator Play()
        {
            isPlaying = true;
            StartCoroutine(MoveIn());
            yield return new WaitForSeconds(MOVE_TIME + durationTime);
            StartCoroutine(MoveOut());
            yield return new WaitForSeconds(MOVE_TIME);
            isPlaying = false;
            yield return null;
        }

        private IEnumerator MoveIn()
        {
            for (float t = 0; t <= MOVE_TIME; t += Time.deltaTime)
            {
                canvas.transform.localPosition = new Vector3(0, ((float)(1 - Math.Pow(1 - (t / MOVE_TIME), 3))) * moveDistance - moveDistance, 0);
                yield return null;
            }
            canvas.transform.localPosition = new Vector3(0, 0, 0);
            yield return null;
        }

        private IEnumerator MoveOut()
        {
            for (float t = MOVE_TIME; t >= 0; t -= Time.deltaTime)
            {
                canvas.transform.localPosition = new Vector3(0, (float)(Math.Pow(t / MOVE_TIME, 3) * moveDistance - moveDistance), 0);
                yield return null;
            }
            canvas.transform.localPosition = new Vector3(0, -moveDistance, 0);
            yield return null;
        }
    }
}
