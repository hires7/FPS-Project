using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{

    [Header("Camera Forward Offset")]
    public Transform actualCameraTransform;
    public float runningCamForwardOffset = 0.2f;
    public float crouchingCamForwardOffset = 0.35f;
    public float camOffsetSmoothSpeed = 8f;
    public float walkSpeed = 5f;
    public float crouchSpeed = 3f;
    public float gravity = -9.81f;
    public Animator animator; 
    public InputAction crouchAction; 
    private bool isCrouched = false;

    public Transform cameraTransform;
    public float standingHeight = 0.96f;
    public float crouchingHeight = 0.1f;
    public float cameraSmoothSpeed = 10f;
        
    public InputAction moveAction;

    private CharacterController controller;
    private Vector3 velocity;

    void OnEnable()
    {
        moveAction.Enable();
        crouchAction.Enable();
    }

    void OnDisable()
    {
        moveAction.Disable();
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        Vector2 inputDirection = moveAction.ReadValue<Vector2>();

        Vector3 move = transform.right * inputDirection.x + transform.forward * inputDirection.y;

        float speed = isCrouched ? crouchSpeed : walkSpeed;

        controller.Move(move * speed * Time.deltaTime);

        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        float realMovementSpeed = inputDirection.magnitude * speed;
        animator.SetFloat("Speed", realMovementSpeed);

        if (crouchAction.WasPressedThisFrame())
        {
            isCrouched = !isCrouched;
            animator.SetBool("IsCrouching", isCrouched);
            
        }

        float targetHeight = isCrouched ? crouchingHeight : standingHeight;

        Vector3 currentCamPos = cameraTransform.localPosition;

        currentCamPos.y = Mathf.Lerp(currentCamPos.y, targetHeight, cameraSmoothSpeed * Time.deltaTime);

        cameraTransform.localPosition = currentCamPos;

        bool isMovingForward = inputDirection.y > 0.1f;
        float targetZOffset = 0f;

        if (isCrouched)
        {
            targetZOffset = crouchingCamForwardOffset;
        }
        else if (isMovingForward)
        {
            targetZOffset = runningCamForwardOffset;
        }

        Vector3 currentActualCamPos = actualCameraTransform.localPosition;
        currentActualCamPos.z = Mathf.Lerp(currentActualCamPos.z, targetZOffset, camOffsetSmoothSpeed * Time.deltaTime);

        actualCameraTransform.localPosition = currentActualCamPos;
    }
}