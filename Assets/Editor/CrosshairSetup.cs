using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CrosshairSetup
{
    private const string ScenePath = "Assets/Scenes/SampleScene.unity";

    [MenuItem("Tools/FPS/Add or Update Crosshair")]
    public static void AddOrUpdateCrosshair()
    {
        Scene scene = EditorSceneManager.OpenScene(
            ScenePath,
            OpenSceneMode.Single
        );
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();

        if (canvas == null)
        {
            throw new MissingComponentException(
                "SampleScene requires a Canvas for the crosshair."
            );
        }

        Transform existing = canvas.transform.Find("Crosshair");
        GameObject crosshairObject;

        if (existing != null)
        {
            crosshairObject = existing.gameObject;
        }
        else
        {
            crosshairObject = new GameObject(
                "Crosshair",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(TextMeshProUGUI)
            );
            crosshairObject.transform.SetParent(canvas.transform, false);
        }

        RectTransform rect = crosshairObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(32f, 32f);
        rect.localScale = Vector3.one;

        TextMeshProUGUI text =
            crosshairObject.GetComponent<TextMeshProUGUI>();
        TMP_FontAsset sceneFont = Object
            .FindObjectsByType<TextMeshProUGUI>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            )
            .Where(candidate => candidate != text)
            .Select(candidate => candidate.font)
            .FirstOrDefault(font => font != null);

        if (sceneFont != null)
        {
            text.font = sceneFont;
        }

        text.text = "+";
        text.fontSize = 24f;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.outlineColor = new Color32(0, 0, 0, 220);
        text.outlineWidth = 0.2f;
        text.enableWordWrapping = false;
        text.raycastTarget = false;

        crosshairObject.transform.SetAsLastSibling();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        Debug.Log("Crosshair added at the exact center of the gameplay Canvas.");
    }
}
