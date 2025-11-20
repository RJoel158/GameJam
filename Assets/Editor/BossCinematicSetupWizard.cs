using UnityEngine;
using UnityEditor;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Cinemachine;
using System.IO;
using System.Linq;

#if UNITY_EDITOR
/// <summary>
/// Wizard automático para configurar la cinemática del boss
/// Menu: Tools > Boss Cinematic Setup Wizard
/// </summary>
public class BossCinematicSetupWizard : EditorWindow
{
    [Header("Referencias Necesarias")]
    private GameObject bossPrefab;
    private Vector3 bossSpawnPosition = new Vector3(0, 0, 50);
    private GameObject player;
    
    [Header("Configuración")]
    private float cinematicDuration = 12f;
    private bool createCameras = true;
    private bool createTimeline = true;
    private bool createUI = true;
    private bool setupSignals = true;
    
    [Header("Audio (Opcional)")]
    private AudioClip bossRoarSound;
    private AudioClip groundImpactSound;
    private AudioClip dramaticMusic;

    private Vector2 scrollPos;

    [MenuItem("Tools/Boss Cinematic Setup Wizard")]
    public static void ShowWindow()
    {
        var window = GetWindow<BossCinematicSetupWizard>("Boss Cinematic Setup");
        window.minSize = new Vector2(500, 700);
        window.Show();
    }

    private void OnEnable()
    {
        // Intentar encontrar el jugador automáticamente
        var playerController = FindAnyObjectByType<StarterAssets.ThirdPersonController>();
        if (playerController != null)
        {
            player = playerController.gameObject;
        }
    }

    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        GUILayout.Space(10);
        EditorGUILayout.LabelField("🎬 BOSS CINEMATIC SETUP WIZARD", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Este wizard configurará automáticamente toda la cinemática del boss, " +
            "incluyendo Timeline, cámaras Cinemachine, spawner, eventos y UI.", MessageType.Info);

        GUILayout.Space(20);
        EditorGUILayout.LabelField("📋 REFERENCIAS REQUERIDAS", EditorStyles.boldLabel);
        
        bossPrefab = (GameObject)EditorGUILayout.ObjectField("Boss Prefab", bossPrefab, typeof(GameObject), false);
        bossSpawnPosition = EditorGUILayout.Vector3Field("Boss Spawn Position", bossSpawnPosition);
        player = (GameObject)EditorGUILayout.ObjectField("Player GameObject", player, typeof(GameObject), true);

        GUILayout.Space(20);
        EditorGUILayout.LabelField("⚙️ CONFIGURACIÓN", EditorStyles.boldLabel);
        
        cinematicDuration = EditorGUILayout.Slider("Cinematic Duration (seconds)", cinematicDuration, 5f, 30f);
        createCameras = EditorGUILayout.Toggle("Create Cinemachine Cameras", createCameras);
        createTimeline = EditorGUILayout.Toggle("Create Timeline", createTimeline);
        createUI = EditorGUILayout.Toggle("Create Letterbox UI", createUI);
        setupSignals = EditorGUILayout.Toggle("Setup Timeline Signals", setupSignals);

        GUILayout.Space(20);
        EditorGUILayout.LabelField("🔊 AUDIO (Opcional)", EditorStyles.boldLabel);
        
        bossRoarSound = (AudioClip)EditorGUILayout.ObjectField("Boss Roar Sound", bossRoarSound, typeof(AudioClip), false);
        groundImpactSound = (AudioClip)EditorGUILayout.ObjectField("Ground Impact Sound", groundImpactSound, typeof(AudioClip), false);
        dramaticMusic = (AudioClip)EditorGUILayout.ObjectField("Dramatic Music", dramaticMusic, typeof(AudioClip), false);

        GUILayout.Space(30);

        // Botón principal
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("🚀 CREAR SETUP COMPLETO", GUILayout.Height(50)))
        {
            if (ValidateInputs())
            {
                CreateCompleteSetup();
            }
        }
        GUI.backgroundColor = Color.white;

        GUILayout.Space(10);

        // Botones individuales
        EditorGUILayout.LabelField("🔧 CREAR COMPONENTES INDIVIDUALES", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Create Boss Spawner Only"))
        {
            CreateBossSpawner();
        }
        
        if (GUILayout.Button("Create Cinemachine Cameras Only"))
        {
            CreateCinemachineCameras();
        }
        
        if (GUILayout.Button("Create Timeline Only"))
        {
            CreateTimelineSetup();
        }
        
        if (GUILayout.Button("Create Letterbox UI Only"))
        {
            CreateLetterboxUI();
        }

        GUILayout.Space(10);

        // Botón de limpieza
        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("🗑️ Remove All Boss Cinematic Objects"))
        {
            if (EditorUtility.DisplayDialog("Confirm Deletion", 
                "Are you sure you want to delete all boss cinematic objects?", 
                "Yes, Delete", "Cancel"))
            {
                CleanupAll();
            }
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndScrollView();
    }

    private bool ValidateInputs()
    {
        if (bossPrefab == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign a Boss Prefab!", "OK");
            return false;
        }

        if (player == null)
        {
            EditorUtility.DisplayDialog("Warning", 
                "Player not assigned. Some features may not work properly.\nContinue anyway?", 
                "OK");
        }

        return true;
    }

    private void CreateCompleteSetup()
    {
        Debug.Log("<color=cyan>========================================</color>");
        Debug.Log("<color=cyan>[Boss Cinematic Wizard] Starting complete setup...</color>");
        Debug.Log("<color=cyan>========================================</color>");

        // 1. Crear el spawner
        GameObject spawner = CreateBossSpawner();
        
        // 2. Crear las cámaras
        GameObject[] cameras = null;
        if (createCameras)
        {
            cameras = CreateCinemachineCameras();
        }

        // 3. Crear el timeline
        GameObject timelineObj = null;
        if (createTimeline)
        {
            timelineObj = CreateTimelineSetup();
            
            // Conectar timeline al spawner
            if (spawner != null && timelineObj != null)
            {
                BossSpawner spawnerScript = spawner.GetComponent<BossSpawner>();
                PlayableDirector director = timelineObj.GetComponent<PlayableDirector>();
                
                if (spawnerScript != null && director != null)
                {
                    SerializedObject so = new SerializedObject(spawnerScript);
                    so.FindProperty("cinematicTimeline").objectReferenceValue = director;
                    so.ApplyModifiedProperties();
                    Debug.Log("<color=green>[Wizard] Connected Timeline to BossSpawner</color>");
                }
            }

            // Configurar eventos del timeline
            if (timelineObj != null)
            {
                BossCinematicEvents eventsScript = timelineObj.GetComponent<BossCinematicEvents>();
                if (eventsScript != null && spawner != null)
                {
                    SerializedObject so = new SerializedObject(eventsScript);
                    so.FindProperty("bossSpawner").objectReferenceValue = spawner.GetComponent<BossSpawner>();
                    so.FindProperty("bossRoarSound").objectReferenceValue = bossRoarSound;
                    so.FindProperty("groundImpactSound").objectReferenceValue = groundImpactSound;
                    so.FindProperty("dramaticMusic").objectReferenceValue = dramaticMusic;
                    so.ApplyModifiedProperties();
                    Debug.Log("<color=green>[Wizard] Configured BossCinematicEvents</color>");
                }
            }
        }

        // 4. Crear UI
        if (createUI)
        {
            CreateLetterboxUI();
        }

        // 5. Crear signals si es necesario
        if (setupSignals && createTimeline)
        {
            CreateTimelineSignals();
        }

        Debug.Log("<color=green>========================================</color>");
        Debug.Log("<color=green>[Boss Cinematic Wizard] Setup Complete! ✅</color>");
        Debug.Log("<color=green>========================================</color>");

        EditorUtility.DisplayDialog("Success!", 
            "Boss Cinematic setup completed!\n\n" +
            "Next steps:\n" +
            "1. Select the BossCinematicTimeline object\n" +
            "2. Open Window > Sequencing > Timeline\n" +
            "3. Arrange the camera shots\n" +
            "4. Add Signal Emitters for events\n" +
            "5. Test by completing the mission!", 
            "OK");

        // Seleccionar el timeline para que el usuario pueda editarlo
        if (timelineObj != null)
        {
            Selection.activeGameObject = timelineObj;
            EditorGUIUtility.PingObject(timelineObj);
        }
    }

    private GameObject CreateBossSpawner()
    {
        // Buscar si ya existe
        GameObject existing = GameObject.Find("BossSpawner");
        if (existing != null)
        {
            if (!EditorUtility.DisplayDialog("BossSpawner Already Exists", 
                "A BossSpawner already exists. Replace it?", 
                "Yes", "No"))
            {
                return existing;
            }
            DestroyImmediate(existing);
        }

        // Crear spawner
        GameObject spawner = new GameObject("BossSpawner");
        BossSpawner spawnerScript = spawner.AddComponent<BossSpawner>();

        // Crear spawn point
        GameObject spawnPoint = new GameObject("BossSpawnPoint");
        spawnPoint.transform.position = bossSpawnPosition;
        spawnPoint.transform.SetParent(spawner.transform);

        // Configurar spawner
        SerializedObject so = new SerializedObject(spawnerScript);
        so.FindProperty("bossPrefab").objectReferenceValue = bossPrefab;
        so.FindProperty("spawnPoint").objectReferenceValue = spawnPoint.transform;
        so.FindProperty("instantiateBoss").boolValue = true;
        so.FindProperty("playCinematicBeforeSpawn").boolValue = true;
        so.FindProperty("disablePlayerControls").boolValue = true;
        so.FindProperty("spawnDelay").floatValue = 1f;
        so.ApplyModifiedProperties();

        Debug.Log("<color=green>[Wizard] Created BossSpawner ✅</color>");
        return spawner;
    }

    private GameObject[] CreateCinemachineCameras()
    {
        GameObject camerasParent = GameObject.Find("BossCinematicCameras");
        if (camerasParent != null)
        {
            if (EditorUtility.DisplayDialog("Cameras Already Exist", 
                "Boss cinematic cameras already exist. Replace them?", 
                "Yes", "No"))
            {
                DestroyImmediate(camerasParent);
            }
            else
            {
                var virtualCameras = camerasParent.GetComponentsInChildren<CinemachineVirtualCamera>();
                GameObject[] cameraObjects = new GameObject[virtualCameras.Length];
                for (int i = 0; i < virtualCameras.Length; i++)
                {
                    cameraObjects[i] = virtualCameras[i].gameObject;
                }
                return cameraObjects;
            }
        }

        camerasParent = new GameObject("BossCinematicCameras");
        GameObject[] cameras = new GameObject[4];

        Vector3 spawnPos = bossSpawnPosition;

        // Camera 1: Wide Shot
        cameras[0] = CreateVirtualCamera("CM_BossIntro_Wide", 
            spawnPos + new Vector3(-10, 5, -10), 
            spawnPos, 60f);

        // Camera 2: Boss Close-Up
        cameras[1] = CreateVirtualCamera("CM_BossIntro_CloseUp", 
            spawnPos + new Vector3(0, 2, -5), 
            spawnPos + new Vector3(0, 1.5f, 0), 45f);

        // Camera 3: Player Reaction
        if (player != null)
        {
            Vector3 playerPos = player.transform.position;
            cameras[2] = CreateVirtualCamera("CM_BossIntro_PlayerReaction", 
                playerPos + new Vector3(2, 1.5f, -3), 
                playerPos + new Vector3(0, 1.5f, 0), 50f);
        }
        else
        {
            cameras[2] = CreateVirtualCamera("CM_BossIntro_PlayerReaction", 
                spawnPos + new Vector3(5, 2, -8), 
                spawnPos, 50f);
        }

        // Camera 4: Dramatic Low Angle
        cameras[3] = CreateVirtualCamera("CM_BossIntro_Dramatic", 
            spawnPos + new Vector3(0, 0.5f, -8), 
            spawnPos + new Vector3(0, 3, 0), 55f);

        // Parent all cameras
        foreach (var cam in cameras)
        {
            if (cam != null)
            {
                cam.transform.SetParent(camerasParent.transform);
            }
        }

        Debug.Log("<color=green>[Wizard] Created 4 Cinemachine Virtual Cameras ✅</color>");
        return cameras;
    }

    private GameObject CreateVirtualCamera(string name, Vector3 position, Vector3 lookAt, float fov)
    {
        GameObject camObj = new GameObject(name);
        camObj.transform.position = position;
        camObj.transform.LookAt(lookAt);

        CinemachineVirtualCamera vcam = camObj.AddComponent<CinemachineVirtualCamera>();
        vcam.m_Lens.FieldOfView = fov;
        vcam.Priority = 0;

        return camObj;
    }

    private GameObject CreateTimelineSetup()
    {
        // Buscar si ya existe
        GameObject existing = GameObject.Find("BossCinematicTimeline");
        if (existing != null)
        {
            if (!EditorUtility.DisplayDialog("Timeline Already Exists", 
                "A BossCinematicTimeline already exists. Replace it?", 
                "Yes", "No"))
            {
                return existing;
            }
            DestroyImmediate(existing);
        }

        // Crear GameObject
        GameObject timelineObj = new GameObject("BossCinematicTimeline");
        
        // Agregar PlayableDirector
        PlayableDirector director = timelineObj.AddComponent<PlayableDirector>();
        
        // Crear Timeline Asset
        string scenePath = "Assets/00_Scenes/OpenWorldSceneMerged";
        if (!Directory.Exists(scenePath))
        {
            Directory.CreateDirectory(scenePath);
        }

        string timelinePath = scenePath + "/BossIntro_Timeline.playable";
        
        // Crear el timeline asset
        TimelineAsset timeline = ScriptableObject.CreateInstance<TimelineAsset>();
        AssetDatabase.CreateAsset(timeline, timelinePath);
        AssetDatabase.SaveAssets();

        // Asignar al director
        director.playableAsset = timeline;
        director.playOnAwake = false;

        // Agregar BossCinematicEvents
        BossCinematicEvents eventsScript = timelineObj.AddComponent<BossCinematicEvents>();
        
        // Configurar eventos
        SerializedObject so = new SerializedObject(eventsScript);
        so.FindProperty("enableCameraShake").boolValue = true;
        so.FindProperty("shakeIntensity").floatValue = 0.5f;
        so.FindProperty("shakeDuration").floatValue = 0.3f;
        so.ApplyModifiedProperties();

        Debug.Log($"<color=green>[Wizard] Created Timeline at {timelinePath} ✅</color>");
        return timelineObj;
    }

    private void CreateLetterboxUI()
    {
        // Buscar canvas existente
        Canvas canvas = FindAnyObjectByType<Canvas>();
        GameObject canvasObj;

        if (canvas == null)
        {
            canvasObj = new GameObject("CinematicUI");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }
        else
        {
            canvasObj = canvas.gameObject;
        }

        // Crear letterbox bars
        GameObject letterboxBars = new GameObject("LetterboxBars");
        letterboxBars.transform.SetParent(canvasObj.transform, false);

        // Top bar
        GameObject topBar = CreateLetterboxBar("TopBar", letterboxBars.transform);
        RectTransform topRect = topBar.GetComponent<RectTransform>();
        topRect.anchorMin = new Vector2(0, 1);
        topRect.anchorMax = new Vector2(1, 1);
        topRect.pivot = new Vector2(0.5f, 1);
        topRect.sizeDelta = new Vector2(0, 100);
        topRect.anchoredPosition = Vector2.zero;

        // Bottom bar
        GameObject bottomBar = CreateLetterboxBar("BottomBar", letterboxBars.transform);
        RectTransform bottomRect = bottomBar.GetComponent<RectTransform>();
        bottomRect.anchorMin = new Vector2(0, 0);
        bottomRect.anchorMax = new Vector2(1, 0);
        bottomRect.pivot = new Vector2(0.5f, 0);
        bottomRect.sizeDelta = new Vector2(0, 100);
        bottomRect.anchoredPosition = Vector2.zero;

        // Desactivar por defecto
        letterboxBars.SetActive(false);

        Debug.Log("<color=green>[Wizard] Created Letterbox UI ✅</color>");
    }

    private GameObject CreateLetterboxBar(string name, Transform parent)
    {
        GameObject bar = new GameObject(name);
        bar.transform.SetParent(parent, false);

        RectTransform rect = bar.AddComponent<RectTransform>();
        UnityEngine.UI.Image image = bar.AddComponent<UnityEngine.UI.Image>();
        image.color = Color.black;

        return bar;
    }

    private void CreateTimelineSignals()
    {
        string signalsPath = "Assets/04_Scripts/Signals";
        if (!Directory.Exists(signalsPath))
        {
            Directory.CreateDirectory(signalsPath);
        }

        string[] signalNames = new string[]
        {
            "Signal_SpawnBoss",
            "Signal_GroundImpact",
            "Signal_BossRoar",
            "Signal_DramaticMusic",
            "Signal_EnableBossAI"
        };

        foreach (string signalName in signalNames)
        {
            string path = $"{signalsPath}/{signalName}.asset";
            
            if (!File.Exists(path))
            {
                UnityEngine.Timeline.SignalAsset signal = ScriptableObject.CreateInstance<UnityEngine.Timeline.SignalAsset>();
                AssetDatabase.CreateAsset(signal, path);
                Debug.Log($"<color=green>[Wizard] Created signal: {signalName}</color>");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("<color=green>[Wizard] Created Timeline Signals ✅</color>");
    }

    private void CleanupAll()
    {
        Debug.Log("<color=yellow>[Wizard] Cleaning up boss cinematic objects...</color>");

        // Eliminar GameObjects
        string[] objectsToDelete = new string[]
        {
            "BossSpawner",
            "BossCinematicTimeline",
            "BossCinematicCameras",
            "LetterboxBars"
        };

        foreach (string objName in objectsToDelete)
        {
            GameObject obj = GameObject.Find(objName);
            if (obj != null)
            {
                DestroyImmediate(obj);
                Debug.Log($"<color=yellow>[Wizard] Deleted {objName}</color>");
            }
        }

        Debug.Log("<color=green>[Wizard] Cleanup complete ✅</color>");
    }
}
#endif
