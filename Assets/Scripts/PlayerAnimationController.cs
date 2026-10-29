using UnityEngine;

// Animator receives state after input and locomotion have updated.
[DefaultExecutionOrder(100)]
[DisallowMultipleComponent]
public sealed class PlayerAnimationController : MonoBehaviour
{
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsCrouchingHash = Animator.StringToHash("IsCrouching");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerLocomotion locomotion;
    [SerializeField] private PlayerInput input;

    private void Awake()
    {
        if (animator == null || locomotion == null || input == null)
        {
            Debug.LogError("PlayerAnimationController references are not configured.", this);
            enabled = false;
        }
    }

    private void Update()
    {
        float normalizedSpeed = Mathf.Clamp01(input.MoveDirection.magnitude);

        animator.SetFloat(SpeedHash, normalizedSpeed);
        animator.SetBool(IsCrouchingHash, locomotion.IsCrouched);
        animator.SetBool(IsGroundedHash, locomotion.IsGrounded);
    }
}
