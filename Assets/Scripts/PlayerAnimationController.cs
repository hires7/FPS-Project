using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    public PlayerLocomotion locomotion;
    public PlayerInput input;

    void Update()
    {

        float realMovementSpeed = input.MoveDirection.magnitude * locomotion.CurrentSpeed;
        
        animator.SetFloat("Speed", realMovementSpeed);

        animator.SetBool("IsCrouching", locomotion.IsCrouched);
    }
}