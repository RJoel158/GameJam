using StarterAssets;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using Unity.VisualScripting;

public class Boss : MonoBehaviour
{
    // Event for boss defeated (for mission system)
    public static event System.Action<Vector3> OnBossDefeated;

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
    public GameObject ShieldParticle;
    public GameObject portalPrefab;

    [Header("Portal Configuration")]
    [SerializeField] private float portalSpawnDelay = 3f;
    [SerializeField] private float portalBackDistance = 5f;

    public bool spawnEnemies = false;
    public Animator animator;
    public SphereCollider sphereCollider;

    private bool phase1Triggered = false;
    private bool phase2Triggered = false;
    private bool phase3Triggered = false;

    private int enemiesAlive = 0;
    public float SpeedChangeRate = 10.0f;

    [Header("Phase Duration")]
    [SerializeField] float phase1Duration = 60f; // 60 segundos para Fase 1
    private float phase1Timer = 0f;

    [Header("Combat")]
    [SerializeField] float attackCD = 2f;
    [SerializeField] float attackRange = 1f;
    [SerializeField] float aggroRange = 10f;
    public bool inAttackAnimation = false;
    public bool isAttacking = false;
    float timePassed;

    [Header("Stadistics")]
    [Range(0f, 100f)]
    public float healthPercent = 100f;
    public int health = 2000;
    public int maxHealth = 2000;
    public bool death = false;
    public bool dead = false;
    public bool inHitAnimation = false;

    // ===============================
    // NAVMESH / MOVIMIENTO (solo fase 2)
    // ===============================
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Animator bossAnimator;
    [SerializeField] GameObject player;
    float _animationBlend;
    bool playerDetected = false;
    bool attackPlayer = false;
    float newDestinationCD = 0f;
    ThirdPersonController playerTPC;

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
        sphereCollider = GetComponent<SphereCollider>();
        bossAnimator = animator;

        player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
            playerTPC = player.GetComponent<ThirdPersonController>();

        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            agent.updateRotation = false;
            agent.updateUpAxis = true;
        }

        health = maxHealth;
    }

    void Update()
    {
        HandleStadistics();

        if (dead) return;

        animator.SetBool("SpawnEnemies", spawnEnemies);

        if (spawnEnemies)
        {
            PlayerAudioManager.Instance?.PlayBossSpawnEnemies();
            spawnEnemies = false;
        }

        HandlePhases();

        // ===================================
        // MOVIMIENTO CON NAVMESH SOLO FASE 2
        // ===================================
        if (currentPhase == BossPhase.Phase2 && !dead)
        {
            BossMovement();
        }
    }

    // ============================================================
    // Lógica de movimiento adaptada de Enemy.cs
    // ============================================================
    void BossMovement()
    {
        if (player == null || playerTPC == null || playerTPC.dead) return;

        playerDetected = Vector3.Distance(player.transform.position, transform.position) <= aggroRange;
        attackPlayer = Vector3.Distance(player.transform.position, transform.position) <= attackRange;

        newDestinationCD -= Time.deltaTime;

        if (timePassed >= attackCD)
        {
            attackPlayer = Vector3.Distance(player.transform.position, transform.position) <= attackRange;

            if (attackPlayer && !dead)
            {
                animator.SetTrigger("Attack");
                timePassed = 0;
            }
        }
        timePassed += Time.deltaTime;

        if (playerDetected && !attackPlayer)
        {
            if (newDestinationCD <= 0)
            {
                if (agent != null && agent.isOnNavMesh)
                    agent.SetDestination(player.transform.position);

                newDestinationCD = 0.5f;
            }
        }

        if (playerDetected)
        {
            Vector3 direction = player.transform.position - transform.position;
            direction.y = 0;
            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Euler(0, targetRot.eulerAngles.y, 0);
            }
        }

        float targetSpeed = 0;
        if (playerDetected && !attackPlayer) targetSpeed = 2f;

        _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
        bossAnimator.SetFloat("Speed", _animationBlend);
    }

    // ============================================================
    // Se llama cada vez que muere un enemigo
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
                    // currentPhase = BossPhase.Phase3;
                    break;
            }
        }
    }

    // ============================================================
    // CONTROL DE FASES
    // ============================================================
    void HandlePhases()
    {
        switch (currentPhase)
        {
            case BossPhase.Phase1:
                // Contar tiempo en Fase 1
                phase1Timer += Time.deltaTime;
                
                if (!phase1Triggered)
                {
                    phase1Triggered = true;
                    StartCoroutine(ExecuteSpawnWithDelay());
                    Debug.Log($"<color=yellow>[Boss] Fase 1 iniciada. Durará {phase1Duration} segundos</color>");
                }

                // Si pasan 60 segundos, cambiar a Fase 2
                if (phase1Timer >= phase1Duration)
                {
                    currentPhase = BossPhase.Phase2;
                    Debug.Log($"<color=yellow>[Boss] Fase 1 completada después de {phase1Duration} segundos. Cambiando a Fase 2...</color>");
                }
                break;

            case BossPhase.Phase2:
                sphereCollider.enabled = false;
                ShieldParticle.SetActive(false);

                if (!phase2Triggered)
                {
                    phase2Triggered = true;
                    StartCoroutine(ExecuteSpawnWithDelay());
                    Debug.Log($"<color=yellow>[Boss] Fase 2 iniciada</color>");
                }
                break;
        }
    }

    IEnumerator ExecuteSpawnWithDelay()
    {
        yield return new WaitForSeconds(3f);
        spawnEnemies = true;
        enemiesAlive = 4;
    }

    public void SpawnEnemies()
    {
        ShieldParticle.SetActive(true);
        sphereCollider.enabled = true;

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
        }

        if (health <= 0)
        {
            dead = true;
            Die();
        }
    }

    void Die()
    {
        animator.SetTrigger("Death");

        // Spawn portal with 3-second delay positioned behind boss
        StartCoroutine(SpawnPortalWithDelay());

        // Emit boss defeated event at the boss's position
        Debug.Log($"<color=red>[Boss] Boss defeated! Emitting OnBossDefeated event at {transform.position}</color>");
        OnBossDefeated?.Invoke(transform.position);

        
    }

    private void HandleStadistics()
    {
        animator.SetBool("Dead", dead);
        healthPercent = (health * 100) / maxHealth;
    }

    // Animation Events
    public void StartBossAttack()
    {
        inAttackAnimation = true;
    }

    public void EndBossAttack()
    {
        inAttackAnimation = false;
    }

    public void EnterBossAttack()
    {
        isAttacking = true;
    }

    public void ExitBossAttack()
    {
        isAttacking = false;
    }

    public void SpawnPortal()
    {
        portalPrefab.SetActive(true);
    }

    private IEnumerator SpawnPortalWithDelay()
    {
        Debug.Log($"[Boss] Portal spawn iniciado. Aparecerá en {portalSpawnDelay} segundos atrás de la posición del boss");
        
        // Asegurar que el portal prefab esté desactivado inicialmente
        if (portalPrefab != null)
        {
            portalPrefab.SetActive(false);
            Debug.Log("[Boss] Portal prefab desactivado inicialmente");
        }
        
        // Guardar la posición actual del boss
        Vector3 bossDeathPosition = transform.position;
        
        // Esperar el delay
        yield return new WaitForSeconds(portalSpawnDelay);
        
        // Calcular posición atrás del boss
        Vector3 portalPosition = bossDeathPosition - transform.forward * portalBackDistance;
        
        // Spawnear el portal
        if (portalPrefab != null)
        {
            GameObject spawnedPortal = Instantiate(portalPrefab, portalPosition, Quaternion.identity);
            spawnedPortal.SetActive(true);
            
            // Activar todos los componentes hijo
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
            
            Debug.Log($"<color=cyan>[Boss] Portal spawned en {portalPosition} después de {portalSpawnDelay}s, {portalBackDistance}m atrás</color>");
        }
        else
        {
            Debug.LogWarning("[Boss] Portal prefab not assigned!");
        }
    }
}
