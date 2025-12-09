using StarterAssets;
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
        // Log wizard configuration for debugging
        Debug.Log($"<color=cyan>[Wizard] Initialized: name={gameObject.name}, tag={gameObject.tag}, layer={LayerMask.LayerToName(gameObject.layer)}, health={health}/{maxHealth}</color>");

        // Check colliders
        var colliders = GetComponents<Collider>();
        bool hasTrigger = false;
        foreach (var col in colliders)
        {
            Debug.Log($"  - Collider: {col.GetType().Name}, isTrigger={col.isTrigger}, enabled={col.enabled}");
            if (col.isTrigger) hasTrigger = true;
        }

        // Auto-fix: ensure we have a trigger collider for sword damage detection
        if (!hasTrigger)
        {
            Debug.LogWarning($"<color=orange>[Wizard] No trigger collider found! Adding one automatically for sword damage detection.</color>");
            var triggerCol = gameObject.AddComponent<CapsuleCollider>();
            triggerCol.isTrigger = true;
            triggerCol.radius = 0.5f;
            triggerCol.height = 2f;
            triggerCol.center = new Vector3(0, 1f, 0);
            Debug.Log($"<color=green>[Wizard] ✓ Added trigger CapsuleCollider (radius=0.5, height=2)</color>");
        }

        // Check animator
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            Debug.Log($"  - Animator found: {(animator != null)}");
        }

        // Auto-find player if not assigned
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogWarning("<color=orange>[Wizard] No player found! Looking for any ThirdPersonController...</color>");
                var controller = FindAnyObjectByType<StarterAssets.ThirdPersonController>();
                if (controller != null)
                {
                    player = controller.gameObject;
                    Debug.Log($"[Wizard] Found player via ThirdPersonController: {player.name}");
                }
            }
            else
            {
                Debug.Log($"[Wizard] Found player by tag: {player.name}");
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
        Debug.Log($"<color=red>[Wizard.TakeDamage] ✅ CALLED! GameObject={gameObject.name}, damage={dmg}, health={health}, isAlive={health > 0}</color>");

        health -= dmg;
        Debug.Log($"<color=yellow>[Wizard] Health after damage: {health}/{maxHealth} on {gameObject.name}</color>");

        if (health <= 0)
        {
            Debug.Log($"<color=red>[Wizard] ¡MUERTO! health={health}</color>");
            //AudioManager.instance.PlayRandomPitchSFX(explosionSFX);
            animator.SetBool("Dead", true);
            Destroy(gameObject, 3);
        }
        else
        {
            Debug.Log($"[Wizard] Setting 'Hit' trigger on animator");
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

    public void WizardShootSound()
    {
        // Reproducir sonido de dispara al jugador
        if (PlayerAudioManager.Instance != null)
            PlayerAudioManager.Instance.PlayWizardShootSound();
    }

    public void WizardResetShoot()
    {
        canShoot = false;
    }
}
