using StarterAssets;
using UnityEngine;

public class Sword : MonoBehaviour
{
    [SerializeField] ThirdPersonController thirdPersonController;
    public int damage = 10;
    public float hitTimer = 0;
    public float timeBwtHit = 1f;
    public bool canHit = false;

    void Update()
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
        // Verificar si puede atacar
        if (!canHit || thirdPersonController.dead || !thirdPersonController.isAttacking)
            return;

        // Atacar enemigos normales
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponentInChildren<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                canHit = false;
                Debug.Log($"<color=yellow>[Sword] Golpeaste a un enemigo por {damage} de daño!</color>");
            }
        }

        // Atacar al BOSS
        if (other.CompareTag("Boss"))
        {
            Boss boss = other.GetComponent<Boss>();
            if (boss != null)
            {
                boss.TakeDamage(damage);
                canHit = false;
                Debug.Log($"<color=yellow>[Sword] Golpeaste al jefe por {damage} de daño!</color>");
            }

            //// Usar SendMessage para llamar a TakeDamage en el Boss
            //other.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);

            //// Intentar en el padre si no funcionó
            //if (other.transform.parent != null)
            //{
            //    other.transform.parent.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
            //}

            //canHit = false;
            //Debug.Log($"<color=red>[Sword] GOLPEASTE AL BOSS por {damage} de daño!</color>");
        }
    }
}
