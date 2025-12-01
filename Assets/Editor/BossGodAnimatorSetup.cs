using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class BossGodAnimatorSetup
{
    private const string controllerPath = "Assets/01_Prefabs/BossAnimations/BossGod.controller";
    private const string clipsFolder = "Assets/01_Prefabs/BossAnimations";

    [MenuItem("Game Jam/Setup/Configure BossGod Animator")]
    public static void ConfigureBossGodAnimator()
    {
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        if (controller == null)
        {
            EditorUtility.DisplayDialog("BossGod Animator Setup", $"Animator Controller not found at {controllerPath}", "OK");
            return;
        }

        // Add parameters we need
        AddParameterIfMissing(controller, "Speed", AnimatorControllerParameterType.Float);
        AddParameterIfMissing(controller, "MotionSpeed", AnimatorControllerParameterType.Float);
        AddParameterIfMissing(controller, "Moving", AnimatorControllerParameterType.Bool);
        AddParameterIfMissing(controller, "Grounded", AnimatorControllerParameterType.Bool);
        AddParameterIfMissing(controller, "Unconscious", AnimatorControllerParameterType.Trigger);
        AddParameterIfMissing(controller, "WakeUp", AnimatorControllerParameterType.Trigger);
        AddParameterIfMissing(controller, "Damage", AnimatorControllerParameterType.Trigger);
        AddParameterIfMissing(controller, "Death", AnimatorControllerParameterType.Trigger);

        // Load clips in the BossAnimations folder
        var clipGuids = AssetDatabase.FindAssets("t:AnimationClip", new[] { clipsFolder });
        var clips = clipGuids.Select(g => AssetDatabase.GUIDToAssetPath(g))
                              .Select(p => AssetDatabase.LoadAssetAtPath<AnimationClip>(p))
                              .Where(c => c != null)
                              .ToList();

        AnimationClip idleClip = FindClipByNameExact(clips, "IDLE") ?? FindClipByNameContains(clips, "Idle");
        AnimationClip walkClip = FindClipByNameExact(clips, "WALK") ?? FindClipByNameContains(clips, "Walk");
        AnimationClip fallClip = FindClipByNameExact(clips, "FALL") ?? FindClipByNameContains(clips, "Fall");
        AnimationClip standClip = FindClipByNameExact(clips, "STAND") ?? FindClipByNameContains(clips, "Stand");
        AnimationClip tposeClip = FindClipByNameExact(clips, "TPOSE") ?? FindClipByNameContains(clips, "TPose");

        // Base layer
        var layer = controller.layers[0];
        var sm = layer.stateMachine;

        var stateIdle = CreateOrReplaceState(sm, "Idle", idleClip, new Vector3(-200, 0, 0));
        var stateWalk = CreateOrReplaceState(sm, "Walk", walkClip, new Vector3(0, 0, 0));
        var stateFall = CreateOrReplaceState(sm, "Fall", fallClip, new Vector3(-100, -150, 0));
        var stateStand = CreateOrReplaceState(sm, "Stand", standClip, new Vector3(-300, 0, 0));
        var stateTpose = CreateOrReplaceState(sm, "TPose", tposeClip, new Vector3(-400, 0, 0));

        // Entry -> Idle
        EnsureTransitionFromEntry(sm, stateIdle);

        // Idle <-> Walk
        EnsureTransition(controller, sm, stateIdle, stateWalk, "Speed", 0.1f, greaterThan: true);
        EnsureTransition(controller, sm, stateWalk, stateIdle, "Speed", 0.1f, greaterThan: false);

        // AnyState -> Fall (trigger)
        EnsureAnyStateTransition(sm, stateFall);

        // Fall -> Idle when grounded (using bool condition)
        EnsureBoolTransition(controller, sm, stateFall, stateIdle, "Grounded", true);

        // Save
        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("BossGod Animator Setup", "BossGod animator configured (parameters and basic states).", "OK");
        Debug.Log("[BossGodAnimatorSetup] Configured BossGod controller and assigned clips where found.");
    }

    private static void AddParameterIfMissing(AnimatorController controller, string name, AnimatorControllerParameterType type)
    {
        if (!controller.parameters.Any(p => p.name == name))
        {
            controller.AddParameter(name, type);
            Debug.Log($"[BossGodAnimatorSetup] Added parameter {name}");
        }
    }

    private static AnimationClip FindClipByNameExact(System.Collections.Generic.List<AnimationClip> clips, string name)
    {
        return clips.FirstOrDefault(c => c.name == name);
    }

    private static AnimationClip FindClipByNameContains(System.Collections.Generic.List<AnimationClip> clips, string key)
    {
        if (clips == null) return null;
        return clips.FirstOrDefault(c => c.name.ToLower().Contains(key.ToLower()));
    }

    private static AnimatorState CreateOrReplaceState(AnimatorStateMachine sm, string stateName, AnimationClip clip, Vector3 pos)
    {
        var existingEntry = sm.states.FirstOrDefault(s => s.state.name == stateName);
        if (existingEntry.state != null)
        {
            if (clip != null) existingEntry.state.motion = clip;
            return existingEntry.state;
        }

        var s = sm.AddState(stateName, pos);
        if (clip != null) s.motion = clip;
        Debug.Log($"[BossGodAnimatorSetup] Created state {stateName} assigned clip {clip?.name}");
        return s;
    }

    private static void EnsureTransitionFromEntry(AnimatorStateMachine sm, AnimatorState toState)
    {
        var existing = sm.entryTransitions.FirstOrDefault(t => t.destinationState == toState);
        if (existing == null)
        {
            sm.AddEntryTransition(toState);
            Debug.Log($"[BossGodAnimatorSetup] Entry -> {toState.name}");
        }
    }

    private static void EnsureTransition(AnimatorController controller, AnimatorStateMachine sm, AnimatorState from, AnimatorState to, string param, float threshold, bool greaterThan)
    {
        var existing = from.transitions.FirstOrDefault(t => t.destinationState == to);
        if (existing != null)
        {
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
        var t = from.AddTransition(to);
        t.hasExitTime = false;
        t.duration = 0.1f;
        var paramInfo = controller.parameters.FirstOrDefault(x => x.name == param);
        if (paramInfo != null && paramInfo.type == AnimatorControllerParameterType.Bool)
        {
            t.AddCondition(greaterThan ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0f, param);
        }
        else
        {
            t.AddCondition(greaterThan ? AnimatorConditionMode.Greater : AnimatorConditionMode.Less, threshold, param);
        }
        Debug.Log($"[BossGodAnimatorSetup] Added transition {from.name} -> {to.name} on {param}");
    }

    private static void EnsureAnyStateTransition(AnimatorStateMachine sm, AnimatorState to)
    {
        var existing = sm.anyStateTransitions.FirstOrDefault(tr => tr.destinationState == to && tr.conditions.Any());
        if (existing != null) return;
        var trAny = sm.AddAnyStateTransition(to);
        trAny.hasExitTime = false;
        trAny.duration = 0f;
        trAny.AddCondition(AnimatorConditionMode.If, 0f, "Unconscious");
        Debug.Log($"[BossGodAnimatorSetup] AnyState -> {to.name} on Unconscious");
    }

    private static void EnsureBoolTransition(AnimatorController controller, AnimatorStateMachine sm, AnimatorState from, AnimatorState to, string boolParam, bool value)
    {
        var existing = from.transitions.FirstOrDefault(t => t.destinationState == to);
        if (existing != null)
        {
            if (!existing.conditions.Any())
            {
                existing.AddCondition(value ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0f, boolParam);
            }
            return;
        }
        var t = from.AddTransition(to);
        t.hasExitTime = false;
        t.duration = 0.1f;
        t.AddCondition(value ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0f, boolParam);
        Debug.Log($"[BossGodAnimatorSetup] Added bool transition {from.name} -> {to.name} on {boolParam}={value}");
    }
}

