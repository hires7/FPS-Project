using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Health))]
public sealed class EnemyHitboxRig : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private Health health;
    private bool hasBuiltHitboxes;

    private void Awake()
    {
        if (hasBuiltHitboxes)
        {
            return;
        }

        health = GetComponent<Health>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (animator == null || !animator.isHuman)
        {
            Debug.LogError(
                "EnemyHitboxRig requires a humanoid Animator.",
                this
            );
            enabled = false;
            return;
        }

        BuildHitboxes();
        hasBuiltHitboxes = true;
    }

    private void BuildHitboxes()
    {
        AddSphere(
            "Hitbox - Head",
            HumanBodyBones.Head,
            new Vector3(0f, 0.08f, 0f),
            0.16f,
            HitZone.Head
        );

        AddBox(
            "Hitbox - Chest",
            HumanBodyBones.Chest,
            new Vector3(0f, 0.03f, 0f),
            new Vector3(0.46f, 0.48f, 0.28f),
            HitZone.Torso
        );
        AddBox(
            "Hitbox - Pelvis",
            HumanBodyBones.Hips,
            new Vector3(0f, 0.08f, 0f),
            new Vector3(0.4f, 0.3f, 0.26f),
            HitZone.Torso
        );

        AddLimb("Hitbox - Left Upper Arm", HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm, 0.075f);
        AddLimb("Hitbox - Left Lower Arm", HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftHand, 0.065f);
        AddLimb("Hitbox - Right Upper Arm", HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm, 0.075f);
        AddLimb("Hitbox - Right Lower Arm", HumanBodyBones.RightLowerArm, HumanBodyBones.RightHand, 0.065f);
        AddLimb("Hitbox - Left Upper Leg", HumanBodyBones.LeftUpperLeg, HumanBodyBones.LeftLowerLeg, 0.1f);
        AddLimb("Hitbox - Left Lower Leg", HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftFoot, 0.085f);
        AddLimb("Hitbox - Right Upper Leg", HumanBodyBones.RightUpperLeg, HumanBodyBones.RightLowerLeg, 0.1f);
        AddLimb("Hitbox - Right Lower Leg", HumanBodyBones.RightLowerLeg, HumanBodyBones.RightFoot, 0.085f);
    }

    private void AddSphere(
        string objectName,
        HumanBodyBones bone,
        Vector3 center,
        float radius,
        HitZone zone
    )
    {
        Transform boneTransform = animator.GetBoneTransform(bone);

        if (boneTransform == null)
        {
            return;
        }

        GameObject hitbox = CreateHitboxObject(objectName, boneTransform, zone);
        SphereCollider collider = hitbox.AddComponent<SphereCollider>();
        collider.center = center;
        collider.radius = radius;
        collider.isTrigger = true;
    }

    private void AddBox(
        string objectName,
        HumanBodyBones bone,
        Vector3 center,
        Vector3 size,
        HitZone zone
    )
    {
        Transform boneTransform = animator.GetBoneTransform(bone);

        if (boneTransform == null)
        {
            return;
        }

        GameObject hitbox = CreateHitboxObject(objectName, boneTransform, zone);
        BoxCollider collider = hitbox.AddComponent<BoxCollider>();
        collider.center = center;
        collider.size = size;
        collider.isTrigger = true;
    }

    private void AddLimb(
        string objectName,
        HumanBodyBones startBone,
        HumanBodyBones endBone,
        float radius
    )
    {
        Transform start = animator.GetBoneTransform(startBone);
        Transform end = animator.GetBoneTransform(endBone);

        if (start == null || end == null)
        {
            return;
        }

        Vector3 endPosition = start.InverseTransformPoint(end.position);
        float length = endPosition.magnitude;

        if (length <= Mathf.Epsilon)
        {
            return;
        }

        GameObject hitbox = CreateHitboxObject(
            objectName,
            start,
            HitZone.Limb
        );
        hitbox.transform.localPosition = endPosition * 0.5f;
        hitbox.transform.localRotation = Quaternion.FromToRotation(
            Vector3.up,
            endPosition.normalized
        );

        CapsuleCollider collider = hitbox.AddComponent<CapsuleCollider>();
        collider.direction = 1;
        collider.radius = radius;
        collider.height = length + radius * 2f;
        collider.isTrigger = true;
    }

    private GameObject CreateHitboxObject(
        string objectName,
        Transform parent,
        HitZone zone
    )
    {
        GameObject hitbox = new(objectName)
        {
            layer = 0
        };

        hitbox.transform.SetParent(parent, false);
        hitbox.AddComponent<DamageHitbox>().Configure(health, zone);
        return hitbox;
    }
}
