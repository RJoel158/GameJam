using UnityEngine;
using Cinemachine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Positions cinematic cameras around the boss spawn point with cinematic angles
/// </summary>
public class PositionCinematicCameras : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform bossSpawnPoint;
    [SerializeField] private Transform playerTransform;

    [Header("Camera Positioning")]
    [SerializeField] private float cameraDistance = 10f;
    [SerializeField] private float cameraHeight = 2f;

    [Header("Auto-Find")]
    [SerializeField] private bool autoFind = true;

#if UNITY_EDITOR
    [ContextMenu("Position All Cinematic Cameras")]
    public void PositionAllCameras()
    {
        Debug.Log("<color=cyan>[Camera Position] Positioning cinematic cameras...</color>");

        if (autoFind)
        {
            FindReferences();
        }

        if (bossSpawnPoint == null)
        {
            Debug.LogError("<color=red>[Camera Position] Boss Spawn Point not found!</color>");
            return;
        }

        Vector3 spawnPos = bossSpawnPoint.position;
        CinemachineVirtualCamera[] allCameras = FindObjectsByType<CinemachineVirtualCamera>(FindObjectsSortMode.None);

        foreach (var vcam in allCameras)
        {
            if (vcam.name.Contains("Wide"))
            {
                PositionWideCamera(vcam, spawnPos);
            }
            else if (vcam.name.Contains("CloseUp"))
            {
                PositionCloseUpCamera(vcam, spawnPos);
            }
            else if (vcam.name.Contains("Dramatic"))
            {
                PositionDramaticCamera(vcam, spawnPos);
            }
            else if (vcam.name.Contains("PlayerReaction"))
            {
                PositionPlayerReactionCamera(vcam);
            }
        }

        Debug.Log("<color=green>[Camera Position] ✓ All cameras positioned!</color>");
    }

    private void PositionWideCamera(CinemachineVirtualCamera vcam, Vector3 spawnPos)
    {
        // Wide shot: Far away, elevated, to show the whole scene
        Vector3 offset = new Vector3(-15f, 8f, -15f);
        vcam.transform.position = spawnPos + offset;
        vcam.transform.LookAt(spawnPos);
        
        vcam.LookAt = bossSpawnPoint;
        vcam.Follow = null;
        vcam.m_Lens.FieldOfView = 60f;

        EditorUtility.SetDirty(vcam.transform);
        EditorUtility.SetDirty(vcam);
        
        Debug.Log($"<color=green>[Camera Position] {vcam.name} positioned at {vcam.transform.position}</color>");
    }

    private void PositionCloseUpCamera(CinemachineVirtualCamera vcam, Vector3 spawnPos)
    {
        // Close-up: Near the boss, eye level
        Vector3 offset = new Vector3(3f, 2f, 3f);
        vcam.transform.position = spawnPos + offset;
        vcam.transform.LookAt(spawnPos + Vector3.up * 2f); // Look at boss head height
        
        vcam.LookAt = bossSpawnPoint;
        vcam.Follow = bossSpawnPoint;
        vcam.m_Lens.FieldOfView = 45f;

        EditorUtility.SetDirty(vcam.transform);
        EditorUtility.SetDirty(vcam);
        
        Debug.Log($"<color=green>[Camera Position] {vcam.name} positioned at {vcam.transform.position}</color>");
    }

    private void PositionDramaticCamera(CinemachineVirtualCamera vcam, Vector3 spawnPos)
    {
        // Dramatic: Low angle looking up at boss
        Vector3 offset = new Vector3(5f, 0.5f, -5f);
        vcam.transform.position = spawnPos + offset;
        vcam.transform.LookAt(spawnPos + Vector3.up * 3f); // Look up at boss
        
        vcam.LookAt = bossSpawnPoint;
        vcam.Follow = bossSpawnPoint;
        vcam.m_Lens.FieldOfView = 55f;

        EditorUtility.SetDirty(vcam.transform);
        EditorUtility.SetDirty(vcam);
        
        Debug.Log($"<color=green>[Camera Position] {vcam.name} positioned at {vcam.transform.position}</color>");
    }

    private void PositionPlayerReactionCamera(CinemachineVirtualCamera vcam)
    {
        if (playerTransform == null)
        {
            Debug.LogWarning("<color=orange>[Camera Position] Player not found for PlayerReaction camera</color>");
            return;
        }

        // Player reaction: Behind and slightly to the side of player, looking at player's face
        Vector3 playerPos = playerTransform.position;
        Vector3 offset = new Vector3(2f, 1.5f, -2f);
        vcam.transform.position = playerPos + offset;
        vcam.transform.LookAt(playerPos + Vector3.up * 1.5f);
        
        vcam.LookAt = playerTransform;
        vcam.Follow = playerTransform;
        vcam.m_Lens.FieldOfView = 50f;

        EditorUtility.SetDirty(vcam.transform);
        EditorUtility.SetDirty(vcam);
        
        Debug.Log($"<color=green>[Camera Position] {vcam.name} positioned at {vcam.transform.position}</color>");
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
            }
            else
            {
                // Try to find from BossSpawner
                BossSpawner spawner = FindAnyObjectByType<BossSpawner>();
                if (spawner != null)
                {
                    Debug.LogWarning("<color=orange>[Camera Position] Looking for spawn point in BossSpawner...</color>");
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
            }
            else
            {
                var controller = FindAnyObjectByType<StarterAssets.ThirdPersonController>();
                if (controller != null)
                {
                    playerTransform = controller.transform;
                }
            }
        }

        if (bossSpawnPoint != null)
        {
            Debug.Log($"<color=green>[Camera Position] Boss Spawn Point: {bossSpawnPoint.name} at {bossSpawnPoint.position}</color>");
        }
        
        if (playerTransform != null)
        {
            Debug.Log($"<color=green>[Camera Position] Player: {playerTransform.name} at {playerTransform.position}</color>");
        }
    }

    [ContextMenu("List Camera Positions")]
    public void ListCameraPositions()
    {
        if (bossSpawnPoint == null)
        {
            Debug.LogWarning("<color=orange>[Camera Position] Boss Spawn Point not set!</color>");
            return;
        }

        Vector3 spawnPos = bossSpawnPoint.position;
        CinemachineVirtualCamera[] allCameras = FindObjectsByType<CinemachineVirtualCamera>(FindObjectsSortMode.None);

        Debug.Log("<color=cyan>========== CAMERA POSITIONS ==========</color>");
        Debug.Log($"<color=white>Boss Spawn Point: {spawnPos}</color>");
        Debug.Log("");

        foreach (var vcam in allCameras)
        {
            if (vcam.name.Contains("BossIntro") || vcam.name.Contains("CM_Boss"))
            {
                float distance = Vector3.Distance(vcam.transform.position, spawnPos);
                Debug.Log($"<color=yellow>{vcam.name}</color>");
                Debug.Log($"  Position: {vcam.transform.position}");
                Debug.Log($"  Distance from spawn: {distance:F2}m");
                Debug.Log($"  FOV: {vcam.m_Lens.FieldOfView}");
                Debug.Log($"  LookAt: {(vcam.LookAt != null ? vcam.LookAt.name : "NONE")}");
                Debug.Log($"  Follow: {(vcam.Follow != null ? vcam.Follow.name : "NONE")}");
                Debug.Log("");
            }
        }

        Debug.Log("<color=cyan>======================================</color>");
    }

    [ContextMenu("Move Cameras to Boss Spawn Parent")]
    public void MoveCamerasToParent()
    {
        if (bossSpawnPoint == null)
        {
            FindReferences();
        }

        if (bossSpawnPoint == null)
        {
            Debug.LogError("<color=red>[Camera Position] Need boss spawn point!</color>");
            return;
        }

        // Create a parent object for better organization
        GameObject cameraParent = GameObject.Find("BossCinematicCameras");
        if (cameraParent == null)
        {
            cameraParent = new GameObject("BossCinematicCameras");
            cameraParent.transform.position = bossSpawnPoint.position;
        }

        CinemachineVirtualCamera[] allCameras = FindObjectsByType<CinemachineVirtualCamera>(FindObjectsSortMode.None);

        foreach (var vcam in allCameras)
        {
            if (vcam.name.Contains("BossIntro") || vcam.name.Contains("CM_Boss"))
            {
                vcam.transform.SetParent(cameraParent.transform);
                Debug.Log($"<color=green>[Camera Position] {vcam.name} moved to parent</color>");
            }
        }
    }
#endif
}
