using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

[InitializeOnLoad]
public static class AutoAssignBossClips
{
    static AutoAssignBossClips()
    {
        EditorApplication.delayCall += AssignClipsOnce;
    }

    private static void AssignClipsOnce()
    {
        EditorApplication.delayCall -= AssignClipsOnce;

        // Only run if the marker file doesn't exist
        string markerPath = "Assets/Editor/BossClipsAssigned.marker";
        if (System.IO.File.Exists(markerPath))
        {
            return; // Already assigned
        }

        Debug.Log("[AutoAssignBossClips] Starting automatic assignment of boss animation clips...");

        string[] controllerPaths = new[] {
            "Assets/01_Prefabs/BossAnimations/BossGod.controller",
            "Assets/09_BossAnimations/BossAnimator.controller"
        };

        int totalAssigned = 0;
        foreach (var path in controllerPaths)
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if (controller == null)
            {
                Debug.LogWarning($"[AutoAssignBossClips] Controller not found at {path}");
                continue;
            }

            // Gather clips from BossAnimations folder
            string searchFolder = "Assets/01_Prefabs/BossAnimations";
            var guids = AssetDatabase.FindAssets("t:AnimationClip", new[] { searchFolder });
            var clips = guids.Select(g => AssetDatabase.GUIDToAssetPath(g))
                             .Select(p => AssetDatabase.LoadAssetAtPath<AnimationClip>(p))
                             .Where(c => c != null)
                             .ToList();

            if (clips.Count == 0)
            {
                guids = AssetDatabase.FindAssets("t:AnimationClip");
                clips = guids.Select(g => AssetDatabase.GUIDToAssetPath(g))
                             .Select(p => AssetDatabase.LoadAssetAtPath<AnimationClip>(p))
                             .Where(c => c != null)
                             .ToList();
            }

            System.Func<string, AnimationClip> findClip = (name) =>
            {
                var exact = clips.FirstOrDefault(c => c.name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
                if (exact != null) return exact;
                return clips.FirstOrDefault(c => c.name.IndexOf(name, System.StringComparison.OrdinalIgnoreCase) >= 0);
            };

            var desired = new System.Collections.Generic.Dictionary<string, string[]>()
            {
                { "Fall", new[]{ "FALL", "Fall" } },
                { "Idle", new[]{ "IDLE", "Idle" } },
                { "Walk", new[]{ "WALK", "Walk" } },
                { "Slash", new[]{ "JUMPATTACK", "Slash", "JumpAttack", "Jump Attack" } },
                { "Stand", new[]{ "STAND", "Stand" } },
                { "TPose", new[]{ "TPOSE", "TPose" } }
            };

            var layer = controller.layers[0];
            var sm = layer.stateMachine;
            int assigned = 0;

            foreach (var kv in desired)
            {
                var stateName = kv.Key;
                var candidates = kv.Value;
                var child = sm.states.FirstOrDefault(s => s.state.name == stateName);
                if (child.state == null)
                {
                    continue;
                }

                AnimationClip clip = null;
                foreach (var cn in candidates)
                {
                    clip = findClip(cn);
                    if (clip != null) break;
                }

                if (clip != null)
                {
                    child.state.motion = clip;
                    assigned++;
                    Debug.Log($"[AutoAssignBossClips] Assigned '{clip.name}' -> {stateName} on {path}");
                }
            }

            if (assigned > 0)
            {
                EditorUtility.SetDirty(controller);
                totalAssigned += assigned;
            }
        }

        if (totalAssigned > 0)
        {
            AssetDatabase.SaveAssets();
            Debug.Log($"<color=green>[AutoAssignBossClips] ✓ Successfully assigned {totalAssigned} animation clips to boss controllers!</color>");

            // Create marker file so we don't run this again
            System.IO.File.WriteAllText(markerPath, "Clips assigned on " + System.DateTime.Now.ToString());
            AssetDatabase.Refresh();
        }
        else
        {
            Debug.LogWarning("[AutoAssignBossClips] No clips were assigned. Check if states exist in the controllers.");
        }
    }

    [MenuItem("Game Jam/Boss/Reset Clip Assignment (Force Reassign)")]
    public static void ResetAssignment()
    {
        string markerPath = "Assets/Editor/BossClipsAssigned.marker";
        if (System.IO.File.Exists(markerPath))
        {
            System.IO.File.Delete(markerPath);
            AssetDatabase.Refresh();
            Debug.Log("[AutoAssignBossClips] Marker file deleted. Restart Unity to reassign clips.");
            EditorUtility.DisplayDialog("Reset Complete", "Marker file deleted. Restart Unity to reassign clips automatically.", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Reset", "No marker file found. Clips will be assigned on next Unity restart.", "OK");
        }
    }
}
