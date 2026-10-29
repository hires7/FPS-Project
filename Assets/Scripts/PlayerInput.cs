using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-100)]
public class PlayerInput : MonoBehaviour
{
    public InputAction moveAction;
    public InputAction crouchAction;
    public InputAction jumpAction;

    public Vector2 MoveDirection { get; private set; }
    public bool CrouchTriggered { get; private set; }
    public bool JumpTriggered { get; private set; }
    public bool JumpHeld { get; private set; }

    void OnEnable()
    {
        moveAction.Enable();
        crouchAction.Enable();
        jumpAction.Enable();
    }

    void OnDisable()
    {
        moveAction.Disable();
        crouchAction.Disable();
        jumpAction.Disable();

        MoveDirection = Vector2.zero;
        CrouchTriggered = false;
        JumpTriggered = false;
        JumpHeld = false;
    }

    void Update()
    {
        MoveDirection = moveAction.ReadValue<Vector2>();
        CrouchTriggered = crouchAction.WasPressedThisFrame();
        JumpTriggered = jumpAction.WasPressedThisFrame();
        JumpHeld = jumpAction.IsPressed();
    }
}