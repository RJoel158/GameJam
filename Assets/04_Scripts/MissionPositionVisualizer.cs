using UnityEngine;

/// <summary>
/// Visualiza las posiciones guardadas de enemigos derrotados
/// Útil para debugging, spawning de recompensas, efectos visuales, etc.
/// </summary>
public class MissionPositionVisualizer : MonoBehaviour
{
    [Header("Mission to Visualize")]
    public DefeatEnemiesMission mission;

    [Header("Visualization Settings")]
    public GameObject markerPrefab; // Opcional: prefab para mostrar en cada posición
    public bool spawnMarkersOnComplete = false;
    public bool showGizmos = true;
    public Color gizmoColor = Color.cyan;
    public float gizmoRadius = 0.5f;

    [Header("Runtime")]
    public bool missionWasCompleted = false;

    private GameObject[] spawnedMarkers;

    private void Start()
    {
        if (mission != null && MissionManager.Instance != null)
        {
            // Subscribe to mission completion
            MissionManager.Instance.OnMissionCompleted.AddListener(OnMissionCompleted);
        }
    }

    private void OnDestroy()
    {
        if (MissionManager.Instance != null)
        {
            MissionManager.Instance.OnMissionCompleted.RemoveListener(OnMissionCompleted);
        }
    }

    private void OnMissionCompleted(DefeatEnemiesMission completedMission)
    {
        if (completedMission == mission)
        {
            missionWasCompleted = true;

            Debug.Log($"[MissionPositionVisualizer] Mission completed! {mission.defeatPositions.Count} positions recorded.");

            if (spawnMarkersOnComplete && markerPrefab != null)
            {
                SpawnMarkers();
            }

            // Aquí puedes añadir tu lógica personalizada
            // Por ejemplo: spawner recompensas, efectos, etc.
        }
    }

    /// <summary>
    /// Spawns visual markers at each defeat position
    /// </summary>
    [ContextMenu("Spawn Markers")]
    public void SpawnMarkers()
    {
        if (mission == null || mission.defeatPositions.Count == 0)
        {
            Debug.LogWarning("[MissionPositionVisualizer] No positions to visualize!");
            return;
        }

        // Clear previous markers
        ClearMarkers();

        if (markerPrefab == null)
        {
            Debug.LogWarning("[MissionPositionVisualizer] No marker prefab assigned!");
            return;
        }

        spawnedMarkers = new GameObject[mission.defeatPositions.Count];

        for (int i = 0; i < mission.defeatPositions.Count; i++)
        {
            Vector3 pos = mission.defeatPositions[i];
            GameObject marker = Instantiate(markerPrefab, pos, Quaternion.identity, transform);
            marker.name = $"DefeatMarker_{i + 1}";
            spawnedMarkers[i] = marker;
        }

        Debug.Log($"[MissionPositionVisualizer] Spawned {spawnedMarkers.Length} markers");
    }

    /// <summary>
    /// Clears all spawned markers
    /// </summary>
    [ContextMenu("Clear Markers")]
    public void ClearMarkers()
    {
        if (spawnedMarkers != null)
        {
            foreach (var marker in spawnedMarkers)
            {
                if (marker != null)
                {
                    Destroy(marker);
                }
            }
            spawnedMarkers = null;
        }

        // Also clear any child markers
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// Gets all defeat positions from the mission
    /// </summary>
    public Vector3[] GetDefeatPositions()
    {
        if (mission != null)
        {
            return mission.defeatPositions.ToArray();
        }
        return new Vector3[0];
    }

    /// <summary>
    /// Do something at each defeat position
    /// Example: spawn rewards, effects, etc.
    /// </summary>
    [ContextMenu("Process All Positions")]
    public void ProcessAllPositions()
    {
        if (mission == null || mission.defeatPositions.Count == 0)
        {
            Debug.LogWarning("[MissionPositionVisualizer] No positions to process!");
            return;
        }

        for (int i = 0; i < mission.defeatPositions.Count; i++)
        {
            Vector3 pos = mission.defeatPositions[i];

            // AQUÍ añade tu lógica personalizada
            // Ejemplos:
            // - Spawner recompensas
            // - Crear efectos de partículas
            // - Marcar zonas de interés
            // - Guardar en base de datos

            Debug.Log($"Processing position {i + 1}: {pos}");
        }
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos || mission == null) return;

        Gizmos.color = gizmoColor;

        foreach (var pos in mission.defeatPositions)
        {
            // Draw sphere
            Gizmos.DrawWireSphere(pos, gizmoRadius);

            // Draw X marker
            Vector3 right = Vector3.right * gizmoRadius;
            Vector3 forward = Vector3.forward * gizmoRadius;

            Gizmos.DrawLine(pos - right - forward, pos + right + forward);
            Gizmos.DrawLine(pos + right - forward, pos - right + forward);
        }
    }
}
