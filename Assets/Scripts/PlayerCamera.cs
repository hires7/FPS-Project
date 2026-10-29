using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerCamera : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerLocomotion locomotion;
    [SerializeField] private Transform cameraRoot;
    [SerializeField] private Transform actualCamera;

    [Header("Height Settings")]
    [SerializeField] private float standingHeight = 0.96f;
    [SerializeField] private float crouchingHeight = 0.1f;
    [Min(0f)]
    [SerializeField] private float heightSmoothSpeed = 10f;

    [Header("Forward Offset Settings")]
    [SerializeField] private float movingForwardOffset = 0.2f;
    [SerializeField] private float sprintingForwardOffset = 0.3f;
    [SerializeField] private float crouchingForwardOffset = 0.35f;
    [Min(0f)]
    [SerializeField] private float offsetSmoothSpeed = 8f;

    private void Awake()
    {
        if (locomotion == null || cameraRoot == null || actualCamera == null)
        {
            Debug.LogError("PlayerCamera references are not configured.", this);
            enabled = false;
        }
    }

    private void LateUpdate()
    {
        float heightTarget = locomotion.IsCrouched ? crouchingHeight : standingHeight;
        float heightBlend = DampFactor(heightSmoothSpeed);
        Vector3 rootPosition = cameraRoot.localPosition;
        rootPosition.y = Mathf.Lerp(rootPosition.y, heightTarget, heightBlend);
        cameraRoot.localPosition = rootPosition;

        float offsetTarget = 0f;
        if (locomotion.IsCrouched)
        {
            offsetTarget = crouchingForwardOffset;
        }
        else if (locomotion.IsSprinting)
        {
            offsetTarget = sprintingForwardOffset;
        }
        else if (locomotion.IsMovingForward)
        {
            offsetTarget = movingForwardOffset;
        }

        float offsetBlend = DampFactor(offsetSmoothSpeed);
        Vector3 cameraPosition = actualCamera.localPosition;
        cameraPosition.z = Mathf.Lerp(cameraPosition.z, offsetTarget, offsetBlend);
        actualCamera.localPosition = cameraPosition;
    }

    private static float DampFactor(float speed)
    {
        return 1f - Mathf.Exp(-speed * Time.deltaTime);
    }
}
