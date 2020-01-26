using System.Collections;
using UnityEngine;

namespace DebuggingConsole.UI
{
    public class ToggleContent : MonoBehaviour
    {
        public RectTransform header;
        public RectTransform content;
        public CanvasGroup contentGroup;

        public float resizeTime = .1f;
        public AnimationCurve resizeCurve = AnimationCurve.Linear(0, 0, 1, 1);

        private RectTransform rectTransform;
        private bool hidden;
        private Vector2 prevSize;

        private void Awake()
        {
            rectTransform = transform as RectTransform;
        }

        public void Toggle()
        {
            StartCoroutine(hidden ? Show() : Hide());
        }

        private IEnumerator Show()
        {
            hidden = false;
            content.gameObject.SetActive(true);

            float timer = 0;
            var startSize = rectTransform.sizeDelta;

            while (timer < resizeTime)
            {
                rectTransform.sizeDelta = SetTransition(timer / resizeTime, startSize, prevSize);
                timer += Time.deltaTime;
                yield return null;
            }
            rectTransform.sizeDelta = SetTransition(1f, startSize, prevSize);
        }

        private IEnumerator Hide()
        {
            hidden = true;
            prevSize = rectTransform.sizeDelta;

            float timer = 0;
            var startSize = prevSize;
            var targetSize = new Vector2(prevSize.x, header.sizeDelta.y);

            while (timer < resizeTime)
            {
                rectTransform.sizeDelta = SetTransition(1f - timer / resizeTime, targetSize, startSize);
                timer += Time.deltaTime;
                yield return null;
            }
            rectTransform.sizeDelta = SetTransition(0f, targetSize, startSize);

            content.gameObject.SetActive(false);
        }

        private Vector2 SetTransition(float t, Vector2 a, Vector2 b)
        {
            t = resizeCurve.Evaluate(t);
            contentGroup.alpha = Mathf.Clamp01(t);
            return Vector2.Lerp(a, b, t);
        }
    }
}