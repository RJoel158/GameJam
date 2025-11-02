using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance { get; private set; }

    [Header("Mission Settings")]
    public List<DefeatEnemiesMission> availableMissions = new List<DefeatEnemiesMission>();
    public DefeatEnemiesMission currentMission;

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

            // Check if completed
            if (currentMission.isCompleted)
            {
                Debug.Log($"<color=green>[MissionManager] Invoking OnMissionCompleted event!</color>");
                OnMissionCompleted?.Invoke(currentMission);
                Debug.Log($"<color=green>Mission Completed: {currentMission.missionName}</color>");
                currentMission = null;
            }
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
}
