using UnityEngine;

[CreateAssetMenu(fileName = "NewDefeatBossMission", menuName = "Missions/Defeat Boss Mission")]
public class DefeatBossMission : DefeatEnemiesMission
{
    [Header("Boss Specific Settings")]
    [Tooltip("The tag of the boss GameObject to track")]
    public string bossTag = "Boss";

    [Header("Boss Information (Runtime)")]
    public Vector3 bossDefeatPosition;
    public float bossDefeatTime;

    private bool bossEventSubscribed = false;

    /// <summary>
    /// Starts the mission and resets progress
    /// </summary>
    public new void StartMission()
    {
        // Set enemy requirement to 1 (one boss to defeat)
        enemiesRequired = 1;
        missionName = string.IsNullOrEmpty(missionName) ? "Defeat the Boss" : missionName;
        description = string.IsNullOrEmpty(description) ? "Defeat the powerful boss to complete this mission" : description;

        // Call base implementation
        base.StartMission();

        // Subscribe to boss defeated event (in addition to enemy event)
        if (!bossEventSubscribed)
        {
            Boss.OnBossDefeated += OnBossDefeated;
            bossEventSubscribed = true;
            Debug.Log($"<color=yellow>[DefeatBossMission] Subscribed to Boss.OnBossDefeated event</color>");
        }
    }

    /// <summary>
    /// Called when the boss is defeated
    /// </summary>
    private void OnBossDefeated(Vector3 position)
    {
        if (!isActive)
        {
            Debug.LogWarning($"[DefeatBossMission] Boss defeated but mission is NOT ACTIVE!");
            return;
        }

        if (isCompleted)
        {
            Debug.LogWarning($"[DefeatBossMission] Boss defeated but mission is ALREADY COMPLETED!");
            return;
        }

        bossDefeatPosition = position;
        bossDefeatTime = Time.time;

        Debug.Log($"<color=cyan>[DefeatBossMission] Boss defeated at position: {position}</color>");

        // Increment enemies defeated (boss counts as 1 enemy)
        enemiesDefeated = 1;
        defeatPositions.Add(position);

        // Notify MissionManager of progress change
        var manager = UnityEngine.Object.FindAnyObjectByType<MissionManager>();
        if (manager != null)
        {
            manager.OnMissionProgressChanged?.Invoke(1, 1);
        }

        // The base class CheckProgress will handle completion automatically
    }

    /// <summary>
    /// Cancels the mission
    /// </summary>
    public new void CancelMission()
    {
        if (bossEventSubscribed)
        {
            Boss.OnBossDefeated -= OnBossDefeated;
            bossEventSubscribed = false;
        }

        base.CancelMission();
    }

    /// <summary>
    /// Reset mission data (useful for testing)
    /// </summary>
    public new void ResetMission()
    {
        if (bossEventSubscribed)
        {
            Boss.OnBossDefeated -= OnBossDefeated;
            bossEventSubscribed = false;
        }

        bossDefeatPosition = Vector3.zero;
        bossDefeatTime = 0f;

        base.ResetMission();
    }
}
