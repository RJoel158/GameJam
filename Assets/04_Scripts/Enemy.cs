using StarterAssets;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Windows;

public class Enemy : MonoBehaviour
{
    // Event for mission system - fires when enemy is defeated with position
    public static event System.Action<Vector3> OnEnemyDefeated;
    // Instance event for camp tracking
    public event System.Action OnDied;

    // Public method for testing - simulates enemy defeat
    public static void TEST_TriggerEnemyDefeated(Vector3 position)
    {
        OnEnemyDefeated?.Invoke(position);
    }

    public float _animationBlend;
    public float SpeedChangeRate = 10.0f;
    public bool attackPlayer = false;
    public bool playerDetected = false;
    public float targetSpeed = 0f;
    public ThirdPersonController playerThirdPersonController;
    public CapsuleCollider CapsuleEnemyCollider;
    public bool inAttackAnimation = false;
    public bool isAttacking = false;
    public bool dead = false;

    public int health = 100;
    //[SerializeField] GameObject hitVFX;

    [Header("Combat")]
    [SerializeField] float attackCD = 3f;
    [SerializeField] float attackRange = 1f;
    [SerializeField] float aggroRange = 4f;

    [SerializeField] GameObject player;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Animator animator;
    float timePassed;
    float newDestinationCD = 0.5f;

    void Start()
    {
        // Ensure NavMeshAgent reference is assigned
        if (agent == null)
        {
            agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
            {
                agent.updateRotation = false; // we rotate manually on Y axis
                agent.updateUpAxis = true;
            }
            else
            {
                Debug.LogWarning($"[Enemy] NavMeshAgent not found on {gameObject.name}. Enemigo no podrá navegar por NavMesh.");
            }
        }
        //animator = GetComponent<Animator>();
        //CapsuleEnemyCollider = GetComponent<CapsuleCollider>();
        player = GameObject.FindGameObjectWithTag("Player");
        playerThirdPersonController = player.GetComponent<ThirdPersonController>();

        // NOTE: Dropping power-ups on death was causing unwanted objects in scene.
        // The auto-add of EnemyPowerUpDropper has been disabled. If you need drops,
        // re-enable by restoring the code below.
        /*
        // Asegurar que EnemyPowerUpDropper existe
        var dropperType = System.Type.GetType("EnemyPowerUpDropper");
        if (dropperType != null && GetComponent(dropperType) == null)
        {
            gameObject.AddComponent(dropperType);
            Debug.Log($"[ENEMY] EnemyPowerUpDropper agregado automáticamente a {gameObject.name}");
        }
        */

        // Ensure a health bar is present at runtime (adds component if prefab wasn't edited)
        // Use reflection to avoid a hard compile-time dependency on the healthbar script symbol order
        System.Type hbType = null;
        foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            hbType = asm.GetType("EnemyHealthBar");
            if (hbType != null) break;
        }
        if (hbType != null)
        {
            if (gameObject.GetComponent(hbType) == null)
            {
                gameObject.AddComponent(hbType);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        Move();

        if (player == null && playerThirdPersonController.dead)
        {
            return;
        }

        if (health > 0)
        {
            dead = false;
        }
        else
        {
            dead = true;
        }

        if (dead)
        {
            // Stop agent if dead
            if (agent != null && agent.enabled) agent.isStopped = true;
            return;
        }

        if (timePassed >= attackCD)
        {
            attackPlayer = Vector3.Distance(player.transform.position, transform.position) <= attackRange;

            if (attackPlayer && !playerThirdPersonController.dead)
            {
                animator.SetTrigger("Attack");
                timePassed = 0;
            }
        }
        timePassed += Time.deltaTime;

        playerDetected = newDestinationCD <= 0 && Vector3.Distance(player.transform.position, transform.position) <= aggroRange;

        if (playerDetected && !isAttacking && !inAttackAnimation && !dead && !attackPlayer)
        {
            //newDestinationCD = 0.5f;
            if (agent != null && agent.isOnNavMesh)
            {
                agent.SetDestination(player.transform.position);
            }
            else
            {
                // Fallback simple movement towards player for cases where navmesh isn't available
                Vector3 dir = (player.transform.position - transform.position);
                dir.y = 0f;
                transform.position += dir.normalized * 1.5f * Time.deltaTime;
            }
        }
        newDestinationCD -= Time.deltaTime;

        if (!dead && playerDetected)
        {
            Vector3 direction = player.transform.position - transform.position;
            direction.y = 0f; // 🔹 ignora la altura
            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                // 🔹 solo rota el eje Y
                transform.rotation = Quaternion.Euler(0f, targetRotation.eulerAngles.y, 0f);
            }
        }
    }

    void Move()
    {
        targetSpeed = playerDetected ? 2 : 0;

        if (playerDetected)
        {
            if (attackPlayer)
            {
                targetSpeed = 0;
            }
            else
            {
                targetSpeed = 2;
            }
        }
        else
        {
            targetSpeed = 0;
        }

        _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);

        animator.SetFloat("Speed", _animationBlend);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            //print(true);
            player = collision.gameObject;
        }
    }

    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;

        if (!dead)
        {
            animator.SetTrigger("Damage");
            //CameraShake.Instance.ShakeCamera(2f, 0.2f);
        }

        if (health <= 0)
        {
            dead = true;
            CapsuleEnemyCollider.enabled = false;
            Die();
        }
    }

    void Die()
    {
        //Instantiate(ragdoll, transform.position, transform.rotation);
        animator.SetTrigger("Death");

        // Fire mission event with enemy position
        Debug.Log($"<color=red>[Enemy] Firing OnEnemyDefeated event at position: {transform.position}</color>");
        Debug.Log($"<color=red>[Enemy] Event has {(OnEnemyDefeated != null ? OnEnemyDefeated.GetInvocationList().Length : 0)} subscribers</color>");
        // Invoke instance event first so local owners (camps) get notified
        try
        {
            OnDied?.Invoke();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[Enemy] Error invoking OnDied: {ex}");
        }

        OnEnemyDefeated?.Invoke(transform.position);

        // Power-up drops on death have been disabled. To re-enable, restore the
        // reflection code below that calls DropPowerUp on EnemyPowerUpDropper.
        /*
        // Soltar poder-up al morir usando reflection para evitar dependencias
        var dropperType = System.Type.GetType("EnemyPowerUpDropper");
        if (dropperType != null)
        {
            var dropper = GetComponent(dropperType);
            if (dropper != null)
            {
                var method = dropperType.GetMethod("DropPowerUp");
                if (method != null)
                {
                    method.Invoke(dropper, null);
                }
            }
        }
        */
        //Destroy(this.gameObject);
    }

    // Animation Events
    public void StartBasicEnemyAttack()
    {
        inAttackAnimation = true;
    }

    public void EndBasicEnemyAttack()
    {
        inAttackAnimation = false;
    }

    public void EnterBasicEnemyAttack()
    {
        isAttacking = true;
    }

    public void ExitBasicEnemyAttack()
    {
        isAttacking = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroRange);
    }
}
