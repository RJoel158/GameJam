using UnityEngine;
using UnityEngine.UI;
using StarterAssets;

public class FaseColorController : MonoBehaviour
{
    [SerializeField]
    private Image faseImage;
    
    [SerializeField]
    private Image manaImage;
    
    [SerializeField]
    private Image staminaImage;
    
    [SerializeField]
    private Color startColor = new Color(1f, 0f, 0f, 1f); // Rojo (FF0000)
    
    [SerializeField]
    private Color endColor = new Color(1f, 1f, 1f, 1f); // Blanco (FFFFFF)

    private ThirdPersonController thirdPersonController;

    [Header("Stamina")]
    [Range(0f, 100f)]
    public float staminaPercent = 100f;
    public int stamina = 2000;
    public int maxStamina = 2000;
    
    [SerializeField]
    private float sprintStaminaCost = 400f; // Stamina consumed per second while sprinting
    [SerializeField]
    private float attackStaminaCost = 500f; // Stamina consumed per attack (1.25x más que correr)
    [SerializeField]
    private int staminaDecrementStep = 200; // Stamina baja en incrementos de este valor
    private float currentStamina; // Usar float interno para precisión
    private float staminaTickTimer = 0f; // Timer para controlar cuándo bajar stamina
    private bool lastAttackInputState = false; // Guardar el estado anterior del input de ataque
    private float attackCooldownTimer = 0f; // Cooldown entre ataques

    [Header("Mana")]
    [Range(0f, 100f)]
    public float manaPercent = 0f;
    public int mana = 0;
    public int maxMana = 100;
    
    private float currentMana = 0f; // Mana actual (float para precisión)
    private float manaTickTimer = 0f; // Timer para controlar cuándo subir mana
    private int manaIncrementStep = 25; // Mana sube en incrementos de 25

    private void Start()
    {
        // Buscar el ThirdPersonController
        thirdPersonController = FindFirstObjectByType<ThirdPersonController>();
        if (thirdPersonController == null)
        {
            Debug.LogError("ThirdPersonController no encontrado en la escena");
        }

        // Buscar las imágenes automáticamente si no están asignadas
        if (faseImage == null)
        {
            faseImage = GameObject.Find("fase")?.GetComponent<Image>();
        }
        
        if (manaImage == null)
        {
            manaImage = GameObject.Find("mana")?.GetComponent<Image>();
            // Si no encuentra "mana" directamente, busca dentro de "fase"
            if (manaImage == null && faseImage != null)
            {
                manaImage = faseImage.transform.Find("mana")?.GetComponent<Image>();
            }
        }

        if (staminaImage == null)
        {
            staminaImage = GameObject.Find("estamina")?.GetComponent<Image>();
            if (staminaImage == null)
            {
                Debug.LogWarning("No se encontró la imagen 'estamina'. Busca manualmente en el Inspector.");
            }
        }

        // Inicializar color en rojo
        if (faseImage != null)
        {
            faseImage.color = startColor;
        }
        
        // Inicializar mana en 0
        if (manaImage != null)
        {
            manaImage.fillAmount = 0f;
        }

        // Inicializar stamina correctamente
        stamina = maxStamina;
        currentStamina = maxStamina;

        // Inicializar mana correctamente
        mana = 0;
        currentMana = 0f;
    }

    private void Update()
    {
        if (thirdPersonController != null)
        {
            HandleStaminaConsumption();
            HandleManaRegeneration();
            UpdateUI();
        }
    }

    private void HandleStaminaConsumption()
    {
        // Consume stamina when sprinting de forma fluida
        if (thirdPersonController._input.sprint && thirdPersonController._input.move != Vector2.zero && currentStamina > 0)
        {
            // Velocidad de consumo: 400 stamina por segundo (fluido y continuo)
            currentStamina -= sprintStaminaCost * Time.deltaTime;
            if (currentStamina < 0) currentStamina = 0;
        }
        // NO regenerate si sigue presionando Shift o atacando
        else if (!thirdPersonController._input.sprint && !thirdPersonController._input.attack)
        {
            // Regenerate stamina cuando no está presionando Shift ni atacando
            float regenRate = sprintStaminaCost * 0.5f;
            currentStamina += regenRate * Time.deltaTime;
            
            if (currentStamina > maxStamina) currentStamina = maxStamina;
        }

        // Convertir float a int para el stamina público
        stamina = (int)currentStamina;
        staminaPercent = (stamina * 100) / maxStamina;
    }

    private void HandleManaRegeneration()
    {
        // Mana sube de forma fluida y continua
        if (currentMana < maxMana)
        {
            // Velocidad de regeneración: 15 mana por segundo (suave y fluido)
            float manaRegenRate = 15f;
            currentMana += manaRegenRate * Time.deltaTime;
            
            if (currentMana > maxMana) currentMana = maxMana;
        }

        // Convertir float a int para el mana público
        mana = (int)currentMana;
        manaPercent = (mana * 100) / maxMana;
    }

    private void UpdateUI()
    {
        if (manaImage != null)
        {
            float fillAmount = manaPercent / 100f;
            manaImage.fillAmount = fillAmount;
            
            // Interpolar color basado en el fillAmount
            Color newColor = Color.Lerp(startColor, endColor, fillAmount);
            faseImage.color = newColor;
        }

        if (staminaImage != null)
        {
            float fillAmount = staminaPercent / 100f;
            staminaImage.fillAmount = fillAmount;
        }
        else
        {
            if (staminaImage == null)
            {
                staminaImage = GameObject.Find("estamina")?.GetComponent<Image>();
            }
        }
    }

    // Método público para consumir stamina en ataques
    public bool TryConsumeStaminaForAttack()
    {
        if (currentStamina >= attackStaminaCost && currentStamina > 0)
        {
            currentStamina -= attackStaminaCost;
            if (currentStamina < 0) currentStamina = 0;
            stamina = (int)currentStamina;
            return true;
        }
        return false;
    }

    // Propiedad pública para acceder al stamina actual
    public int GetCurrentStamina()
    {
        return stamina;
    }

    // Propiedad pública para acceder al stamina máximo
    public int GetMaxStamina()
    {
        return maxStamina;
    }

    // Propiedad pública para acceder a la capacidad de sprint
    public bool CanSprint()
    {
        return currentStamina > 0;
    }
}