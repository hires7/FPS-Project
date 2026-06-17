using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("References")]
    public PlayerLocomotion locomotion;
    public Transform cameraRoot;
    public Transform actualCamera;

    [Header("Height Settings")]
    public float standingHeight = 0.96f;
    public float crouchingHeight = 0.1f;
    public float heightSmoothSpeed = 10f;

    [Header("Forward Offset Settings")]
    public float runningForwardOffset = 0.2f;
    public float crouchingForwardOffset = 0.35f;
    public float offsetSmoothSpeed = 8f;

    void Update()
    {
        float targetHeight = locomotion.IsCrouched ? crouchingHeight : standingHeight;
        Vector3 rootPos = cameraRoot.localPosition;
        rootPos.y = Mathf.Lerp(rootPos.y, targetHeight, heightSmoothSpeed * Time.deltaTime);
        cameraRoot.localPosition = rootPos;

        float targetZOffset = 0f;
        if (locomotion.IsCrouched)
        {
            targetZOffset = crouchingForwardOffset;
        }
        else if (locomotion.IsMovingForward)
        {
            targetZOffset = runningForwardOffset;
        }

        Vector3 camPos = actualCamera.localPosition;
        camPos.z = Mathf.Lerp(camPos.z, targetZOffset, offsetSmoothSpeed * Time.deltaTime);
        actualCamera.localPosition = camPos;
    }
}