using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public class EnsureBossGodTransitions
{
    private const string controllerPath = "Assets/01_Prefabs/BossAnimations/BossGod.controller";

    [MenuItem("Tools/Boss/Ensure BossGod Transitions")]
    public static void EnsureTransitions()
    {
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        if (controller == null)
        {
            Debug.LogError($"No se encontró AnimatorController en '{controllerPath}'. Ajusta la ruta en '{nameof(EnsureBossGodTransitions)}'.");
            return;
        }

        var layer = controller.layers.Length > 0 ? controller.layers[0] : default(AnimatorControllerLayer);
        var sm = layer.stateMachine;
        if (sm == null)
        {
            Debug.LogError("El AnimatorController no contiene StateMachine en la primera capa.");
            return;
        }

        // Buscar estados
        AnimatorState fallState = null;
        AnimatorState idleState = null;
        foreach (var state in sm.states)
        {
            if (state.state == null) continue;
            var name = state.state.name.ToLowerInvariant();
            if (name.Contains("fall") && fallState == null) fallState = state.state;
            if (name.Contains("idle") && idleState == null) idleState = state.state;
        }

        if (fallState == null)
        {
            Debug.LogWarning("No se encontró un estado que contenga 'fall' en su nombre dentro del controlador. Revisa nombres de estados.");
        }

        // 1) Fijar Fall como estado por defecto (Entry → Fall)
        if (fallState != null && sm.defaultState != fallState)
        {
            sm.defaultState = fallState;
            Debug.Log("Fall establecido como estado por defecto (Entry → Fall).");
        }

        // 2) Asegurar AnyState -> Fall transition
        if (fallState != null)
        {
            bool anyToFall = false;
            foreach (var t in sm.anyStateTransitions)
            {
                if (t.destinationState == fallState) { anyToFall = true; break; }
            }
            if (!anyToFall)
            {
                var trans = sm.AddAnyStateTransition(fallState);
                trans.hasExitTime = false;
                trans.canTransitionToSelf = false;
                Debug.Log("Creada transición AnyState -> Fall.");
            }
        }

        // 3) Asegurar Fall -> Idle (si existe Idle)
        if (fallState != null && idleState != null)
        {
            bool fallToIdle = false;
            foreach (var t in fallState.transitions)
            {
                if (t.destinationState == idleState) { fallToIdle = true; break; }
            }
            if (!fallToIdle)
            {
                var t = fallState.AddTransition(idleState);
                t.hasExitTime = true;
                t.exitTime = 1f; // esperar a terminar la animación
                t.conditions = new AnimatorCondition[0];
                Debug.Log("Creada transición Fall -> Idle (con exitTime = 1).");
            }
        }

        // Guardar cambios
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        Debug.Log("EnsureBossGodTransitions: comprobación/creación completada.");
    }
}
