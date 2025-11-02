using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Editor tool to automatically create Mission UI hierarchy in the scene
/// Right-click in Hierarchy → Mission System → Create Mission UI
/// </summary>
public class MissionUISetup : MonoBehaviour
{
    [MenuItem("GameObject/Mission System/Create Mission UI", false, 0)]
    public static void CreateMissionUI()
    {
        // Check if Canvas exists
        Canvas canvas = FindObjectOfType<Canvas>();

        if (canvas == null)
        {
            // Create Canvas if it doesn't exist
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            Debug.Log("[MissionUISetup] Created new Canvas");
        }

        // Create main Mission UI container
        GameObject missionContainer = new GameObject("MisionUI");
        missionContainer.transform.SetParent(canvas.transform);
        RectTransform containerRect = missionContainer.AddComponent<RectTransform>();

        // Position in top-left corner
        containerRect.anchorMin = new Vector2(0, 1);
        containerRect.anchorMax = new Vector2(0, 1);
        containerRect.pivot = new Vector2(0, 1);
        containerRect.anchoredPosition = new Vector2(20, -20);
        containerRect.sizeDelta = new Vector2(400, 180);

        // Create Panel background
        GameObject panelObj = new GameObject("MisionPanel");
        panelObj.transform.SetParent(missionContainer.transform);
        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        panelRect.anchoredPosition = Vector2.zero;

        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.85f);

        // Create Mission Name Text
        GameObject nameTextObj = new GameObject("MisionNameText");
        nameTextObj.transform.SetParent(panelObj.transform);
        RectTransform nameRect = nameTextObj.AddComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 1);
        nameRect.anchorMax = new Vector2(1, 1);
        nameRect.pivot = new Vector2(0.5f, 1);
        nameRect.anchoredPosition = new Vector2(0, -10);
        nameRect.sizeDelta = new Vector2(-20, 30);

        TextMeshProUGUI nameText = nameTextObj.AddComponent<TextMeshProUGUI>();
        nameText.text = "Mision";
        nameText.fontSize = 24;
        nameText.fontStyle = FontStyles.Bold;
        nameText.color = Color.yellow;
        nameText.alignment = TextAlignmentOptions.TopLeft;

        // Create Mission Description Text
        GameObject descTextObj = new GameObject("MisionDescriptionText");
        descTextObj.transform.SetParent(panelObj.transform);
        RectTransform descRect = descTextObj.AddComponent<RectTransform>();
        descRect.anchorMin = new Vector2(0, 1);
        descRect.anchorMax = new Vector2(1, 1);
        descRect.pivot = new Vector2(0.5f, 1);
        descRect.anchoredPosition = new Vector2(0, -45);
        descRect.sizeDelta = new Vector2(-20, 40);

        TextMeshProUGUI descText = descTextObj.AddComponent<TextMeshProUGUI>();
        descText.text = "Descripcion de la mision";
        descText.fontSize = 16;
        descText.color = Color.white;
        descText.alignment = TextAlignmentOptions.TopLeft;
        descText.enableWordWrapping = true;

        // Create Progress Text
        GameObject progressTextObj = new GameObject("ProgressText");
        progressTextObj.transform.SetParent(panelObj.transform);
        RectTransform progressTextRect = progressTextObj.AddComponent<RectTransform>();
        progressTextRect.anchorMin = new Vector2(0, 0);
        progressTextRect.anchorMax = new Vector2(1, 0);
        progressTextRect.pivot = new Vector2(0.5f, 0);
        progressTextRect.anchoredPosition = new Vector2(0, 50);
        progressTextRect.sizeDelta = new Vector2(-20, 25);

        TextMeshProUGUI progressText = progressTextObj.AddComponent<TextMeshProUGUI>();
        progressText.text = "Progreso: 0/5";
        progressText.fontSize = 18;
        progressText.fontStyle = FontStyles.Bold;
        progressText.color = Color.cyan;
        progressText.alignment = TextAlignmentOptions.BottomLeft;

        // Create Slider
        GameObject sliderObj = new GameObject("ProgressSlider");
        sliderObj.transform.SetParent(panelObj.transform);
        RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0, 0);
        sliderRect.anchorMax = new Vector2(1, 0);
        sliderRect.pivot = new Vector2(0.5f, 0);
        sliderRect.anchoredPosition = new Vector2(0, 15);
        sliderRect.sizeDelta = new Vector2(-20, 20);

        Slider slider = sliderObj.AddComponent<Slider>();
        slider.transition = Selectable.Transition.None;

        // Slider Background
        GameObject sliderBgObj = new GameObject("Background");
        sliderBgObj.transform.SetParent(sliderObj.transform);
        RectTransform sliderBgRect = sliderBgObj.AddComponent<RectTransform>();
        sliderBgRect.anchorMin = Vector2.zero;
        sliderBgRect.anchorMax = Vector2.one;
        sliderBgRect.sizeDelta = Vector2.zero;
        sliderBgRect.anchoredPosition = Vector2.zero;

        Image sliderBgImage = sliderBgObj.AddComponent<Image>();
        sliderBgImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        // Slider Fill Area
        GameObject fillAreaObj = new GameObject("Fill Area");
        fillAreaObj.transform.SetParent(sliderObj.transform);
        RectTransform fillAreaRect = fillAreaObj.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.sizeDelta = Vector2.zero;
        fillAreaRect.anchoredPosition = Vector2.zero;

        // Slider Fill
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(fillAreaObj.transform);
        RectTransform fillRect = fillObj.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        fillRect.anchoredPosition = Vector2.zero;

        Image fillImage = fillObj.AddComponent<Image>();
        fillImage.color = new Color(0, 1, 0, 1);
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;

        // Configure Slider references
        slider.fillRect = fillRect;
        slider.minValue = 0;
        slider.maxValue = 1;
        slider.value = 0;

        // Add MissionUI component
        MissionUI missionUI = missionContainer.AddComponent<MissionUI>();
        missionUI.missionPanel = panelObj;
        missionUI.missionNameText = nameText;
        missionUI.missionDescriptionText = descText;
        missionUI.progressText = progressText;
        missionUI.progressSlider = slider;
        missionUI.progressFillImage = fillImage;

        // Select the created object
        Selection.activeGameObject = missionContainer;

        // Mark scene as dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene()
        );

        Debug.Log("<color=green>[MissionUISetup] ✅ Mission UI created successfully!</color>");
        Debug.Log("<color=cyan>GameObject 'MisionUI' created with all components configured.</color>");
        Debug.Log("<color=yellow>Make sure you have a MissionManager in the scene!</color>");

        EditorUtility.DisplayDialog(
            "Mission UI Created",
            "Mission UI has been created in the top-left corner of your Canvas!\n\n" +
            "Components:\n" +
            "• MisionUI (container)\n" +
            "• MisionPanel (background)\n" +
            "• MisionNameText\n" +
            "• MisionDescriptionText\n" +
            "• ProgressText\n" +
            "• ProgressSlider\n\n" +
            "The MissionUI component is already configured with all references!",
            "OK"
        );
    }

    [MenuItem("GameObject/Mission System/Create Mission UI", true)]
    public static bool ValidateCreateMissionUI()
    {
        // Only show in Hierarchy context menu
        return true;
    }
}
