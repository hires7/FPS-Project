using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CharacterController), typeof(Health))]
public sealed class EnemyWalker : MonoBehaviour
{
    [SerializeField] private Animator animator;

    [Header("Patrol")]
    [Min(0.1f)]
    [SerializeField] private float walkSpeed = 1.6f;

    [SerializeField] private Vector3 patrolOffset = new(0f, 0f, 7f);

    [Min(0f)]
    [SerializeField] private float waitAtEnd = 0.75f;

    [Min(1f)]
    [SerializeField] private float turnSpeed = 360f;

    [Min(0.01f)]
    [SerializeField] private float arrivalDistance = 0.1f;

    [Header("Animation")]
    [Range(0f, 1f)]
    [SerializeField] private float walkingAnimationSpeed = 0.55f;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsCrouchingHash = Animator.StringToHash("IsCrouching");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");

    private CharacterController controller;
    private Health health;
    private Vector3 pointA;
    private Vector3 pointB;
    private Vector3 target;
    private float verticalVelocity;
    private float waitTimer;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        health = GetComponent<Health>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        pointA = transform.position;
        pointB = pointA + transform.TransformVector(patrolOffset);
        target = pointB;

        health.Died += OnDied;
    }

    private void OnDestroy()
    {
        if (health != null)
        {
            health.Died -= OnDied;
        }
    }

    private void Update()
    {
        if (health.IsDead)
        {
            return;
        }

        Vector3 toTarget = target - transform.position;
        toTarget.y = 0f;

        bool isWaiting = waitTimer > 0f;

        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
        }
        else if (toTarget.sqrMagnitude <= arrivalDistance * arrivalDistance)
        {
            target = target == pointA ? pointB : pointA;
            waitTimer = waitAtEnd;
            isWaiting = true;
        }

        Vector3 horizontalVelocity = Vector3.zero;

        if (!isWaiting && toTarget.sqrMagnitude > 0.0001f)
        {
            Vector3 direction = toTarget.normalized;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                turnSpeed * Time.deltaTime
            );
            horizontalVelocity = direction * walkSpeed;
        }

        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }
        else
        {
            verticalVelocity += Physics.gravity.y * Time.deltaTime;
        }

        Vector3 velocity = horizontalVelocity
            + Vector3.up * verticalVelocity;
        controller.Move(velocity * Time.deltaTime);

        if (animator != null)
        {
            animator.SetFloat(
                SpeedHash,
                horizontalVelocity.sqrMagnitude > 0f
                    ? walkingAnimationSpeed
                    : 0f,
                0.12f,
                Time.deltaTime
            );
            animator.SetBool(IsCrouchingHash, false);
            animator.SetBool(IsGroundedHash, controller.isGrounded);
        }
    }

    private void OnDied(Health _)
    {
        if (animator != null)
        {
            animator.SetFloat(SpeedHash, 0f);
        }

        enabled = false;
    }

    private void OnValidate()
    {
        walkSpeed = Mathf.Max(0.1f, walkSpeed);
        waitAtEnd = Mathf.Max(0f, waitAtEnd);
        turnSpeed = Mathf.Max(1f, turnSpeed);
        arrivalDistance = Mathf.Max(0.01f, arrivalDistance);
    }
}
