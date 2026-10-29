using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public sealed class MouseLook : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInput input;
    [SerializeField] private Transform playerBody;

    [Header("Look Settings")]
    [Min(0f)]
    [SerializeField] private float mouseSensitivity = 0.1f;
    [Min(0f)]
    [SerializeField] private float stickDegreesPerSecond = 180f;
    [SerializeField] private float minPitch = -90f;
    [SerializeField] private float maxPitch = 75f;

    private float pitch;
    private bool cursorLocked;

    private void Awake()
    {
        if (input == null || playerBody == null)
        {
            Debug.LogError("MouseLook references are not configured.", this);
            enabled = false;
            return;
        }

        pitch = NormalizeAngle(transform.localEulerAngles.x);
    }

    private void OnEnable()
    {
        SetCursorLocked(true);
    }

    private void OnDisable()
    {
        SetCursorLocked(false);
    }

    private void Update()
    {
        HandleCursorLock();
        if (!cursorLocked)
        {
            return;
        }

        Vector2 lookDelta = input.LookDirection;
        lookDelta *= input.LookUsesPointerDelta
            ? mouseSensitivity
            : stickDegreesPerSecond * Time.unscaledDeltaTime;

        pitch = Mathf.Clamp(pitch - lookDelta.y, minPitch, maxPitch);
        transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        playerBody.Rotate(Vector3.up, lookDelta.x, Space.Self);
    }

    private void HandleCursorLock()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            SetCursorLocked(false);
        }
        else if (!cursorLocked && Mouse.current != null
            && Mouse.current.leftButton.wasPressedThisFrame)
        {
            SetCursorLocked(true);
        }
    }

    private void SetCursorLocked(bool isLocked)
    {
        cursorLocked = isLocked;
        Cursor.lockState = isLocked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !isLocked;
    }

    private void OnValidate()
    {
        mouseSensitivity = Mathf.Max(0f, mouseSensitivity);
        stickDegreesPerSecond = Mathf.Max(0f, stickDegreesPerSecond);
        if (minPitch > maxPitch)
        {
            (minPitch, maxPitch) = (maxPitch, minPitch);
        }
    }

    private static float NormalizeAngle(float angle)
    {
        return angle > 180f ? angle - 360f : angle;
    }
}
