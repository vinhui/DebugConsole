using UnityEngine;

namespace DebugConsole.UI
{
    public class Resize : MonoBehaviour
    {
        public RectTransform resizeBox;
        public Vector2 minSize;
        private Vector2 maxSize;

        private Vector2 offset;
        private bool dragging;

        private void Awake()
        {
            if (resizeBox == null)
                Debug.LogError("ResizeBox is null on Resize", this);

            maxSize = new Vector2(Screen.width, Screen.height);
        }

        public void BeginDrag()
        {
            if (resizeBox == null)
                return;

            offset = resizeBox.sizeDelta - new Vector2(Input.mousePosition.x, -Input.mousePosition.y);
            dragging = true;
        }

        private void Update()
        {
            if (resizeBox == null)
                return;

            if (dragging)
            {
                Vector2 newSize = new Vector2(Input.mousePosition.x, -Input.mousePosition.y) + offset;
                newSize = ClampVector2(newSize, minSize, maxSize);
                resizeBox.sizeDelta = newSize;
            }
        }

        public void EndDrag()
        {
            dragging = false;
        }

        private Vector2 ClampVector2(Vector2 toClamp, Vector2 min, Vector2 max)
        {
            return new Vector2(Mathf.Clamp(toClamp.x, min.x, max.x), Mathf.Clamp(toClamp.y, min.y, max.y));
        }
    }
}