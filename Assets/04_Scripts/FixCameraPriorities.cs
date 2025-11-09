using UnityEngine;
using Cinemachine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Fixes virtual camera priorities for cinematic system
/// </summary>
public class FixCameraPriorities : MonoBehaviour
{
#if UNITY_EDITOR
    [ContextMenu("Fix All Camera Priorities")]
    public void FixCameraPriorities_Execute()
    {
        Debug.Log("<color=cyan>[Camera Priorities] Fixing camera priorities...</color>");

        CinemachineVirtualCamera[] allCameras = FindObjectsByType<CinemachineVirtualCamera>(FindObjectsSortMode.None);

        int fixedCount = 0;

        foreach (var vcam in allCameras)
        {
            // Las cámaras de cinemática (BossIntro) deben tener prioridad 0
            if (vcam.name.Contains("BossIntro") || vcam.name.Contains("CM_Boss"))
            {
                if (vcam.Priority != 0)
                {
                    vcam.Priority = 0;
                    EditorUtility.SetDirty(vcam);
                    Debug.Log($"<color=yellow>[Camera Priorities] {vcam.name}: Priority set to 0 (cinematic camera)</color>");
                    fixedCount++;
                }
            }
            // La cámara del jugador debe tener prioridad 10
            else if (vcam.name.Contains("Player") || vcam.name.Contains("Follow"))
            {
                if (vcam.Priority != 10)
                {
                    vcam.Priority = 10;
                    EditorUtility.SetDirty(vcam);
                    Debug.Log($"<color=green>[Camera Priorities] {vcam.name}: Priority set to 10 (player camera)</color>");
                    fixedCount++;
                }
            }
        }

        Debug.Log($"<color=cyan>[Camera Priorities] ✓ Fixed {fixedCount} cameras</color>");
        ListCurrentPriorities();
    }

    [ContextMenu("List Current Priorities")]
    public void ListCurrentPriorities()
    {
        CinemachineVirtualCamera[] allCameras = FindObjectsByType<CinemachineVirtualCamera>(FindObjectsSortMode.None);

        Debug.Log("<color=cyan>========== CURRENT CAMERA PRIORITIES ==========</color>");

        foreach (var vcam in allCameras)
        {
            string color = vcam.Priority == 0 ? "yellow" : "green";
            Debug.Log($"<color={color}>  • {vcam.name} - Priority: {vcam.Priority}</color>");
        }

        Debug.Log("<color=cyan>===============================================</color>");
    }

    [ContextMenu("Set Cinematic Cameras to Priority 15 (For Testing)")]
    public void SetCinematicCamerasHighPriority()
    {
        Debug.Log("<color=magenta>[Camera Priorities] Setting cinematic cameras to HIGH priority for testing...</color>");

        CinemachineVirtualCamera[] allCameras = FindObjectsByType<CinemachineVirtualCamera>(FindObjectsSortMode.None);

        foreach (var vcam in allCameras)
        {
            if (vcam.name.Contains("BossIntro") || vcam.name.Contains("CM_Boss"))
            {
                vcam.Priority = 15;
                EditorUtility.SetDirty(vcam);
                Debug.Log($"<color=magenta>[Camera Priorities] {vcam.name}: Priority set to 15 (TESTING)</color>");
            }
        }

        Debug.Log("<color=magenta>[Camera Priorities] ✓ Test setup complete - you should see cinematic cameras now</color>");
    }
#endif
}
