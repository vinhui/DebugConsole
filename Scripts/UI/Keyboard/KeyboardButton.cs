using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DebugConsole.UI.Keyboard
{
    [RequireComponent(typeof(Button))]
    public class KeyboardButton : MonoBehaviour
    {
        public TMP_InputField inputField;
        public string character;

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(OnButtonClick);
            if (string.IsNullOrEmpty(character))
                character = GetComponentInChildren<TextMeshProUGUI>().text;
        }

        private void OnButtonClick()
        {
            inputField.text += character;
            DebugConsole.DoAutoCompletion();
        }
    }
}