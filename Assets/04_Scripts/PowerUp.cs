using UnityEngine;
using System.Collections;
using StarterAssets;

public class PowerUp : MonoBehaviour
{
    public enum PowerUpType { FullHealth, UnlimitedStamina }
    
    [SerializeField] PowerUpType type = PowerUpType.FullHealth;
    [SerializeField] float rotationSpeed = 100f;
    [SerializeField] float bobSpeed = 2f;
    [SerializeField] float bobHeight = 0.5f;

    private Vector3 startPosition;
    private ThirdPersonController playerController;
    private FaseColorController faseColorController;

    void Start()
    {
        startPosition = transform.position;
        
        // Si el tipo está en default (FullHealth), elegir uno aleatorio
        if (type == PowerUpType.FullHealth)
        {
            type = Random.value > 0.5f ? PowerUpType.FullHealth : PowerUpType.UnlimitedStamina;
            Debug.Log($"[POWER-UP] Tipo asignado aleatoriamente: {type}");
        }
    }

    void Update()
    {
        // Rotación
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);

        // Movimiento arriba y abajo (bobbing)
        Vector3 newPos = startPosition;
        newPos.y += Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = newPos;
    }

    void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerController = collision.GetComponent<ThirdPersonController>();
            faseColorController = FindFirstObjectByType<FaseColorController>();
            
            ApplyPowerUp();
            // NO destruir el GameObject aún - esperar a que el coroutine termine
        }
    }

    void ApplyPowerUp()
    {
        if (type == PowerUpType.FullHealth)
        {
            if (playerController != null)
            {
                StartCoroutine(AutoRegenHealthEffect());
            }
        }
        else if (type == PowerUpType.UnlimitedStamina)
        {
            if (faseColorController != null)
            {
                StartCoroutine(UnlimitedStaminaEffect());
            }
        }
    }

    IEnumerator AutoRegenHealthEffect()
    {
        float duration = 30f;
        float elapsed = 0f;

        Debug.Log("[POWER-UP] ¡Regeneración de vida activada por 30 segundos!");

        while (elapsed < duration && playerController != null)
        {
            // Regenerar vida automáticamente
            playerController.health = playerController.maxHealth;
            elapsed += Time.deltaTime;
            yield return null;
        }

        Debug.Log("[POWER-UP] Regeneración de vida terminada");
        Destroy(gameObject);
    }

    IEnumerator UnlimitedStaminaEffect()
    {
        float duration = 30f;
        float elapsed = 0f;

        Debug.Log("[POWER-UP] ¡Estamina infinita activada por 30 segundos!");
        
        // Activar el flag de estamina infinita
        faseColorController.unlimitedStaminaActive = true;

        // Esperar exactamente 30 segundos
        while (elapsed < duration)
        {
            if (faseColorController == null)
            {
                Debug.LogWarning("[POWER-UP] FaseColorController se destruyó");
                break;
            }
            
            elapsed += Time.deltaTime;
            Debug.Log($"[POWER-UP] Tiempo transcurrido: {elapsed:F2}/{duration} segundos");
            
            yield return null;
        }
        
        // Desactivar el flag cuando termine
        if (faseColorController != null)
        {
            faseColorController.unlimitedStaminaActive = false;
            Debug.Log("[POWER-UP] Estamina infinita terminada");
        }
        
        Destroy(gameObject);
    }
}
