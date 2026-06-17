using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class KeyPressUI : MonoBehaviour
{
    public TextMeshProUGUI uiText;

    void Update()
    {
        string pressedKeys = "Pressed Keys: \n";
        bool isAnyKeyPressed = false;

        if (Keyboard.current != null)
        {
            foreach (var key in Keyboard.current.allKeys)
            {
                if (key.isPressed)
                {
                    pressedKeys += "[" + key.displayName + "] ";
                    isAnyKeyPressed = true;
                }
            }
        }

        if (Mouse.current != null)
        {
            if (Mouse.current.leftButton.isPressed) 
            { 
                pressedKeys += "[LMB] "; 
                isAnyKeyPressed = true; 
            }
            if (Mouse.current.rightButton.isPressed) 
            { 
                pressedKeys += "[RMB] "; 
                isAnyKeyPressed = true; 
            }
        }

        if (isAnyKeyPressed)
        {
            uiText.text = pressedKeys;
        }
        else
        {
            uiText.text = "No keys pressed";
        }
    }
}