using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

/// <summary>
/// Wizard para crear automáticamente la UI del boss (barras de vida y energía)
/// </summary>
public class SetupBossUI : EditorWindow
{
    private Canvas targetCanvas;
    private BossController bossController;

    [MenuItem("Game Jam/Setup/Boss UI (Health & Energy Bars)")]
    static void OpenWindow()
    {
        SetupBossUI window = GetWindow<SetupBossUI>("Boss UI Setup");
        window.minSize = new Vector2(400, 300);
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Boss UI Setup", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "Este wizard creará automáticamente:\n" +
            "• Panel con fondo para las barras del boss\n" +
            "• Barra de vida (roja)\n" +
            "• Barra de energía (azul)\n" +
            "• Script BossHealthBarUI configurado\n\n" +
            "La UI se creará en la esquina superior de la pantalla.",
            MessageType.Info
        );

        EditorGUILayout.Space();

        targetCanvas = (Canvas)EditorGUILayout.ObjectField(
            "Canvas (opcional)",
            targetCanvas,
            typeof(Canvas),
            true
        );

        EditorGUILayout.HelpBox(
            "Si no seleccionas un Canvas, se creará uno nuevo con CanvasScaler.",
            MessageType.Info
        );

        EditorGUILayout.Space();

        bossController = (BossController)EditorGUILayout.ObjectField(
            "Boss Controller (opcional)",
            bossController,
            typeof(BossController),
            true
        );

        EditorGUILayout.HelpBox(
            "Si no seleccionas el BossController, el script lo buscará automáticamente.",
            MessageType.Info
        );

        EditorGUILayout.Space();

        if (GUILayout.Button("Create Boss UI", GUILayout.Height(40)))
        {
            CreateBossUI();
        }
    }

    void CreateBossUI()
    {
        // 1. Crear o encontrar Canvas
        Canvas canvas = targetCanvas;
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("BossUI_Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            // Configurar CanvasScaler
            CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            Debug.Log("[SetupBossUI] Created new Canvas: BossUI_Canvas");
        }

        // 2. Crear contenedor principal
        GameObject container = new GameObject("BossUI_Container");
        container.transform.SetParent(canvas.transform, false);

        RectTransform containerRect = container.AddComponent<RectTransform>();
        // Posicionar en la parte superior central
        containerRect.anchorMin = new Vector2(0.5f, 1f);
        containerRect.anchorMax = new Vector2(0.5f, 1f);
        containerRect.pivot = new Vector2(0.5f, 1f);
        containerRect.anchoredPosition = new Vector2(0, -50); // 50px desde arriba
        containerRect.sizeDelta = new Vector2(400, 120);

        // 3. Crear panel de fondo
        GameObject bgPanel = new GameObject("Background");
        bgPanel.transform.SetParent(container.transform, false);

        Image bgImage = bgPanel.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.7f); // Negro semi-transparente

        RectTransform bgRect = bgPanel.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgRect.anchoredPosition = Vector2.zero;

        // 4. Crear texto "BOSS"
        GameObject bossText = new GameObject("BossLabel");
        bossText.transform.SetParent(container.transform, false);

        Text text = bossText.AddComponent<Text>();
        text.text = "BOSS";
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 24;
        text.fontStyle = FontStyle.Bold;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;

        RectTransform textRect = bossText.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0f, 0.7f);
        textRect.anchorMax = new Vector2(1f, 1f);
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;

        // 5. Crear barra de vida
        GameObject healthBar = CreateBar("HealthBar", container.transform, 0.35f, 0.55f, new Color(0.8f, 0.1f, 0.1f));

        // 6. Crear barra de energía
        GameObject energyBar = CreateBar("EnergyBar", container.transform, 0.1f, 0.3f, new Color(0.2f, 0.5f, 1f));

        // 7. Añadir script BossHealthBarUI
        BossHealthBarUI uiScript = container.AddComponent<BossHealthBarUI>();
        uiScript.bossController = bossController;
        uiScript.bossUIContainer = container;

        // Configurar referencias a las barras usando Image (fillAmount)
        uiScript.healthFillImage = healthBar.transform.Find("Fill").GetComponent<Image>();
        uiScript.energyFillImage = energyBar.transform.Find("Fill").GetComponent<Image>();

        Debug.Log("<color=green>[SetupBossUI] ✓ Boss UI created successfully!</color>");
        Debug.Log($"<color=cyan>[SetupBossUI] UI Container: {container.name}</color>");

        Selection.activeGameObject = container;
        EditorGUIUtility.PingObject(container);
    }

    GameObject CreateBar(string name, Transform parent, float anchorMinY, float anchorMaxY, Color fillColor)
    {
        // Contenedor de la barra
        GameObject bar = new GameObject(name);
        bar.transform.SetParent(parent, false);

        RectTransform barRect = bar.AddComponent<RectTransform>();
        barRect.anchorMin = new Vector2(0.1f, anchorMinY);
        barRect.anchorMax = new Vector2(0.9f, anchorMaxY);
        barRect.sizeDelta = Vector2.zero;
        barRect.anchoredPosition = Vector2.zero;

        // Background de la barra
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(bar.transform, false);

        Image bgImage = bg.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        RectTransform bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgRect.anchoredPosition = Vector2.zero;

        // Fill de la barra
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(bar.transform, false);

        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = fillColor;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;

        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = new Vector2(-4, -4); // Padding de 2px
        fillRect.anchoredPosition = Vector2.zero;

        // Label de la barra
        GameObject label = new GameObject("Label");
        label.transform.SetParent(bar.transform, false);

        Text labelText = label.AddComponent<Text>();
        labelText.text = name.Replace("Bar", "");
        labelText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        labelText.fontSize = 14;
        labelText.color = Color.white;
        labelText.alignment = TextAnchor.MiddleLeft;

        RectTransform labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 0f);
        labelRect.anchorMax = new Vector2(0f, 1f);
        labelRect.pivot = new Vector2(1f, 0.5f);
        labelRect.anchoredPosition = new Vector2(-5, 0);
        labelRect.sizeDelta = new Vector2(100, 0);

        return bar;
    }
}
