using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using Cinemachine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Automatically configures Timeline with Cinemachine cameras
/// Run this from the Inspector to fix missing Timeline setup
/// </summary>
public class AutoConfigureTimeline : MonoBehaviour
{
    [Header("Auto-Configuration")]
    [SerializeField] private PlayableDirector director;
    [SerializeField] private float clipDuration = 3f;
    
    [Header("Quick Setup")]
    [Tooltip("Click this button to automatically configure the Timeline")]
    [SerializeField] private bool executeSetup = false;
    
    [Header("Camera Names")]
    [SerializeField] private string[] cameraNames = new string[]
    {
        "CM_BossIntro_Wide",
        "CM_BossIntro_CloseUp",
        "CM_BossIntro_Dramatic",
        "CM_BossIntro_PlayerReaction"
    };

    private void Reset()
    {
        director = GetComponent<PlayableDirector>();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (executeSetup && Application.isEditor && !Application.isPlaying)
        {
            executeSetup = false;
            UnityEditor.EditorApplication.delayCall += () => 
            {
                AutoConfigureTimelineWithCameras();
            };
        }
    }

    [ContextMenu("Auto-Configure Timeline with Cameras")]
    public void AutoConfigureTimelineWithCameras()
    {
        if (director == null)
        {
            Debug.LogError("<color=red>[AutoConfig] No PlayableDirector found!</color>");
            return;
        }

        TimelineAsset timeline = director.playableAsset as TimelineAsset;
        if (timeline == null)
        {
            Debug.LogError("<color=red>[AutoConfig] No Timeline Asset assigned to PlayableDirector!</color>");
            return;
        }

        Debug.Log("<color=cyan>[AutoConfig] Starting Timeline auto-configuration...</color>");

        // Find all virtual cameras
        CinemachineVirtualCamera[] allVCams = FindObjectsByType<CinemachineVirtualCamera>(FindObjectsSortMode.None);
        
        if (allVCams.Length == 0)
        {
            Debug.LogError("<color=red>[AutoConfig] No virtual cameras found in scene!</color>");
            return;
        }

        // Find or create Cinemachine track
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
            cinemachineTrack = timeline.CreateTrack<CinemachineTrack>(null, "Cinemachine Track");
            Debug.Log("<color=green>[AutoConfig] Created new Cinemachine Track</color>");
        }

        // Find CinemachineBrain to bind to
        CinemachineBrain brain = FindAnyObjectByType<CinemachineBrain>();
        if (brain != null)
        {
            director.SetGenericBinding(cinemachineTrack, brain.gameObject);
            Debug.Log($"<color=green>[AutoConfig] Bound Cinemachine track to: {brain.gameObject.name}</color>");
        }
        else
        {
            Debug.LogWarning("<color=orange>[AutoConfig] No CinemachineBrain found - track not bound!</color>");
        }

        // Clear existing clips
        var existingClips = cinemachineTrack.GetClips();
        foreach (var clip in existingClips)
        {
            timeline.DeleteClip(clip);
        }

        // Add clips for each camera we find
        double currentTime = 0;
        int cameraIndex = 0;

        foreach (string camName in cameraNames)
        {
            // Find the camera
            CinemachineVirtualCamera vcam = null;
            foreach (var cam in allVCams)
            {
                if (cam.name == camName)
                {
                    vcam = cam;
                    break;
                }
            }

            if (vcam == null)
            {
                Debug.LogWarning($"<color=orange>[AutoConfig] Camera '{camName}' not found - skipping</color>");
                continue;
            }

            // Create clip
            var shot = cinemachineTrack.CreateClip<CinemachineShot>();
            shot.start = currentTime;
            shot.duration = clipDuration;
            
            // Set the virtual camera reference
            CinemachineShot shotAsset = shot.asset as CinemachineShot;
            if (shotAsset != null)
            {
                shotAsset.VirtualCamera.exposedName = UnityEditor.GUID.Generate().ToString();
                director.SetReferenceValue(shotAsset.VirtualCamera.exposedName, vcam);
            }

            Debug.Log($"<color=green>[AutoConfig] Added clip {cameraIndex + 1}: {camName} ({currentTime}s - {currentTime + clipDuration}s)</color>");
            
            currentTime += clipDuration;
            cameraIndex++;
        }

        if (cameraIndex > 0)
        {
            // Set timeline duration
            timeline.durationMode = TimelineAsset.DurationMode.FixedLength;
            timeline.fixedDuration = currentTime;
            
            EditorUtility.SetDirty(timeline);
            EditorUtility.SetDirty(director);
            
            Debug.Log($"<color=cyan>[AutoConfig] ✓ Timeline configured with {cameraIndex} camera shots! Total duration: {currentTime}s</color>");
        }
        else
        {
            Debug.LogError("<color=red>[AutoConfig] Failed to add any camera clips!</color>");
        }
    }

    [ContextMenu("List Available Cameras")]
    public void ListAvailableCameras()
    {
        CinemachineVirtualCamera[] vcams = FindObjectsByType<CinemachineVirtualCamera>(FindObjectsSortMode.None);
        
        Debug.Log($"<color=cyan>========== AVAILABLE CAMERAS ({vcams.Length}) ==========</color>");
        
        foreach (var vcam in vcams)
        {
            Debug.Log($"  • {vcam.name} (Priority: {vcam.Priority}, FOV: {vcam.m_Lens.FieldOfView})");
        }
    }
#endif
}
