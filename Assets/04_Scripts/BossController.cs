using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Controla la salud, energía y estados del boss
/// </summary>
public class BossController : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("Vida actual del boss")]
    public int currentHealth = 1000;

    [Tooltip("Vida máxima del boss")]
    public int maxHealth = 1000;

    [Header("Energy Settings")]
    [Tooltip("Energía actual del boss")]
    public float currentEnergy = 100f;

    [Tooltip("Energía máxima del boss")]
    public float maxEnergy = 100f;

    [Tooltip("Coste de energía por disparo")]
    [Range(1f, 50f)]
    public float energyCostPerShot = 10f;

    [Tooltip("Regeneración de energía por segundo")]
    [Range(1f, 20f)]
    public float energyRegenRate = 5f;

    [Tooltip("Tiempo que permanece inconsciente cuando se queda sin energía")]
    [Range(3f, 15f)]
    public float unconsciousDuration = 5f;

    [Header("State")]
    [Tooltip("¿Está el boss inconsciente?")]
    public bool isUnconscious = false;

    [Tooltip("¿Está el boss muerto?")]
    public bool isDead = false;

    [Header("References")]
    [Tooltip("Referencia al God script para controlar disparos")]
    public God godScript;

    [Header("Portal (On Death)")]
    [Tooltip("Prefab del portal a spawnear cuando el boss muere")]
    public GameObject portalPrefab;

    private NavMeshAgent navAgent;
    private Animator animator;
    private Rigidbody rb;
    private float unconsciousTimer = 0f;
    private bool wasKinematic = true;

    private void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        if (godScript == null)
        {
            godScript = GetComponent<God>();
        }

        // Guardar estado inicial del Rigidbody
        if (rb != null)
        {
            wasKinematic = rb.isKinematic;
        }
    }

    private void Update()
    {
        if (isDead) return;

        // Regenerar energía si no está inconsciente
        if (!isUnconscious && currentEnergy < maxEnergy)
        {
            currentEnergy += energyRegenRate * Time.deltaTime;
            currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy);
        }

        // Manejar estado inconsciente
        if (isUnconscious)
        {
            unconsciousTimer -= Time.deltaTime;

            if (unconsciousTimer <= 0f)
            {
                WakeUp();
            }
        }
    }

    /// <summary>
    /// Llama a este método cuando el boss dispara
    /// </summary>
    public bool TryUseEnergy()
    {
        if (isDead || isUnconscious) return false;

        if (currentEnergy >= energyCostPerShot)
        {
            currentEnergy -= energyCostPerShot;
            Debug.Log($"<color=yellow>[Boss] Energy used: {energyCostPerShot}. Remaining: {currentEnergy}/{maxEnergy}</color>");
            return true;
        }
        else
        {
            // Sin energía suficiente, quedar inconsciente
            BecomeUnconscious();
            return false;
        }
    }

    /// <summary>
    /// El boss queda inconsciente
    /// </summary>
    private void BecomeUnconscious()
    {
        if (isUnconscious || isDead) return;

        isUnconscious = true;
        unconsciousTimer = unconsciousDuration;
        currentEnergy = 0f;

        Debug.Log($"<color=red>[Boss] Out of energy! Becoming unconscious for {unconsciousDuration} seconds</color>");

        // Deshabilitar IA
        if (navAgent != null)
        {
            navAgent.enabled = false;
        }

        // Deshabilitar el script de ataque
        if (godScript != null)
        {
            godScript.enabled = false;
        }

        // Activar física para que caiga al suelo
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        // Animación de caída/inconsciente
        if (animator != null)
        {
            animator.SetTrigger("Unconscious");
        }
    }

    /// <summary>
    /// El boss se despierta
    /// </summary>
    private void WakeUp()
    {
        if (!isUnconscious || isDead) return;

        isUnconscious = false;
        currentEnergy = maxEnergy; // Restaurar energía completa

        Debug.Log("<color=green>[Boss] Waking up! Energy restored!</color>");

        // Restaurar Rigidbody al estado original
        if (rb != null)
        {
            rb.isKinematic = wasKinematic;
            rb.useGravity = false;
        }

        // Reactivar IA
        if (navAgent != null)
        {
            navAgent.enabled = true;
        }

        // Reactivar el script de ataque
        if (godScript != null)
        {
            godScript.enabled = true;
        }

        // Animación de despertar
        if (animator != null)
        {
            animator.SetTrigger("WakeUp");
        }
    }

    /// <summary>
    /// El boss recibe daño
    /// </summary>
    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log($"<color=orange>[Boss] Took {damageAmount} damage! Health: {currentHealth}/{maxHealth}</color>");

        // Animación de daño (si no está inconsciente)
        if (!isUnconscious && animator != null)
        {
            animator.SetTrigger("Damage");
        }

        // Verificar si murió
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// El boss muere
    /// </summary>
    private void Die()
    {
        if (isDead) return;

        isDead = true;
        isUnconscious = false;

        Debug.Log("<color=red>[Boss] Boss defeated!</color>");

        // Spawnear portal cuando el boss muere
        if (portalPrefab != null)
        {
            GameObject spawnedPortal = Instantiate(portalPrefab, transform.position, Quaternion.identity);
            spawnedPortal.SetActive(true);
            
            // Activar todos los componentes hijo por si acaso
            foreach (Transform child in spawnedPortal.transform)
            {
                child.gameObject.SetActive(true);
            }
            
            // Activar ParticleSystem si existe
            ParticleSystem ps = spawnedPortal.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();
            }
            
            Debug.Log($"<color=cyan>[BossController] Portal spawned at {transform.position}</color>");
        }
        else
        {
            Debug.LogWarning("[BossController] Portal prefab not assigned!");
        }

        // Deshabilitar IA
        if (navAgent != null)
        {
            navAgent.enabled = false;
        }

        // Deshabilitar script de ataque
        if (godScript != null)
        {
            godScript.enabled = false;
        }

        // Animación de muerte
        if (animator != null)
        {
            animator.SetTrigger("Death");
        }

        // Desactivar colisiones de ataque
        var colliders = GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }
    }

    /// <summary>
    /// Obtiene el porcentaje de vida (0-1)
    /// </summary>
    public float GetHealthPercent()
    {
        return (float)currentHealth / maxHealth;
    }

    /// <summary>
    /// Obtiene el porcentaje de energía (0-1)
    /// </summary>
    public float GetEnergyPercent()
    {
        return currentEnergy / maxEnergy;
    }

    /// <summary>
    /// Verifica si el boss está muerto
    /// </summary>
    public bool IsDead()
    {
        return isDead;
    }

    /// <summary>
    /// Verifica si el boss está inconsciente
    /// </summary>
    public bool IsUnconscious()
    {
        return isUnconscious;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Cuando el boss inconsciente colisiona con el suelo
        if (isUnconscious && collision.gameObject.CompareTag("Ground"))
        {
            Debug.Log("<color=yellow>[Boss] Hit the ground while unconscious</color>");
        }
    }

    // Gizmos para debug
    private void OnDrawGizmosSelected()
    {
        // Mostrar estado de energía
        Gizmos.color = isUnconscious ? Color.red : Color.green;
        Vector3 pos = transform.position + Vector3.up * 3f;
        Gizmos.DrawWireSphere(pos, 0.5f);
    }
}
