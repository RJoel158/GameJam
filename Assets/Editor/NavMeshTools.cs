using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AI;
using UnityEngine.AI;

/// <summary>
/// Herramienta de editor para marcar objetos como Navigation Static y rebakear NavMesh
/// con parámetros personalizables. Coloca este archivo en `Assets/Editor`.
/// </summary>
public class NavMeshTools : EditorWindow
{
    // Bake parameters
    float agentRadius = 0.5f;
    float agentHeight = 2.0f;
    float agentSlope = 45f;
    float agentClimb = 0.4f; // step height
    bool overrideVoxel = true;
    float voxelSize = 0.1f;
    float minRegionArea = 2f;

    // Scope options
    bool markSelected = true;
    bool markAllMeshesAndTerrains = false;

    [MenuItem("Tools/NavMesh Tools/Open Window")]
    public static void ShowWindow()
    {
        var w = GetWindow<NavMeshTools>("NavMesh Tools");
        w.minSize = new Vector2(420, 260);
    }

    void OnGUI()
    {
        GUILayout.Label("Marking Options", EditorStyles.boldLabel);
        markSelected = EditorGUILayout.ToggleLeft("Mark Selected Objects Navigation Static", markSelected);
        markAllMeshesAndTerrains = EditorGUILayout.ToggleLeft("Also mark all MeshRenderers & Terrains in Scene", markAllMeshesAndTerrains);

        EditorGUILayout.Space();
        GUILayout.Label("Bake Parameters (recommended starting values)", EditorStyles.boldLabel);
        agentRadius = EditorGUILayout.FloatField("Agent Radius", agentRadius);
        agentHeight = EditorGUILayout.FloatField("Agent Height", agentHeight);
        agentSlope = EditorGUILayout.FloatField("Max Slope", agentSlope);
        agentClimb = EditorGUILayout.FloatField("Step Height", agentClimb);
        overrideVoxel = EditorGUILayout.Toggle("Override Voxel Size", overrideVoxel);
        EditorGUI.BeginDisabledGroup(!overrideVoxel);
        voxelSize = EditorGUILayout.FloatField("Voxel Size", voxelSize);
        EditorGUI.EndDisabledGroup();
        minRegionArea = EditorGUILayout.FloatField("Min Region Area", minRegionArea);

        EditorGUILayout.Space();

        if (GUILayout.Button("Mark & Bake Using Current Editor Settings"))
        {
            MarkNavigationStatic(markSelected, markAllMeshesAndTerrains);
            // Use the standard editor bake (keeps existing NavMeshEditorWindow settings)
            UnityEditor.AI.NavMeshBuilder.BuildNavMeshAsync();
            Debug.Log("[NavMeshTools] Triggered async bake (editor settings)");
        }

        if (GUILayout.Button("Bake With Custom Parameters"))
        {
            MarkNavigationStatic(markSelected, markAllMeshesAndTerrains);
            BakeWithCustomSettings();
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Collect All Navigation Static Meshes (Log)"))
        {
            var count = CollectAndLogStaticObjects();
            Debug.Log($"[NavMeshTools] Found {count} static mesh/terrain objects in scene.");
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Workflow: mark objects static -> adjust params -> Bake With Custom Parameters. If you prefer the default editor settings, use 'Mark & Bake Using Current Editor Settings'.", MessageType.Info);
    }

    private int CollectAndLogStaticObjects()
    {
        int count = 0;
        var renderers = Object.FindObjectsOfType<MeshRenderer>(true);
        foreach (var r in renderers)
        {
            if ((GameObjectUtility.GetStaticEditorFlags(r.gameObject) & StaticEditorFlags.NavigationStatic) != 0)
                count++;
        }
        var terrains = Object.FindObjectsOfType<Terrain>(true);
        foreach (var t in terrains)
        {
            if ((GameObjectUtility.GetStaticEditorFlags(t.gameObject) & StaticEditorFlags.NavigationStatic) != 0)
                count++;
        }
        return count;
    }

    private void MarkNavigationStatic(bool onlySelected, bool markAllMeshes)
    {
        if (onlySelected)
        {
            var selection = Selection.gameObjects;
            if (selection == null || selection.Length == 0)
            {
                Debug.LogWarning("[NavMeshTools] No objects selected to mark.");
            }
            foreach (var go in selection)
            {
                var flags = GameObjectUtility.GetStaticEditorFlags(go);
                flags |= StaticEditorFlags.NavigationStatic;
                GameObjectUtility.SetStaticEditorFlags(go, flags);
                EditorUtility.SetDirty(go);
            }
        }

        if (markAllMeshes)
        {
            var renderers = Object.FindObjectsOfType<Renderer>(true);
            foreach (var r in renderers)
            {
                var go = r.gameObject;
                var flags = GameObjectUtility.GetStaticEditorFlags(go);
                flags |= StaticEditorFlags.NavigationStatic;
                GameObjectUtility.SetStaticEditorFlags(go, flags);
                EditorUtility.SetDirty(go);
            }
        }

        // Ensure scene saved state reflects changes
        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        Debug.Log("[NavMeshTools] Marked objects as Navigation Static.");
    }

    private void BakeWithCustomSettings()
    {
        // Collect sources within scene bounds
        var renderers = Object.FindObjectsOfType<Renderer>(true);
        Bounds bounds;
        if (renderers.Length > 0)
        {
            bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
        }
        else
        {
            bounds = new Bounds(Vector3.zero, Vector3.one * 500f);
        }

        var sources = new List<NavMeshBuildSource>();

        // Manual collection: add MeshRenderers (meshes) and Terrains
        var renderersAll = Object.FindObjectsOfType<Renderer>(true);
        foreach (var r in renderersAll)
        {
            // Skip non-mesh renderers without MeshFilter
            var mf = r.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null)
            {
                var src = new NavMeshBuildSource();
                src.shape = NavMeshBuildSourceShape.Mesh;
                src.sourceObject = mf.sharedMesh;
                src.transform = r.transform.localToWorldMatrix;
                src.area = 0;
                sources.Add(src);
            }
        }

        var terrains = Object.FindObjectsOfType<Terrain>(true);
        foreach (var t in terrains)
        {
            if (t.terrainData != null)
            {
                var src = new NavMeshBuildSource();
                src.shape = NavMeshBuildSourceShape.Terrain;
                src.sourceObject = t.terrainData;
                src.transform = t.transform.localToWorldMatrix;
                src.area = 0;
                sources.Add(src);
            }
        }

        if (sources.Count == 0)
        {
            Debug.LogWarning("[NavMeshTools] No NavMesh sources found to build.");
            return;
        }

        // Get default build settings (agentType 0) and override selected fields
        NavMeshBuildSettings settings = NavMesh.GetSettingsByIndex(0);
        settings.agentRadius = agentRadius;
        settings.agentHeight = agentHeight;
        settings.agentSlope = agentSlope;
        settings.agentClimb = agentClimb;
        settings.minRegionArea = minRegionArea;
        settings.overrideVoxelSize = overrideVoxel;
        if (overrideVoxel) settings.voxelSize = voxelSize;

        // Build NavMeshData with our settings
        var data = UnityEngine.AI.NavMeshBuilder.BuildNavMeshData(settings, sources, bounds, Vector3.zero, Quaternion.identity);
        if (data == null)
        {
            Debug.LogError("[NavMeshTools] BuildNavMeshData returned null. Bake failed.");
            return;
        }

        // Remove existing NavMeshDataInstances we previously added (optional behavior)
        // Add the new data
        var instance = NavMesh.AddNavMeshData(data);
        if (instance.valid)
        {
            Debug.Log("[NavMeshTools] NavMesh baked successfully. NavMeshDataInstance valid=true");
        }
        else
        {
            Debug.LogError("[NavMeshTools] NavMeshDataInstance is invalid after adding.");
        }
    }
}
