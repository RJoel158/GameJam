using UnityEngine;

public class God : MonoBehaviour
{
    [SerializeField] GameObject player;
    public Transform firePoint;
    public GameObject bulletPrefab;

    [Header("Stadistics")]
    [Range(0f, 100f)]
    public float healthPercent = 100f;
    public int health = 1000;
    public int maxHealth = 2000;

    [SerializeField] float aggroRange = 4f;
    [SerializeField] float timer = 0;
    public float timeBtwShoot = 1f;

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
    }

    // Update is called once per frame
    void Update()
    {
        // Safety check
        if (player == null) return;

        CheckIfCanShoot();

        Vector3 direction = player.transform.position - transform.position;
        direction.y = 0f; // 🔹 ignora la altura
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            // 🔹 solo rota el eje Y
            transform.rotation = Quaternion.Euler(0f, targetRotation.eulerAngles.y, 0f);
        }

        // 🔹 Hace que el firePoint mire directamente al jugador
        Vector3 fireDirection = player.transform.position - firePoint.position;
        firePoint.rotation = Quaternion.LookRotation(fireDirection);
    }

    public void TakeDamage(int dmg)
    {
        health -= dmg;
        if (health <= 0)
        {
            //AudioManager.instance.PlayRandomPitchSFX(explosionSFX);
            //Spawner.instance.EnemyKilled();
            Destroy(gameObject);
        }
    }

    void CheckIfCanShoot()
    {
        if (timer <= timeBtwShoot)
        {
            timer += Time.deltaTime;
        }
        else
        {
            timer = 0;
            //AudioManager.instance.PlayRandomPitchSFX(shootSFX);
            Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroRange);
    }
}
