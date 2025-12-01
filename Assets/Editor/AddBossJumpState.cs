using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class AddBossJumpState
{
    private const string controllerPath = "Assets/01_Prefabs/BossAnimations/BossGod.controller";
    private static readonly string[] clipCandidates = new[] { "JUMPATTACK", "JumpAttack", "Jump Attack", "Jump_Attack" };

    [MenuItem("Tools/Boss/Add JumpAttack State to BossGod")]
    public static void AddJumpState()
    {
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        if (controller == null)
        {
            Debug.LogError($"AnimatorController not found at '{controllerPath}'.");
            return;
        }

        var layer = controller.layers[0];
        var sm = layer.stateMachine;

        // Check if a state named JUMPATTACK (or any candidate) already exists
        foreach (var s in sm.states)
        {
            var n = s.state.name;
            if (n.Equals("JUMPATTACK", System.StringComparison.OrdinalIgnoreCase) || n.Equals("JumpAttack", System.StringComparison.OrdinalIgnoreCase))
            {
                Debug.Log($"JumpAttack-like state already exists in {controller.name}: '{n}'");
                return;
            }
        }

        // Find an AnimationClip asset in project matching candidates (broader search)
        AnimationClip chosenClip = null;
        var allClipGuids = AssetDatabase.FindAssets("t:AnimationClip");
        for (int i = 0; i < allClipGuids.Length && chosenClip == null; ++i)
        {
            var path = AssetDatabase.GUIDToAssetPath(allClipGuids[i]);
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null) continue;

            string clipName = clip.name ?? "";
            // Exact or contains match for any candidate (case-insensitive)
            foreach (var candidate in clipCandidates)
            {
                if (clipName.Equals(candidate, System.StringComparison.OrdinalIgnoreCase) ||
                    clipName.IndexOf(candidate, System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    chosenClip = clip;
                    Debug.Log($"Found clip '{clip.name}' at '{path}' to use for JumpAttack state.");
                    break;
                }
            }
        }

        if (chosenClip == null)
        {
            // Try explicit FBX path commonly used in this project
            string candidateFbx = "Assets/01_Prefabs/BossAnimations/JUMPATTACK.fbx";
            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(candidateFbx);
            if (asset != null)
            {
                var subAssets = AssetDatabase.LoadAllAssetsAtPath(candidateFbx);
                foreach (var sa in subAssets)
                {
                    if (sa is AnimationClip ac)
                    {
                        chosenClip = ac;
                        Debug.Log($"Found AnimationClip '{ac.name}' inside '{candidateFbx}'. Using it for JumpAttack state.");
                        break;
                    }
                }
            }
        }

        if (chosenClip == null)
        {
            Debug.LogWarning("No AnimationClip found for JumpAttack candidates. Scanned all AnimationClips but none matched. Place or rename the JUMPATTACK clip under Assets and try again.");
            return;
        }

        // Create the state and force its name to 'JUMPATTACK' (so runtime can Play by that name deterministically)
        var stateName = "JUMPATTACK";
        var newState = sm.AddState(stateName, new Vector3(200, 0, 0));
        newState.motion = chosenClip;

        // Ensure controller has a trigger parameter named 'JumpAttack'
        bool hasParam = false;
        foreach (var p in controller.parameters)
        {
            if (p.name == "JumpAttack") { hasParam = true; break; }
        }
        if (!hasParam)
        {
            var param = new AnimatorControllerParameter { name = "JumpAttack", type = AnimatorControllerParameterType.Trigger };
            controller.AddParameter(param);
            Debug.Log("Added 'JumpAttack' trigger parameter to AnimatorController.");
        }

        // Create an AnyState -> Jump transition conditioned on the JumpAttack trigger
        var anyToJump = sm.AddAnyStateTransition(newState);
        anyToJump.hasExitTime = false;
        anyToJump.canTransitionToSelf = false;
        try
        {
            anyToJump.AddCondition(AnimatorConditionMode.If, 0f, "JumpAttack");
        }
        catch
        {
            Debug.LogWarning("Unable to add condition to transition via API. The transition was created without a condition — please set the trigger condition manually in the Animator if needed.");
        }

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();

        Debug.Log($"Added JumpAttack state '{chosenClip.name}' to {controller.name} and AnyState->{chosenClip.name} transition.");
    }
}
