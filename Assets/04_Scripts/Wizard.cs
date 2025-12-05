using UnityEngine;

public class Wizard : MonoBehaviour
{
    [SerializeField] GameObject player;
    public Transform firePoint;
    public GameObject bulletPrefab;

    [Header("Stadistics")]
    [Range(0f, 100f)]
    public float healthPercent = 100f;
    public int health = 100;
    public int maxHealth = 100;

    [SerializeField] float aggroRange = 4f;
    [SerializeField] float timer = 0;
    public float timeBtwShoot = 1f;
    public bool canShoot = false;

    public Animator animator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Auto-find player if not assigned
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogWarning("<color=orange>[God] No player found! Looking for any ThirdPersonController...</color>");
                var controller = FindAnyObjectByType<StarterAssets.ThirdPersonController>();
                if (controller != null)
                {
                    player = controller.gameObject;
                }
            }
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (health <= 0f)
        {
            return;
        }

        Vector3 direction = player.transform.position - transform.position;
        direction.y = 0f; // 🔹 ignora la altura
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            // 🔹 solo rota el eje Y
            transform.rotation = Quaternion.Euler(0f, targetRotation.eulerAngles.y, 0f);
        }

        // 🔹 Hace que el firePoint mire directamente al jugador (si existe)
        if (firePoint != null)
        {
            Vector3 fireDirection = player.transform.position - firePoint.position;
            firePoint.rotation = Quaternion.LookRotation(fireDirection);
        }

        CheckIfCanShoot();
    }

    public void TakeDamage(int dmg)
    {
        health -= dmg;
        if (health <= 0)
        {
            //AudioManager.instance.PlayRandomPitchSFX(explosionSFX);
            animator.SetBool("Dead", true);
            Destroy(gameObject, 3);
        }
        else
        {
            animator.SetTrigger("Hit");
        }
    }
    void CheckIfCanShoot()
    {
        if (!canShoot)
        {
            if (timer <= timeBtwShoot)
            {
                timer += Time.deltaTime;
            }
            else
            {
                animator.SetTrigger("Shoot");
                timer = 0;
                canShoot = true;
            }
        }
    }

    public void WizardShoot()
    {
        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
    }

    public void WizardResetShoot()
    {
        canShoot = false;
    }
}
