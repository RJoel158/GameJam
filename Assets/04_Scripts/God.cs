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

    // Referencia al BossController
    private BossController bossController;

    // Invulnerable flag (set by external controller during phases)
    [HideInInspector]
    public bool isInvulnerable = false;

    // Defensive flags to avoid spamming logs
    private bool warnedFirePointMissing = false;
    private bool warnedBulletMissing = false;

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

        // Obtener referencia al BossController
        bossController = GetComponent<BossController>();

        // Defensive: try to find a firePoint transform if not assigned
        if (firePoint == null)
        {
            // common child names
            string[] candidates = new string[] { "FirePoint", "firePoint", "Muzzle", "BulletSpawn", "Fire_Point" };
            foreach (var name in candidates)
            {
                var t = transform.Find(name);
                if (t != null)
                {
                    firePoint = t;
                    Debug.Log($"<color=cyan>[God] Auto-assigned firePoint to child '{name}'.</color>");
                    break;
                }
            }

            // fallback: any child named 'firepoint' ignoring case
            if (firePoint == null)
            {
                foreach (Transform child in transform)
                {
                    if (child.name.ToLowerInvariant().Contains("fire") || child.name.ToLowerInvariant().Contains("muzzle") || child.name.ToLowerInvariant().Contains("bullet"))
                    {
                        firePoint = child;
                        Debug.Log($"<color=cyan>[God] Auto-assigned firePoint to child '{child.name}' (fallback).</color>");
                        break;
                    }
                }
            }
        }

        // Ensure initial health matches maxHealth by default
        health = maxHealth;
        healthPercent = (maxHealth > 0) ? (health * 100f) / maxHealth : 100f;
        Debug.Log($"<color=green>[God] Initialized health for '{gameObject.name}': {health}/{maxHealth}</color>");
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

        // 🔹 Hace que el firePoint mire directamente al jugador (si existe)
        if (firePoint != null)
        {
            Vector3 fireDirection = player.transform.position - firePoint.position;
            firePoint.rotation = Quaternion.LookRotation(fireDirection);
        }
        else
        {
            if (!warnedFirePointMissing)
            {
                Debug.LogWarning("<color=orange>[God] firePoint is not assigned. Skipping firePoint rotation and shooting. Assign a Transform or child named 'FirePoint'.</color>");
                warnedFirePointMissing = true;
            }
        }
    }

    public void TakeDamage(int dmg)
    {
        if (isInvulnerable)
        {
            Debug.Log("<color=cyan>[God] Ignored damage while invulnerable.</color>");
            return;
        }

        int old = health;
        health -= dmg;
        Debug.Log($"<color=magenta>[God] TakeDamage called on '{gameObject.name}': -{dmg} -> health={health}/{maxHealth}</color>");
        if (health <= 0)
        {
            Debug.Log($"<color=red>[God] '{gameObject.name}' died due to TakeDamage.</color>");
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// External API to set invulnerability (useful for phase control)
    /// </summary>
    public void SetInvulnerable(bool flag)
    {
        isInvulnerable = flag;
        Debug.Log($"<color=cyan>[God] Invulnerable = {flag}</color>");
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

            // NUEVO: Verificar si el boss tiene energía suficiente
            bool canShoot = true;
            if (bossController != null)
            {
                canShoot = bossController.TryUseEnergy();
            }

            // Solo disparar si tiene energía (o si no tiene BossController)
            if (canShoot)
            {
                if (bulletPrefab == null)
                {
                    if (!warnedBulletMissing)
                    {
                        Debug.LogWarning("<color=orange>[God] bulletPrefab is not assigned. Unable to spawn projectiles.</color>");
                        warnedBulletMissing = true;
                    }
                }
                else if (firePoint == null)
                {
                    if (!warnedFirePointMissing)
                    {
                        Debug.LogWarning("<color=orange>[God] firePoint is not assigned. Unable to spawn projectiles.</color>");
                        warnedFirePointMissing = true;
                    }
                }
                else
                {
                    //AudioManager.instance.PlayRandomPitchSFX(shootSFX);
                    Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroRange);
    }
}
