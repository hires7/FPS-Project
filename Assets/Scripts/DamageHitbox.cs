using UnityEngine;

public enum HitZone
{
    Head,
    Torso,
    Limb
}

[DisallowMultipleComponent]
public sealed class DamageHitbox : MonoBehaviour, IDamageable
{
    [SerializeField] private Health health;
    [SerializeField] private HitZone hitZone = HitZone.Limb;

    public HitZone Zone => hitZone;

    public void Configure(Health owner, HitZone zone)
    {
        health = owner;
        hitZone = zone;
    }

    public void TakeDamage(float ignoredWeaponDamage)
    {
        if (health == null || health.IsDead)
        {
            return;
        }

        float damage = hitZone switch
        {
            HitZone.Head => health.CurrentHealth,
            HitZone.Torso => 50f,
            _ => 25f
        };

        health.TakeDamage(damage);
    }

    private void Reset()
    {
        health = GetComponentInParent<Health>();
    }
}
