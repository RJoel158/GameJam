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

    void Start()
    {
        // Override de stats para hacerlo rápido y débil
        health = fastEnemyHealth;

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
