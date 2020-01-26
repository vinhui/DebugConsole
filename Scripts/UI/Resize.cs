using UnityEngine;

namespace DebuggingConsole.UI
{
    public class Resize : MonoBehaviour
    {
        public RectTransform resizeBox;
        public Vector2 minSize;
        private Vector2 maxSize;

        public bool resizeHorizontal = true;
        public bool resizeVertical = true;

        private bool dragging;
        private Vector2 startSize;
        private Vector2 startMousePos;

        private void Awake()
        {
            if (resizeBox == null)
                Debug.LogError("ResizeBox is null on Resize", this);

            var oldPivot = resizeBox.pivot;
            var newPivot = new Vector2(0, 1);
            resizeBox.localPosition +=
                (Vector3) ((newPivot - oldPivot) * resizeBox.lossyScale * resizeBox.sizeDelta);
            resizeBox.pivot = newPivot;

            maxSize = new Vector2(Screen.width, Screen.height);
        }

        public void BeginDrag()
        {
            if (resizeBox == null)
                return;

            startSize = resizeBox.sizeDelta;
            startMousePos = Input.mousePosition;
            dragging = true;
        }

        private void Update()
        {
            if (resizeBox == null)
                return;

            if (dragging)
            {
                var mouseOffset = startMousePos - (Vector2) Input.mousePosition;

                var newSizeX = startSize.x - mouseOffset.x;
                var newSizeY = startSize.y + mouseOffset.y;
                newSizeX = Mathf.Clamp(newSizeX, minSize.x, maxSize.x);
                newSizeY = Mathf.Clamp(newSizeY, minSize.y, maxSize.y);
                if (resizeHorizontal)
                    resizeBox.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newSizeX);
                if (resizeVertical)
                    resizeBox.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newSizeY);
            }
        }

        public void EndDrag()
        {
            dragging = false;
        }
    }
}