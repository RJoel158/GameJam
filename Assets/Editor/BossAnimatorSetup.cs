using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class BossAnimatorSetup
{
    private const string controllerPath = "Assets/09_BossAnimations/BossAnimator.controller";
    private const string clipsFolder = "Assets/09_BossAnimations";

    [MenuItem("Game Jam/Setup/Ensure Boss Animator States")]
    public static void EnsureBossAnimatorStates()
    {
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        if (controller == null)
        {
            EditorUtility.DisplayDialog("Boss Animator Setup", $"Animator Controller not found at {controllerPath}", "OK");
            return;
        }

        // Ensure parameters
        AddParameterIfMissing(controller, "Speed", AnimatorControllerParameterType.Float);
        AddParameterIfMissing(controller, "MotionSpeed", AnimatorControllerParameterType.Float);
        AddParameterIfMissing(controller, "Moving", AnimatorControllerParameterType.Bool);
        AddParameterIfMissing(controller, "Grounded", AnimatorControllerParameterType.Bool);
        AddParameterIfMissing(controller, "Unconscious", AnimatorControllerParameterType.Trigger);
        AddParameterIfMissing(controller, "WakeUp", AnimatorControllerParameterType.Trigger);
        AddParameterIfMissing(controller, "Damage", AnimatorControllerParameterType.Trigger);
        AddParameterIfMissing(controller, "Death", AnimatorControllerParameterType.Trigger);

        // Load available clips in the folder
        var clipGuids = AssetDatabase.FindAssets("t:AnimationClip", new[] { clipsFolder });
        var clips = clipGuids.Select(g => AssetDatabase.GUIDToAssetPath(g))
                              .Select(p => AssetDatabase.LoadAssetAtPath<AnimationClip>(p))
                              .Where(c => c != null)
                              .ToList();

        AnimationClip idleClip = FindClipByNameContains(clips, "Idle");
        AnimationClip walkClip = FindClipByNameContains(clips, "Walk");
        AnimationClip slashClip = FindClipByNameContains(clips, "Slash");
        AnimationClip deathClip = FindClipByNameContains(clips, "Death");
        AnimationClip fallClip = FindClipByNameContains(clips, "Fall") ?? FindClipByNameContains(clips, "FreeFall");

        // If fall clip not found, create an empty placeholder clip
        if (fallClip == null)
        {
            string fallPath = clipsFolder + "/Boss_Fall.anim";
            AnimationClip newClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(fallPath);
            if (newClip == null)
            {
                newClip = new AnimationClip();
                AssetDatabase.CreateAsset(newClip, fallPath);
                AssetDatabase.SaveAssets();
                Debug.Log($"[BossAnimatorSetup] Created placeholder Fall clip at {fallPath}");
            }
            fallClip = newClip;
        }

        // Get base layer state machine
        var layer = controller.layers[0];
        var sm = layer.stateMachine;

        // Create or get states
        var stateIdle = CreateOrReplaceState(sm, "Idle", idleClip, new Vector3(-200, 0, 0));
        var stateWalk = CreateOrReplaceState(sm, "Walk", walkClip, new Vector3(0, 0, 0));
        var stateFall = CreateOrReplaceState(sm, "Fall", fallClip, new Vector3(-100, -150, 0));
        var stateSlash = CreateOrReplaceState(sm, "Slash", slashClip, new Vector3(200, 0, 0));
        var stateDeath = CreateOrReplaceState(sm, "Death", deathClip, new Vector3(400, 0, 0));

        // Entry -> Idle
        EnsureTransitionFromEntry(sm, stateIdle);

        // Idle <-> Walk based on Speed
        EnsureTransition(controller, sm, stateIdle, stateWalk, "Speed", 0.1f, greaterThan: true);
        EnsureTransition(controller, sm, stateWalk, stateIdle, "Speed", 0.1f, greaterThan: false);

        // AnyState -> Death (trigger)
        EnsureAnyStateTransition(sm, stateDeath, "Death", AnimatorConditionMode.If);

        // AnyState -> Fall (trigger Unconscious)
        EnsureAnyStateTransition(sm, stateFall, "Unconscious", AnimatorConditionMode.If);

        // Fall -> Idle when Grounded == true
        EnsureTransition(controller, sm, stateFall, stateIdle, "Grounded", 1f, greaterThan: true);

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Boss Animator Setup", "Animator states ensured and basic transitions created.", "OK");
        Debug.Log("[BossAnimatorSetup] Completed ensuring states and transitions.");
    }

    private static void AddParameterIfMissing(AnimatorController controller, string name, AnimatorControllerParameterType type)
    {
        if (!controller.parameters.Any(p => p.name == name))
        {
            controller.AddParameter(name, type);
            Debug.Log($"[BossAnimatorSetup] Added parameter {name} of type {type}");
        }
    }

    private static AnimationClip FindClipByNameContains(System.Collections.Generic.List<AnimationClip> clips, string key)
    {
        if (clips == null) return null;
        return clips.FirstOrDefault(c => c.name.ToLower().Contains(key.ToLower()));
    }

    private static AnimatorState CreateOrReplaceState(AnimatorStateMachine sm, string stateName, AnimationClip clip, Vector3 pos)
    {
        // Try find existing state
        var existing = sm.states.FirstOrDefault(s => s.state.name == stateName).state;
        if (existing != null)
        {
            if (clip != null) existing.motion = clip;
            return existing;
        }

        var newState = sm.AddState(stateName, pos);
        if (clip != null) newState.motion = clip;
        Debug.Log($"[BossAnimatorSetup] Created state {stateName} and assigned clip {clip?.name}");
        return newState;
    }

    private static void EnsureTransitionFromEntry(AnimatorStateMachine sm, AnimatorState toState)
    {
        // Remove existing direct entry transitions to avoid duplicates
        var entryTransitions = sm.entryTransitions.Where(t => t.destinationState == toState).ToArray();
        if (entryTransitions.Length == 0)
        {
            sm.AddEntryTransition(toState);
            Debug.Log($"[BossAnimatorSetup] Entry -> {toState.name} transition added");
        }
    }

    private static void EnsureTransition(AnimatorController controller, AnimatorStateMachine sm, AnimatorState from, AnimatorState to, string param, float threshold, bool greaterThan)
    {
        // Create transition
        var existing = from.transitions.FirstOrDefault(t => t.destinationState == to);
        if (existing != null)
        {
            // Ensure condition exists
            if (!existing.conditions.Any())
            {
                var p = controller.parameters.FirstOrDefault(x => x.name == param);
                if (p != null && p.type == AnimatorControllerParameterType.Bool)
                {
                    existing.AddCondition(greaterThan ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0f, param);
                }
                else
                {
                    existing.AddCondition(greaterThan ? AnimatorConditionMode.Greater : AnimatorConditionMode.Less, threshold, param);
                }
            }
            return;
        }

        var trans = from.AddTransition(to);
        trans.hasExitTime = false;
        trans.duration = 0.1f;
        var paramInfo = controller.parameters.FirstOrDefault(x => x.name == param);
        if (paramInfo != null && paramInfo.type == AnimatorControllerParameterType.Bool)
        {
            trans.AddCondition(greaterThan ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0f, param);
        }
        else
        {
            trans.AddCondition(greaterThan ? AnimatorConditionMode.Greater : AnimatorConditionMode.Less, threshold, param);
        }
        Debug.Log($"[BossAnimatorSetup] Transition {from.name} -> {to.name} on {param} {(greaterThan ? ">" : "<=")}{threshold}");
    }

    private static void EnsureAnyStateTransition(AnimatorStateMachine sm, AnimatorState to, string triggerName, AnimatorConditionMode mode)
    {
        // Check existing anyState transitions
        var anyTrans = sm.anyStateTransitions.FirstOrDefault(t => t.destinationState == to && t.conditions.Any(c => c.parameter == triggerName));
        if (anyTrans != null) return;

        var trans = sm.AddAnyStateTransition(to);
        trans.hasExitTime = false;
        trans.duration = 0f;
        trans.AddCondition(mode, 0f, triggerName);
        Debug.Log($"[BossAnimatorSetup] AnyState -> {to.name} on trigger {triggerName}");
    }
}
