using UnityEngine;

/// <summary>
/// Ejemplo simple de cómo usar el sistema de misiones desde código
/// Adjunta este script a cualquier GameObject en la escena
/// </summary>
public class MissionStarter : MonoBehaviour
{
    [Header("Mission to Start")]
    [Tooltip("Arrastra aquí el ScriptableObject de la misión que quieres iniciar")]
    public DefeatEnemiesMission missionToStart;

    [Header("Settings")]
    [Tooltip("Iniciar la misión automáticamente al comenzar")]
    public bool startOnAwake = true;

    [Tooltip("Delay en segundos antes de iniciar la misión")]
    public float startDelay = 0f;

    private void Start()
    {
        if (startOnAwake)
        {
            if (startDelay > 0)
            {
                Invoke(nameof(StartMission), startDelay);
            }
            else
            {
                StartMission();
            }
        }
    }

    /// <summary>
    /// Inicia la misión configurada. Puedes llamar este método desde un botón o evento
    /// </summary>
    public void StartMission()
    {
        if (missionToStart == null)
        {
            Debug.LogError("[MissionStarter] No mission assigned! Drag a DefeatEnemiesMission asset to the field.");
            return;
        }

        MissionManager manager = MissionManager.Instance;

        if (manager == null)
        {
            Debug.LogError("[MissionStarter] MissionManager not found in scene! Add a MissionManager component to the scene.");
            return;
        }

        manager.StartMission(missionToStart);
        Debug.Log($"[MissionStarter] Started mission: {missionToStart.missionName}");
    }

    /// <summary>
    /// Cancela la misión actual
    /// </summary>
    public void CancelMission()
    {
        MissionManager manager = MissionManager.Instance;

        if (manager != null)
        {
            manager.CancelCurrentMission();
            Debug.Log("[MissionStarter] Mission cancelled");
        }
    }
}
