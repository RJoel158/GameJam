using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class EnemyPowerUpDropper : MonoBehaviour
{
    [SerializeField] GameObject powerUpPrefab;
    [SerializeField] float dropHeight = 1f;

    void Start()
    {
        // Si no está asignado el prefab, intentar encontrarlo automáticamente
        if (powerUpPrefab == null)
        {
            powerUpPrefab = Resources.Load<GameObject>("PowerUpCapsule");
            
#if UNITY_EDITOR
            if (powerUpPrefab == null)
            {
                // Buscar en Assets/01_Prefabs/ si existe
                powerUpPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/01_Prefabs/PowerUpCapsule.prefab");
            }
#endif
            
            if (powerUpPrefab == null)
            {
                Debug.LogWarning($"[EnemyPowerUpDropper] PowerUpCapsule prefab no encontrado en {gameObject.name}");
            }
        }
    }

    public void DropPowerUp()
    {
        if (powerUpPrefab != null)
        {
            Vector3 dropPosition = transform.position + Vector3.up * dropHeight;
            GameObject powerUp = Instantiate(powerUpPrefab, dropPosition, Quaternion.identity);
            Debug.Log($"[POWER-UP] ¡Cápsula soltada en {dropPosition}!");
        }
        else
        {
            Debug.LogWarning("[EnemyPowerUpDropper] No se pudo soltar el power-up, prefab es null");
        }
    }
}

