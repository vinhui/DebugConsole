using UnityEngine;

namespace DebugConsole.UI
{
    public class DragDrop : MonoBehaviour
    {
        public RectTransform moveBox;

        private Vector2 offset;
        private bool dragging;

        private void Awake()
        {
            if (moveBox == null)
                Debug.LogError("MoveBox is null on DragDrop", this);
        }

        public void BeginDrag()
        {
            if (moveBox == null)
                return;

            offset = moveBox.position - Input.mousePosition;
            dragging = true;
        }

        private void Update()
        {
            if (moveBox == null)
                return;

            if (dragging)
                moveBox.position = (Vector2)Input.mousePosition + offset;
        }

        public void EndDrag()
        {
            dragging = false;
        }
    }
}