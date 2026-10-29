using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-100)]
[DisallowMultipleComponent]
public sealed class PlayerInput : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionAsset actionsAsset;
    [SerializeField] private string actionMapName = "Player";

    public Vector2 MoveDirection { get; private set; }
    public Vector2 LookDirection { get; private set; }
    public bool LookUsesPointerDelta { get; private set; }
    public bool CrouchTriggered { get; private set; }
    public bool JumpTriggered { get; private set; }
    public bool JumpHeld { get; private set; }
    public bool SprintHeld { get; private set; }
    public bool FireTriggered { get; private set; }
    public bool FireHeld { get; private set; }

    private InputActionMap actionMap;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction crouchAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private InputAction fireAction;

    private void Awake()
    {
        if (!ResolveActions())
        {
            enabled = false;
        }
    }

    private void OnEnable()
    {
        actionMap?.Enable();
    }

    private void OnDisable()
    {
        actionMap?.Disable();
        ResetState();
    }

    private void Update()
    {
        MoveDirection = moveAction.ReadValue<Vector2>();
        LookDirection = lookAction.ReadValue<Vector2>();
        LookUsesPointerDelta = lookAction.activeControl?.device is Pointer;
        CrouchTriggered = crouchAction.WasPressedThisFrame();
        JumpTriggered = jumpAction.WasPressedThisFrame();
        JumpHeld = jumpAction.IsPressed();
        SprintHeld = sprintAction.IsPressed();
        FireTriggered = fireAction.WasPressedThisFrame();
        FireHeld = fireAction.IsPressed();
    }

    private bool ResolveActions()
    {
        if (actionsAsset == null)
        {
            Debug.LogError("PlayerInput requires an InputActionAsset.", this);
            return false;
        }

        actionMap = actionsAsset.FindActionMap(actionMapName, false);

        if (actionMap == null)
        {
            Debug.LogError(
                $"Input action map '{actionMapName}' was not found.",
                this
            );
            return false;
        }

        moveAction = actionMap.FindAction("Move", false);
        lookAction = actionMap.FindAction("Look", false);
        crouchAction = actionMap.FindAction("Crouch", false);
        jumpAction = actionMap.FindAction("Jump", false);
        sprintAction = actionMap.FindAction("Sprint", false);
        fireAction = actionMap.FindAction("Fire", false);

        if (moveAction == null || lookAction == null
            || crouchAction == null || jumpAction == null
            || sprintAction == null || fireAction == null)
        {
            Debug.LogError(
                $"Input map '{actionMapName}' must contain Move, Look, Crouch, Jump, Sprint and Fire actions.",
                this
            );
            return false;
        }

        return true;
    }

    private void ResetState()
    {
        MoveDirection = Vector2.zero;
        LookDirection = Vector2.zero;
        LookUsesPointerDelta = false;
        CrouchTriggered = false;
        JumpTriggered = false;
        JumpHeld = false;
        SprintHeld = false;
        FireTriggered = false;
        FireHeld = false;
    }
}