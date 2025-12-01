using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Barra de vida simple para el boss en la parte superior de la pantalla
/// Similar al estilo de los enemigos comunes pero en Screen Space
/// </summary>
public class SimpleBossHealthBar : MonoBehaviour
{
    [Header("Boss Reference")]
    [Tooltip("El componente Boss que estamos monitoreando (se intentará resolver automáticamente si se deja vacío). Si no existe, el sistema intentará enlazar a 'God' como fallback.")]
    public Boss boss;

    [Tooltip("Fallback: si no hay componente Boss, se buscará un componente God y se mostrará su vida.")]
    public God god;

    [Tooltip("En lugar de asignar el componente Boss directamente, arrastra aquí el GameObject del boss y el sistema intentará resolver el componente automáticamente.")]
    public GameObject bossObject;

    [Header("UI Elements")]
    [Tooltip("Image con fillAmount para la barra de vida")]
    public Image healthFillImage;

    [Tooltip("Image para el fondo de la barra")]
    public Image healthBackgroundImage;

    [Tooltip("Texto opcional para mostrar nombre del boss")]
    public TMPro.TextMeshProUGUI bossNameText;

    [Tooltip("Texto opcional para mostrar vida numérica")]
    public TMPro.TextMeshProUGUI healthText;

    [Header("Colors")]
    public Color fullHealthColor = new Color(0.8f, 0.1f, 0.1f, 1f); // Rojo
    public Color lowHealthColor = new Color(1f, 0.5f, 0f, 1f); // Naranja
    public Color criticalHealthColor = Color.yellow;
    public Color backgroundColor = new Color(0f, 0f, 0f, 0.6f);

    [Range(0f, 1f)]
    public float lowHealthThreshold = 0.5f;
    [Range(0f, 1f)]
    public float criticalHealthThreshold = 0.25f;

    [Header("Settings")]
    public string bossName = "G O T T";
    public bool hideWhenDead = true;
    public bool hideWhenInactive = true;
    public bool smoothUpdate = true;
    public float smoothSpeed = 5f;

    [Header("Debug")]
    public bool showDebugLogs = false;

    [Tooltip("Si es true, la UI se mostrará automáticamente en Start() si se resolvió el boss. Útil para pruebas en Play mode.")]
    public bool autoShowWhenBossAssigned = true;

    [Tooltip("Si es true, la barra del boss permanecerá siempre activa en pantalla (ignora auto-hide).")]
    public bool alwaysShow = false;

    private GameObject uiContainer;
    private float currentFillAmount;
    private bool wasInactive = false; // Para detectar cuando el boss se activa

    // Global request flag so Show requests survive script execution order
    private static bool globalShowRequested = false;

    void Start()
    {
        // Encontrar el contenedor de UI
        uiContainer = gameObject;

        // Configurar colores
        if (healthBackgroundImage != null)
        {
            healthBackgroundImage.color = backgroundColor;
        }

        // Configurar nombre
        if (bossNameText != null)
        {
            bossNameText.text = bossName;
        }

        // Inicializar fill amount
        currentFillAmount = 1f;

        // IMPORTANTE: Ocultar al inicio y esperar a que el boss se active
        if (uiContainer != null)
        {
            // If a global show was already requested by another script, keep visible
            if (globalShowRequested || (autoShowWhenBossAssigned && boss != null) || alwaysShow)
            {
                uiContainer.SetActive(true);
                Debug.Log("[SimpleBossHealthBar] UI mantenida activa al inicio (global request/autoShow/alwaysShow).");
            }
            else
            {
                uiContainer.SetActive(false);
                Debug.Log("[SimpleBossHealthBar] UI ocultada al inicio - esperando al boss");
            }
        }

        // Buscar el boss (incluso si está inactivo)
        FindBoss();

        // If we resolved a boss at startup and auto-show is enabled, show the UI immediately
        if (autoShowWhenBossAssigned && boss != null)
        {
            Debug.Log("[SimpleBossHealthBar] boss encontrado en Start() y autoShowWhenBossAssigned=true -> mostrando UI.");
            wasInactive = false;
            hideWhenInactive = false;
            if (uiContainer != null) uiContainer.SetActive(true);
        }
    }

    void Update()
    {
        if (boss == null && god == null)
        {
            // Buscar el boss/god activamente (incluso si está inactivo)
            FindBoss();
            return;
        }

        UpdateVisibility();
        UpdateHealthBar();
    }

    void FindBoss()
    {
        // 1) If a GameObject was explicitly assigned, try resolve Boss from it first
        if (boss == null && bossObject != null)
        {
            var fromObj = bossObject.GetComponent<Boss>() ?? bossObject.GetComponentInChildren<Boss>(true);
            if (fromObj != null)
            {
                boss = fromObj;
                Debug.Log($"<color=green>[SimpleBossHealthBar] Boss resuelto desde bossObject: {boss.gameObject.name} (Activo: {boss.gameObject.activeInHierarchy})</color>");
                return;
            }
            else
            {
                Debug.LogWarning($"[SimpleBossHealthBar] bossObject asignado ('{bossObject.name}') no contiene componente Boss. Intentando God como fallback.");
                var godFromObj = bossObject.GetComponent<God>() ?? bossObject.GetComponentInChildren<God>(true);
                if (godFromObj != null)
                {
                    god = godFromObj;
                    Debug.Log($"<color=green>[SimpleBossHealthBar] God resuelto desde bossObject: {god.gameObject.name} (Activo: {god.gameObject.activeInHierarchy})</color>");
                    return;
                }
            }
        }

        // 2) Search all loaded scenes' root GameObjects and their children (handles scene setups)
        for (int s = 0; s < SceneManager.sceneCount; ++s)
        {
            var scene = SceneManager.GetSceneAt(s);
            if (!scene.isLoaded) continue;
            var roots = scene.GetRootGameObjects();
            foreach (var root in roots)
            {
                var found = root.GetComponentInChildren<Boss>(true);
                if (found != null)
                {
                    boss = found;
                    Debug.Log($"<color=green>[SimpleBossHealthBar] Boss resuelto desde escena '{scene.name}' root '{root.name}' -> {boss.gameObject.name} (Activo: {boss.gameObject.activeInHierarchy})</color>");
                    return;
                }
                // try God as fallback
                var foundGod = root.GetComponentInChildren<God>(true);
                if (foundGod != null)
                {
                    god = foundGod;
                    Debug.Log($"<color=green>[SimpleBossHealthBar] God resuelto desde escena '{scene.name}' root '{root.name}' -> {god.gameObject.name} (Activo: {god.gameObject.activeInHierarchy})</color>");
                    return;
                }
            }
        }

        // 3) Fallback: Resources.FindObjectsOfTypeAll to include disabled objects and assets
        Boss[] allBosses = Resources.FindObjectsOfTypeAll<Boss>();
        if (allBosses != null && allBosses.Length > 0)
        {
            Debug.Log($"[SimpleBossHealthBar] Resources.FindObjectsOfTypeAll found {allBosses.Length} Boss instances (listing)...");
            foreach (Boss b in allBosses)
            {
                string sceneName = "<no-scene>";
                try { sceneName = b.gameObject.scene.name; } catch { }
                Debug.Log($"  - {b.gameObject.name} (active={b.gameObject.activeInHierarchy}) scene={sceneName}");
                // pick first that appears to be part of a scene
                if (b.gameObject.scene.name != null)
                {
                    boss = b;
                    Debug.Log($"<color=green>[SimpleBossHealthBar] Boss seleccionado: {boss.gameObject.name} (Activo: {boss.gameObject.activeInHierarchy})</color>");
                    return;
                }
            }
        }

        // Fallback: try to find any God instances
        God[] allGods = Resources.FindObjectsOfTypeAll<God>();
        if (allGods != null && allGods.Length > 0)
        {
            Debug.Log($"[SimpleBossHealthBar] Resources.FindObjectsOfTypeAll found {allGods.Length} God instances (listing)...");
            foreach (God g in allGods)
            {
                string sceneName = "<no-scene>";
                try { sceneName = g.gameObject.scene.name; } catch { }
                Debug.Log($"  - {g.gameObject.name} (active={g.gameObject.activeInHierarchy}) scene={sceneName}");
                if (g.gameObject.scene.name != null)
                {
                    god = g;
                    Debug.Log($"<color=green>[SimpleBossHealthBar] God seleccionado: {god.gameObject.name} (Activo: {god.gameObject.activeInHierarchy})</color>");
                    return;
                }
            }
        }

        Debug.LogWarning("[SimpleBossHealthBar] No se encontró Boss en la escena tras las búsquedas; asegúrate de que exista un componente 'Boss' en el GameObject del boss o asigna 'bossObject' en el inspector.");
    }

    void UpdateVisibility()
    {
        if (alwaysShow)
        {
            // If alwaysShow is enabled, force visible and skip other rules
            if (uiContainer != null && !uiContainer.activeSelf)
            {
                uiContainer.SetActive(true);
                Debug.Log("[SimpleBossHealthBar] alwaysShow=true -> forcing UI visible.");
            }
            return;
        }

        bool shouldShow = true;
        // Ocultar si el boss/god está muerto
        bool isDead = false;
        GameObject targetGO = null;
        if (boss != null) targetGO = boss.gameObject;
        else if (god != null) targetGO = god.gameObject;

        if (hideWhenDead)
        {
            if (boss != null) isDead = boss.dead;
            else if (god != null) isDead = false; // God doesn't have 'dead' flag — treat as alive unless destroyed
            if (isDead)
            {
                shouldShow = false;
                if (showDebugLogs) Debug.Log("[SimpleBossHealthBar] Ocultando - boss muerto");
            }
        }

        // Ocultar si el boss/god no está activo
        bool targetIsActive = (targetGO != null) ? targetGO.activeInHierarchy : false;
        if (hideWhenInactive && !targetIsActive)
        {
            shouldShow = false;
            wasInactive = true;
            if (showDebugLogs) Debug.Log("[SimpleBossHealthBar] Ocultando - boss inactivo");
        }
        else if (targetIsActive && wasInactive)
        {
            // El boss acaba de activarse
            shouldShow = true;
            wasInactive = false;
            Debug.Log("<color=green>[SimpleBossHealthBar] ¡BOSS ACTIVADO! Mostrando UI</color>");
        }

        if (uiContainer != null && uiContainer.activeSelf != shouldShow)
        {
            uiContainer.SetActive(shouldShow);
            Debug.Log($"<color=yellow>[SimpleBossHealthBar] UI {(shouldShow ? "MOSTRADA ✅" : "OCULTADA ❌")}</color>");
        }
    }

    void UpdateHealthBar()
    {
        if (healthFillImage == null) return;

        // Calcular porcentaje de vida (0-1) desde Boss o God
        int currentHealth = 0;
        int currentMax = 1;
        if (boss != null)
        {
            currentHealth = boss.health;
            currentMax = Mathf.Max(1, boss.maxHealth);
        }
        else if (god != null)
        {
            currentHealth = god.health;
            currentMax = Mathf.Max(1, god.maxHealth);
        }

        float healthPercent = Mathf.Clamp01((float)currentHealth / (float)currentMax);

        // Actualizar fill amount (con o sin suavizado)
        if (smoothUpdate)
        {
            currentFillAmount = Mathf.Lerp(currentFillAmount, healthPercent, Time.deltaTime * smoothSpeed);
            healthFillImage.fillAmount = currentFillAmount;
        }
        else
        {
            healthFillImage.fillAmount = healthPercent;
        }

        // Cambiar color según la vida
        Color targetColor = fullHealthColor;
        if (healthPercent <= criticalHealthThreshold)
        {
            targetColor = criticalHealthColor;
        }
        else if (healthPercent <= lowHealthThreshold)
        {
            // Interpolar entre rojo y naranja
            float t = (healthPercent - criticalHealthThreshold) / (lowHealthThreshold - criticalHealthThreshold);
            targetColor = Color.Lerp(lowHealthColor, fullHealthColor, t);
        }

        healthFillImage.color = targetColor;

        // Actualizar texto numérico (opcional)
        if (healthText != null)
        {
            healthText.text = $"{currentHealth} / {currentMax}";
        }
    }

    /// <summary>
    /// Muestra la UI del boss
    /// </summary>
    public void ShowBossUI()
    {
        Debug.Log("<color=cyan>[SimpleBossHealthBar] ===== ShowBossUI() LLAMADO =====</color>");

        // Buscar el boss si no está asignado
        if (boss == null && god == null)
        {
            FindBoss();
        }

        wasInactive = false; // Resetear el flag
        hideWhenInactive = false; // IMPORTANTE: Desactivar auto-hide para que permanezca visible
        // mark global request so Start() won't hide it later
        globalShowRequested = true;

        // Ensure uiContainer is valid even if Start() hasn't run yet
        if (uiContainer == null)
        {
            uiContainer = this.gameObject;
            if (uiContainer == null)
            {
                Debug.LogError("<color=red>[SimpleBossHealthBar] ❌ ERROR: uiContainer es null y no se pudo resolver fallback!</color>");
                return;
            }
        }

        uiContainer.SetActive(true);
        Debug.Log("<color=green>[SimpleBossHealthBar] ✅ UI ACTIVADA MANUALMENTE - Debe estar visible ahora!</color>");
    }    /// <summary>
         /// Oculta la UI del boss
         /// </summary>
    public void HideBossUI()
    {
        if (showDebugLogs)
        {
            Debug.Log("[SimpleBossHealthBar] HideBossUI() llamado");
        }

        // If alwaysShow is enabled, ignore hide requests
        if (alwaysShow)
        {
            if (showDebugLogs) Debug.Log("[SimpleBossHealthBar] HideBossUI ignored because alwaysShow=true");
            return;
        }

        // cancel global request
        globalShowRequested = false;

        if (uiContainer == null)
        {
            uiContainer = this.gameObject;
        }

        if (uiContainer != null)
        {
            uiContainer.SetActive(false);
        }
    }

    /// <summary>
    /// Force refresh the UI immediately (resolve boss if missing, update visibility and health bar).
    /// </summary>
    public void RefreshUI()
    {
        if (boss == null && god == null) FindBoss();
        UpdateVisibility();
        UpdateHealthBar();
    }
}
