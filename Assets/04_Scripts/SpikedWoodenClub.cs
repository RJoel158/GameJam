using StarterAssets;
using UnityEngine;

public class SpikedWoodenClub : MonoBehaviour
{
    public int damage = 100;
    public Enemy enemyScript;
    public Boss bossScript; // Referencia específica para el boss
    public float hitTimer = 0;
    public float timeBwtHit = 1f;
    public bool canHit = false;

    private void Start()
    {
        // Si enemyScript no está asignado, buscar en el padre
        if (enemyScript == null && bossScript == null)
        {
            enemyScript = GetComponentInParent<Enemy>();

            if (enemyScript == null)
            {
                // Intentar buscar Boss si no es Enemy
                bossScript = GetComponentInParent<Boss>();
            }

            if (enemyScript != null)
            {
                Debug.Log($"<color=green>[SpikedWoodenClub] Enemy script encontrado automáticamente: {enemyScript.gameObject.name}</color>");
            }
            else if (bossScript != null)
            {
                Debug.Log($"<color=green>[SpikedWoodenClub] Boss script encontrado automáticamente: {bossScript.gameObject.name}</color>");
            }
            else
            {
                Debug.LogError("<color=red>[SpikedWoodenClub] No se pudo encontrar Enemy/Boss script en el padre! Asigna manualmente en el Inspector.</color>");
            }
        }
    }

    void FixedUpdate()
    {
        CheckIfCanHit();
    }

    void CheckIfCanHit()
    {
        if (!canHit)
        {
            hitTimer += Time.deltaTime;

            if (hitTimer >= timeBwtHit)
            {
                canHit = true;
                hitTimer = 0;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Verificar si es un enemigo o boss atacando
        bool isAttacking = false;

        if (enemyScript != null)
        {
            isAttacking = enemyScript.isAttacking;
        }
        else if (bossScript != null)
        {
            isAttacking = bossScript.isAttacking;
        }
        else
        {
            Debug.LogWarning("<color=orange>[SpikedWoodenClub] Ni enemyScript ni bossScript están asignados!</color>");
            return;
        }

        if (other.CompareTag("Player") && canHit && isAttacking)
        {
            ThirdPersonController playerController = other.GetComponent<ThirdPersonController>();
            if (playerController == null)
            {
                playerController = other.GetComponentInChildren<ThirdPersonController>();
            }
            if (playerController != null)
            {
                playerController.TakeDamage(damage);
                Debug.Log($"<color=red>[SpikedWoodenClub] Player recibió {damage} de daño!</color>");
            }
            canHit = false;
        }
    }
}
