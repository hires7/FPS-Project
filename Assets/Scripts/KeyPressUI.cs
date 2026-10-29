using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public sealed class KeyPressUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI uiText;

    private readonly StringBuilder textBuilder = new(128);
    private string lastText;

    private void Awake()
    {
        if (uiText == null)
        {
            Debug.LogError("KeyPressUI requires a TextMeshProUGUI reference.", this);
            enabled = false;
        }
    }

    private void Update()
    {
        textBuilder.Clear();
        textBuilder.Append("Pressed Keys:\n");
        bool hasInput = false;

        if (Keyboard.current != null)
        {
            foreach (var key in Keyboard.current.allKeys)
            {
                if (!key.isPressed)
                {
                    continue;
                }

                AppendInput(key.displayName);
                hasInput = true;
            }
        }

        if (Mouse.current != null)
        {
            if (Mouse.current.leftButton.isPressed)
            {
                AppendInput("LMB");
                hasInput = true;
            }

            if (Mouse.current.rightButton.isPressed)
            {
                AppendInput("RMB");
                hasInput = true;
            }
        }

        string currentText = hasInput ? textBuilder.ToString() : "No keys pressed";
        if (currentText == lastText)
        {
            return;
        }

        lastText = currentText;
        uiText.SetText(currentText);
    }

    private void AppendInput(string displayName)
    {
        textBuilder.Append('[');
        textBuilder.Append(displayName);
        textBuilder.Append("] ");
    }
}
