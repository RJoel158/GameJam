using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class DiagnoseBossAnimator
{
    [MenuItem("Game Jam/Boss/Diagnose BossGod Animator")]
    public static void DiagnoseAnimator()
    {
        string controllerPath = "Assets/01_Prefabs/BossAnimations/BossGod.controller";
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        
        if (controller == null)
        {
            Debug.LogError($"Controller not found at {controllerPath}");
            return;
        }

        Debug.Log("<color=cyan>========== BOSS GOD ANIMATOR DIAGNOSIS ==========</color>");
        
        // Check Parameters
        Debug.Log("\n<color=yellow>PARAMETERS:</color>");
        foreach (var param in controller.parameters)
        {
            Debug.Log($"  • {param.name} ({param.type})");
        }
        
        // Check States and their Motion clips
        var layer = controller.layers[0];
        var sm = layer.stateMachine;
        
        Debug.Log("\n<color=yellow>STATES & MOTION CLIPS:</color>");
        foreach (var childState in sm.states)
        {
            var state = childState.state;
            string motionName = state.motion != null ? state.motion.name : "<NONE>";
            string motionType = state.motion != null ? state.motion.GetType().Name : "N/A";
            
            Debug.Log($"  • State: <b>{state.name}</b>");
            Debug.Log($"    - Motion: {motionName} ({motionType})");
            Debug.Log($"    - Speed: {state.speed}");
            Debug.Log($"    - Transitions: {state.transitions.Length}");
            
            // Check transitions
            foreach (var trans in state.transitions)
            {
                string destName = trans.destinationState != null ? trans.destinationState.name : "<EXIT>";
                Debug.Log($"      → {destName}");
                Debug.Log($"        - HasExitTime: {trans.hasExitTime}");
                Debug.Log($"        - Duration: {trans.duration}");
                Debug.Log($"        - Conditions: {trans.conditions.Length}");
                
                foreach (var cond in trans.conditions)
                {
                    Debug.Log($"          * {cond.parameter} {cond.mode} {cond.threshold}");
                }
            }
        }
        
        // Check if FALL clip exists
        Debug.Log("\n<color=yellow>CHECKING ANIMATION CLIPS:</color>");
        string[] clipPaths = {
            "Assets/01_Prefabs/BossAnimations/FALL.fbx",
            "Assets/01_Prefabs/BossAnimations/IDLE.fbx",
            "Assets/01_Prefabs/BossAnimations/WALK.fbx",
            "Assets/01_Prefabs/BossAnimations/JUMPATTACK.fbx"
        };
        
        foreach (var path in clipPaths)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip != null)
            {
                Debug.Log($"  ✓ {clip.name} - Length: {clip.length}s");
            }
            else
            {
                Debug.LogWarning($"  ✗ Clip not found at {path}");
            }
        }
        
        // Check default state
        Debug.Log($"\n<color=yellow>DEFAULT STATE:</color> {sm.defaultState.name}");
        
        Debug.Log("<color=cyan>========== END DIAGNOSIS ==========</color>");
        
        EditorUtility.DisplayDialog("Diagnosis Complete", "Check the Console for detailed animator information.", "OK");
    }
    
    [MenuItem("Game Jam/Boss/Show BossGod Runtime State")]
    public static void ShowRuntimeState()
    {
        var boss = GameObject.Find("FinalBoss");
        if (boss == null)
        {
            boss = GameObject.Find("BossGod");
        }
        
        if (boss == null)
        {
            EditorUtility.DisplayDialog("Not Found", "Boss GameObject not found in scene. Make sure it's named 'FinalBoss' or 'BossGod'.", "OK");
            return;
        }
        
        var animator = boss.GetComponent<Animator>();
        if (animator == null)
        {
            EditorUtility.DisplayDialog("Not Found", "Boss doesn't have an Animator component.", "OK");
            return;
        }
        
        Debug.Log("<color=cyan>========== BOSS RUNTIME STATE ==========</color>");
        
        // Show current state
        var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        Debug.Log($"Current State Hash: {stateInfo.shortNameHash}");
        Debug.Log($"Current State Name: {GetStateName(animator, 0)}");
        Debug.Log($"Normalized Time: {stateInfo.normalizedTime}");
        
        // Show parameters
        Debug.Log("\n<color=yellow>PARAMETER VALUES:</color>");
        Debug.Log($"  Speed: {animator.GetFloat("Speed")}");
        Debug.Log($"  MotionSpeed: {animator.GetFloat("MotionSpeed")}");
        Debug.Log($"  Moving: {animator.GetBool("Moving")}");
        Debug.Log($"  Grounded: {animator.GetBool("Grounded")}");
        
        Debug.Log("<color=cyan>========== END RUNTIME STATE ==========</color>");
    }
    
    private static string GetStateName(Animator animator, int layerIndex)
    {
        var controller = animator.runtimeAnimatorController as AnimatorController;
        if (controller != null)
        {
            var stateInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);
            var states = controller.layers[layerIndex].stateMachine.states;
            
            foreach (var state in states)
            {
                if (Animator.StringToHash(state.state.name) == stateInfo.shortNameHash)
                {
                    return state.state.name;
                }
            }
        }
        
        return "Unknown";
    }
}
