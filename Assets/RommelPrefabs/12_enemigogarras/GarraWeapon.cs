using UnityEngine;

public class GarraWeapon : MonoBehaviour
{
    private FastEnemy enemy;

    void Start()
    {
        enemy = GetComponentInParent<FastEnemy>();
        
        if (enemy == null)
        {
            Debug.LogWarning("[GarraWeapon] No se encontró FastEnemy en el padre. El arma no hará daño.");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<StarterAssets.ThirdPersonController>();
            if (player != null && enemy != null)
            {
                player.TakeDamage(enemy.GetDamage());
                Debug.Log($"<color=red>[GarraWeapon] ¡Golpeaste al jugador por {enemy.GetDamage()} de daño!</color>");
            }
        }
    }
}
