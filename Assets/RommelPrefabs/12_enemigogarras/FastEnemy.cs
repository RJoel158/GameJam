using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Enemigo rápido y ágil con menos vida y daño, pero mayor velocidad de movimiento y ataque.
/// Hereda toda la funcionalidad base de Enemy.cs
/// </summary>
public class FastEnemy : Enemy
{
    [Header("Fast Enemy Stats Override")]
    [Tooltip("Multiplica la velocidad base del NavMeshAgent")]
    [Range(1.5f, 3f)]
    public float speedMultiplier = 1.8f;

    [Tooltip("Reduce el cooldown de ataque (valores menores = ataca más rápido)")]
    [Range(0.3f, 2f)]
    public float attackCooldownMultiplier = 0.5f;

    [Tooltip("Vida base del enemigo rápido (menor que enemigo normal)")]
    public int fastEnemyHealth = 50;

    [Tooltip("Daño que hace al jugador (menor que enemigo normal)")]
    public int fastEnemyDamage = 8;

    [Header("Fast Enemy Audio")]
    public AudioClip[] fastAttackSounds;
    public AudioClip fastDeathSound;

    private float originalAttackCD;
    private bool hasPlayedDeathSound = false;
    private bool deathTriggered = false;
    private int lastHealth = -1;

    void Start()
    {
        // Override de stats para hacerlo rápido y débil
        health = fastEnemyHealth;
        lastHealth = health;

        // Ajustar velocidad del NavMeshAgent
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed *= speedMultiplier;
            agent.acceleration *= 1.5f; // Más aceleración para movimiento ágil
            agent.angularSpeed *= 1.3f; // Gira más rápido
            
            Debug.Log($"<color=cyan>[FastEnemy] Velocidad ajustada a: {agent.speed}</color>");
        }

        // Reducir cooldown de ataque usando reflection para acceder al campo privado
        try
        {
            var field = typeof(Enemy).GetField("attackCD", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                float baseCD = (float)field.GetValue(this);
                originalAttackCD = baseCD;
                float newCD = baseCD * attackCooldownMultiplier;
                field.SetValue(this, newCD);
                
                Debug.Log($"<color=cyan>[FastEnemy] AttackCD ajustado: {baseCD}s → {newCD}s</color>");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[FastEnemy] No se pudo ajustar attackCD: {e.Message}");
        }

        Debug.Log($"<color=green>[FastEnemy] Inicializado - Health: {health}, Speed: {agent?.speed}, Damage: {fastEnemyDamage}</color>");
    }

    void LateUpdate()
    {
        // LateUpdate() se ejecuta DESPUÉS de Update(), así no interfiere con Enemy.cs
        
        // Interceptar ANTES de que Enemy.cs llame a Die()
        if (health <= 0 && lastHealth > 0 && !hasPlayedDeathSound)
        {
            // El enemigo acaba de morir, reproducir sonido ANTES de que PlayerAudioManager lo haga
            if (fastDeathSound != null)
            {
                AudioSource.PlayClipAtPoint(fastDeathSound, transform.position, 1f);
                hasPlayedDeathSound = true;
                Debug.Log("<color=purple>[FastEnemy] Sonido de muerte personalizado reproducido</color>");
            }
        }
        lastHealth = health;
        
        // Detectar cuando el enemigo muere
        if (dead && !deathTriggered)
        {
            deathTriggered = true;
            OnDeathCustom();
        }
    }

    /// <summary>
    /// Override del método de daño para usar el daño específico del fast enemy
    /// </summary>
    public int GetDamage()
    {
        return fastEnemyDamage;
    }

    /// <summary>
    /// Método para reproducir sonidos específicos del fast enemy
    /// </summary>
    public void PlayFastAttackSound()
    {
        if (fastAttackSounds != null && fastAttackSounds.Length > 0)
        {
            AudioClip clip = fastAttackSounds[Random.Range(0, fastAttackSounds.Length)];
            if (clip != null)
            {
                AudioSource.PlayClipAtPoint(clip, transform.position, 0.7f);
            }
        }
    }

    /// <summary>
    /// Override del comportamiento de muerte para reproducir sonido específico
    /// </summary>
    private void OnDeathCustom()
    {
        Debug.Log("<color=purple>[FastEnemy] ¡Enemigo murió! Preparando destrucción...</color>");
        
        // El sonido ya se reprodujo en LateUpdate(), solo desactivar agente y destruir
        
        // Desactivar el NavMeshAgent para que no siga moviéndose
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.enabled = false;
        }

        // Destruir el GameObject completo después de 3 segundos (para que termine la animación)
        Destroy(gameObject, 3f);
        Debug.Log("<color=purple>[FastEnemy] GameObject será destruido en 3 segundos</color>");
    }

    /// <summary>
    /// Método obsoleto - mantener para compatibilidad
    /// </summary>
    protected void OnDeath()
    {
        if (fastDeathSound != null)
        {
            AudioSource.PlayClipAtPoint(fastDeathSound, transform.position, 0.8f);
        }
    }

    // Visualización en editor
    void OnDrawGizmosSelected()
    {
        // Dibujar rango de ataque en color cyan para diferenciarlo
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 1f); // attackRange

        // Dibujar rango de aggro
        Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, 5f); // aggroRange aumentado
    }
}
