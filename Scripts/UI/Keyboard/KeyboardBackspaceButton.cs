using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DebuggingConsole.UI.Keyboard
{
    public class KeyboardBackspaceButton : MonoBehaviour
    {
        public TMP_InputField inputField;

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(OnButtonClick);
        }

        private void OnButtonClick()
        {
            if (inputField.text.Length > 0)
                inputField.text = inputField.text.Remove(inputField.text.Length - 1);
        }
    }
}