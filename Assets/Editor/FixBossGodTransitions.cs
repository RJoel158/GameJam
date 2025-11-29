using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class FixBossGodTransitions
{
    [MenuItem("Game Jam/Boss/Fix BossGod Grounded Transition")]
    public static void FixGroundedTransition()
    {
        string controllerPath = "Assets/01_Prefabs/BossAnimations/BossGod.controller";
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        
        if (controller == null)
        {
            EditorUtility.DisplayDialog("Fix Transitions", $"Controller not found at {controllerPath}", "OK");
            return;
        }

        var layer = controller.layers[0];
        var sm = layer.stateMachine;

        // Find Fall and Idle states
        var fallState = sm.states.FirstOrDefault(s => s.state.name == "Fall").state;
        var idleState = sm.states.FirstOrDefault(s => s.state.name == "Idle").state;

        if (fallState == null || idleState == null)
        {
            EditorUtility.DisplayDialog("Fix Transitions", "Fall or Idle state not found in controller.", "OK");
            return;
        }

        // Remove ALL existing transitions from Fall to Idle
        var transitionsToRemove = fallState.transitions.Where(t => t.destinationState == idleState).ToArray();
        foreach (var trans in transitionsToRemove)
        {
            fallState.RemoveTransition(trans);
            Debug.Log("[FixBossGodTransitions] Removed old Fall->Idle transition");
        }

        // Create new transition with correct bool condition
        var newTransition = fallState.AddTransition(idleState);
        newTransition.hasExitTime = false;
        newTransition.duration = 0.1f;
        newTransition.AddCondition(AnimatorConditionMode.If, 0f, "Grounded");
        
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        
        Debug.Log("<color=green>[FixBossGodTransitions] ✓ Fixed Fall->Idle transition to use bool condition 'Grounded == true'</color>");
        EditorUtility.DisplayDialog("Fix Complete", "Fall->Idle transition fixed! The 'Grounded' parameter now uses a bool condition (If) instead of numeric comparison.", "OK");
    }
}
