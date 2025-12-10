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
    [Tooltip("El componente Boss que estamos monitoreando (se intentará resolver automáticamente si se deja vacío)")]
    public Boss boss;

    [Tooltip("Componente God alternativo (si el boss usa God en lugar de Boss)")]
    private God godBoss;

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
    public Color fullHealthColor = new Color(0.9f, 0.15f, 0.15f, 1f); // Rojo brillante
    public Color lowHealthColor = new Color(1f, 0.4f, 0f, 1f); // Naranja intenso más visible
    public Color criticalHealthColor = new Color(1f, 0.9f, 0f, 1f); // Amarillo más brillante
    public Color backgroundColor = new Color(0f, 0f, 0f, 0.6f);

    [Range(0f, 1f)]
    public float lowHealthThreshold = 0.67f; // 67% = después de 1ra fase (pierde 33%)
    [Range(0f, 1f)]
    public float criticalHealthThreshold = 0.34f; // 34% = después de 2da fase (pierde otro 33%)

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
        if (autoShowWhenBossAssigned && (boss != null || godBoss != null))
        {
            Debug.Log("[SimpleBossHealthBar] boss/god encontrado en Start() y autoShowWhenBossAssigned=true -> mostrando UI.");
            wasInactive = false;
            hideWhenInactive = false;
            if (uiContainer != null) uiContainer.SetActive(true);
        }
    }

    void Update()
    {
        if (boss == null && godBoss == null)
        {
            // Buscar el boss activamente (incluso si está inactivo)
            FindBoss();
            return;
        }

        UpdateVisibility();
        UpdateHealthBar();
    }

    void FindBoss()
    {
        // 1) If a GameObject was explicitly assigned, try resolve Boss from it first
        if (boss == null && godBoss == null && bossObject != null)
        {
            var fromObj = bossObject.GetComponent<Boss>() ?? bossObject.GetComponentInChildren<Boss>(true);
            if (fromObj != null)
            {
                boss = fromObj;
                Debug.Log($"<color=green>[SimpleBossHealthBar] Boss resuelto desde bossObject: {boss.gameObject.name} (Activo: {boss.gameObject.activeInHierarchy})</color>");
                return;
            }

            // Try God component if Boss not found
            var godObj = bossObject.GetComponent<God>() ?? bossObject.GetComponentInChildren<God>(true);
            if (godObj != null)
            {
                godBoss = godObj;
                Debug.Log($"<color=green>[SimpleBossHealthBar] God resuelto desde bossObject: {godBoss.gameObject.name} (Activo: {godBoss.gameObject.activeInHierarchy})</color>");
                return;
            }
            else
            {
                Debug.LogWarning($"[SimpleBossHealthBar] bossObject asignado ('{bossObject.name}') no contiene componente Boss ni God.");
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

                // Try God if Boss not found
                var foundGod = root.GetComponentInChildren<God>(true);
                if (foundGod != null)
                {
                    godBoss = foundGod;
                    Debug.Log($"<color=green>[SimpleBossHealthBar] God resuelto desde escena '{scene.name}' root '{root.name}' -> {godBoss.gameObject.name} (Activo: {godBoss.gameObject.activeInHierarchy})</color>");
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

        // 4) Try God as last resort
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
                    godBoss = g;
                    Debug.Log($"<color=green>[SimpleBossHealthBar] God seleccionado: {godBoss.gameObject.name} (Activo: {godBoss.gameObject.activeInHierarchy})</color>");
                    return;
                }
            }
        }

        Debug.LogWarning("[SimpleBossHealthBar] No se encontró Boss ni God en la escena tras las búsquedas; asegúrate de que exista un componente 'Boss' o 'God' en el GameObject del boss o asigna 'bossObject' en el inspector.");
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

        // Determine which component we're using
        bool isDead = false;
        bool isActive = false;

        if (boss != null)
        {
            isDead = boss.dead;
            isActive = boss.gameObject.activeInHierarchy;
        }
        else if (godBoss != null)
        {
            isDead = (godBoss.health <= 0);
            isActive = godBoss.gameObject.activeInHierarchy;
        }

        // Ocultar si el boss está muerto
        if (hideWhenDead && isDead)
        {
            shouldShow = false;
            if (showDebugLogs)
            {
                Debug.Log("[SimpleBossHealthBar] Ocultando - boss muerto");
            }
        }

        // Ocultar si el boss no está activo
        if (hideWhenInactive && !isActive)
        {
            shouldShow = false;
            wasInactive = true;
            if (showDebugLogs)
            {
                Debug.Log("[SimpleBossHealthBar] Ocultando - boss inactivo");
            }
        }
        else if (isActive && wasInactive)
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

        // Get health values from either Boss or God
        int currentHealth = 0;
        int maxHealth = 1;

        if (boss != null)
        {
            currentHealth = boss.health;
            maxHealth = boss.maxHealth;
        }
        else if (godBoss != null)
        {
            currentHealth = godBoss.health;
            maxHealth = godBoss.maxHealth;
        }
        else
        {
            return; // No boss reference available
        }

        // Calcular porcentaje de vida (0-1)
        float healthPercent = Mathf.Clamp01((float)currentHealth / (float)maxHealth);

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

        // Cambiar color según la vida con transiciones suaves y visibles
        Color targetColor = fullHealthColor;
        if (healthPercent <= criticalHealthThreshold)
        {
            // Tercera fase (≤34%): Amarillo brillante
            targetColor = criticalHealthColor;
        }
        else if (healthPercent <= lowHealthThreshold)
        {
            // Segunda fase (34%-67%): Transición de naranja a amarillo
            float t = (healthPercent - criticalHealthThreshold) / (lowHealthThreshold - criticalHealthThreshold);
            targetColor = Color.Lerp(criticalHealthColor, lowHealthColor, t);
        }
        else
        {
            // Primera fase (>67%): Rojo brillante
            targetColor = fullHealthColor;
        }

        healthFillImage.color = targetColor;

        // Actualizar texto numérico (opcional)
        if (healthText != null)
        {
            healthText.text = $"{currentHealth} / {maxHealth}";
        }
    }

    /// <summary>
    /// Muestra la UI del boss
    /// </summary>
    public void ShowBossUI()
    {
        Debug.Log("<color=cyan>[SimpleBossHealthBar] ===== ShowBossUI() LLAMADO =====</color>");

        // Buscar el boss si no está asignado
        if (boss == null && godBoss == null)
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
        if (boss == null) FindBoss();
        UpdateVisibility();
        UpdateHealthBar();
    }
}
