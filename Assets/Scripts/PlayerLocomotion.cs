using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerCrouch))]
public class PlayerLocomotion : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float crouchSpeed = 3f;

    [Header("Jump")]
    [Min(0f)]
    public float jumpHeight = 1.2f;

    public float gravity = -30f;

    [Range(0f, 1f)]
    public float jumpCutMultiplier = 0.5f;

    [Min(1f)]
    public float fallGravityMultiplier = 1.8f;

    [Min(0.1f)]
    public float maxFallSpeed = 35f;

    [Header("Jump Forgiveness")]
    [Min(0f)]
    public float coyoteTime = 0.1f;

    [Min(0f)]
    public float jumpBufferTime = 0.12f;

    public bool IsCrouched => crouch != null && crouch.IsCrouched;
    public float CurrentSpeed { get; private set; }
    public bool IsMovingForward { get; private set; }
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

        CurrentSpeed = IsCrouched ? crouchSpeed : walkSpeed;
        IsMovingForward = input.MoveDirection.y > 0.1f;
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
            VerticalSpeed = -2f;
        }
    }
}