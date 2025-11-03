using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance { get; private set; }

    [Header("Mission Settings")]
    public List<DefeatEnemiesMission> availableMissions = new List<DefeatEnemiesMission>();
    public DefeatEnemiesMission currentMission;

    [Header("Mission Queue Settings")]
    public bool autoStartNextMission = true;
    public float delayBeforeNextMission = 2f;

    private Queue<DefeatEnemiesMission> missionQueue = new Queue<DefeatEnemiesMission>();

    [Header("Events")]
    public UnityEvent<DefeatEnemiesMission> OnMissionStarted;
    public UnityEvent<DefeatEnemiesMission> OnMissionCompleted;
    public UnityEvent<int, int> OnMissionProgressChanged; // current, total

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Initialize events if null
        if (OnMissionStarted == null) OnMissionStarted = new UnityEvent<DefeatEnemiesMission>();
        if (OnMissionCompleted == null) OnMissionCompleted = new UnityEvent<DefeatEnemiesMission>();
        if (OnMissionProgressChanged == null) OnMissionProgressChanged = new UnityEvent<int, int>();
    }

    private void Update()
    {
        // Check current mission progress
        if (currentMission != null && currentMission.isActive)
        {
            // Notify UI of progress changes
            if (Time.frameCount % 10 == 0) // Check every 10 frames to avoid spam
            {
                Debug.Log($"<color=magenta>[MissionManager] Progress check: {currentMission.enemiesDefeated}/{currentMission.enemiesRequired}</color>");
                OnMissionProgressChanged?.Invoke(currentMission.enemiesDefeated, currentMission.enemiesRequired);
            }

            // Note: Completion is now handled immediately in OnMissionComplete() 
            // called by DefeatEnemiesMission.CompleteMission()
        }
    }

    /// <summary>
    /// Starts a mission by index from available missions
    /// </summary>
    public void StartMission(int missionIndex)
    {
        if (missionIndex < 0 || missionIndex >= availableMissions.Count)
        {
            Debug.LogError($"Mission index {missionIndex} out of range!");
            return;
        }

        StartMission(availableMissions[missionIndex]);
    }

    /// <summary>
    /// Starts a specific mission
    /// </summary>
    public void StartMission(DefeatEnemiesMission mission)
    {
        if (mission == null)
        {
            Debug.LogError("Cannot start null mission!");
            return;
        }

        // Cancel current mission if any
        if (currentMission != null && currentMission.isActive)
        {
            Debug.LogWarning($"Cancelling current mission: {currentMission.missionName}");
            currentMission.CancelMission();
        }

        currentMission = mission;
        currentMission.StartMission();

        OnMissionStarted?.Invoke(currentMission);

        Debug.Log($"<color=yellow>Mission Started: {mission.missionName}</color>");
        Debug.Log($"Objective: Defeat {mission.enemiesRequired} enemies");
    }

    /// <summary>
    /// Cancels the current mission
    /// </summary>
    public void CancelCurrentMission()
    {
        if (currentMission != null && currentMission.isActive)
        {
            currentMission.CancelMission();
            Debug.Log($"Mission cancelled: {currentMission.missionName}");
            currentMission = null;
        }
    }

    /// <summary>
    /// Called by DefeatEnemiesMission when it completes
    /// </summary>
    public void OnMissionComplete(DefeatEnemiesMission mission)
    {
        if (mission == currentMission)
        {
            Debug.Log($"<color=green>[MissionManager] OnMissionComplete called for: {mission.missionName}</color>");
            Debug.Log($"<color=green>[MissionManager] Invoking OnMissionCompleted event IMMEDIATELY!</color>");
            OnMissionCompleted?.Invoke(currentMission);
            Debug.Log($"<color=green>Mission Completed: {currentMission.missionName}</color>");
            currentMission = null;

            // Check if there are more missions in queue
            CheckAndStartNextMission();
        }
        else
        {
            Debug.LogWarning($"<color=orange>[MissionManager] Received completion for mission '{mission.missionName}' but it's not the current mission!</color>");
        }
    }

    /// <summary>
    /// Gets current mission progress percentage
    /// </summary>
    public float GetCurrentMissionProgress()
    {
        if (currentMission != null)
        {
            return currentMission.GetProgressPercentage();
        }
        return 0f;
    }

    /// <summary>
    /// Checks if there's an active mission
    /// </summary>
    public bool HasActiveMission()
    {
        return currentMission != null && currentMission.isActive;
    }

    /// <summary>
    /// Gets all defeat positions from current mission
    /// </summary>
    public List<Vector3> GetCurrentMissionDefeatPositions()
    {
        if (currentMission != null)
        {
            return new List<Vector3>(currentMission.defeatPositions);
        }
        return new List<Vector3>();
    }

    /// <summary>
    /// Debug method to visualize defeat positions
    /// </summary>
    private void OnDrawGizmos()
    {
        if (currentMission != null && currentMission.defeatPositions.Count > 0)
        {
            Gizmos.color = Color.red;
            foreach (var pos in currentMission.defeatPositions)
            {
                Gizmos.DrawWireSphere(pos, 0.5f);
                Gizmos.DrawLine(pos, pos + Vector3.up * 2f);
            }
        }
    }

    /// <summary>
    /// Adds a mission to the queue
    /// </summary>
    public void QueueMission(DefeatEnemiesMission mission)
    {
        if (mission == null)
        {
            Debug.LogError("[MissionManager] Cannot queue null mission!");
            return;
        }

        missionQueue.Enqueue(mission);
        Debug.Log($"<color=cyan>[MissionManager] Mission '{mission.missionName}' added to queue. Queue size: {missionQueue.Count}</color>");

        // If no mission is active, start this one
        if (currentMission == null)
        {
            CheckAndStartNextMission();
        }
    }

    /// <summary>
    /// Queues all available missions
    /// </summary>
    public void QueueAllMissions()
    {
        foreach (var mission in availableMissions)
        {
            if (mission != null)
            {
                missionQueue.Enqueue(mission);
            }
        }
        Debug.Log($"<color=cyan>[MissionManager] All missions queued. Total: {missionQueue.Count}</color>");

        // Start first mission if none is active
        if (currentMission == null)
        {
            CheckAndStartNextMission();
        }
    }

    /// <summary>
    /// Checks if there's a next mission and starts it
    /// </summary>
    public void CheckAndStartNextMission()
    {
        if (!autoStartNextMission)
        {
            Debug.Log("[MissionManager] Auto-start is disabled. Call StartNextMission() manually.");
            return;
        }

        if (missionQueue.Count > 0)
        {
            Debug.Log($"<color=cyan>[MissionManager] Next mission in queue. Starting in {delayBeforeNextMission}s...</color>");
            StartCoroutine(StartNextMissionDelayed());
        }
        else
        {
            Debug.Log("<color=yellow>[MissionManager] No more missions in queue.</color>");
        }
    }

    private IEnumerator StartNextMissionDelayed()
    {
        yield return new WaitForSeconds(delayBeforeNextMission);

        if (missionQueue.Count > 0)
        {
            DefeatEnemiesMission nextMission = missionQueue.Dequeue();
            Debug.Log($"<color=cyan>[MissionManager] Starting next mission: {nextMission.missionName}</color>");
            StartMission(nextMission);
        }
    }

    /// <summary>
    /// Manually starts the next mission in queue
    /// </summary>
    public void StartNextMission()
    {
        if (missionQueue.Count > 0)
        {
            DefeatEnemiesMission nextMission = missionQueue.Dequeue();
            StartMission(nextMission);
        }
        else
        {
            Debug.LogWarning("[MissionManager] No missions in queue!");
        }
    }

    /// <summary>
    /// Gets the number of missions in queue
    /// </summary>
    public int GetQueueCount()
    {
        return missionQueue.Count;
    }

    /// <summary>
    /// Clears all missions from queue
    /// </summary>
    public void ClearQueue()
    {
        missionQueue.Clear();
        Debug.Log("[MissionManager] Mission queue cleared.");
    }
}
