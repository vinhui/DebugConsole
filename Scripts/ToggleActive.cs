using UnityEngine;

namespace DebugConsole
{
    public class ToggleActive : MonoBehaviour
    {
        public void Toggle()
        {
            gameObject.SetActive(!gameObject.activeSelf);
        }
    }
}