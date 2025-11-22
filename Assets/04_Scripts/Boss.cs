using StarterAssets;
using UnityEngine;
using System.Collections;

public class Boss : MonoBehaviour
{
    public enum BossPhase
    {
        Phase1,
        Phase2,
        Phase3
    }

    public BossPhase currentPhase = BossPhase.Phase1;

    public GameObject BasicEnemySpawn;
    public GameObject BasicEnemySpawn1;
    public GameObject BasicEnemySpawn2;
    public GameObject BasicEnemySpawn3;

    public GameObject BasicEnemyPrefab;
    public GameObject SpawnParticle;

    public bool spawnEnemies = false;
    public Animator animator;

    private bool phase1Triggered = false;
    private bool phase2Triggered = false;
    private bool phase3Triggered = false;

    private int enemiesAlive = 0;

    [Header("Stadistics")]
    [Range(0f, 100f)]
    public float healthPercent = 100f;
    public int health = 2000;
    public int maxHealth = 2000;
    public bool death = false;
    public bool dead = false;
    public bool inHitAnimation = false;

    void OnEnable()
    {
        Enemy.OnEnemyDefeated += OnEnemyKilled;
    }

    void OnDisable()
    {
        Enemy.OnEnemyDefeated -= OnEnemyKilled;
    }

    void Start()
    {
        animator = GetComponent<Animator>();

        health = maxHealth;
    }

    void Update()
    {
        HandleStadistics();

        if (dead)
        {
            return;
        }

        // 🔥 Tu lógica ORIGINAL no se toca.
        animator.SetBool("SpawnEnemies", spawnEnemies);

        if (spawnEnemies)
        {
            PlayerAudioManager.Instance?.PlayBossSpawnEnemies();
            spawnEnemies = false;
        }

        HandlePhases();
    }

    // ============================================================
    //           Se llama cada vez que muere un enemigo
    // ============================================================
    void OnEnemyKilled(Vector3 pos)
    {
        enemiesAlive--;

        if (enemiesAlive <= 0)
        {
            switch (currentPhase)
            {
                case BossPhase.Phase1:
                    currentPhase = BossPhase.Phase2;
                    break;

                case BossPhase.Phase2:
                    currentPhase = BossPhase.Phase3;
                    break;
            }
        }
    }

    // ============================================================
    //                     CONTROL DE FASES
    // ============================================================
    void HandlePhases()
    {
        switch (currentPhase)
        {
            case BossPhase.Phase1:
                if (!phase1Triggered)
                {
                    phase1Triggered = true;
                    StartCoroutine(ExecuteSpawnWithDelay());
                }
                break;

            case BossPhase.Phase2:
                if (!phase2Triggered)
                {
                    phase2Triggered = true;
                    StartCoroutine(ExecuteSpawnWithDelay());
                }
                break;

            case BossPhase.Phase3:
                if (!phase3Triggered)
                {
                    phase3Triggered = true;
                    StartCoroutine(ExecuteSpawnWithDelay());
                }
                break;
        }
    }

    // ============================================================
    //    ESPERAR 3 SEGUNDOS → activar spawnEnemies = true
    // ============================================================
    IEnumerator ExecuteSpawnWithDelay()
    {
        yield return new WaitForSeconds(3f);

        // 🔥 NO · SE · TOCA · LA · LÓGICA
        spawnEnemies = true;

        // 🔥 Como se van a spawnnear, contamos 4 enemigos vivos
        enemiesAlive = 4;
    }

    // ============================================================
    //     MÉTODO ORIGINAL SpawnEnemies — NO CAMBIADO
    // ============================================================
    public void SpawnEnemies()
    {
        Instantiate(BasicEnemyPrefab, BasicEnemySpawn.transform.position, BasicEnemySpawn.transform.rotation);
        Instantiate(SpawnParticle, BasicEnemySpawn.transform.position, BasicEnemySpawn.transform.rotation);

        Instantiate(BasicEnemyPrefab, BasicEnemySpawn1.transform.position, BasicEnemySpawn1.transform.rotation);
        Instantiate(SpawnParticle, BasicEnemySpawn1.transform.position, BasicEnemySpawn1.transform.rotation);

        Instantiate(BasicEnemyPrefab, BasicEnemySpawn2.transform.position, BasicEnemySpawn2.transform.rotation);
        Instantiate(SpawnParticle, BasicEnemySpawn2.transform.position, BasicEnemySpawn2.transform.rotation);

        Instantiate(BasicEnemyPrefab, BasicEnemySpawn3.transform.position, BasicEnemySpawn3.transform.rotation);
        Instantiate(SpawnParticle, BasicEnemySpawn3.transform.position, BasicEnemySpawn3.transform.rotation);
    }

    public void TakeDamage(int damageAmount)
    {
        if (!dead)
        {
            health -= damageAmount;
            animator.SetTrigger("Damage");
            //CameraShake.Instance.ShakeCamera(2f, 0.2f);

            // Reproducir sonido de quejido al recibir daño
            // if (PlayerAudioManager.Instance != null)
            //     PlayerAudioManager.Instance.PlayHurtSound();
        }

        if (health <= 0)
        {
            dead = true;
            Die();
        }
    }

    void Die()
    {
        //Instantiate(ragdoll, transform.position, transform.rotation);
        animator.SetTrigger("Death");
        //Destroy(this.gameObject);
    }

    private void HandleStadistics()
    {
        animator.SetBool("Dead", dead);
        //animator.SetBool(_animIDHitting, inHitAnimation);

        healthPercent = (health * 100) / maxHealth;
    }
}
