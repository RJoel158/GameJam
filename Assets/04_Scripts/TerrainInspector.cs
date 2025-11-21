using UnityEngine;

/// <summary>
/// Debug helper: imprime información relevante sobre el Terrain activo y todos los Terrains en la escena.
/// También puede comprobar la altura del terreno en la posición de los `EnemyCamp` existentes.
/// Adjuntar a un GameObject en la escena y pulsar Play; revisa la consola.
/// </summary>
public class TerrainInspector : MonoBehaviour
{
    public bool checkEnemyCamps = true;

    void Start()
    {
        InspectTerrains();

        if (checkEnemyCamps)
        {
            var camps = FindObjectsOfType<EnemyCamp>();
            foreach (var c in camps)
            {
                LogCampTerrainInfo(c);
            }
        }
    }

    void InspectTerrains()
    {
        var terrains = FindObjectsOfType<Terrain>();
        if (terrains == null || terrains.Length == 0)
        {
            Debug.LogWarning("TerrainInspector: No Terrain objects found in scene.");
            return;
        }

        Debug.Log($"TerrainInspector: Found {terrains.Length} Terrain(s) in the scene.");

        for (int i = 0; i < terrains.Length; i++)
        {
            Terrain t = terrains[i];
            var td = t.terrainData;
            var pos = t.GetPosition();
            bool hasCollider = t.GetComponent<TerrainCollider>() != null;
            Debug.Log($"--- Terrain[{i}] name='{t.name}' ---\nPosition: {pos} \nSize: {td.size} \nHeightmapResolution: {td.heightmapResolution} \nAlphamapResolution: {td.alphamapResolution} \nDetailResolution: {td.detailResolution} \nHas TerrainCollider: {hasCollider}");
        }
    }

    void LogCampTerrainInfo(EnemyCamp camp)
    {
        if (camp == null) return;
        Vector3 campPos = camp.transform.position;
        float sampledY = float.NaN;
        string source = "none";

        if (Terrain.activeTerrain != null)
        {
            sampledY = Terrain.activeTerrain.SampleHeight(campPos) + Terrain.activeTerrain.GetPosition().y;
            source = "Terrain.activeTerrain";
        }

        RaycastHit hit;
        bool hitGround = Physics.Raycast(campPos + Vector3.up * 0.5f, Vector3.down, out hit, 500f);
        if (hitGround)
        {
            Debug.Log($"EnemyCamp '{camp.name}' at {campPos}: Raycast hit '{hit.collider.name}' at y={hit.point.y} (collider layer={hit.collider.gameObject.layer})");
        }
        else if (!float.IsNaN(sampledY))
        {
            Debug.Log($"EnemyCamp '{camp.name}' at {campPos}: Sampled terrain height via {source} => y={sampledY}");
        }
        else
        {
            Debug.LogWarning($"EnemyCamp '{camp.name}' at {campPos}: No ground detected by Raycast and no active Terrain.");
        }
    }
}
