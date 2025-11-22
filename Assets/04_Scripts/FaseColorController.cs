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
    public float blockStaminaCostPercent = 0f; // Porcentaje de estamina que cuesta bloquear
    public float damageStaminaCostPercent = 30f; // Porcentaje de estamina que cuesta al recibir daño

    public float currentStamina; // Usar float interno para precisión
    private float staminaTickTimer = 0f; // Timer para controlar cuándo bajar stamina
    private bool lastAttackInputState = false; // Guardar el estado anterior del input de ataque
    private float attackCooldownTimer = 0f; // Cooldown entre ataques
    public bool unlimitedStaminaActive = false; // Flag para poder-up de estamina infinita

    [Header("Force (antes Mana)")]
    [Tooltip("Este sistema ahora usa el atributo 'force' del ThirdPersonController")]
    public float hardModeManaCostPercent = 15f; // Porcentaje de force que cuesta activar Hard Mode (15%)
    public float hardModeManaCostDrainPercent = 1f; // Porcentaje de force que se drena por segundo durante Hard Mode (1% - como en ThirdPersonController)

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

        // Force se maneja completamente en ThirdPersonController
        // No necesitamos inicializar nada aquí

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

        // Force Image se actualiza desde ThirdPersonController
        if (manaImage != null)
        {
            manaImage.fillAmount = 0f; // Comenzará en 0 y subirá automáticamente
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
            UpdateHealthUI();
            UpdateUI();

            // SINCRONIZAR la estamina con ThirdPersonController
            thirdPersonController.UpdateStaminaFromUI(stamina, staminaPercent);
        }

        if (thirdPersonController.hardModeEnabled)
        {
            unlimitedStaminaActive = true;
        }
        else
        {
            unlimitedStaminaActive = false;
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

        // Proteger contra referencias nulas en _input
        var input = thirdPersonController._input;
        if (input == null)
        {
            input = thirdPersonController.GetComponent<StarterAssets.StarterAssetsInputs>();
            if (input != null)
            {
                thirdPersonController._input = input;
            }
            else
            {
                return;
            }
        }

        bool isSprinting = input.sprint && input.move != Vector2.zero && currentStamina > 0;
        bool isAttacking = input.attack && thirdPersonController.isAttacking;
        bool isBlocking = thirdPersonController.isBlocking;

        // 1. SPRINT - Consume 400 stamina por segundo (pero solo si hay estamina > 0)
        if (isSprinting && currentStamina > 0)
        {
            currentStamina -= sprintStaminaCost * Time.deltaTime;
            if (currentStamina < 0) currentStamina = 0;
        }
        // 2. ATTACK - Consumir 20% de estamina por golpe (una sola vez)
        else if (isAttacking)
        {
            // Se consume en el método TryConsumeStaminaForAttack() que se llama desde ThirdPersonController
        }
        // 3. BLOCK - Consumir 15% de estamina continuo mientras se mantiene presionado
        else if (blockingActive)
        {
            float blockStaminaCost = maxStamina * (blockStaminaCostPercent / 100f);
            currentStamina -= blockStaminaCost * Time.deltaTime;

            if (currentStamina <= 0)
            {
                currentStamina = 0;
                blockingActive = false; // Detener bloqueo automáticamente
                Debug.Log("[FaseColorController] Bloqueo finalizado - Estamina agotada");
            }
        }
        // 4. IDLE - Regenerar estamina solo cuando NO está corriendo, atacando o bloqueando
        else
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            if (currentStamina > maxStamina) currentStamina = maxStamina;
        }

        // Convertir float a int para el stamina público
        stamina = (int)currentStamina;
        staminaPercent = (stamina * 100) / maxStamina;
    }

    private bool attackingActive = false; // Flag para rastrear si el ataque está activo

    // Método para intentar atacar - Consume 10% de estamina
    public bool TryAttack()
    {
        float attackStaminaCost = maxStamina * (0.10f); // 10% de estamina
        if (currentStamina >= attackStaminaCost)
        {
            currentStamina -= attackStaminaCost;
            if (currentStamina < 0) currentStamina = 0;
            stamina = (int)currentStamina;
            staminaPercent = (stamina * 100) / maxStamina;
            attackingActive = true;
            Debug.Log($"[Estamina] Consumido 10% en ataque. Estamina actual: {stamina}/{maxStamina}");
            return true;
        }
        else
        {
            Debug.Log("[Estamina] No hay suficiente estamina para atacar");
            return false;
        }
    }

    // Método para registrar fin del ataque
    public void EndAttack()
    {
        attackingActive = false;
    }

    private void UpdateUI()
    {
        if (manaImage != null && thirdPersonController != null)
        {
            // Usar forcePercent del ThirdPersonController
            float fillAmount = thirdPersonController.forcePercent / 100f;
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

    public bool blockingActive = false; // Rastrear si el bloqueo está activo internamente

    // Método para intentar iniciar bloqueo
    public bool TryStartBlock()
    {
        if (staminaPercent > 10)
        {
            blockingActive = true;
            return true;
        }
        return false;
    }

    // Método para detener el bloqueo
    public void StopBlock()
    {
        blockingActive = false;
    }

    // Método para verificar si el bloqueo está activo
    public bool IsBlockingActive()
    {
        return blockingActive;
    }

    // Método para verificar si puede bloquear
    public bool CanBlock()
    {
        return staminaPercent > 0;
    }

    // Método para consumir force al activar Hard Mode (porcentaje del máximo)
    public void ConsumeForceForHardMode()
    {
        if (thirdPersonController != null)
        {
            float forceCost = thirdPersonController.maxForce * (hardModeManaCostPercent / 100f);
            thirdPersonController.force -= (int)forceCost;
            if (thirdPersonController.force < 0) thirdPersonController.force = 0;
            Debug.Log($"[Force] Consumido {(int)forceCost} force al activar Hard Mode. Force actual: {thirdPersonController.force}/{thirdPersonController.maxForce}");
        }
    }
}