using UnityEngine;

[DefaultExecutionOrder(200)]
[RequireComponent(typeof(PlayerInput))]
[DisallowMultipleComponent]
public sealed class PlayerShooting : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera aimCamera;

    [Header("Weapon")]
    [Min(0.1f)]
    [SerializeField] private float damage = 25f;

    [Min(0.1f)]
    [SerializeField] private float range = 100f;

    [Min(0.1f)]
    [SerializeField] private float shotsPerSecond = 4f;

    [SerializeField] private bool automaticFire;
    [SerializeField] private LayerMask hitMask = Physics.DefaultRaycastLayers;

    [Header("Stealth")]
    [SerializeField] private bool suppressed;

    [Min(0f)]
    [SerializeField] private float gunshotNoiseRadius = 35f;

    [Min(0f)]
    [SerializeField] private float suppressedNoiseRadius = 8f;

    [Min(0f)]
    [SerializeField] private float impactNoiseRadius = 4f;

    private PlayerInput input;
    private float nextShotTime;
    private bool hadGameplayControl;
    private bool waitForFireRelease;

    private void Awake()
    {
        input = GetComponent<PlayerInput>();

        if (aimCamera == null)
        {
            aimCamera = Camera.main;
        }

        if (aimCamera == null)
        {
            Debug.LogError(
                "PlayerShooting requires an aim camera.",
                this
            );
            enabled = false;
        }
    }

    private void OnEnable()
    {
        hadGameplayControl = false;
        waitForFireRelease = true;
    }

    private void LateUpdate()
    {
        bool hasGameplayControl =
            Application.isFocused
            && input.isActiveAndEnabled
            && Cursor.lockState == CursorLockMode.Locked
            && Time.timeScale > 0f;

        if (!hasGameplayControl)
        {
            hadGameplayControl = false;
            waitForFireRelease = true;
            return;
        }

        if (!hadGameplayControl)
        {
            hadGameplayControl = true;
            waitForFireRelease = input.FireHeld || input.FireTriggered;
            return;
        }

        if (waitForFireRelease)
        {
            if (!input.FireHeld && !input.FireTriggered)
            {
                waitForFireRelease = false;
            }

            return;
        }

        bool wantsToFire = automaticFire
            ? input.FireHeld
            : input.FireTriggered;

        if (!wantsToFire || Time.time < nextShotTime)
        {
            return;
        }

        Fire();
    }

    private void Fire()
    {
        nextShotTime = Time.time
            + 1f / Mathf.Max(0.1f, shotsPerSecond);

        float noiseRadius = suppressed
            ? suppressedNoiseRadius
            : gunshotNoiseRadius;

        NoiseSystem.Emit(
            transform.position,
            noiseRadius,
            gameObject,
            NoiseKind.Gunshot
        );

        Ray ray = aimCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f)
        );

        if (!Physics.Raycast(
            ray,
            out RaycastHit hit,
            range,
            hitMask,
            QueryTriggerInteraction.Collide
        ))
        {
            return;
        }

        NoiseSystem.Emit(
            hit.point,
            impactNoiseRadius,
            gameObject,
            NoiseKind.Impact
        );

        IDamageable target =
            hit.collider.GetComponentInParent<IDamageable>();

        target?.TakeDamage(damage);
    }

    private void OnValidate()
    {
        damage = Mathf.Max(0.1f, damage);
        range = Mathf.Max(0.1f, range);
        shotsPerSecond = Mathf.Max(0.1f, shotsPerSecond);
        gunshotNoiseRadius = Mathf.Max(0f, gunshotNoiseRadius);
        suppressedNoiseRadius = Mathf.Max(0f, suppressedNoiseRadius);
        impactNoiseRadius = Mathf.Max(0f, impactNoiseRadius);
    }
}
