using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    public InputAction moveAction;
    public InputAction crouchAction;

    public Vector2 MoveDirection { get; private set; }
    public bool CrouchTriggered { get; private set; }

    void OnEnable()
    {
        moveAction.Enable();
        crouchAction.Enable();
    }

    void OnDisable()
    {
        moveAction.Disable();
        crouchAction.Disable();
    }

    void Update()
    {
        MoveDirection = moveAction.ReadValue<Vector2>();
        CrouchTriggered = crouchAction.WasPressedThisFrame();
    }
}