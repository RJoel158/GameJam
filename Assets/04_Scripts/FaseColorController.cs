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
    private Image healthImage;

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

    public float sprintStaminaCost = 400f; // Stamina consumed per second while sprinting
    public float attackStaminaCost = 500f; // Stamina consumed per attack (1.25x más que correr)
    public int staminaDecrementStep = 200; // Stamina baja en incrementos de este valor
    public float staminaRegenRate = 200f; // Stamina regeneration per second (sprintStaminaCost * 0.5f)

    private float currentStamina; // Usar float interno para precisión
    private float staminaTickTimer = 0f; // Timer para controlar cuándo bajar stamina
    private bool lastAttackInputState = false; // Guardar el estado anterior del input de ataque
    private float attackCooldownTimer = 0f; // Cooldown entre ataques
    public bool unlimitedStaminaActive = false; // Flag para poder-up de estamina infinita

    [Header("Mana")]
    [Range(0f, 100f)]
    public float manaPercent = 0f;
    public int mana = 0;
    public int maxMana = 100;

    private float currentMana = 0f; // Mana actual (float para precisión)
    private float manaTickTimer = 0f; // Timer para controlar cuándo subir mana
    private int manaIncrementStep = 25; // Mana sube en incrementos de 25

    private void Awake()
    {
        // Buscar el ThirdPersonController ANTES de Start
        thirdPersonController = FindFirstObjectByType<ThirdPersonController>();
        if (thirdPersonController == null)
        {
            Debug.LogError("ThirdPersonController no encontrado en la escena");
        }
        
        // Inicializar stamina correctamente LO ANTES POSIBLE
        stamina = maxStamina;
        currentStamina = maxStamina;
        staminaPercent = 100f;

        // Inicializar mana correctamente
        mana = 0;
        currentMana = 0f;
        manaPercent = 0f;
        
        // Sincronizar INMEDIATAMENTE con ThirdPersonController
        if (thirdPersonController != null)
        {
            thirdPersonController.stamina = stamina;
            thirdPersonController.maxStamina = maxStamina;
            thirdPersonController.staminaPercent = 100f;
        }
    }

    private void Start()
    {
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

        if (healthImage == null)
        {
            healthImage = GameObject.Find("health")?.GetComponent<Image>();
            if (healthImage == null)
            {
                healthImage = GameObject.Find("vida")?.GetComponent<Image>();
            }
            if (healthImage == null)
            {
                Debug.LogWarning("No se encontró la imagen 'health' o 'vida'. Busca manualmente en el Inspector.");
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
        
        // Inicializar stamina UI
        if (staminaImage != null)
        {
            staminaImage.fillAmount = 1f;
        }
    }

    private void Update()
    {
        if (thirdPersonController != null)
        {
            HandleStaminaConsumption();
            HandleManaRegeneration();
            UpdateHealthUI();
            UpdateUI();
            
            // SINCRONIZAR la estamina con ThirdPersonController
            thirdPersonController.UpdateStaminaFromUI(stamina, staminaPercent);
        }
    }

    private void HandleStaminaConsumption()
    {
        // Si está activo el poder-up de estamina infinita, no consumir stamina
        if (unlimitedStaminaActive)
        {
            currentStamina = maxStamina;
            stamina = maxStamina;
            staminaPercent = 100f;
            return;
        }
        // DESPUÉS DE QUE TERMINA EL PODER-UP, CONSUMIR NORMALMENTE
        // Proteger contra referencias nulas en _input: intentar obtener el componente si no está presente
        var input = thirdPersonController._input;
        if (input == null)
        {
            input = thirdPersonController.GetComponent<StarterAssets.StarterAssetsInputs>();
            if (input != null)
            {
                // opcionalmente sincronizar con el tercer person controller
                thirdPersonController._input = input;
            }
            else
            {
                // No podemos procesar el consumo de stamina sin input
                return;
            }
        }

        // Consume stamina when sprinting de forma fluida
        if (input.sprint && input.move != Vector2.zero && currentStamina > 0)
        {
            // Velocidad de consumo: 400 stamina por segundo (fluido y continuo)
            currentStamina -= sprintStaminaCost * Time.deltaTime;
            if (currentStamina < 0)
            {
                currentStamina = 0;
            }
        }
        // Regenerate stamina cuando no está presionando Shift ni atacando
        else if (!input.sprint && !input.attack)
        {
            // No hacer nada, solo detener regeneración
        }
        // 3. BLOCK - Si está bloqueando, no regenerar
        else if (isBlockingNow)
        {
            // No hacer nada, solo detener regeneración
        }
        // 4. IDLE - No está haciendo nada: regenerar inmediatamente
        else
        {
            // Regenerar stamina continuamente cuando está en reposo
            currentStamina += staminaRegenRate * Time.deltaTime;

            if (currentStamina > maxStamina)
            {
                currentStamina = maxStamina;
            }
        }

        // Detener bloqueo si no hay estamina
        if (currentStamina <= 0 && thirdPersonController.isBlocking)
        {
            thirdPersonController._animator.SetBool(Animator.StringToHash("Block"), false);
            thirdPersonController.isBlocking = false;
            thirdPersonController.canBlock = false;
        }

        // Convertir float a int para el stamina público
        stamina = (int)currentStamina;
        staminaPercent = (stamina * 100) / maxStamina;
    }

    private void HandleManaRegeneration()
    {
        // Si Hard Mode está activo, drenar mana continuamente (3% por segundo)
        if (thirdPersonController != null && thirdPersonController.hardModeEnabled)
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

    private void UpdateHealthUI()
    {
        if (healthImage != null && thirdPersonController != null)
        {
            // Calcular el porcentaje de vida
            float healthPercent = (thirdPersonController.health * 100f) / thirdPersonController.maxHealth;
            float fillAmount = Mathf.Clamp01(healthPercent / 100f);
            healthImage.fillAmount = fillAmount;

            // Verificar si el jugador está muerto
            if (thirdPersonController.health <= 0)
            {
                thirdPersonController.dead = true;
                if (!thirdPersonController.death)
                {
                    thirdPersonController.death = true;
                    thirdPersonController._animator.SetTrigger("Death");
                    Debug.Log("[MUERTE] ¡El jugador ha muerto!");
                }
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
        return unlimitedStaminaActive || currentStamina > 0;
    }

    // Método para consumir estamina al bloquear (porcentaje del máximo)
    public void ConsumeStaminaForBlock()
    {
        float staminaCost = maxStamina * (blockStaminaCostPercent / 100f);
        currentStamina -= staminaCost;
        if (currentStamina < 0) currentStamina = 0;
    }

    // Método para consumir estamina al recibir daño del enemigo (porcentaje del máximo)
    public void ConsumeStaminaForDamage()
    {
        float staminaCost = maxStamina * (damageStaminaCostPercent / 100f);
        currentStamina -= staminaCost;
        if (currentStamina < 0) currentStamina = 0;
    }

    // Método para verificar si puede bloquear
    public bool CanBlock()
    {
        return currentStamina > 0;
    }

    // Método para consumir mana al activar Hard Mode (porcentaje del máximo)
    public void ConsumeManForHardMode()
    {
        float manaCost = maxMana * (hardModeManaCostPercent / 100f);
        currentMana -= manaCost;
        if (currentMana < 0) currentMana = 0;
    }
}