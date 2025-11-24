using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

/// <summary>
/// Editor tool para crear rápidamente la barra de vida del boss
/// </summary>
public class BossHealthBarQuickSetup : EditorWindow
{
    private Boss boss;
    private Color healthColor = new Color(0.8f, 0.1f, 0.1f, 1f);
    private string bossName = "JEFE";
    private float topOffset = 50f;

    [MenuItem("Game Jam/Quick Setup/Boss Health Bar")]
    public static void ShowWindow()
    {
        BossHealthBarQuickSetup window = GetWindow<BossHealthBarQuickSetup>("Boss Health Bar Setup");
        window.minSize = new Vector2(400, 300);
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Configuración Rápida - Barra de Vida del Boss", EditorStyles.boldLabel);
        GUILayout.Space(10);

        boss = (Boss)EditorGUILayout.ObjectField("Boss en la escena:", boss, typeof(Boss), true);

        GUILayout.Space(5);
        bossName = EditorGUILayout.TextField("Nombre del Boss:", bossName);
        topOffset = EditorGUILayout.FloatField("Distancia desde arriba:", topOffset);
        healthColor = EditorGUILayout.ColorField("Color de la barra:", healthColor);

        GUILayout.Space(20);

        if (GUILayout.Button("🚀 CREAR BARRA DE VIDA", GUILayout.Height(40)))
        {
            CreateBossHealthBar();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("🗑️ Eliminar Barra de Vida", GUILayout.Height(30)))
        {
            RemoveBossHealthBar();
        }

        GUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "Este script creará automáticamente:\n" +
            "• Panel en la parte superior\n" +
            "• Barra de vida con fill horizontal\n" +
            "• Texto del nombre del boss\n" +
            "• Script SimpleBossHealthBar configurado",
            MessageType.Info
        );

        GUILayout.Space(10);

        if (boss == null)
        {
            EditorGUILayout.HelpBox(
                "⚠️ Arrastra el GameObject del Boss desde la jerarquía al campo 'Boss en la escena'",
                MessageType.Warning
            );
        }
    }

    void CreateBossHealthBar()
    {
        Debug.Log("<color=cyan>========================================</color>");
        Debug.Log("<color=cyan>[Quick Setup] Creando barra de vida del boss...</color>");

        // 1. Buscar Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Error", "No se encontró Canvas en la escena.\nPor favor crea un Canvas primero.", "OK");
            return;
        }

        // 2. Eliminar UI antigua si existe
        Transform oldPanel = canvas.transform.Find("BossHealthBarPanel");
        if (oldPanel != null)
        {
            DestroyImmediate(oldPanel.gameObject);
            Debug.Log("<color=yellow>[Quick Setup] UI antigua eliminada</color>");
        }

        // 3. Crear Panel
        GameObject panel = new GameObject("BossHealthBarPanel");
        panel.transform.SetParent(canvas.transform, false);

        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 1f);
        panelRect.anchorMax = new Vector2(0.5f, 1f);
        panelRect.pivot = new Vector2(0.5f, 1f);
        panelRect.anchoredPosition = new Vector2(0, -topOffset);
        panelRect.sizeDelta = new Vector2(400, 80);

        // 4. Crear Barra de Fondo
        GameObject background = new GameObject("HealthBarBackground");
        background.transform.SetParent(panel.transform, false);

        RectTransform bgRect = background.AddComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0.5f, 0.5f);
        bgRect.anchorMax = new Vector2(0.5f, 0.5f);
        bgRect.pivot = new Vector2(0.5f, 0.5f);
        bgRect.anchoredPosition = Vector2.zero;
        bgRect.sizeDelta = new Vector2(350, 20);

        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0f, 0f, 0f, 0.6f);

        // 5. Crear Fill
        GameObject fill = new GameObject("HealthBarFill");
        fill.transform.SetParent(background.transform, false);

        RectTransform fillRect = fill.AddComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(1f, 1f);
        fillRect.pivot = new Vector2(0.5f, 0.5f);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = healthColor;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;

        // 6. Crear Texto de Nombre
        GameObject nameObj = new GameObject("BossNameText");
        nameObj.transform.SetParent(panel.transform, false);

        RectTransform nameRect = nameObj.AddComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0.5f, 1f);
        nameRect.anchorMax = new Vector2(0.5f, 1f);
        nameRect.pivot = new Vector2(0.5f, 1f);
        nameRect.anchoredPosition = new Vector2(0, 0);
        nameRect.sizeDelta = new Vector2(350, 30);

        TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
        nameText.text = bossName;
        nameText.fontSize = 20;
        nameText.alignment = TextAlignmentOptions.Center;
        nameText.color = Color.white;
        nameText.fontStyle = FontStyles.Bold;

        // 7. Crear Texto de Vida
        GameObject healthTextObj = new GameObject("HealthText");
        healthTextObj.transform.SetParent(panel.transform, false);

        RectTransform healthTextRect = healthTextObj.AddComponent<RectTransform>();
        healthTextRect.anchorMin = new Vector2(0.5f, 0f);
        healthTextRect.anchorMax = new Vector2(0.5f, 0f);
        healthTextRect.pivot = new Vector2(0.5f, 0f);
        healthTextRect.anchoredPosition = new Vector2(0, -25);
        healthTextRect.sizeDelta = new Vector2(350, 25);

        TextMeshProUGUI healthText = healthTextObj.AddComponent<TextMeshProUGUI>();
        healthText.text = "2000 / 2000";
        healthText.fontSize = 16;
        healthText.alignment = TextAlignmentOptions.Center;
        healthText.color = Color.white;

        // 8. Configurar Script
        SimpleBossHealthBar script = panel.AddComponent<SimpleBossHealthBar>();
        script.boss = boss;
        script.healthFillImage = fillImage;
        script.healthBackgroundImage = bgImage;
        script.bossNameText = nameText;
        script.healthText = healthText;
        script.bossName = bossName;
        script.fullHealthColor = healthColor;
        script.hideWhenDead = true;
        script.hideWhenInactive = false; // IMPORTANTE: No ocultar cuando esté inactivo
        script.smoothUpdate = true;
        script.showDebugLogs = true;

        // 9. ACTIVAR el panel inmediatamente
        panel.SetActive(true);

        Debug.Log("<color=green>[Quick Setup] ✅ Barra de vida creada exitosamente!</color>");
        Debug.Log("<color=green>[Quick Setup] El panel está ACTIVO y visible</color>");
        Debug.Log("<color=cyan>========================================</color>");

        Selection.activeGameObject = panel;

        EditorUtility.DisplayDialog(
            "¡Éxito!",
            "La barra de vida del boss se creó correctamente.\n\n" +
            "El panel está ACTIVO en la jerarquía.\n" +
            "Revisa la parte superior de la pantalla en modo Play.",
            "OK"
        );
    }

    void RemoveBossHealthBar()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            Transform panel = canvas.transform.Find("BossHealthBarPanel");
            if (panel != null)
            {
                DestroyImmediate(panel.gameObject);
                Debug.Log("<color=yellow>[Quick Setup] Barra de vida eliminada</color>");
                EditorUtility.DisplayDialog("Eliminado", "La barra de vida del boss fue eliminada.", "OK");
                return;
            }
        }

        EditorUtility.DisplayDialog("Info", "No se encontró barra de vida para eliminar.", "OK");
    }

    void OnEnable()
    {
        // Auto-buscar el boss en la escena
        if (boss == null)
        {
            boss = FindObjectOfType<Boss>();
        }
    }
}
