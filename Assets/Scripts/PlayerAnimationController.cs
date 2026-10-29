using UnityEngine;

// Animator dostane stav až po aktualizácii pohybu.
[DefaultExecutionOrder(100)]
public class PlayerAnimationController : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    public PlayerLocomotion locomotion;
    public PlayerInput input;

    void Update()
    {
        float movementSpeed = Mathf.Min(input.MoveDirection.magnitude, 1f) * locomotion.CurrentSpeed;

        animator.SetFloat("Speed", movementSpeed);
        animator.SetBool("IsCrouching", locomotion.IsCrouched);
        animator.SetBool("IsGrounded", locomotion.IsGrounded);
    }
}