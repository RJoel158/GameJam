using UnityEngine;
using TMPro;

/// <summary>
/// Diagnostic tool to check Mission System setup
/// Add this to any GameObject and use the context menu commands
/// </summary>
public class MissionSystemDiagnostic : MonoBehaviour
{
    [ContextMenu("Run Full Diagnostic")]
    public void RunFullDiagnostic()
    {
        Debug.Log("========================================");
        Debug.Log("<color=yellow>MISSION SYSTEM DIAGNOSTIC</color>");
        Debug.Log("========================================");

        // Check MissionManager
        CheckMissionManager();

        // Check MissionUI
        CheckMissionUI();

        // Check Enemy event
        CheckEnemyEvent();

        // Check Mission Asset
        CheckMissionAsset();

        Debug.Log("========================================");
        Debug.Log("<color=yellow>DIAGNOSTIC COMPLETE</color>");
        Debug.Log("========================================");
    }

    void CheckMissionManager()
    {
        Debug.Log("\n--- Checking MissionManager ---");

        var manager = MissionManager.Instance;
        if (manager == null)
        {
            Debug.LogError("❌ MissionManager.Instance is NULL! Add MissionManager component to scene.");
            return;
        }

        Debug.Log("✅ MissionManager found");

        if (manager.availableMissions == null || manager.availableMissions.Count == 0)
        {
            Debug.LogError("❌ MissionManager has NO missions in availableMissions array!");
        }
        else
        {
            Debug.Log($"✅ MissionManager has {manager.availableMissions.Count} available mission(s)");
            for (int i = 0; i < manager.availableMissions.Count; i++)
            {
                var mission = manager.availableMissions[i];
                if (mission == null)
                {
                    Debug.LogError($"❌ Mission at index {i} is NULL!");
                }
                else
                {
                    Debug.Log($"  Mission {i}: {mission.missionName} (Requires: {mission.enemiesRequired} enemies)");
                }
            }
        }

        if (manager.currentMission == null)
        {
            Debug.LogWarning("⚠️ No mission is currently active. Start a mission to test.");
        }
        else
        {
            Debug.Log($"✅ Active mission: {manager.currentMission.missionName}");
            Debug.Log($"  - Active: {manager.currentMission.isActive}");
            Debug.Log($"  - Completed: {manager.currentMission.isCompleted}");
            Debug.Log($"  - Progress: {manager.currentMission.enemiesDefeated}/{manager.currentMission.enemiesRequired}");
        }
    }

    void CheckMissionUI()
    {
        Debug.Log("\n--- Checking MissionUI ---");

        var missionUI = FindObjectOfType<MissionUI>();
        if (missionUI == null)
        {
            Debug.LogError("❌ MissionUI component not found in scene!");
            Debug.LogError("   Use GameObject > Mission System > Create Mission UI to create it.");
            return;
        }

        Debug.Log("✅ MissionUI component found");

        // Check references
        int nullReferences = 0;

        if (missionUI.missionPanel == null)
        {
            Debug.LogError("  ❌ missionPanel is NULL");
            nullReferences++;
        }
        else
        {
            Debug.Log($"  ✅ missionPanel assigned: {missionUI.missionPanel.name} (Active: {missionUI.missionPanel.activeSelf})");
        }

        if (missionUI.missionNameText == null)
        {
            Debug.LogError("  ❌ missionNameText is NULL");
            nullReferences++;
        }
        else
        {
            Debug.Log($"  ✅ missionNameText assigned: {missionUI.missionNameText.text}");
        }

        if (missionUI.missionDescriptionText == null)
        {
            Debug.LogError("  ❌ missionDescriptionText is NULL");
            nullReferences++;
        }
        else
        {
            Debug.Log($"  ✅ missionDescriptionText assigned");
        }

        if (missionUI.progressText == null)
        {
            Debug.LogError("  ❌ progressText is NULL");
            nullReferences++;
        }
        else
        {
            Debug.Log($"  ✅ progressText assigned: {missionUI.progressText.text}");
        }

        if (missionUI.progressSlider == null)
        {
            Debug.LogError("  ❌ progressSlider is NULL");
            nullReferences++;
        }
        else
        {
            Debug.Log($"  ✅ progressSlider assigned (Value: {missionUI.progressSlider.value}/{missionUI.progressSlider.maxValue})");
        }

        if (missionUI.progressFillImage == null)
        {
            Debug.LogError("  ❌ progressFillImage is NULL");
            nullReferences++;
        }
        else
        {
            Debug.Log("  ✅ progressFillImage assigned");
        }

        if (nullReferences > 0)
        {
            Debug.LogError($"❌ {nullReferences} NULL references found! Re-create Mission UI or assign references manually.");
        }
    }

    void CheckEnemyEvent()
    {
        Debug.Log("\n--- Checking Enemy Event ---");

        var enemies = FindObjectsOfType<Enemy>();
        if (enemies.Length == 0)
        {
            Debug.LogWarning("⚠️ No Enemy objects found in scene!");
        }
        else
        {
            Debug.Log($"✅ Found {enemies.Length} Enemy object(s) in scene");
        }

        // Check if event has subscribers
        var eventInfo = typeof(Enemy).GetEvent("OnEnemyDefeated",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

        if (eventInfo != null)
        {
            Debug.Log("✅ Enemy.OnEnemyDefeated event exists");

            var fieldInfo = typeof(Enemy).GetField("OnEnemyDefeated",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

            if (fieldInfo != null)
            {
                var eventDelegate = fieldInfo.GetValue(null) as System.Delegate;
                if (eventDelegate == null)
                {
                    Debug.LogWarning("⚠️ Enemy.OnEnemyDefeated has NO subscribers!");
                    Debug.LogWarning("   Make sure a mission is started (mission subscribes to this event).");
                }
                else
                {
                    Debug.Log($"✅ Enemy.OnEnemyDefeated has {eventDelegate.GetInvocationList().Length} subscriber(s)");
                }
            }
        }
    }

    void CheckMissionAsset()
    {
        Debug.Log("\n--- Checking Mission Asset ---");

        var manager = MissionManager.Instance;
        if (manager == null || manager.availableMissions == null || manager.availableMissions.Count == 0)
        {
            Debug.LogError("❌ Cannot check mission asset - no missions available");
            return;
        }

        var mission = manager.availableMissions[0];
        if (mission == null)
        {
            Debug.LogError("❌ First mission in array is NULL!");
            return;
        }

        Debug.Log($"Mission Name: {mission.missionName}");
        Debug.Log($"Description: {mission.description}");
        Debug.Log($"Enemies Required: {mission.enemiesRequired}");
        Debug.Log($"Is Active: {mission.isActive}");
        Debug.Log($"Is Completed: {mission.isCompleted}");
        Debug.Log($"Enemies Defeated: {mission.enemiesDefeated}");
        Debug.Log($"Defeat Positions Count: {mission.defeatPositions.Count}");
    }

    [ContextMenu("Test Kill Enemy")]
    public void TestKillEnemy()
    {
        Debug.Log("<color=yellow>Testing manual enemy defeat event...</color>");
        Enemy.OnEnemyDefeated?.Invoke(transform.position);
        Debug.Log("<color=yellow>Manual event fired!</color>");
    }

    [ContextMenu("Force Start First Mission")]
    public void ForceStartMission()
    {
        var manager = MissionManager.Instance;
        if (manager != null && manager.availableMissions.Count > 0)
        {
            manager.StartMission(0);
            Debug.Log("<color=green>Mission started!</color>");
        }
        else
        {
            Debug.LogError("Cannot start mission - MissionManager or missions not found!");
        }
    }
}
