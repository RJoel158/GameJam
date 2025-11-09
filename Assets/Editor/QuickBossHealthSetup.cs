using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

/// <summary>
/// Setup rápido con un solo click para añadir vida/energía al boss
/// </summary>
public class QuickBossHealthSetup : EditorWindow
{
    [MenuItem("Game Jam/Quick Setup/Boss Health & Energy (One Click!) 🚀")]
    static void QuickSetup()
    {
        Debug.Log("<color=cyan>======= QUICK BOSS HEALTH SETUP =======</color>");

        // 1. Encontrar el boss en la escena
        GameObject boss = GameObject.Find("God");

        if (boss == null)
        {
            // Buscar por componente God
            God godScript = FindAnyObjectByType<God>();
            if (godScript != null)
            {
                boss = godScript.gameObject;
            }
        }

        if (boss == null)
        {
            EditorUtility.DisplayDialog("Error",
                "No se encontró el boss en la escena!\n\n" +
                "Asegúrate de que:\n" +
                "1. El boss esté spawneado en la escena\n" +
                "2. El GameObject se llame 'God' o tenga el script God.cs",
                "OK");
            Debug.LogError("[QuickBossHealthSetup] No se encontró el boss!");
            return;
        }

        Debug.Log($"<color=green>✓ Boss encontrado: {boss.name}</color>");

        // 2. Añadir BossController
        BossController controller = boss.GetComponent<BossController>();
        if (controller == null)
        {
            controller = boss.AddComponent<BossController>();
            Debug.Log("<color=green>✓ BossController añadido</color>");
        }
        else
        {
            Debug.Log("<color=yellow>○ BossController ya existe</color>");
        }

        // Configurar valores
        controller.maxHealth = 1000;
        controller.maxEnergy = 100f;
        controller.energyCostPerShot = 10f;
        controller.energyRegenRate = 5f;
        controller.unconsciousDuration = 5f;

        // 3. Añadir/configurar Rigidbody
        Rigidbody rb = boss.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = boss.AddComponent<Rigidbody>();
            Debug.Log("<color=green>✓ Rigidbody añadido</color>");
        }
        else
        {
            Debug.Log("<color=yellow>○ Rigidbody ya existe</color>");
        }

        rb.isKinematic = true;
        rb.useGravity = true;
        rb.mass = 1f;

        // 4. Verificar Collider
        Collider col = boss.GetComponent<Collider>();
        if (col == null)
        {
            CapsuleCollider capsule = boss.AddComponent<CapsuleCollider>();
            capsule.height = 2f;
            capsule.radius = 0.5f;
            capsule.center = new Vector3(0, 1f, 0);
            Debug.Log("<color=green>✓ CapsuleCollider añadido</color>");
        }
        else
        {
            Debug.Log($"<color=yellow>○ Collider ya existe ({col.GetType().Name})</color>");
        }

        // 5. Crear UI
        CreateBossUI(controller);

        // 6. Verificar tag del suelo
        CheckGroundTag();

        EditorUtility.SetDirty(boss);

        Debug.Log("<color=green>======= SETUP COMPLETADO =======</color>");
        Debug.Log("<color=cyan>Próximos pasos:</color>");
        Debug.Log("  1. Verifica que el suelo tenga el tag 'Ground'");
        Debug.Log("  2. ¡Entra en Play Mode y prueba!");

        EditorUtility.DisplayDialog("Setup Completo! ✅",
            "El boss ahora tiene:\n\n" +
            "✅ Sistema de vida\n" +
            "✅ Sistema de energía\n" +
            "✅ UI de barras\n" +
            "✅ Mecánica de inconsciencia\n\n" +
            "IMPORTANTE: Asegúrate de que el suelo tenga el tag 'Ground'",
            "OK");
    }

    static void CreateBossUI(BossController bossController)
    {
        // Buscar Canvas existente
        Canvas canvas = FindAnyObjectByType<Canvas>();

        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("BossUI_Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            Debug.Log("<color=green>✓ Canvas creado</color>");
        }
        else
        {
            Debug.Log($"<color=yellow>○ Usando Canvas existente: {canvas.name}</color>");
        }

        // Verificar si ya existe BossUI
        BossHealthBarUI existingUI = FindAnyObjectByType<BossHealthBarUI>();
        if (existingUI != null)
        {
            Debug.Log("<color=yellow>○ BossUI ya existe, actualizando referencias...</color>");
            existingUI.bossController = bossController;
            EditorUtility.SetDirty(existingUI);
            return;
        }

        // Crear contenedor
        GameObject container = new GameObject("BossUI_Container");
        container.transform.SetParent(canvas.transform, false);

        RectTransform containerRect = container.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 1f);
        containerRect.anchorMax = new Vector2(0.5f, 1f);
        containerRect.pivot = new Vector2(0.5f, 1f);
        containerRect.anchoredPosition = new Vector2(0, -50);
        containerRect.sizeDelta = new Vector2(400, 120);

        // Background
        GameObject bgPanel = new GameObject("Background");
        bgPanel.transform.SetParent(container.transform, false);
        Image bgImage = bgPanel.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.7f);

        RectTransform bgRect = bgPanel.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        // Texto "BOSS"
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

        // Barra de vida
        GameObject healthBar = CreateBar("HealthBar", container.transform, 0.35f, 0.55f,
            new Color(0.8f, 0.1f, 0.1f));

        // Barra de energía
        GameObject energyBar = CreateBar("EnergyBar", container.transform, 0.1f, 0.3f,
            new Color(0.2f, 0.5f, 1f));

        // Añadir script
        BossHealthBarUI uiScript = container.AddComponent<BossHealthBarUI>();
        uiScript.bossController = bossController;
        uiScript.bossUIContainer = container;
        uiScript.healthFillImage = healthBar.transform.Find("Fill").GetComponent<Image>();
        uiScript.energyFillImage = energyBar.transform.Find("Fill").GetComponent<Image>();

        Debug.Log("<color=green>✓ BossUI creada</color>");
    }

    static GameObject CreateBar(string name, Transform parent, float anchorMinY, float anchorMaxY, Color fillColor)
    {
        GameObject bar = new GameObject(name);
        bar.transform.SetParent(parent, false);

        RectTransform barRect = bar.AddComponent<RectTransform>();
        barRect.anchorMin = new Vector2(0.1f, anchorMinY);
        barRect.anchorMax = new Vector2(0.9f, anchorMaxY);
        barRect.sizeDelta = Vector2.zero;

        // Background
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(bar.transform, false);
        Image bgImage = bg.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        RectTransform bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        // Fill
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
        fillRect.sizeDelta = new Vector2(-4, -4);

        // Label
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

    static void CheckGroundTag()
    {
        // Buscar el terrain o suelo
        Terrain terrain = FindAnyObjectByType<Terrain>();
        if (terrain != null)
        {
            if (terrain.tag != "Ground")
            {
                Debug.LogWarning($"<color=orange>⚠ El Terrain '{terrain.name}' no tiene el tag 'Ground'</color>");
                Debug.Log("<color=yellow>Asignando tag 'Ground' al terrain...</color>");

                // Añadir tag si no existe
                if (!TagExists("Ground"))
                {
                    Debug.LogWarning("<color=orange>El tag 'Ground' no existe. Créalo manualmente en Tags & Layers</color>");
                }
                else
                {
                    terrain.tag = "Ground";
                    EditorUtility.SetDirty(terrain);
                    Debug.Log("<color=green>✓ Tag 'Ground' asignado al terrain</color>");
                }
            }
            else
            {
                Debug.Log("<color=green>✓ Terrain tiene el tag 'Ground'</color>");
            }
        }
        else
        {
            Debug.LogWarning("<color=orange>⚠ No se encontró Terrain. Asigna manualmente el tag 'Ground' al suelo</color>");
        }
    }

    static bool TagExists(string tagName)
    {
        // Check if tag exists in TagManager
        var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
            if (t.stringValue.Equals(tagName)) return true;
        }
        return false;
    }
}
