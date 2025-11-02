using UnityEngine;

/// <summary>
/// Helper component to easily start missions in the scene and visualize defeat positions
/// </summary>
public class MissionDebugHelper : MonoBehaviour
{
    [Header("Mission Testing")]
    [Tooltip("Start mission automatically on scene start")]
    public bool startMissionOnStart = false;

    [Tooltip("Mission index to start (from MissionManager's available missions list)")]
    public int missionIndexToStart = 0;

    [Header("Visualization")]
    public bool showDefeatPositions = true;
    public Color defeatPositionColor = Color.red;
    public float defeatPositionRadius = 0.5f;

    private MissionManager missionManager;

    private void Start()
    {
        missionManager = MissionManager.Instance;

        if (missionManager == null)
        {
            Debug.LogError("[MissionDebugHelper] MissionManager not found in scene!");
            return;
        }

        if (startMissionOnStart)
        {
            StartMission();
        }
    }

    [ContextMenu("Start Mission")]
    public void StartMission()
    {
        if (missionManager == null)
        {
            missionManager = MissionManager.Instance;
        }

        if (missionManager != null)
        {
            missionManager.StartMission(missionIndexToStart);
        }
        else
        {
            Debug.LogError("[MissionDebugHelper] Cannot start mission - MissionManager not found!");
        }
    }

    [ContextMenu("Cancel Current Mission")]
    public void CancelMission()
    {
        if (missionManager != null)
        {
            missionManager.CancelCurrentMission();
        }
    }

    [ContextMenu("Show Mission Progress")]
    public void ShowMissionProgress()
    {
        if (missionManager != null && missionManager.HasActiveMission())
        {
            var mission = missionManager.currentMission;
            Debug.Log($"=== MISSION PROGRESS ===");
            Debug.Log($"Mission: {mission.missionName}");
            Debug.Log($"Progress: {mission.enemiesDefeated}/{mission.enemiesRequired}");
            Debug.Log($"Percentage: {mission.GetProgressPercentage():F1}%");
            Debug.Log($"Positions recorded: {mission.defeatPositions.Count}");

            for (int i = 0; i < mission.defeatPositions.Count; i++)
            {
                Debug.Log($"  Enemy {i + 1}: {mission.defeatPositions[i]}");
            }
        }
        else
        {
            Debug.Log("No active mission");
        }
    }

    [ContextMenu("Reset Current Mission")]
    public void ResetMission()
    {
        if (missionManager != null && missionManager.currentMission != null)
        {
            missionManager.currentMission.ResetMission();
            Debug.Log("Mission reset!");
        }
    }

    [ContextMenu("🔍 DIAGNOSTICO COMPLETO")]
    public void RunFullDiagnostic()
    {
        Debug.Log("========================================");
        Debug.Log("<color=yellow>🔍 DIAGNOSTICO DEL SISTEMA DE MISIONES</color>");
        Debug.Log("========================================");

        // 1. Check MissionManager
        Debug.Log("\n--- ✅ MissionManager ---");
        if (missionManager == null)
        {
            missionManager = MissionManager.Instance;
        }

        if (missionManager == null)
        {
            Debug.LogError("❌ MissionManager.Instance es NULL! Añade MissionManager a la escena.");
        }
        else
        {
            Debug.Log($"✅ MissionManager encontrado");
            Debug.Log($"   - Misiones disponibles: {missionManager.availableMissions.Count}");

            if (missionManager.currentMission == null)
            {
                Debug.LogWarning("⚠️ NO hay misión activa actualmente");
                Debug.LogWarning("   Usa: Start Mission para iniciar una");
            }
            else
            {
                Debug.Log($"✅ Misión activa: {missionManager.currentMission.missionName}");
                Debug.Log($"   - isActive: {missionManager.currentMission.isActive}");
                Debug.Log($"   - isCompleted: {missionManager.currentMission.isCompleted}");
                Debug.Log($"   - Progreso: {missionManager.currentMission.enemiesDefeated}/{missionManager.currentMission.enemiesRequired}");
            }
        }

        // 2. Check MissionUI
        Debug.Log("\n--- ✅ MissionUI ---");
        var missionUI = FindAnyObjectByType<MissionUI>();
        if (missionUI == null)
        {
            Debug.LogError("❌ NO se encontró MissionUI en la escena!");
            Debug.LogError("   Usa: GameObject > Mission System > Create Mission UI");
        }
        else
        {
            Debug.Log("✅ MissionUI encontrado");
            int nullRefs = 0;
            if (missionUI.missionPanel == null) { Debug.LogError("  ❌ missionPanel es NULL"); nullRefs++; }
            if (missionUI.missionNameText == null) { Debug.LogError("  ❌ missionNameText es NULL"); nullRefs++; }
            if (missionUI.progressText == null) { Debug.LogError("  ❌ progressText es NULL"); nullRefs++; }
            if (missionUI.progressSlider == null) { Debug.LogError("  ❌ progressSlider es NULL"); nullRefs++; }

            if (nullRefs == 0)
            {
                Debug.Log("✅ Todas las referencias de MissionUI están asignadas");
            }
            else
            {
                Debug.LogError($"❌ {nullRefs} referencias NULL en MissionUI!");
            }
        }

        // 3. Check Enemy event
        Debug.Log("\n--- ✅ Enemy Event ---");
        var enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        Debug.Log($"   Enemigos en escena: {enemies.Length}");

        var fieldInfo = typeof(Enemy).GetField("OnEnemyDefeated",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

        if (fieldInfo != null)
        {
            var eventDelegate = fieldInfo.GetValue(null) as System.Delegate;
            if (eventDelegate == null)
            {
                Debug.LogWarning("⚠️ Enemy.OnEnemyDefeated NO tiene suscriptores!");
                Debug.LogWarning("   Inicia una misión para que se suscriba al evento");
            }
            else
            {
                Debug.Log($"✅ Enemy.OnEnemyDefeated tiene {eventDelegate.GetInvocationList().Length} suscriptor(es)");
            }
        }

        Debug.Log("\n========================================");
        Debug.Log("<color=yellow>DIAGNOSTICO COMPLETADO</color>");
        Debug.Log("========================================\n");
    }

    [ContextMenu("🧪 Test: Simular Matar Enemigo")]
    public void TestKillEnemy()
    {
        Debug.Log("<color=yellow>🧪 Simulando muerte de enemigo...</color>");
        Enemy.TEST_TriggerEnemyDefeated(transform.position);
        Debug.Log("<color=green>✅ Evento Enemy.OnEnemyDefeated disparado manualmente!</color>");
    }

    private void OnDrawGizmos()
    {
        if (!showDefeatPositions) return;

        if (missionManager == null)
        {
            missionManager = MissionManager.Instance;
        }

        if (missionManager != null && missionManager.currentMission != null)
        {
            var positions = missionManager.currentMission.defeatPositions;

            Gizmos.color = defeatPositionColor;

            for (int i = 0; i < positions.Count; i++)
            {
                Vector3 pos = positions[i];

                // Draw sphere at defeat position
                Gizmos.DrawWireSphere(pos, defeatPositionRadius);

                // Draw vertical line
                Gizmos.DrawLine(pos, pos + Vector3.up * 2f);

                // Draw number label position
                Vector3 labelPos = pos + Vector3.up * 2.5f;

#if UNITY_EDITOR
                UnityEditor.Handles.Label(labelPos, $"Enemy {i + 1}");
#endif
            }
        }
    }
}
