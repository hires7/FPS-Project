using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerCrouch))]
[DisallowMultipleComponent]
public sealed class PlayerLocomotion : MonoBehaviour
{
    [Header("Movement")]
    [Min(0f)]
    [SerializeField] private float walkSpeed = 5f;

    [Min(0f)]
    [SerializeField] private float sprintSpeed = 8f;

    [Min(0f)]
    [SerializeField] private float crouchSpeed = 3f;

    [Header("Jump")]
    [Min(0f)]
    [SerializeField] private float jumpHeight = 1.2f;

    [SerializeField] private float gravity = -30f;

    [Range(0f, 1f)]
    [SerializeField] private float jumpCutMultiplier = 0.5f;

    [Min(1f)]
    [SerializeField] private float fallGravityMultiplier = 1.8f;

    [Min(0.1f)]
    [SerializeField] private float maxFallSpeed = 35f;

    [Header("Jump Forgiveness")]
    [Min(0f)]
    [SerializeField] private float coyoteTime = 0.1f;

    [Min(0f)]
    [SerializeField] private float jumpBufferTime = 0.12f;

    public bool IsCrouched => crouch != null && crouch.IsCrouched;
    public float CurrentSpeed { get; private set; }
    public bool IsMovingForward { get; private set; }
    public bool IsSprinting { get; private set; }
    public bool IsGrounded { get; private set; }
    public float VerticalSpeed { get; private set; }

    private CharacterController controller;
    private PlayerInput input;

    private float lastGroundedTime = float.NegativeInfinity;
    private float lastJumpPressedTime = float.NegativeInfinity;
    private PlayerCrouch crouch;
    private bool jumpCutApplied;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        input = GetComponent<PlayerInput>();
        crouch = GetComponent<PlayerCrouch>();
    }

    void Update()
    {
        HandleState();
        UpdateGroundState();
        HandleJump();
        HandleVerticalSpeed();
        HandleMovement();
    }

    private void HandleState()
    {
        if (input.CrouchTriggered)
        {
            crouch.TryToggle();
        }

        IsMovingForward = input.MoveDirection.y > 0.1f;
        IsSprinting = !IsCrouched && IsMovingForward && input.SprintHeld;
        CurrentSpeed = IsCrouched
            ? crouchSpeed
            : IsSprinting ? sprintSpeed : walkSpeed;
    }

    private void UpdateGroundState()
    {
        IsGrounded = controller.isGrounded
            && VerticalSpeed <= 0f;

        if (IsGrounded)
        {
            lastGroundedTime = Time.time;
            VerticalSpeed = -2f;
        }
    }

    private void HandleJump()
    {
        if (input.JumpTriggered)
        {
            lastJumpPressedTime = Time.time;
        }

        bool hasBufferedJump =
            Time.time - lastJumpPressedTime <= jumpBufferTime;

        bool canUseGroundJump =
            Time.time - lastGroundedTime <= coyoteTime;

        if (!hasBufferedJump
            || !canUseGroundJump
            || IsCrouched
            || gravity >= 0f
            || jumpHeight <= 0f)
        {
            return;
        }

        VerticalSpeed = Mathf.Sqrt(-2f * gravity * jumpHeight);

        IsGrounded = false;
        jumpCutApplied = false;

        // Spotrebujeme vstup aj možnosť skoku zo zeme.
        // Tým zabránime druhému skoku v rámci coyote time.
        lastJumpPressedTime = float.NegativeInfinity;
        lastGroundedTime = float.NegativeInfinity;
    }

    private void HandleVerticalSpeed()
    {
        // Pustenie Space skráti stúpanie iba raz.
        if (!input.JumpHeld
            && VerticalSpeed > 0f
            && !jumpCutApplied)
        {
            VerticalSpeed *= jumpCutMultiplier;
            jumpCutApplied = true;
        }

        float gravityMultiplier =
            VerticalSpeed < 0f ? fallGravityMultiplier : 1f;

        VerticalSpeed +=
            gravity * gravityMultiplier * Time.deltaTime;

        VerticalSpeed = Mathf.Max(
            VerticalSpeed,
            -maxFallSpeed
        );
    }

    private void HandleMovement()
    {
        Vector2 moveInput = Vector2.ClampMagnitude(
            input.MoveDirection,
            1f
        );

        Vector3 horizontalMove =
            transform.right * moveInput.x
            + transform.forward * moveInput.y;

        Vector3 motion =
            horizontalMove * CurrentSpeed
            + Vector3.up * VerticalSpeed;

        CollisionFlags collisions = controller.Move(
            motion * Time.deltaTime
        );

        if ((collisions & CollisionFlags.Above) != 0
            && VerticalSpeed > 0f)
        {
            VerticalSpeed = 0f;
        }

        IsGrounded =
            (collisions & CollisionFlags.Below) != 0
            && VerticalSpeed <= 0f;

        if (IsGrounded)
        {
            lastGroundedTime = Time.time;
            VerticalSpeed = -2f;
        }
    }

    private void OnValidate()
    {
        walkSpeed = Mathf.Max(0f, walkSpeed);
        sprintSpeed = Mathf.Max(walkSpeed, sprintSpeed);
        crouchSpeed = Mathf.Clamp(crouchSpeed, 0f, walkSpeed);
        gravity = Mathf.Min(-0.01f, gravity);
        maxFallSpeed = Mathf.Max(0.1f, maxFallSpeed);
    }
}
