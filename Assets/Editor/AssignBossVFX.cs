using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using StarterAssets;

// Utility to quickly assign a Hovl effect prefab to BossSpawner and Player components
public static class AssignBossVFX
{
    [MenuItem("Game Jam/Setup/Assign Boss Spawn VFX (Portal blue)")]
    public static void AssignPortalBlueToBossSpawner()
    {
        // Try to find a prefab named "Portal blue" in the project
        string[] guids = AssetDatabase.FindAssets("Portal blue t:Prefab");

        if (guids == null || guids.Length == 0)
        {
            EditorUtility.DisplayDialog("Assign Boss VFX", "No prefab named 'Portal blue' found.\nPlease locate the desired Hovl prefab under Assets/Hovl Studio/... and try again.", "OK");
            return;
        }

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

        if (prefab == null)
        {
            EditorUtility.DisplayDialog("Assign Boss VFX", $"Failed to load prefab at {path}", "OK");
            return;
        }

        // Assign to all BossSpawner instances in open scenes
        var spawners = Object.FindObjectsOfType<BossSpawner>();
        int assignedSpawners = 0;
        foreach (var sp in spawners)
        {
            Undo.RecordObject(sp, "Assign spawnEffectPrefab");
            sp.spawnEffectPrefab = prefab;
            sp.spawnEffectDuration = 4f;
            EditorUtility.SetDirty(sp);
            EditorSceneManager.MarkSceneDirty(sp.gameObject.scene);
            assignedSpawners++;
        }

        // Find player(s) (ThirdPersonController) and add/assign BossTeleportOnPlayerSpawn
        var players = Object.FindObjectsOfType<ThirdPersonController>();
        int assignedPlayers = 0;
        foreach (var player in players)
        {
            GameObject go = player.gameObject;
            var comp = go.GetComponent<BossTeleportOnPlayerSpawn>();
            if (comp == null)
            {
                Undo.AddComponent<BossTeleportOnPlayerSpawn>(go);
                comp = go.GetComponent<BossTeleportOnPlayerSpawn>();
            }

            if (comp != null)
            {
                Undo.RecordObject(comp, "Assign hovlEffectPrefab");
                comp.hovlEffectPrefab = prefab;
                comp.effectDuration = 4f;
                comp.teleportOnStart = true;
                EditorUtility.SetDirty(comp);
                EditorSceneManager.MarkSceneDirty(go.scene);
                assignedPlayers++;
            }
        }

        EditorUtility.DisplayDialog("Assign Boss VFX",
            $"Assigned prefab '{prefab.name}' to {assignedSpawners} BossSpawner(s) and {assignedPlayers} player(s).\nPrefab path: {path}", "OK");
    }
}
