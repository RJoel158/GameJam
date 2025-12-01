using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class AnimatorInspectorTools
{
    [MenuItem("Game Jam/Inspect/Inspect Selected Animator (States & Clips)")]
    public static void InspectSelectedAnimator()
    {
        var go = Selection.activeGameObject;
        if (go == null)
        {
            EditorUtility.DisplayDialog("Inspect Animator", "No GameObject selected.", "OK");
            return;
        }

        var animator = go.GetComponent<Animator>() ?? go.GetComponentInChildren<Animator>(true);
        if (animator == null)
        {
            EditorUtility.DisplayDialog("Inspect Animator", "No Animator found on selected GameObject or its children.", "OK");
            return;
        }

        var rac = animator.runtimeAnimatorController as AnimatorController;
        if (rac == null)
        {
            EditorUtility.DisplayDialog("Inspect Animator", "Selected Animator does not have an AnimatorController (or it's not an AnimatorController asset).", "OK");
            return;
        }

        // Build report
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine($"Animator on: {go.name}");
        sb.AppendLine($"Controller: {AssetDatabase.GetAssetPath(rac)}");
        sb.AppendLine("Parameters:");
        foreach (var p in rac.parameters)
        {
            sb.AppendLine($" - {p.name} ({p.type})");
        }

        sb.AppendLine("\nAnimation Clips in Controller (runtime clips):");
        var clips = rac.animationClips;
        foreach (var c in clips)
        {
            sb.AppendLine($" - {c.name}");
        }

        sb.AppendLine("\nLayers and States:");
        for (int i = 0; i < rac.layers.Length; i++)
        {
            var layer = rac.layers[i];
            sb.AppendLine($" Layer {i}: {layer.name}");
            var sm = layer.stateMachine;
            foreach (var child in sm.states)
            {
                var s = child.state;
                sb.AppendLine($"  - State: {s.name}  | Motion: {(s.motion != null ? s.motion.name : "(none)")}");
            }
        }

        // Show results in dialog and console (console for long output)
        EditorUtility.DisplayDialog("Animator Inspector", "Report printed to Console (see Editor Console).", "OK");
        Debug.Log(sb.ToString());
    }

    [MenuItem("Game Jam/Inspect/Assign 'FALL' Clip To Selected Animator's Fall State")]
    public static void AssignFallClipToSelected()
    {
        var go = Selection.activeGameObject;
        if (go == null)
        {
            EditorUtility.DisplayDialog("Assign FALL", "No GameObject selected.", "OK");
            return;
        }

        var animator = go.GetComponent<Animator>() ?? go.GetComponentInChildren<Animator>(true);
        if (animator == null)
        {
            EditorUtility.DisplayDialog("Assign FALL", "No Animator found on selected GameObject.", "OK");
            return;
        }

        var rac = animator.runtimeAnimatorController as AnimatorController;
        if (rac == null)
        {
            EditorUtility.DisplayDialog("Assign FALL", "Animator has no AnimatorController.", "OK");
            return;
        }

        // Search for a clip named FALL (case-insensitive) in the BossAnimations folder first
        string[] guids = AssetDatabase.FindAssets("t:AnimationClip", new[] { "Assets/01_Prefabs/BossAnimations" });
        AnimationClip found = null;
        foreach (var g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip != null && clip.name.ToLower() == "fall")
            {
                found = clip; break;
            }
        }

        if (found == null)
        {
            // fallback: search whole project
            guids = AssetDatabase.FindAssets("t:AnimationClip");
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                if (clip != null && clip.name.ToLower() == "fall")
                {
                    found = clip; break;
                }
            }
        }

        if (found == null)
        {
            EditorUtility.DisplayDialog("Assign FALL", "Could not find an AnimationClip named 'Fall' in the project.", "OK");
            return;
        }

        // Assign to 'Fall' state in base layer if present
        var layer = rac.layers[0];
        var sm = layer.stateMachine;
        var child = sm.states.FirstOrDefault(s => s.state.name == "Fall");
        if (child.state == null)
        {
            EditorUtility.DisplayDialog("Assign FALL", "No 'Fall' state found in the controller's base layer.", "OK");
            return;
        }

        child.state.motion = found;
        EditorUtility.SetDirty(rac);
        AssetDatabase.SaveAssets();
        Debug.Log($"[AnimatorInspectorTools] Assigned clip '{found.name}' to state 'Fall' in controller {AssetDatabase.GetAssetPath(rac)}");
        EditorUtility.DisplayDialog("Assign FALL", $"Assigned clip '{found.name}' to 'Fall' state.", "OK");
    }

    [MenuItem("Game Jam/Inspect/Assign All Boss Clips To Selected Animator")]
    public static void AssignAllBossClipsToSelected()
    {
        var go = Selection.activeGameObject;
        if (go == null)
        {
            EditorUtility.DisplayDialog("Assign All", "No GameObject selected.", "OK");
            return;
        }

        var animator = go.GetComponent<Animator>() ?? go.GetComponentInChildren<Animator>(true);
        if (animator == null)
        {
            EditorUtility.DisplayDialog("Assign All", "No Animator found on selected GameObject.", "OK");
            return;
        }

        var rac = animator.runtimeAnimatorController as AnimatorController;
        if (rac == null)
        {
            EditorUtility.DisplayDialog("Assign All", "Animator has no AnimatorController.", "OK");
            return;
        }

        // Find clips in BossAnimations folder first
        string searchFolder = "Assets/01_Prefabs/BossAnimations";
        var guids = AssetDatabase.FindAssets("t:AnimationClip", new[] { searchFolder });
        var clips = guids.Select(g => AssetDatabase.GUIDToAssetPath(g))
                         .Select(p => AssetDatabase.LoadAssetAtPath<AnimationClip>(p))
                         .Where(c => c != null)
                         .ToList();

        // If none found there, fallback to any animation clips in project
        if (clips.Count == 0)
        {
            guids = AssetDatabase.FindAssets("t:AnimationClip");
            clips = guids.Select(g => AssetDatabase.GUIDToAssetPath(g))
                         .Select(p => AssetDatabase.LoadAssetAtPath<AnimationClip>(p))
                         .Where(c => c != null)
                         .ToList();
        }

        if (clips.Count == 0)
        {
            EditorUtility.DisplayDialog("Assign All", "No AnimationClips found in project.", "OK");
            return;
        }

        // Helper to find clip by exact name or by contains (case-insensitive)
        System.Func<string, AnimationClip> findClip = (name) =>
        {
            var exact = clips.FirstOrDefault(c => c.name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
            if (exact != null) return exact;
            // fallback to contains
            return clips.FirstOrDefault(c => c.name.IndexOf(name, System.StringComparison.OrdinalIgnoreCase) >= 0);
        };

        var map = new System.Collections.Generic.Dictionary<string, string[]>()
        {
            { "Fall", new[]{ "FALL", "Fall" } },
            { "Idle", new[]{ "IDLE", "Idle" } },
            { "Walk", new[]{ "WALK", "Walk" } },
            { "Slash", new[]{ "JUMPATTACK", "Slash", "JumpAttack", "Jump Attack" } },
            { "JumpAttack", new[]{ "JUMPATTACK", "JumpAttack", "Jump Attack" } },
            { "Stand", new[]{ "STAND", "Stand" } },
            { "TPose", new[]{ "TPOSE", "TPose" } }
        };

        var layer = rac.layers[0];
        var sm = layer.stateMachine;

        int assigned = 0;
        foreach (var kv in map)
        {
            string stateName = kv.Key;
            string[] candidateNames = kv.Value;

            var child = sm.states.FirstOrDefault(s => s.state.name == stateName);
            if (child.state == null)
            {
                Debug.LogWarning($"[AnimatorInspectorTools] State '{stateName}' not found in controller {AssetDatabase.GetAssetPath(rac)}");
                continue;
            }

            AnimationClip clip = null;
            foreach (var cn in candidateNames)
            {
                clip = findClip(cn);
                if (clip != null) break;
            }

            if (clip == null)
            {
                Debug.LogWarning($"[AnimatorInspectorTools] None of the candidate clips [{string.Join(",", candidateNames)}] were found for state '{stateName}'.");
                continue;
            }

            child.state.motion = clip;
            assigned++;
            Debug.Log($"[AnimatorInspectorTools] Assigned clip '{clip.name}' to state '{stateName}'");
        }

        if (assigned > 0)
        {
            EditorUtility.SetDirty(rac);
            AssetDatabase.SaveAssets();
        }

        EditorUtility.DisplayDialog("Assign All", $"Assigned {assigned} clips to states (check Console for details).", "OK");
    }

    [MenuItem("Game Jam/Inspect/Assign Boss Clips To Known Controllers (Project)")]
    public static void AssignBossClipsToKnownControllers()
    {
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
                Debug.LogWarning($"[AnimatorInspectorTools] Controller not found at {path}");
                continue;
            }

            // gather clips from BossAnimations folder
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
                    Debug.LogWarning($"[AnimatorInspectorTools] State '{stateName}' not found in controller {path}");
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
                    Debug.Log($"[AnimatorInspectorTools] Assigned '{clip.name}' -> {stateName} on {path}");
                }
                else
                {
                    Debug.LogWarning($"[AnimatorInspectorTools] No clip found for state {stateName} in controller {path}");
                }
            }

            if (assigned > 0)
            {
                EditorUtility.SetDirty(controller);
                AssetDatabase.SaveAssets();
            }

            totalAssigned += assigned;
        }

        EditorUtility.DisplayDialog("Assign Boss Clips", $"Assigned a total of {totalAssigned} clips across known controllers. See Console for details.", "OK");
    }

    [MenuItem("Game Jam/Inspect/Inspect Animator Controller By Path")]
    public static void InspectAnimatorControllerByPath()
    {
        string path = EditorUtility.OpenFilePanel("Select AnimatorController", Application.dataPath, "controller");
        if (string.IsNullOrEmpty(path)) return;

        // Convert absolute path to relative project path
        if (path.StartsWith(Application.dataPath))
        {
            path = "Assets" + path.Substring(Application.dataPath.Length);
        }
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
        if (controller == null)
        {
            EditorUtility.DisplayDialog("Animator Inspector", "Could not load AnimatorController at path.", "OK");
            return;
        }

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine($"Controller: {path}");
        sb.AppendLine("Parameters:");
        foreach (var p in controller.parameters)
        {
            sb.AppendLine($" - {p.name} ({p.type})");
        }

        sb.AppendLine("\nClips in controller:");
        var clips = controller.animationClips;
        foreach (var c in clips)
        {
            sb.AppendLine($" - {c.name}");
        }

        sb.AppendLine("\nStates:");
        for (int i = 0; i < controller.layers.Length; i++)
        {
            var layer = controller.layers[i];
            sb.AppendLine($" Layer {i}: {layer.name}");
            var sm = layer.stateMachine;
            foreach (var child in sm.states)
            {
                var s = child.state;
                sb.AppendLine($"  - State: {s.name}  | Motion: {(s.motion != null ? s.motion.name : "(none)")}");
                if (s.transitions != null && s.transitions.Length > 0)
                {
                    foreach (var t in s.transitions)
                    {
                        if (t.conditions != null && t.conditions.Length > 0)
                        {
                            foreach (var cnd in t.conditions)
                            {
                                sb.AppendLine($"     - Transition -> {t.destinationState?.name} on {cnd.parameter} mode:{cnd.mode} thresh:{cnd.threshold}");
                            }
                        }
                    }
                }
            }
        }

        EditorUtility.DisplayDialog("Animator Inspector", "Report printed to Console (see Editor Console).", "OK");
        Debug.Log(sb.ToString());
    }
}
