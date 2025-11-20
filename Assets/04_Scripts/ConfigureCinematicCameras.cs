using UnityEngine;
using Cinemachine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Configures cinematic virtual cameras to look at the boss spawn point
/// </summary>
public class ConfigureCinematicCameras : MonoBehaviour
{
    [Header("Target References")]
    [SerializeField] private Transform bossSpawnPoint;
    [SerializeField] private Transform playerTransform;

    [Header("Auto-Find")]
    [Tooltip("Automatically find Boss Spawn Point and Player")]
    [SerializeField] private bool autoFind = true;

#if UNITY_EDITOR
    [ContextMenu("Configure Cinematic Cameras")]
    public void ConfigureCameras()
    {
        Debug.Log("<color=cyan>[Camera Config] Configuring cinematic cameras...</color>");

        // Auto-find references if needed
        if (autoFind)
        {
            FindReferences();
        }

        if (bossSpawnPoint == null)
        {
            Debug.LogError("<color=red>[Camera Config] Boss Spawn Point not found! Please assign it manually or create a GameObject named 'BossSpawnPoint'</color>");
            return;
        }

        // Find all cinematic cameras
        CinemachineVirtualCamera[] allCameras = FindObjectsByType<CinemachineVirtualCamera>(FindObjectsSortMode.None);
        int configured = 0;

        foreach (var vcam in allCameras)
        {
            // Only configure cinematic cameras (BossIntro)
            if (vcam.name.Contains("BossIntro") || vcam.name.Contains("CM_Boss"))
            {
                // Set LookAt to boss spawn point
                vcam.LookAt = bossSpawnPoint;

                // Configure Follow based on camera type
                if (vcam.name.Contains("Wide"))
                {
                    // Wide shot: no follow, just look at spawn point
                    vcam.Follow = null;
                    Debug.Log($"<color=green>[Camera Config] {vcam.name}: LookAt = {bossSpawnPoint.name}, Follow = None (wide shot)</color>");
                }
                else if (vcam.name.Contains("PlayerReaction"))
                {
                    // Player reaction: look at player
                    if (playerTransform != null)
                    {
                        vcam.LookAt = playerTransform;
                        vcam.Follow = playerTransform;
                        Debug.Log($"<color=green>[Camera Config] {vcam.name}: LookAt = Player, Follow = Player</color>");
                    }
                    else
                    {
                        vcam.LookAt = bossSpawnPoint;
                        Debug.LogWarning($"<color=orange>[Camera Config] {vcam.name}: Player not found, using spawn point</color>");
                    }
                }
                else
                {
                    // Other cameras: follow boss spawn point
                    vcam.Follow = bossSpawnPoint;
                    Debug.Log($"<color=green>[Camera Config] {vcam.name}: LookAt = {bossSpawnPoint.name}, Follow = {bossSpawnPoint.name}</color>");
                }

                EditorUtility.SetDirty(vcam);
                configured++;
            }
        }

        Debug.Log($"<color=cyan>[Camera Config] ✓ Configured {configured} cinematic cameras</color>");
    }

    [ContextMenu("Find References")]
    public void FindReferences()
    {
        // Find Boss Spawn Point
        if (bossSpawnPoint == null)
        {
            GameObject spawnObj = GameObject.Find("BossSpawnPoint");
            if (spawnObj != null)
            {
                bossSpawnPoint = spawnObj.transform;
                Debug.Log($"<color=green>[Camera Config] Found BossSpawnPoint: {spawnObj.name}</color>");
            }
            else
            {
                // Try to find BossSpawner component
                BossSpawner spawner = FindAnyObjectByType<BossSpawner>();
                if (spawner != null)
                {
                    // Look for spawn point field via reflection or just log
                    Debug.LogWarning("<color=orange>[Camera Config] BossSpawnPoint GameObject not found. Using scene origin (0,0,0) temporarily.</color>");
                    
                    // Create a temporary target at origin
                    GameObject tempTarget = new GameObject("BossSpawnPoint_Temp");
                    tempTarget.transform.position = Vector3.zero;
                    bossSpawnPoint = tempTarget.transform;
                }
            }
        }

        // Find Player
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                Debug.Log($"<color=green>[Camera Config] Found Player: {player.name}</color>");
            }
            else
            {
                var controller = FindAnyObjectByType<StarterAssets.ThirdPersonController>();
                if (controller != null)
                {
                    playerTransform = controller.transform;
                    Debug.Log($"<color=green>[Camera Config] Found Player via ThirdPersonController: {controller.name}</color>");
                }
            }
        }
    }

    [ContextMenu("List Camera Configurations")]
    public void ListCameraConfigurations()
    {
        CinemachineVirtualCamera[] allCameras = FindObjectsByType<CinemachineVirtualCamera>(FindObjectsSortMode.None);

        Debug.Log("<color=cyan>========== CAMERA CONFIGURATIONS ==========</color>");

        foreach (var vcam in allCameras)
        {
            if (vcam.name.Contains("BossIntro") || vcam.name.Contains("CM_Boss"))
            {
                string lookAt = vcam.LookAt != null ? vcam.LookAt.name : "NONE";
                string follow = vcam.Follow != null ? vcam.Follow.name : "NONE";
                
                Debug.Log($"<color=yellow>  • {vcam.name}</color>");
                Debug.Log($"    LookAt: {lookAt}");
                Debug.Log($"    Follow: {follow}");
                Debug.Log($"    Priority: {vcam.Priority}");
            }
        }

        Debug.Log("<color=cyan>==========================================</color>");
    }

    [ContextMenu("Create Boss Spawn Point Visual")]
    public void CreateBossSpawnPointVisual()
    {
        GameObject visual = GameObject.Find("BossSpawnPoint");
        if (visual == null)
        {
            visual = new GameObject("BossSpawnPoint");
            visual.transform.position = Vector3.zero;
            
            // Add a visual marker
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = "Visual";
            sphere.transform.SetParent(visual.transform);
            sphere.transform.localPosition = Vector3.zero;
            sphere.transform.localScale = Vector3.one * 2f;
            
            var renderer = sphere.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.red;
            }

            Debug.Log("<color=green>[Camera Config] Created BossSpawnPoint visual marker at origin. Move it to where the boss should appear!</color>");
        }
        else
        {
            Debug.Log($"<color=yellow>[Camera Config] BossSpawnPoint already exists at {visual.transform.position}</color>");
        }

        bossSpawnPoint = visual.transform;
    }
#endif
}
