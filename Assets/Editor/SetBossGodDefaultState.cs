using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class SetBossGodDefaultState
{
    [MenuItem("Game Jam/Boss/Set Fall as Default State")]
    public static void SetFallAsDefault()
    {
        string controllerPath = "Assets/01_Prefabs/BossAnimations/BossGod.controller";
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);

        if (controller == null)
        {
            EditorUtility.DisplayDialog("Error", $"Controller not found at {controllerPath}", "OK");
            return;
        }

        var layer = controller.layers[0];
        var sm = layer.stateMachine;

        // Find Fall state
        var fallState = sm.states.FirstOrDefault(s => s.state.name == "Fall").state;

        if (fallState == null)
        {
            EditorUtility.DisplayDialog("Error", "Fall state not found in controller.", "OK");
            return;
        }

        // Set Fall as default state
        sm.defaultState = fallState;

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();

        Debug.Log("<color=green>[SetBossGodDefaultState] ✓ Fall state set as DEFAULT state</color>");
        EditorUtility.DisplayDialog("Success", "Fall is now the default state! Boss will start falling when spawned.", "OK");
    }

    [MenuItem("Game Jam/Boss/Set Idle as Default State")]
    public static void SetIdleAsDefault()
    {
        string controllerPath = "Assets/01_Prefabs/BossAnimations/BossGod.controller";
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);

        if (controller == null)
        {
            EditorUtility.DisplayDialog("Error", $"Controller not found at {controllerPath}", "OK");
            return;
        }

        var layer = controller.layers[0];
        var sm = layer.stateMachine;

        // Find Idle state
        var idleState = sm.states.FirstOrDefault(s => s.state.name == "Idle").state;

        if (idleState == null)
        {
            EditorUtility.DisplayDialog("Error", "Idle state not found in controller.", "OK");
            return;
        }

        // Set Idle as default state
        sm.defaultState = idleState;

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();

        Debug.Log("<color=green>[SetBossGodDefaultState] ✓ Idle state set as DEFAULT state</color>");
        EditorUtility.DisplayDialog("Success", "Idle is now the default state.", "OK");
    }
}
