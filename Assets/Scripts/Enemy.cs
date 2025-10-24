using StarterAssets;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Windows;

public class Enemy : MonoBehaviour
{
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
    //[SerializeField] GameObject ragdoll;

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
        //agent = GetComponent<NavMeshAgent>();
        //animator = GetComponent<Animator>();
        //CapsuleEnemyCollider = GetComponent<CapsuleCollider>();
        player = GameObject.FindGameObjectWithTag("Player");
        //playerThirdPersonController = player.GetComponent<ThirdPersonController>();
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

        if (playerDetected && !isAttacking && !inAttackAnimation && !dead)
        {
            //newDestinationCD = 0.5f;
            agent.SetDestination(player.transform.position);
        }
        newDestinationCD -= Time.deltaTime;
        //transform.LookAt(player.transform);

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

        //if (health <= 0)
        //{
        //    Die();
        //}
        //else
        //{
        //    animator.SetTrigger("Damage");
        //    //CameraShake.Instance.ShakeCamera(2f, 0.2f);
        //}

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


    //public void StartDealDamage()
    //{
    //    GetComponentInChildren<EnemyDamageDealer>().StartDealDamage();
    //}
    //public void EndDealDamage()
    //{
    //    GetComponentInChildren<EnemyDamageDealer>().EndDealDamage();
    //}

    //public void HitVFX(Vector3 hitPosition)
    //{
    //    GameObject hit = Instantiate(hitVFX, hitPosition, Quaternion.identity);
    //    Destroy(hit, 3f);
    //}

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroRange);
    }
}
