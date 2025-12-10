using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalTeleport : MonoBehaviour
{
    [Header("Portal Settings")]
    [SerializeField] string sceneToLoad = "NextLevel";
    [SerializeField] bool useSceneIndex = false;
    [SerializeField] int sceneIndex = 1;
    [SerializeField] string playerTag = "Player";
    
    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider collision)
    {
        // Solo activarse una vez
        if (hasTriggered) return;

        // Detectar si el jugador toca el portal
        if (collision.CompareTag(playerTag))
        {
            hasTriggered = true;
            TeleportPlayer();
        }
    }

    private void TeleportPlayer()
    {
        Debug.Log($"<color=yellow>[PortalTeleport] Player entered portal! Loading scene...</color>");

        // Cargar escena por nombre o índice
        if (useSceneIndex)
        {
            SceneManager.LoadScene(sceneIndex);
            Debug.Log($"<color=cyan>[PortalTeleport] Loading scene at index: {sceneIndex}</color>");
        }
        else
        {
            SceneManager.LoadScene(sceneToLoad);
            Debug.Log($"<color=cyan>[PortalTeleport] Loading scene: {sceneToLoad}</color>");
        }
    }
}
