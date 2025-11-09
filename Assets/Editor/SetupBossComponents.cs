using UnityEngine;
using UnityEditor;

/// <summary>
/// Wizard para configurar automáticamente todos los componentes necesarios en el boss
/// </summary>
public class SetupBossComponents : EditorWindow
{
    private GameObject bossObject;

    [MenuItem("Game Jam/Setup/Boss Components (Health & Energy)")]
    static void OpenWindow()
    {
        SetupBossComponents window = GetWindow<SetupBossComponents>("Boss Components Setup");
        window.minSize = new Vector2(400, 350);
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Boss Components Setup", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "Este wizard configurará automáticamente el boss con:\n\n" +
            "• BossController (health & energy system)\n" +
            "• Rigidbody (para caídas cuando está inconsciente)\n" +
            "• Collider (si no tiene)\n" +
            "• Configuración correcta de God.cs\n\n" +
            "También verificará que el boss tenga los componentes necesarios.",
            MessageType.Info
        );

        EditorGUILayout.Space();

        bossObject = (GameObject)EditorGUILayout.ObjectField(
            "Boss GameObject",
            bossObject,
            typeof(GameObject),
            true
        );

        EditorGUILayout.Space();

        if (bossObject == null)
        {
            EditorGUILayout.HelpBox(
                "Selecciona el GameObject del boss (prefab o instancia en la escena).",
                MessageType.Warning
            );
        }
        else
        {
            ShowComponentStatus();
        }

        EditorGUILayout.Space();

        GUI.enabled = bossObject != null;
        if (GUILayout.Button("Setup Boss Components", GUILayout.Height(40)))
        {
            SetupComponents();
        }
        GUI.enabled = true;
    }

    void ShowComponentStatus()
    {
        EditorGUILayout.LabelField("Current Components:", EditorStyles.boldLabel);

        CheckComponent<God>("God (Shooting AI)", true);
        CheckComponent<BossController>("BossController", false);
        CheckComponent<Rigidbody>("Rigidbody", false);
        CheckComponent<Collider>("Collider", false);
        CheckComponent<UnityEngine.AI.NavMeshAgent>("NavMeshAgent", true);
        CheckComponent<Animator>("Animator", true);
    }

    void CheckComponent<T>(string name, bool shouldExist) where T : Component
    {
        T component = bossObject.GetComponent<T>();
        bool exists = component != null;

        GUIStyle style = new GUIStyle(EditorStyles.label);
        
        if (exists)
        {
            style.normal.textColor = Color.green;
            EditorGUILayout.LabelField($"✓ {name}", style);
        }
        else if (shouldExist)
        {
            style.normal.textColor = Color.red;
            EditorGUILayout.LabelField($"✗ {name} (REQUIRED!)", style);
        }
        else
        {
            style.normal.textColor = Color.yellow;
            EditorGUILayout.LabelField($"○ {name} (will be added)", style);
        }
    }

    void SetupComponents()
    {
        Debug.Log($"<color=cyan>[SetupBossComponents] Setting up components for: {bossObject.name}</color>");

        // 1. Verificar componentes requeridos
        if (!VerifyRequiredComponents())
        {
            return;
        }

        // 2. Añadir/configurar BossController
        BossController bossController = SetupBossController();

        // 3. Añadir/configurar Rigidbody
        Rigidbody rb = SetupRigidbody();

        // 4. Verificar Collider
        SetupCollider();

        // 5. Conectar God con BossController
        SetupGodScript(bossController);

        Debug.Log("<color=green>[SetupBossComponents] ✓ Boss setup complete!</color>");
        Debug.Log("<color=yellow>[SetupBossComponents] Next steps:</color>");
        Debug.Log("  1. Create UI using 'Game Jam > Setup > Boss UI'");
        Debug.Log("  2. Tag the ground with 'Ground'");
        Debug.Log("  3. Test in Play mode!");

        EditorUtility.SetDirty(bossObject);
        Selection.activeGameObject = bossObject;
    }

    bool VerifyRequiredComponents()
    {
        bool valid = true;

        if (bossObject.GetComponent<God>() == null)
        {
            Debug.LogError("[SetupBossComponents] Boss must have God component! Aborting.");
            EditorUtility.DisplayDialog("Error", "The boss must have a God component (shooting AI)!", "OK");
            valid = false;
        }

        if (bossObject.GetComponent<UnityEngine.AI.NavMeshAgent>() == null)
        {
            Debug.LogError("[SetupBossComponents] Boss must have NavMeshAgent! Aborting.");
            EditorUtility.DisplayDialog("Error", "The boss must have a NavMeshAgent component!", "OK");
            valid = false;
        }

        if (bossObject.GetComponent<Animator>() == null)
        {
            Debug.LogWarning("[SetupBossComponents] Boss doesn't have Animator. Animations won't work.");
        }

        return valid;
    }

    BossController SetupBossController()
    {
        BossController controller = bossObject.GetComponent<BossController>();
        
        if (controller == null)
        {
            controller = bossObject.AddComponent<BossController>();
            Debug.Log("<color=green>[SetupBossComponents] Added BossController</color>");
        }
        else
        {
            Debug.Log("[SetupBossComponents] BossController already exists");
        }

        // Configurar valores por defecto
        controller.maxHealth = 1000;
        controller.maxEnergy = 100f;
        controller.energyCostPerShot = 10f;
        controller.energyRegenRate = 5f;
        controller.unconsciousDuration = 5f;

        Debug.Log("[SetupBossComponents] Configured BossController with default values");

        return controller;
    }

    Rigidbody SetupRigidbody()
    {
        Rigidbody rb = bossObject.GetComponent<Rigidbody>();
        
        if (rb == null)
        {
            rb = bossObject.AddComponent<Rigidbody>();
            Debug.Log("<color=green>[SetupBossComponents] Added Rigidbody</color>");
        }
        else
        {
            Debug.Log("[SetupBossComponents] Rigidbody already exists");
        }

        // Configurar para el sistema de unconscious state
        rb.isKinematic = true;  // El boss no debe caer normalmente
        rb.useGravity = true;   // Pero debe caer cuando está inconsciente
        rb.mass = 1f;
        rb.linearDamping = 0f;
        rb.angularDamping = 0.05f;

        Debug.Log("[SetupBossComponents] Configured Rigidbody (isKinematic=true, useGravity=true)");

        return rb;
    }

    void SetupCollider()
    {
        Collider col = bossObject.GetComponent<Collider>();
        
        if (col == null)
        {
            // Añadir CapsuleCollider por defecto
            CapsuleCollider capsule = bossObject.AddComponent<CapsuleCollider>();
            capsule.height = 2f;
            capsule.radius = 0.5f;
            capsule.center = new Vector3(0, 1f, 0);
            
            Debug.Log("<color=green>[SetupBossComponents] Added CapsuleCollider</color>");
        }
        else
        {
            Debug.Log($"[SetupBossComponents] Collider already exists ({col.GetType().Name})");
        }

        // Asegurarse de que NO sea trigger (para colisionar con el suelo)
        if (col != null && col.isTrigger)
        {
            Debug.LogWarning("[SetupBossComponents] Collider is set as Trigger. This may prevent ground collision detection!");
        }
    }

    void SetupGodScript(BossController bossController)
    {
        God god = bossObject.GetComponent<God>();
        
        if (god == null)
        {
            Debug.LogError("[SetupBossComponents] God component not found!");
            return;
        }

        Debug.Log("[SetupBossComponents] God script will auto-connect to BossController in Start()");
        Debug.Log("  Note: Make sure God.cs has the bossController reference code!");
    }
}
