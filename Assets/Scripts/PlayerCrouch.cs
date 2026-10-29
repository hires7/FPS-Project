using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerCrouch : MonoBehaviour
{
    [Min(0.1f)]
    public float crouchingHeight = 1.2f;

    [Min(0f)]
    public float clearance = 0.02f;

    public LayerMask obstructionMask = ~0;

    public bool IsCrouched { get; private set; }

    private CharacterController controller;
    private float standingHeight;
    private Vector3 standingCenter;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        standingHeight = controller.height;
        standingCenter = controller.center;
    }

    public bool TryToggle()
    {
        if (IsCrouched && !CanStand())
        {
            return false;
        }

        IsCrouched = !IsCrouched;

        float targetHeight = IsCrouched
            ? Mathf.Clamp(
                crouchingHeight,
                controller.radius * 2f,
                standingHeight
            )
            : standingHeight;

        Vector3 targetCenter = standingCenter;
        targetCenter.y -= (standingHeight - targetHeight) * 0.5f;

        controller.height = targetHeight;
        controller.center = targetCenter;

        return true;
    }

    private bool CanStand()
    {
        if (controller.height >= standingHeight)
        {
            return true;
        }

        float radius = controller.radius;
        float margin = Mathf.Clamp(clearance, 0f, radius * 0.25f);

        Vector3 lowerPoint = transform.TransformPoint(
            controller.center
            + Vector3.up * (controller.height * 0.5f - radius)
        );

        Vector3 upperPoint = transform.TransformPoint(
            standingCenter
            + Vector3.up * (standingHeight * 0.5f - radius)
        );

        Collider[] overlaps = Physics.OverlapCapsule(
            lowerPoint,
            upperPoint,
            radius - margin,
            obstructionMask,
            QueryTriggerInteraction.Ignore
        );

        foreach (Collider overlap in overlaps)
        {
            if (overlap == controller
                || overlap.transform.IsChildOf(transform))
            {
                continue;
            }

            return false;
        }

        return true;
    }
}