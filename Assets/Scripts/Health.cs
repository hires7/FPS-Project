using System;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public sealed class Health : MonoBehaviour, IDamageable
{
    [Min(1f)]
    [SerializeField] private float maxHealth = 100f;

    [Tooltip("Useful for disposable targets. Leave disabled for enemies whose bodies should remain.")]
    [SerializeField] private bool disableGameObjectOnDeath;

    [Header("Events")]
    [SerializeField] private UnityEvent onDamaged = new();
    [SerializeField] private UnityEvent onDied = new();

    public event Action<Health, float> Damaged;
    public event Action<Health> Died;

    public float CurrentHealth { get; private set; }
    public float MaxHealth => maxHealth;
    public bool IsDead => CurrentHealth <= 0f;

    private void Awake()
    {
        ResetHealth();
    }

    public void TakeDamage(float damage)
    {
        if (IsDead || damage <= 0f || !float.IsFinite(damage))
        {
            return;
        }

        float appliedDamage = Mathf.Min(CurrentHealth, damage);
        CurrentHealth -= appliedDamage;

        Damaged?.Invoke(this, appliedDamage);
        onDamaged.Invoke();

        if (!IsDead)
        {
            return;
        }

        Died?.Invoke(this);
        onDied.Invoke();

        if (disableGameObjectOnDeath)
        {
            gameObject.SetActive(false);
        }
    }

    public void ResetHealth()
    {
        CurrentHealth = Mathf.Max(1f, maxHealth);
    }

    private void OnValidate()
    {
        maxHealth = Mathf.Max(1f, maxHealth);
    }
}
