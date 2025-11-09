using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla la UI de la barra de vida y energía del boss
/// </summary>
public class BossHealthBarUI : MonoBehaviour
{
    [Header("Boss Reference")]
    [Tooltip("El BossController que estamos monitoreando")]
    public BossController bossController;

    [Header("UI Elements")]
    [Tooltip("Slider para la barra de vida (o Image con fillAmount)")]
    public Slider healthSlider;
    public Image healthFillImage; // Alternativa si usas Image en lugar de Slider

    [Tooltip("Slider para la barra de energía")]
    public Slider energySlider;
    public Image energyFillImage;

    [Header("Colors")]
    public Color healthColor = Color.red;
    public Color energyColor = new Color(0.2f, 0.5f, 1f); // Azul
    public Color lowEnergyColor = Color.yellow;
    [Range(0f, 1f)]
    public float lowEnergyThreshold = 0.3f;

    [Header("Visibility")]
    public GameObject bossUIContainer; // El GameObject que contiene toda la UI del boss
    public bool hideWhenBossDead = true;
    public bool hideWhenBossNotActive = true;

    void Start()
    {
        // Auto-find boss controller si no está asignado
        if (bossController == null)
        {
            bossController = FindAnyObjectByType<BossController>();
            if (bossController == null)
            {
                Debug.LogWarning("[BossHealthBarUI] No BossController found in scene!");
            }
        }

        // Configurar colores iniciales
        if (healthFillImage != null)
        {
            healthFillImage.color = healthColor;
        }
        if (energyFillImage != null)
        {
            energyFillImage.color = energyColor;
        }

        // Ocultar la UI al inicio si el boss no está activo
        if (bossUIContainer != null && hideWhenBossNotActive)
        {
            bossUIContainer.SetActive(false);
        }
    }

    void Update()
    {
        if (bossController == null) return;

        // Mostrar/ocultar UI según el estado del boss
        UpdateVisibility();

        // Actualizar barras
        UpdateHealthBar();
        UpdateEnergyBar();
    }

    void UpdateVisibility()
    {
        if (bossUIContainer == null) return;

        bool shouldShow = true;

        // Ocultar si el boss está muerto
        if (hideWhenBossDead && bossController.IsDead())
        {
            shouldShow = false;
        }

        // Ocultar si el boss no está activo
        if (hideWhenBossNotActive && !bossController.gameObject.activeInHierarchy)
        {
            shouldShow = false;
        }

        bossUIContainer.SetActive(shouldShow);
    }

    void UpdateHealthBar()
    {
        float healthPercent = bossController.GetHealthPercent();

        if (healthSlider != null)
        {
            healthSlider.value = healthPercent;
        }

        if (healthFillImage != null)
        {
            healthFillImage.fillAmount = healthPercent;
        }
    }

    void UpdateEnergyBar()
    {
        float energyPercent = bossController.GetEnergyPercent();

        if (energySlider != null)
        {
            energySlider.value = energyPercent;
        }

        if (energyFillImage != null)
        {
            energyFillImage.fillAmount = energyPercent;

            // Cambiar color cuando la energía está baja
            if (energyPercent <= lowEnergyThreshold)
            {
                energyFillImage.color = Color.Lerp(lowEnergyColor, energyColor, energyPercent / lowEnergyThreshold);
            }
            else
            {
                energyFillImage.color = energyColor;
            }
        }
    }

    /// <summary>
    /// Llama a este método cuando el boss es spawneado para mostrar la UI
    /// </summary>
    public void ShowBossUI()
    {
        if (bossUIContainer != null)
        {
            bossUIContainer.SetActive(true);
        }
    }

    /// <summary>
    /// Oculta la UI del boss
    /// </summary>
    public void HideBossUI()
    {
        if (bossUIContainer != null)
        {
            bossUIContainer.SetActive(false);
        }
    }
}
