using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Cinemachine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Fixes the Cinemachine Track binding to use CinemachineBrain instead of MainCamera
/// </summary>
public class FixTimelineCameraBinding : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayableDirector director;
    [SerializeField] private GameObject mainCameraObject;

    private void Reset()
    {
        director = GetComponent<PlayableDirector>();
    }

#if UNITY_EDITOR
    [ContextMenu("Fix Cinemachine Track Binding")]
    public void FixCinemachineBinding()
    {
        if (director == null)
        {
            Debug.LogError("<color=red>[Fix Binding] No PlayableDirector found!</color>");
            return;
        }

        TimelineAsset timeline = director.playableAsset as TimelineAsset;
        if (timeline == null)
        {
            Debug.LogError("<color=red>[Fix Binding] No Timeline Asset assigned!</color>");
            return;
        }

        Debug.Log("<color=cyan>[Fix Binding] Starting binding fix...</color>");

        // Find Main Camera
        if (mainCameraObject == null)
        {
            mainCameraObject = GameObject.FindWithTag("MainCamera");
        }

        if (mainCameraObject == null)
        {
            Debug.LogError("<color=red>[Fix Binding] No Main Camera found!</color>");
            return;
        }

        // Find or add CinemachineBrain
        CinemachineBrain brain = mainCameraObject.GetComponent<CinemachineBrain>();
        if (brain == null)
        {
            brain = mainCameraObject.AddComponent<CinemachineBrain>();
            Debug.Log("<color=green>[Fix Binding] Added CinemachineBrain to Main Camera</color>");
        }

        // Find Cinemachine Track
        CinemachineTrack cinemachineTrack = null;
        foreach (var track in timeline.GetOutputTracks())
        {
            if (track is CinemachineTrack)
            {
                cinemachineTrack = track as CinemachineTrack;
                break;
            }
        }

        if (cinemachineTrack == null)
        {
            Debug.LogError("<color=red>[Fix Binding] No Cinemachine Track found in Timeline!</color>");
            return;
        }

        // Bind the track to the CinemachineBrain
        director.SetGenericBinding(cinemachineTrack, brain);
        
        Debug.Log($"<color=green>[Fix Binding] ✓ Cinemachine Track bound to: {brain.gameObject.name}</color>");
        
        // Verify the binding
        var currentBinding = director.GetGenericBinding(cinemachineTrack);
        if (currentBinding != null)
        {
            Debug.Log($"<color=cyan>[Fix Binding] Current binding verified: {currentBinding}</color>");
        }

        EditorUtility.SetDirty(director);
        
        Debug.Log("<color=green>[Fix Binding] ✓ Binding fixed successfully!</color>");
    }

    [ContextMenu("Verify All Bindings")]
    public void VerifyAllBindings()
    {
        if (director == null || director.playableAsset == null)
        {
            Debug.LogError("<color=red>[Verify] No director or timeline!</color>");
            return;
        }

        TimelineAsset timeline = director.playableAsset as TimelineAsset;
        
        Debug.Log("<color=cyan>========== BINDINGS VERIFICATION ==========</color>");
        
        foreach (var track in timeline.GetOutputTracks())
        {
            var binding = director.GetGenericBinding(track);
            
            if (binding != null)
            {
                Debug.Log($"<color=green>✓ {track.name} → {binding}</color>");
            }
            else
            {
                Debug.LogWarning($"<color=orange>⚠️ {track.name} → NO BINDING</color>");
            }
        }
        
        Debug.Log("<color=cyan>==========================================</color>");
    }

    [ContextMenu("List All Virtual Cameras")]
    public void ListVirtualCameras()
    {
        CinemachineVirtualCamera[] vcams = FindObjectsByType<CinemachineVirtualCamera>(FindObjectsSortMode.None);
        
        Debug.Log($"<color=cyan>========== VIRTUAL CAMERAS ({vcams.Length}) ==========</color>");
        
        foreach (var vcam in vcams)
        {
            Debug.Log($"  • {vcam.name} - Priority: {vcam.Priority}, Enabled: {vcam.enabled}, Active: {vcam.gameObject.activeInHierarchy}");
        }
    }
#endif
}
