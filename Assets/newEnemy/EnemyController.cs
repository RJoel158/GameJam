using UnityEngine;
using UnityEngine.AI;
using StarterAssets;

public class EnemyController : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] float detectionRange = 20f;
    [SerializeField] string playerTag = "Player";
    
    [Header("Movement")]
    [SerializeField] float chaseSpeed = 3.5f;
    
    [Header("Animation")]
    [SerializeField] float animationBlendSpeed = 10f;
    
    [Header("Health")]
    [SerializeField] int maxHealth = 100;
    
    [Header("Explosion Effect")]
    [SerializeField] ParticleSystem explosionEffect;
    
    [Header("Audio")]
    [SerializeField] AudioClip detectionAudioClip;
    
    private Animator animator;
    private NavMeshAgent agent;
    private Transform playerTransform;
    private bool isChasing = false;
    private float animationBlend = 0f;
    private int currentHealth;
    private bool hasHitPlayer = false;
    private bool audioHasPlayed = false;
    private AudioSource audioSource;

    private void Start()
    {
        // Obtener referencias
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        
        // Si no hay AudioSource, intentar crear uno
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            Debug.Log("[EnemyController] AudioSource created automatically");
        }
        
        // Inicializar salud
        currentHealth = maxHealth;
        hasHitPlayer = false;
        
        if (animator == null)
        {
            Debug.LogError("[EnemyController] No Animator found");
            return;
        }
        else
        {
            // Asegurar que el parámetro 'isRun' esté inicializado en false
            if (HasAnimatorParameter("isRun", AnimatorControllerParameterType.Bool))
            {
                animator.SetBool("isRun", false);
            }
            else
            {
                // No hacemos nada si no existe el parámetro; dejamos el SetFloat por compatibilidad
            }
        }
        
        if (agent == null)
        {
            Debug.LogWarning("[EnemyController] No NavMeshAgent found - will use simple movement");
        }
        
        // Buscar al jugador por tag
        GameObject playerObject = GameObject.FindWithTag(playerTag);
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
            Debug.Log("[EnemyController] Player found: " + playerObject.name);
        }
        else
        {
            Debug.LogWarning("[EnemyController] Player not found with tag '" + playerTag + "'");
        }
    }

    private void Update()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // Detectar si el jugador está en rango INICIAL
        if (!isChasing && distanceToPlayer <= detectionRange)
        {
            isChasing = true;
            PlayDetectionAudio();
            Debug.Log($"[EnemyController] Player detected at {distanceToPlayer:F1}m! Starting chase.");
            // Activar bool que usa el Animator para la transición a run
            if (animator != null)
            {
                animator.SetBool("isRun", true);
            }
        }
        
        // Una vez que empieza a perseguir, sigue hasta que se destruya
        if (isChasing)
        {
            ChasePlayer(distanceToPlayer);
        }
    }

    private void ChasePlayer(float distanceToPlayer)
    {
        // Usar NavMeshAgent si está disponible
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.SetDestination(playerTransform.position);
            agent.speed = chaseSpeed;
            animationBlend = Mathf.Lerp(animationBlend, 1f, Time.deltaTime * animationBlendSpeed);
        }
        else
        {
            // Fallback: movimiento simple directo al jugador
            Vector3 direction = (playerTransform.position - transform.position).normalized;
            direction.y = 0f; // Mantener en el plano horizontal
            transform.position += direction * chaseSpeed * Time.deltaTime;
            animationBlend = Mathf.Lerp(animationBlend, 1f, Time.deltaTime * animationBlendSpeed);
        }
        
        // Girar hacia el jugador
        Vector3 lookDirection = playerTransform.position - transform.position;
        lookDirection.y = 0f;
        if (lookDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Euler(0f, targetRotation.eulerAngles.y, 0f);
        }
        
        // Actualizar animación
        animator.SetFloat("Speed", animationBlend);
        // Asegurar también la bandera booleana 'isRun' para controladores que usan condiciones booleanas
        if (animator != null)
        {
            animator.SetBool("isRun", true);
        }
    }

    // Visualizar rango de detección
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }

    /// <summary>
    /// Comprueba si el Animator contiene un parámetro con nombre y tipo dados.
    /// </summary>
    private bool HasAnimatorParameter(string name, AnimatorControllerParameterType type)
    {
        if (animator == null) return false;
        foreach (var p in animator.parameters)
        {
            if (p.name == name && p.type == type) return true;
        }
        return false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Detectar si el jugador toca al enemigo
        if (collision.gameObject.CompareTag(playerTag))
        {
            if (!hasHitPlayer)
            {
                hasHitPlayer = true;
                DamagePlayer(collision.gameObject);
            }
        }
    }

    private void DamagePlayer(GameObject playerObject)
    {
        // Intentar obtener ThirdPersonController
        ThirdPersonController playerController = playerObject.GetComponent<ThirdPersonController>();
        
        if (playerController != null)
        {
            // Quitar 25% de la salud máxima del jugador (maxHealth = 2000, 25% = 500)
            int damageAmount = (playerController.maxHealth * 25) / 100;
            playerController.TakeDamage(damageAmount);
            Debug.Log($"[EnemyController] Dealt 25% damage ({damageAmount} HP) to player with explosion!");
        }
        else
        {
            Debug.LogWarning("[EnemyController] ThirdPersonController not found on player");
        }
        
        // Activar efecto de explosión y destruir
        TriggerExplosionAndDestroy();
    }

    private void PlayDetectionAudio()
    {
        // Solo reproducir una vez
        if (!audioHasPlayed && detectionAudioClip != null && audioSource != null)
        {
            audioHasPlayed = true;
            audioSource.PlayOneShot(detectionAudioClip);
            Debug.Log("[EnemyController] Detection audio triggered!");
        }
        else if (detectionAudioClip == null)
        {
            Debug.LogWarning("[EnemyController] No detection audio clip assigned!");
        }
    }

    private void TriggerExplosionAndDestroy()
    {
        // Si hay efecto de explosión, activarlo
        if (explosionEffect != null)
        {
            explosionEffect.gameObject.SetActive(true);
            explosionEffect.Play();
            Debug.Log("[EnemyController] Explosion effect triggered!");
            
            // Destruir el enemigo después de que termine la animación/partículas
            float explosionDuration = explosionEffect.main.duration;
            // Antes de destruir, resetear la animación para evitar quedarse en run
            if (animator != null)
            {
                animator.SetBool("isRun", false);
            }
            Destroy(gameObject, explosionDuration);
        }
        else
        {
            Debug.LogWarning("[EnemyController] No explosion effect assigned!");
            if (animator != null)
            {
                animator.SetBool("isRun", false);
            }
            Destroy(gameObject);
        }
    }
}
