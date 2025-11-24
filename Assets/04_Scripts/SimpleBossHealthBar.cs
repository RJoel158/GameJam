using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Barra de vida simple para el boss en la parte superior de la pantalla
/// Similar al estilo de los enemigos comunes pero en Screen Space
/// </summary>
public class SimpleBossHealthBar : MonoBehaviour
{
    [Header("Boss Reference")]
    [Tooltip("El boss que estamos monitoreando")]
    public Boss boss;

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
    public string bossName = "JEFE";
    public bool hideWhenDead = true;
    public bool hideWhenInactive = true;
    public bool smoothUpdate = true;
    public float smoothSpeed = 5f;

    [Header("Debug")]
    public bool showDebugLogs = false;

    private GameObject uiContainer;
    private float currentFillAmount;
    private bool wasInactive = false; // Para detectar cuando el boss se activa

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
            uiContainer.SetActive(false);
            Debug.Log("[SimpleBossHealthBar] UI ocultada al inicio - esperando al boss");
        }

        // Buscar el boss (incluso si está inactivo)
        FindBoss();
    }

    void Update()
    {
        if (boss == null)
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
        // Buscar el boss incluso si está inactivo usando Resources
        Boss[] allBosses = Resources.FindObjectsOfTypeAll<Boss>();
        foreach (Boss b in allBosses)
        {
            // Verificar que sea un objeto de la escena y no un prefab
            if (b.gameObject.scene.name != null)
            {
                boss = b;
                Debug.Log($"<color=green>[SimpleBossHealthBar] Boss encontrado: {boss.gameObject.name} (Activo: {boss.gameObject.activeInHierarchy})</color>");
                break;
            }
        }

        if (boss == null)
        {
            Debug.LogWarning("[SimpleBossHealthBar] No se encontró Boss en la escena!");
        }
    }

    void UpdateVisibility()
    {
        bool shouldShow = true;

        // Ocultar si el boss está muerto
        if (hideWhenDead && boss.dead)
        {
            shouldShow = false;
            if (showDebugLogs)
            {
                Debug.Log("[SimpleBossHealthBar] Ocultando - boss muerto");
            }
        }

        // Ocultar si el boss no está activo
        bool bossIsActive = boss.gameObject.activeInHierarchy;
        if (hideWhenInactive && !bossIsActive)
        {
            shouldShow = false;
            wasInactive = true;
            if (showDebugLogs)
            {
                Debug.Log("[SimpleBossHealthBar] Ocultando - boss inactivo");
            }
        }
        else if (bossIsActive && wasInactive)
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

        // Calcular porcentaje de vida (0-1)
        float healthPercent = Mathf.Clamp01((float)boss.health / (float)boss.maxHealth);

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
            healthText.text = $"{boss.health} / {boss.maxHealth}";
        }
    }

    /// <summary>
    /// Muestra la UI del boss
    /// </summary>
    public void ShowBossUI()
    {
        Debug.Log("<color=cyan>[SimpleBossHealthBar] ===== ShowBossUI() LLAMADO =====</color>");

        // Buscar el boss si no está asignado
        if (boss == null)
        {
            FindBoss();
        }

        wasInactive = false; // Resetear el flag
        hideWhenInactive = false; // IMPORTANTE: Desactivar auto-hide para que permanezca visible

        if (uiContainer != null)
        {
            uiContainer.SetActive(true);
            Debug.Log("<color=green>[SimpleBossHealthBar] ✅ UI ACTIVADA MANUALMENTE - Debe estar visible ahora!</color>");
        }
        else
        {
            Debug.LogError("<color=red>[SimpleBossHealthBar] ❌ ERROR: uiContainer es null!</color>");
        }
    }    /// <summary>
         /// Oculta la UI del boss
         /// </summary>
    public void HideBossUI()
    {
        if (showDebugLogs)
        {
            Debug.Log("[SimpleBossHealthBar] HideBossUI() llamado");
        }

        if (uiContainer != null)
        {
            uiContainer.SetActive(false);
        }
    }
}
