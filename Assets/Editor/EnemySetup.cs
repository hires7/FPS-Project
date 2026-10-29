using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class EnemySetup
{
    private const string ScenePath = "Assets/Scenes/SampleScene.unity";
    private const string ModelPath = "Assets/Models/Standing Idle.fbx";
    private static readonly string[] MotionPaths =
    {
        "Assets/Models/Slow Run.fbx",
        "Assets/Models/Crouching Idle.fbx",
        "Assets/Models/Crouch Walk Forward.fbx"
    };
    private const string ControllerPath = "Assets/PlayerAnimator.controller";
    private const string MaterialPath = "Assets/Materials/Enemy_yellow.mat";
    private const string PrefabFolder = "Assets/Prefabs";
    private const string PrefabPath = PrefabFolder + "/EnemyGuard.prefab";

    [MenuItem("Tools/FPS/Rebuild Enemy Guards")]
    public static void RebuildEnemies()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        EnsureFolder(PrefabFolder);
        GameObject prefab = BuildPrefab();
        AddEnemiesToScene(prefab);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Created EnemyGuard prefab and three walking guards.");
    }

    [MenuItem("Tools/FPS/Validate Enemy Guards")]
    public static void ValidateEnemies()
    {
        Scene scene = EditorSceneManager.OpenScene(
            ScenePath,
            OpenSceneMode.Single
        );
        GameObject group = GameObject.Find("Enemies");

        if (!scene.IsValid() || group == null
            || group.transform.childCount != 3)
        {
            throw new InvalidOperationException(
                "The scene must contain exactly three enemy guards."
            );
        }

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            PrefabPath
        );
        Animator prefabAnimator = prefab?.GetComponentInChildren<Animator>();

        if (prefab == null
            || prefab.layer != 2
            || prefab.GetComponent<Health>() == null
            || prefab.GetComponent<EnemyWalker>() == null
            || prefab.GetComponent<EnemyHitboxRig>() == null
            || prefabAnimator == null
            || prefabAnimator.avatar == null
            || !prefabAnimator.avatar.isHuman)
        {
            throw new InvalidOperationException(
                "EnemyGuard prefab configuration is incomplete."
            );
        }

        GameObject clone = UnityEngine.Object.Instantiate(prefab);
        clone.name = "Enemy validation clone";
        Health health = clone.GetComponent<Health>();
        health.ResetHealth();
        clone.GetComponent<EnemyHitboxRig>().SendMessage(
            "Awake",
            SendMessageOptions.RequireReceiver
        );

        DamageHitbox[] hitboxes =
            clone.GetComponentsInChildren<DamageHitbox>(true);
        DamageHitbox head = hitboxes.FirstOrDefault(
            hitbox => hitbox.Zone == HitZone.Head
        );
        DamageHitbox torso = hitboxes.FirstOrDefault(
            hitbox => hitbox.Zone == HitZone.Torso
        );
        DamageHitbox limb = hitboxes.FirstOrDefault(
            hitbox => hitbox.Zone == HitZone.Limb
        );

        if (hitboxes.Length != 11
            || head == null
            || torso == null
            || limb == null)
        {
            UnityEngine.Object.DestroyImmediate(clone);
            throw new InvalidOperationException(
                $"Expected 11 body hitboxes, found {hitboxes.Length}."
            );
        }

        health.ResetHealth();
        torso.TakeDamage(999f);
        bool torsoIsCorrect = Mathf.Approximately(
            health.CurrentHealth,
            50f
        );

        health.ResetHealth();
        limb.TakeDamage(999f);
        bool limbIsCorrect = Mathf.Approximately(
            health.CurrentHealth,
            75f
        );

        health.ResetHealth();
        head.TakeDamage(1f);
        bool headIsCorrect = health.IsDead;

        UnityEngine.Object.DestroyImmediate(clone);

        if (!torsoIsCorrect || !limbIsCorrect || !headIsCorrect)
        {
            throw new InvalidOperationException(
                "Enemy body-zone damage validation failed."
            );
        }

        Debug.Log(
            "Enemy validation passed: 3 guards, 11 hitboxes, "
            + "head lethal, torso 50 damage, limbs 25 damage."
        );
    }

    private static GameObject BuildPrefab()
    {
        GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(
            ModelPath
        );
        RuntimeAnimatorController controller =
            AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                ControllerPath
            );
        Material material = AssetDatabase.LoadAssetAtPath<Material>(
            MaterialPath
        );
        Avatar avatar = LoadOrCreateAvatar();

        if (avatar != null)
        {
            ConfigureMotionImports(avatar);
        }

        if (modelAsset == null || controller == null || avatar == null)
        {
            throw new InvalidOperationException(
                $"Enemy asset check failed: model={modelAsset != null}, "
                + $"controller={controller != null}, avatar={avatar != null}."
            );
        }

        GameObject root = new("Enemy Guard")
        {
            layer = 2
        };

        CharacterController character =
            root.AddComponent<CharacterController>();
        character.center = new Vector3(0f, 1f, 0f);
        character.height = 2f;
        character.radius = 0.42f;
        character.stepOffset = 0.25f;

        Health health = root.AddComponent<Health>();
        SerializedObject healthData = new(health);
        healthData.FindProperty("maxHealth").floatValue = 100f;
        healthData.FindProperty("disableGameObjectOnDeath").boolValue = true;
        healthData.ApplyModifiedPropertiesWithoutUndo();

        root.AddComponent<EnemyHitboxRig>();
        root.AddComponent<EnemyWalker>();

        GameObject visual = (GameObject)PrefabUtility.InstantiatePrefab(
            modelAsset
        );
        visual.name = "Visual";
        visual.transform.SetParent(root.transform, false);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.identity;

        Animator animator = visual.GetComponent<Animator>();

        if (animator == null)
        {
            animator = visual.AddComponent<Animator>();
        }

        animator.avatar = avatar;
        animator.runtimeAnimatorController = controller;
        animator.applyRootMotion = false;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        if (material != null)
        {
            foreach (Renderer renderer in
                     visual.GetComponentsInChildren<Renderer>(true))
            {
                Material[] materials = new Material[
                    renderer.sharedMaterials.Length
                ];
                Array.Fill(materials, material);
                renderer.sharedMaterials = materials;
            }
        }

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(
            root,
            PrefabPath
        );
        UnityEngine.Object.DestroyImmediate(root);

        if (prefab == null)
        {
            throw new InvalidOperationException(
                "Could not create the EnemyGuard prefab."
            );
        }

        return prefab;
    }

    private static Avatar LoadOrCreateAvatar()
    {
        Avatar avatar = GameObject.Find("Player")
            ?.GetComponentInChildren<Animator>()
            ?.avatar
            ?? AssetDatabase.LoadAssetAtPath<Avatar>(ModelPath)
            ?? AssetDatabase.LoadAllAssetsAtPath(ModelPath)
                .OfType<Avatar>()
                .FirstOrDefault();

        if (avatar != null)
        {
            return avatar;
        }

        ModelImporter importer = AssetImporter.GetAtPath(ModelPath)
            as ModelImporter;

        if (importer == null)
        {
            return null;
        }

        importer.animationType = ModelImporterAnimationType.Human;
        importer.avatarSetup =
            ModelImporterAvatarSetup.CreateFromThisModel;
        importer.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Avatar>(ModelPath)
            ?? AssetDatabase.LoadAllAssetsAtPath(ModelPath)
                .OfType<Avatar>()
                .FirstOrDefault();
    }

    private static void ConfigureMotionImports(Avatar sourceAvatar)
    {
        foreach (string path in MotionPaths)
        {
            ModelImporter importer = AssetImporter.GetAtPath(path)
                as ModelImporter;

            if (importer == null)
            {
                throw new InvalidOperationException(
                    $"Animation model is missing: {path}"
                );
            }

            bool needsReimport =
                importer.animationType != ModelImporterAnimationType.Human
                || importer.avatarSetup
                    != ModelImporterAvatarSetup.CopyFromOther
                || importer.sourceAvatar != sourceAvatar;

            if (!needsReimport)
            {
                continue;
            }

            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CopyFromOther;
            importer.sourceAvatar = sourceAvatar;
            importer.SaveAndReimport();
        }
    }

    private static void AddEnemiesToScene(GameObject prefab)
    {
        Scene scene = EditorSceneManager.OpenScene(
            ScenePath,
            OpenSceneMode.Single
        );

        GameObject existingGroup = GameObject.Find("Enemies");

        if (existingGroup != null)
        {
            UnityEngine.Object.DestroyImmediate(existingGroup);
        }

        GameObject group = new("Enemies");
        SceneManager.MoveGameObjectToScene(group, scene);

        CreateEnemy(
            prefab,
            scene,
            group.transform,
            "Enemy Guard 01",
            new Vector3(-15f, 0f, 8f),
            0f
        );
        CreateEnemy(
            prefab,
            scene,
            group.transform,
            "Enemy Guard 02",
            new Vector3(13f, 0f, 14f),
            180f
        );
        CreateEnemy(
            prefab,
            scene,
            group.transform,
            "Enemy Guard 03",
            new Vector3(-17f, 0f, -17f),
            90f
        );

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
    }

    private static void CreateEnemy(
        GameObject prefab,
        Scene scene,
        Transform parent,
        string objectName,
        Vector3 position,
        float yRotation
    )
    {
        GameObject enemy = (GameObject)PrefabUtility.InstantiatePrefab(
            prefab,
            scene
        );
        enemy.name = objectName;
        enemy.transform.SetParent(parent, true);
        enemy.transform.SetPositionAndRotation(
            position,
            Quaternion.Euler(0f, yRotation, 0f)
        );
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        string parent = path[..path.LastIndexOf('/')];
        string folder = path[(path.LastIndexOf('/') + 1)..];
        AssetDatabase.CreateFolder(parent, folder);
    }
}
