using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDefeatEnemiesMission", menuName = "Missions/Defeat Enemies Mission")]
public class DefeatEnemiesMission : ScriptableObject
{
    [Header("Mission Info")]
    public string missionName = "Defeat Enemies";
    [TextArea(3, 6)]
    public string description = "Defeat X enemies to complete this mission";

    [Header("Mission Requirements")]
    public int enemiesRequired = 5;

    [Header("Mission Progress (Runtime)")]
    public int enemiesDefeated = 0;
    public bool isActive = false;
    public bool isCompleted = false;

    [Header("Enemy Defeat Positions")]
    public List<Vector3> defeatPositions = new List<Vector3>();

    /// <summary>
    /// Starts the mission and resets progress
    /// </summary>
    public void StartMission()
    {
        isActive = true;
        isCompleted = false;
        enemiesDefeated = 0;
        defeatPositions.Clear();

        // Subscribe to enemy defeated event
        Enemy.OnEnemyDefeated += OnEnemyDefeated;

        Debug.Log($"<color=yellow>[DefeatEnemiesMission] Mission Started: {missionName}</color>");
        Debug.Log($"<color=yellow>[DefeatEnemiesMission] Subscribed to Enemy.OnEnemyDefeated event</color>");
        Debug.Log($"<color=yellow>[DefeatEnemiesMission] Target: {enemiesRequired} enemies</color>");
    }

    /// <summary>
    /// Called when an enemy is defeated
    /// </summary>
    private void OnEnemyDefeated(Vector3 position)
    {
        if (!isActive)
        {
            Debug.LogWarning($"[DefeatEnemiesMission] Enemy defeated but mission is NOT ACTIVE!");
            return;
        }

        if (isCompleted)
        {
            Debug.LogWarning($"[DefeatEnemiesMission] Enemy defeated but mission is ALREADY COMPLETED!");
            return;
        }

        enemiesDefeated++;
        defeatPositions.Add(position);

        Debug.Log($"<color=cyan>[DefeatEnemiesMission] Enemy defeated at position: {position}. Progress: {enemiesDefeated}/{enemiesRequired}</color>");

        // Notify MissionManager IMMEDIATELY of progress change
        var manager = UnityEngine.Object.FindAnyObjectByType<MissionManager>();
        if (manager != null)
        {
            manager.OnMissionProgressChanged?.Invoke(enemiesDefeated, enemiesRequired);
        }

        CheckProgress();
    }    /// <summary>
         /// Checks if mission is complete
         /// </summary>
    private void CheckProgress()
    {
        if (enemiesDefeated >= enemiesRequired)
        {
            CompleteMission();
        }
    }

    /// <summary>
    /// Completes the mission
    /// </summary>
    private void CompleteMission()
    {
        isCompleted = true;
        isActive = false;

        // Unsubscribe from event
        Enemy.OnEnemyDefeated -= OnEnemyDefeated;

        Debug.Log($"<color=green>========================================</color>");
        Debug.Log($"<color=green>[DefeatEnemiesMission] MISSION COMPLETED: {missionName}</color>");
        Debug.Log($"<color=green>Total enemies defeated: {enemiesDefeated}</color>");
        Debug.Log($"<color=green>Defeat positions recorded: {defeatPositions.Count}</color>");
        Debug.Log($"<color=green>========================================</color>");

        // Print all positions
        for (int i = 0; i < defeatPositions.Count; i++)
        {
            Debug.Log($"<color=lime>Enemy {i + 1} defeated at: {defeatPositions[i]}</color>");
        }
    }

    /// <summary>
    /// Cancels the mission
    /// </summary>
    public void CancelMission()
    {
        if (isActive)
        {
            Enemy.OnEnemyDefeated -= OnEnemyDefeated;
        }

        isActive = false;
        isCompleted = false;

        Debug.Log($"Mission Cancelled: {missionName}");
    }

    /// <summary>
    /// Gets current progress as percentage
    /// </summary>
    public float GetProgressPercentage()
    {
        return enemiesRequired > 0 ? (float)enemiesDefeated / enemiesRequired * 100f : 0f;
    }

    /// <summary>
    /// Reset mission data (useful for testing)
    /// </summary>
    public void ResetMission()
    {
        if (isActive)
        {
            Enemy.OnEnemyDefeated -= OnEnemyDefeated;
        }

        isActive = false;
        isCompleted = false;
        enemiesDefeated = 0;
        defeatPositions.Clear();
    }
}
