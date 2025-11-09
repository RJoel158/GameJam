using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Adds Signal markers to Timeline to trigger boss spawn and effects during cinematic
/// </summary>
public class SetupTimelineSignals : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayableDirector director;
    [SerializeField] private BossCinematicEvents cinematicEvents;

    [Header("Signal Timing (seconds)")]
    [SerializeField] private float spawnBossTime = 6f;      // Spawn boss at 6 seconds (during Dramatic camera)
    [SerializeField] private float bossRoarTime = 7f;       // Boss roars at 7 seconds
    [SerializeField] private float groundImpactTime = 6.5f; // Ground impact at 6.5 seconds

    private void Reset()
    {
        director = GetComponent<PlayableDirector>();
        cinematicEvents = FindAnyObjectByType<BossCinematicEvents>();
    }

#if UNITY_EDITOR
    [ContextMenu("Setup Timeline Signals")]
    public void SetupSignals()
    {
        if (director == null)
        {
            Debug.LogError("<color=red>[Timeline Signals] No PlayableDirector found!</color>");
            return;
        }

        TimelineAsset timeline = director.playableAsset as TimelineAsset;
        if (timeline == null)
        {
            Debug.LogError("<color=red>[Timeline Signals] No Timeline Asset assigned!</color>");
            return;
        }

        if (cinematicEvents == null)
        {
            Debug.LogError("<color=red>[Timeline Signals] No BossCinematicEvents found in scene!</color>");
            return;
        }

        Debug.Log("<color=cyan>[Timeline Signals] Setting up signals...</color>");

        // Find or create Signal Track
        SignalTrack signalTrack = null;
        foreach (var track in timeline.GetOutputTracks())
        {
            if (track is SignalTrack)
            {
                signalTrack = track as SignalTrack;
                break;
            }
        }

        if (signalTrack == null)
        {
            signalTrack = timeline.CreateTrack<SignalTrack>(null, "Signal Track");
            Debug.Log("<color=green>[Timeline Signals] Created Signal Track</color>");
        }

        // Bind the signal track to the BossCinematicEvents receiver
        director.SetGenericBinding(signalTrack, cinematicEvents.gameObject);
        Debug.Log($"<color=green>[Timeline Signals] Bound Signal Track to: {cinematicEvents.gameObject.name}</color>");

        // Clear existing markers (optional)
        // Note: We can't easily clear markers via script, so we'll just add new ones

        Debug.Log("<color=yellow>[Timeline Signals] ⚠️ IMPORTANT: You need to manually add Signal Emitters in the Timeline Editor</color>");
        Debug.Log("<color=yellow>Steps:</color>");
        Debug.Log($"<color=yellow>1. Open Timeline Editor</color>");
        Debug.Log($"<color=yellow>2. Right-click on Signal Track at time {spawnBossTime}s → Add Signal Emitter → Choose 'SpawnBoss'</color>");
        Debug.Log($"<color=yellow>3. Right-click on Signal Track at time {bossRoarTime}s → Add Signal Emitter → Choose 'PlayBossRoar'</color>");
        Debug.Log($"<color=yellow>4. Right-click on Signal Track at time {groundImpactTime}s → Add Signal Emitter → Choose 'PlayGroundImpactEffect'</color>");

        EditorUtility.SetDirty(timeline);
        EditorUtility.SetDirty(director);

        Debug.Log("<color=green>[Timeline Signals] ✓ Signal Track configured! Now add the Signal Emitters manually (see instructions above)</color>");
    }

    [ContextMenu("Quick: Spawn Boss at 6 seconds (Manual Setup Guide)")]
    public void ShowManualSetupGuide()
    {
        Debug.Log("<color=cyan>========== MANUAL TIMELINE SIGNAL SETUP ==========</color>");
        Debug.Log("");
        Debug.Log("<color=white>To make the boss appear DURING the cinematic:</color>");
        Debug.Log("");
        Debug.Log("<color=yellow>STEP 1: Open Timeline Editor</color>");
        Debug.Log("  • Select BossCinematicTimeline in Hierarchy");
        Debug.Log("  • Window → Sequencing → Timeline");
        Debug.Log("");
        Debug.Log("<color=yellow>STEP 2: Add Signal at 6 seconds (when boss should spawn)</color>");
        Debug.Log("  • In Timeline, right-click on 'Signal Track' at time marker 6.0s");
        Debug.Log("  • Select 'Add Signal Emitter from Asset' → SpawnBoss (or similar)");
        Debug.Log("  • Or: Add marker manually and assign the SpawnBoss() method");
        Debug.Log("");
        Debug.Log("<color=yellow>STEP 3: Configure Signal Receiver</color>");
        Debug.Log("  • Make sure Signal Track is bound to BossCinematicEvents GameObject");
        Debug.Log("  • In the Signal Emitter, select the method to call: SpawnBoss()");
        Debug.Log("");
        Debug.Log("<color=yellow>ALTERNATIVE: Call from BossCinematicEvents</color>");
        Debug.Log("  • The BossCinematicEvents script has a SpawnBoss() method");
        Debug.Log("  • This method will call BossSpawner.SpawnBoss()");
        Debug.Log("  • Make sure BossSpawner reference is assigned in BossCinematicEvents");
        Debug.Log("");
        Debug.Log("<color=cyan>==================================================</color>");
    }

    [ContextMenu("Verify Signal Track Binding")]
    public void VerifySignalBinding()
    {
        if (director == null || director.playableAsset == null)
        {
            Debug.LogError("<color=red>[Timeline Signals] No director or timeline!</color>");
            return;
        }

        TimelineAsset timeline = director.playableAsset as TimelineAsset;
        
        Debug.Log("<color=cyan>========== SIGNAL TRACK VERIFICATION ==========</color>");

        SignalTrack signalTrack = null;
        foreach (var track in timeline.GetOutputTracks())
        {
            if (track is SignalTrack)
            {
                signalTrack = track as SignalTrack;
                var binding = director.GetGenericBinding(track);
                
                if (binding != null)
                {
                    Debug.Log($"<color=green>✓ Signal Track bound to: {binding}</color>");
                    
                    // Check if it has SignalReceiver component
                    GameObject boundObj = binding as GameObject;
                    if (boundObj != null)
                    {
                        var receiver = boundObj.GetComponent<SignalReceiver>();
                        if (receiver != null)
                        {
                            Debug.Log($"<color=green>✓ SignalReceiver component found</color>");
                        }
                        else
                        {
                            Debug.LogWarning($"<color=orange>⚠️ No SignalReceiver component on {boundObj.name}!</color>");
                            Debug.Log($"<color=yellow>Adding SignalReceiver...</color>");
                            boundObj.AddComponent<SignalReceiver>();
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"<color=orange>⚠️ Signal Track has NO BINDING!</color>");
                }
                
                break;
            }
        }

        if (signalTrack == null)
        {
            Debug.LogWarning("<color=orange>⚠️ No Signal Track found in Timeline!</color>");
        }

        Debug.Log("<color=cyan>===============================================</color>");
    }
#endif
}
