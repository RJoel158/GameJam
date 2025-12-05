using UnityEngine;

public class GarraWeapon : MonoBehaviour
{
    private FastEnemy enemy;
    private float lastDamageTime = -999f; // Inicializado para que pueda atacar inmediatamente
    private float damageCooldown = 2f; // Cooldown de 2 segundos entre ataques

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
            // Solo hacer daño si han pasado al menos 2 segundos desde el último golpe
            // Y solo si el enemigo está atacando
            if (enemy != null && enemy.isAttacking && Time.time >= lastDamageTime + damageCooldown)
            {
                var player = other.GetComponent<StarterAssets.ThirdPersonController>();
                if (player != null)
                {
                    player.TakeDamage(enemy.GetDamage());
                    lastDamageTime = Time.time;
                    Debug.Log($"<color=red>[GarraWeapon] ¡Golpeaste al jugador por {enemy.GetDamage()} de daño! Próximo ataque en 2s</color>");
                }
            }
            else if (enemy != null && !enemy.isAttacking)
            {
                Debug.Log("<color=yellow>[GarraWeapon] Colisión detectada pero el enemigo NO está atacando</color>");
            }
            else if (Time.time < lastDamageTime + damageCooldown)
            {
                float timeLeft = (lastDamageTime + damageCooldown) - Time.time;
                Debug.Log($"<color=orange>[GarraWeapon] En cooldown. Quedan {timeLeft:F1}s</color>");
            }
        }
    }
}
