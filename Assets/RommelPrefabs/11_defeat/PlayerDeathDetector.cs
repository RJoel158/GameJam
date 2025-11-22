using UnityEngine;
using StarterAssets;

/// <summary>
/// Monitorea el estado de muerte del jugador y activa la pantalla de derrota automáticamente.
/// Adjuntar este script al mismo GameObject que tiene ThirdPersonController.
/// </summary>
public class PlayerDeathDetector : MonoBehaviour
{
    private ThirdPersonController playerController;
    private bool deathScreenTriggered = false;

    void Start()
    {
        playerController = GetComponent<ThirdPersonController>();
        
        if (playerController == null)
        {
            Debug.LogError("PlayerDeathDetector: No se encontró ThirdPersonController en este GameObject!");
        }
    }

    void Update()
    {
        if (playerController != null && !deathScreenTriggered)
        {
            // Verificar si el jugador está muerto
            if (playerController.dead || playerController.health <= 0)
            {
                Debug.Log($"PlayerDeathDetector: Muerte detectada! dead={playerController.dead}, health={playerController.health}");
                TriggerDeathScreen();
            }
        }
    }

    void TriggerDeathScreen()
    {
        deathScreenTriggered = true;
        
        Debug.Log("PlayerDeathDetector: Intentando activar pantalla de muerte...");
        
        if (DeathScreenManager.Instance != null)
        {
            Debug.Log("PlayerDeathDetector: DeathScreenManager encontrado, activando pantalla de muerte!");
            DeathScreenManager.Instance.TriggerDeathScreen();
        }
        else
        {
            Debug.LogError("PlayerDeathDetector: DeathScreenManager.Instance es NULL! Asegúrate de que DeathScreenManager esté en la escena.");
        }
    }
}
