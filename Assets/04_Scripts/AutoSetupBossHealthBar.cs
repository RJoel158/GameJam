using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Auto-configura la barra de vida del boss en la escena
/// Usa este script para crear automáticamente toda la UI necesaria
/// </summary>
public class AutoSetupBossHealthBar : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("El boss que se monitoreará (si no se asigna, se busca automáticamente)")]
    public Boss boss;

    [Header("Configuración")]
    [Tooltip("Mostrar nombre del boss")]
    public bool showBossName = true;

    [Tooltip("Mostrar vida numérica (2000/2000)")]
    public bool showHealthText = true;

    [Tooltip("Nombre que aparecerá en la UI")]
    public string bossDisplayName = "JEFE";

    [Header("Posición")]
    [Tooltip("Distancia desde la parte superior de la pantalla")]
    public float topOffset = 50f;

    [Header("Colores")]
    public Color fullHealthColor = new Color(0.8f, 0.1f, 0.1f, 1f);
    public Color lowHealthColor = new Color(1f, 0.5f, 0f, 1f);
    public Color criticalHealthColor = Color.yellow;
    public Color backgroundColor = new Color(0f, 0f, 0f, 0.6f);

    [Header("Debug")]
    [SerializeField] private bool setupCompleted = false;

    /// <summary>
    /// Auto-configura toda la UI del boss
    /// </summary>
    [ContextMenu("🚀 AUTO-CONFIGURAR BARRA DE VIDA")]
    public void AutoSetupHealthBar()
    {
        Debug.Log("<color=cyan>========================================</color>");
        Debug.Log("<color=cyan>[AutoSetup] Iniciando configuración de barra de vida del boss...</color>");
        Debug.Log("<color=cyan>========================================</color>");

        // 1. Buscar o crear Canvas
        Canvas canvas = FindOrCreateCanvas();

        // 2. Crear el panel principal
        GameObject panel = CreateBossHealthPanel(canvas);

        // 3. Crear barra de vida
        GameObject healthBar = CreateHealthBar(panel);

        // 4. Crear texto de nombre (opcional)
        if (showBossName)
        {
            CreateBossNameText(panel);
        }

        // 5. Crear texto de vida (opcional)
        if (showHealthText)
        {
            CreateHealthText(panel);
        }

        // 6. Configurar el script SimpleBossHealthBar
        SetupBossHealthBarScript(panel, healthBar);

        setupCompleted = true;

        Debug.Log("<color=green>========================================</color>");
        Debug.Log("<color=green>[AutoSetup] ¡Configuración completada! ✅</color>");
        Debug.Log("<color=green>La barra de vida del boss está lista</color>");
        Debug.Log("<color=green>========================================</color>");

#if UNITY_EDITOR
        UnityEditor.Selection.activeGameObject = panel;
#endif
    }

    Canvas FindOrCreateCanvas()
    {
        // Buscar Canvas existente
        Canvas canvas = FindAnyObjectByType<Canvas>();

        if (canvas == null)
        {
            // Crear nuevo Canvas
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            Debug.Log("<color=green>[AutoSetup] Canvas creado ✅</color>");
        }
        else
        {
            Debug.Log("<color=yellow>[AutoSetup] Canvas existente encontrado</color>");
        }

        return canvas;
    }

    GameObject CreateBossHealthPanel(Canvas canvas)
    {
        // Verificar si ya existe
        Transform existing = canvas.transform.Find("BossHealthBarPanel");
        if (existing != null)
        {
            Debug.LogWarning("<color=orange>[AutoSetup] BossHealthBarPanel ya existe, eliminando el antiguo...</color>");
            DestroyImmediate(existing.gameObject);
        }

        GameObject panel = new GameObject("BossHealthBarPanel");
        panel.transform.SetParent(canvas.transform, false);

        RectTransform panelRect = panel.AddComponent<RectTransform>();

        // Anchor: Top Center
        panelRect.anchorMin = new Vector2(0.5f, 1f);
        panelRect.anchorMax = new Vector2(0.5f, 1f);
        panelRect.pivot = new Vector2(0.5f, 1f);

        // Position
        panelRect.anchoredPosition = new Vector2(0, -topOffset);
        panelRect.sizeDelta = new Vector2(400, 80);

        Debug.Log("<color=green>[AutoSetup] Panel principal creado ✅</color>");
        return panel;
    }

    GameObject CreateHealthBar(GameObject parent)
    {
        // Crear fondo
        GameObject background = new GameObject("HealthBarBackground");
        background.transform.SetParent(parent.transform, false);

        RectTransform bgRect = background.AddComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0.5f, 0.5f);
        bgRect.anchorMax = new Vector2(0.5f, 0.5f);
        bgRect.pivot = new Vector2(0.5f, 0.5f);
        bgRect.anchoredPosition = Vector2.zero;
        bgRect.sizeDelta = new Vector2(350, 20);

        Image bgImage = background.AddComponent<Image>();
        bgImage.color = backgroundColor;

        // Crear fill
        GameObject fill = new GameObject("HealthBarFill");
        fill.transform.SetParent(background.transform, false);

        RectTransform fillRect = fill.AddComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(1f, 1f);
        fillRect.pivot = new Vector2(0.5f, 0.5f);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = fullHealthColor;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;

        Debug.Log("<color=green>[AutoSetup] Barra de vida creada ✅</color>");
        return fill;
    }

    void CreateBossNameText(GameObject parent)
    {
        GameObject nameTextObj = new GameObject("BossNameText");
        nameTextObj.transform.SetParent(parent.transform, false);

        RectTransform nameRect = nameTextObj.AddComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0.5f, 1f);
        nameRect.anchorMax = new Vector2(0.5f, 1f);
        nameRect.pivot = new Vector2(0.5f, 1f);
        nameRect.anchoredPosition = new Vector2(0, 0);
        nameRect.sizeDelta = new Vector2(350, 30);

        TextMeshProUGUI nameText = nameTextObj.AddComponent<TextMeshProUGUI>();
        nameText.text = bossDisplayName;
        nameText.fontSize = 20;
        nameText.alignment = TextAlignmentOptions.Center;
        nameText.color = Color.white;
        nameText.fontStyle = FontStyles.Bold;

        Debug.Log("<color=green>[AutoSetup] Texto de nombre creado ✅</color>");
    }

    void CreateHealthText(GameObject parent)
    {
        GameObject healthTextObj = new GameObject("HealthText");
        healthTextObj.transform.SetParent(parent.transform, false);

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

        Debug.Log("<color=green>[AutoSetup] Texto de vida creado ✅</color>");
    }

    void SetupBossHealthBarScript(GameObject panel, GameObject healthFill)
    {
        // Buscar el boss si no está asignado
        if (boss == null)
        {
            boss = FindAnyObjectByType<Boss>();
            if (boss != null)
            {
                Debug.Log($"<color=green>[AutoSetup] Boss encontrado: {boss.gameObject.name} ✅</color>");
            }
            else
            {
                Debug.LogWarning("<color=orange>[AutoSetup] ⚠️ No se encontró Boss en la escena. Asígnalo manualmente después.</color>");
            }
        }

        // Agregar el script SimpleBossHealthBar
        SimpleBossHealthBar healthBarScript = panel.GetComponent<SimpleBossHealthBar>();
        if (healthBarScript == null)
        {
            healthBarScript = panel.AddComponent<SimpleBossHealthBar>();
        }

        // Configurar referencias
        healthBarScript.boss = boss;

        // Buscar y asignar elementos
        Transform bgTransform = panel.transform.Find("HealthBarBackground");
        if (bgTransform != null)
        {
            healthBarScript.healthBackgroundImage = bgTransform.GetComponent<Image>();

            Transform fillTransform = bgTransform.Find("HealthBarFill");
            if (fillTransform != null)
            {
                healthBarScript.healthFillImage = fillTransform.GetComponent<Image>();
            }
        }

        Transform nameTransform = panel.transform.Find("BossNameText");
        if (nameTransform != null)
        {
            healthBarScript.bossNameText = nameTransform.GetComponent<TextMeshProUGUI>();
        }

        Transform healthTextTransform = panel.transform.Find("HealthText");
        if (healthTextTransform != null)
        {
            healthBarScript.healthText = healthTextTransform.GetComponent<TextMeshProUGUI>();
        }

        // Configurar colores
        healthBarScript.fullHealthColor = fullHealthColor;
        healthBarScript.lowHealthColor = lowHealthColor;
        healthBarScript.criticalHealthColor = criticalHealthColor;
        healthBarScript.backgroundColor = backgroundColor;

        // Configuración
        healthBarScript.bossName = bossDisplayName;
        healthBarScript.hideWhenDead = true;
        healthBarScript.hideWhenInactive = true;
        healthBarScript.smoothUpdate = true;
        healthBarScript.smoothSpeed = 5f;
        healthBarScript.showDebugLogs = true; // Activar logs para debug

        // Ocultar al inicio (se mostrará cuando el boss se active)
        panel.SetActive(false);

        Debug.Log("<color=green>[AutoSetup] Script SimpleBossHealthBar configurado ✅</color>");
        Debug.Log("<color=cyan>[AutoSetup] La UI se mostrará automáticamente cuando el boss aparezca</color>");
    }

    /// <summary>
    /// Elimina la UI existente
    /// </summary>
    [ContextMenu("🗑️ Eliminar UI del Boss")]
    public void RemoveBossHealthBar()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas != null)
        {
            Transform panel = canvas.transform.Find("BossHealthBarPanel");
            if (panel != null)
            {
                DestroyImmediate(panel.gameObject);
                Debug.Log("<color=yellow>[AutoSetup] UI del boss eliminada</color>");
                setupCompleted = false;
                return;
            }
        }

        Debug.LogWarning("<color=orange>[AutoSetup] No se encontró UI del boss para eliminar</color>");
    }

    /// <summary>
    /// Muestra información del setup actual
    /// </summary>
    [ContextMenu("ℹ️ Info del Setup")]
    public void ShowSetupInfo()
    {
        Debug.Log("<color=cyan>========================================</color>");
        Debug.Log("<color=cyan>[AutoSetup] INFORMACIÓN DEL SETUP</color>");
        Debug.Log("<color=cyan>========================================</color>");

        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas != null)
        {
            Debug.Log("<color=green>✅ Canvas: Encontrado</color>");

            Transform panel = canvas.transform.Find("BossHealthBarPanel");
            if (panel != null)
            {
                Debug.Log("<color=green>✅ BossHealthBarPanel: Encontrado</color>");

                SimpleBossHealthBar script = panel.GetComponent<SimpleBossHealthBar>();
                if (script != null)
                {
                    Debug.Log("<color=green>✅ SimpleBossHealthBar: Configurado</color>");
                    Debug.Log($"   - Boss asignado: {(script.boss != null ? script.boss.gameObject.name : "NO")}");
                    Debug.Log($"   - Health Fill: {(script.healthFillImage != null ? "SÍ" : "NO")}");
                    Debug.Log($"   - Boss Name Text: {(script.bossNameText != null ? "SÍ" : "NO")}");
                    Debug.Log($"   - Health Text: {(script.healthText != null ? "SÍ" : "NO")}");
                }
                else
                {
                    Debug.Log("<color=red>❌ SimpleBossHealthBar: No encontrado</color>");
                }
            }
            else
            {
                Debug.Log("<color=red>❌ BossHealthBarPanel: No encontrado</color>");
            }
        }
        else
        {
            Debug.Log("<color=red>❌ Canvas: No encontrado</color>");
        }

        Boss boss = FindAnyObjectByType<Boss>();
        if (boss != null)
        {
            Debug.Log($"<color=green>✅ Boss en escena: {boss.gameObject.name}</color>");
            Debug.Log($"   - Vida: {boss.health}/{boss.maxHealth}");
        }
        else
        {
            Debug.Log("<color=orange>⚠️ Boss: No encontrado en la escena</color>");
        }

        Debug.Log("<color=cyan>========================================</color>");
    }

    /// <summary>
    /// Test rápido: Simula daño al boss
    /// </summary>
    [ContextMenu("🧪 TEST: Dañar Boss (500 HP)")]
    public void TestDamageBoss()
    {
        Boss testBoss = boss != null ? boss : FindAnyObjectByType<Boss>();

        if (testBoss != null)
        {
            testBoss.TakeDamage(500);
            Debug.Log($"<color=yellow>[AutoSetup TEST] Boss dañado! Vida: {testBoss.health}/{testBoss.maxHealth}</color>");
        }
        else
        {
            Debug.LogWarning("<color=orange>[AutoSetup TEST] No se encontró boss para testear</color>");
        }
    }
}
