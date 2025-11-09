using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Cinemachine;

/// <summary>
/// Diagnostics tool to debug Timeline and Camera issues
/// Attach this to your BossCinematicTimeline GameObject
/// </summary>
public class TimelineDiagnostics : MonoBehaviour
{
    [Header("Auto-detect")]
    [SerializeField] private PlayableDirector director;
    
    [Header("Diagnostics")]
    [SerializeField] private bool runDiagnosticsOnStart = true;
    [SerializeField] private bool continuousDiagnostics = false;

    private void Start()
    {
        if (director == null)
        {
            director = GetComponent<PlayableDirector>();
        }

        if (runDiagnosticsOnStart)
        {
            RunFullDiagnostics();
        }
    }

    private void Update()
    {
        if (continuousDiagnostics && director != null)
        {
            Debug.Log($"<color=white>[Timeline] Playing: {director.state}, Time: {director.time:F2}/{director.duration:F2}, TimeScale: {Time.timeScale}</color>");
        }
    }

    [ContextMenu("Run Full Diagnostics")]
    public void RunFullDiagnostics()
    {
        Debug.Log("<color=cyan>========== TIMELINE DIAGNOSTICS ==========</color>");
        
        CheckDirector();
        CheckTimelineAsset();
        CheckCinemachineBrain();
        CheckVirtualCameras();
        CheckTimelineBindings();
        
        Debug.Log("<color=cyan>========== END DIAGNOSTICS ==========</color>");
    }

    private void CheckDirector()
    {
        Debug.Log("<color=yellow>--- PlayableDirector Check ---</color>");
        
        if (director == null)
        {
            Debug.LogError("<color=red>❌ No PlayableDirector found!</color>");
            return;
        }

        Debug.Log($"<color=green>✓ PlayableDirector found: {director.name}</color>");
        Debug.Log($"  State: {director.state}");
        Debug.Log($"  Update Mode: {director.timeUpdateMode}");
        Debug.Log($"  Duration: {director.duration}");
        Debug.Log($"  Playable Asset: {(director.playableAsset != null ? director.playableAsset.name : "NULL")}");
    }

    private void CheckTimelineAsset()
    {
        Debug.Log("<color=yellow>--- Timeline Asset Check ---</color>");
        
        if (director == null || director.playableAsset == null)
        {
            Debug.LogError("<color=red>❌ No Timeline Asset assigned!</color>");
            return;
        }

        TimelineAsset timeline = director.playableAsset as TimelineAsset;
        if (timeline == null)
        {
            Debug.LogError("<color=red>❌ PlayableAsset is not a TimelineAsset!</color>");
            return;
        }

        Debug.Log($"<color=green>✓ Timeline Asset: {timeline.name}</color>");
        Debug.Log($"  Track Count: {timeline.outputTrackCount}");
        
        foreach (var track in timeline.GetOutputTracks())
        {
            Debug.Log($"  • Track: {track.name} (Type: {track.GetType().Name})");
            
            if (track is CinemachineTrack)
            {
                Debug.Log($"    <color=cyan>Cinemachine Track detected!</color>");
            }
        }
    }

    private void CheckCinemachineBrain()
    {
        Debug.Log("<color=yellow>--- Cinemachine Brain Check ---</color>");
        
        CinemachineBrain brain = FindAnyObjectByType<CinemachineBrain>();
        if (brain == null)
        {
            Debug.LogError("<color=red>❌ No CinemachineBrain found in scene!</color>");
            Debug.LogWarning("<color=orange>⚠️ You need a CinemachineBrain on your Main Camera!</color>");
            return;
        }

        Debug.Log($"<color=green>✓ CinemachineBrain found on: {brain.gameObject.name}</color>");
        Debug.Log($"  Active: {brain.gameObject.activeInHierarchy}");
        Debug.Log($"  Enabled: {brain.enabled}");
        Debug.Log($"  Active Virtual Camera: {(brain.ActiveVirtualCamera != null ? brain.ActiveVirtualCamera.Name : "None")}");
        Debug.Log($"  Default Blend: {brain.m_DefaultBlend.m_Time}s ({brain.m_DefaultBlend.m_Style})");
    }

    private void CheckVirtualCameras()
    {
        Debug.Log("<color=yellow>--- Virtual Cameras Check ---</color>");
        
        CinemachineVirtualCamera[] vcams = FindObjectsByType<CinemachineVirtualCamera>(FindObjectsSortMode.None);
        
        if (vcams.Length == 0)
        {
            Debug.LogWarning("<color=orange>⚠️ No CinemachineVirtualCameras found in scene!</color>");
            return;
        }

        Debug.Log($"<color=green>✓ Found {vcams.Length} Virtual Camera(s):</color>");
        
        foreach (var vcam in vcams)
        {
            Debug.Log($"  • {vcam.name}");
            Debug.Log($"    Priority: {vcam.Priority}");
            Debug.Log($"    FOV: {vcam.m_Lens.FieldOfView}");
            Debug.Log($"    Active: {vcam.gameObject.activeInHierarchy}");
            Debug.Log($"    Enabled: {vcam.enabled}");
        }
    }

    private void CheckTimelineBindings()
    {
        Debug.Log("<color=yellow>--- Timeline Bindings Check ---</color>");
        
        if (director == null || director.playableAsset == null)
        {
            Debug.LogError("<color=red>❌ Cannot check bindings - no director or asset!</color>");
            return;
        }

        TimelineAsset timeline = director.playableAsset as TimelineAsset;
        if (timeline == null) return;

        foreach (var track in timeline.GetOutputTracks())
        {
            var binding = director.GetGenericBinding(track);
            
            if (binding != null)
            {
                Debug.Log($"<color=green>✓ Track '{track.name}' bound to: {binding}</color>");
            }
            else
            {
                Debug.LogWarning($"<color=orange>⚠️ Track '{track.name}' has NO BINDING!</color>");
            }
        }
    }

    [ContextMenu("Force Play Timeline")]
    public void ForcePlayTimeline()
    {
        if (director != null)
        {
            director.timeUpdateMode = DirectorUpdateMode.UnscaledGameTime;
            director.Play();
            Debug.Log("<color=magenta>[Timeline] Forced play with UnscaledGameTime mode</color>");
        }
    }

    [ContextMenu("Set Unscaled Time Mode")]
    public void SetUnscaledTimeMode()
    {
        if (director != null)
        {
            director.timeUpdateMode = DirectorUpdateMode.UnscaledGameTime;
            Debug.Log("<color=green>[Timeline] Set to UnscaledGameTime mode</color>");
        }
    }
}
