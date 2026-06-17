using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))] // Kód vyžaduje náš nový input skript
public class PlayerLocomotion : MonoBehaviour
{
    [Header("Speeds")]
    public float walkSpeed = 5f;
    public float crouchSpeed = 3f;
    public float gravity = -9.81f;

    public bool IsCrouched { get; private set; }
    public float CurrentSpeed { get; private set; }
    public bool IsMovingForward { get; private set; }

    private CharacterController controller;
    private PlayerInput input;
    private Vector3 velocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        input = GetComponent<PlayerInput>(); // Získame referenciu na vstupy
    }

    void Update()
    {
        HandleState();
        HandleMovement();
        HandleGravity();
    }

    private void HandleState()
    {
        if (input.CrouchTriggered)
        {
            IsCrouched = !IsCrouched;
        }

        CurrentSpeed = IsCrouched ? crouchSpeed : walkSpeed;
        IsMovingForward = input.MoveDirection.y > 0.1f;
    }

    private void HandleMovement()
    {
        Vector3 move = transform.right * input.MoveDirection.x + transform.forward * input.MoveDirection.y;
        controller.Move(move * CurrentSpeed * Time.deltaTime);
    }

    private void HandleGravity()
    {
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}