using UnityEngine;

public class GarraWeapon : MonoBehaviour
{
    private FastEnemy enemy;
    private float lastDamageTime = -999f; // Inicializado para que pueda atacar inmediatamente
    public float damageCooldown = 2f; // Cooldown de 2 segundos entre ataques

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
            // Solo hacer daño si ha pasado el cooldown
            if (enemy != null && Time.time >= lastDamageTime + damageCooldown)
            {
                var player = other.GetComponent<StarterAssets.ThirdPersonController>();
                if (player != null)
                {
                    player.TakeDamage(enemy.GetDamage());
                    lastDamageTime = Time.time;
                    Debug.Log($"<color=red>[GarraWeapon] ¡Golpeaste al jugador por {enemy.GetDamage()} de daño!</color>");
                }
            }
        }
    }
}
