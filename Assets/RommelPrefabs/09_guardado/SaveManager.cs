using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;


[Serializable]
public class SaveData
{
    // Posición
    public float px, py, pz;
    public string sceneName;

    // Estadísticas del jugador
    public int health;
    public int maxHealth;
    public int force;
    public int maxForce;
    public bool isEquipped;

    // Constructor vacío para JsonUtility
    public SaveData() { }

    // Constructor con parámetros para facilitar la creación
    public SaveData(Vector3 position, string scene, int hp, int maxHp, int stamina, int maxStamina)
    {
        px = position.x;
        py = position.y;
        pz = position.z;
        sceneName = scene;
        health = hp;
        maxHealth = maxHp;
        force = stamina;
        maxForce = maxStamina;
    }
}


public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    [Header("Runtime / Config")]
    public SaveProfile runtimeProfile;
    public string openWorldSceneName = "OpenWorldSceneMerged";
    public string saveFileName = "savegame.json";

    [HideInInspector]
    public bool loadOnNextOpenWorld = false;

    private string savePath;
    private Canvas fadeCanvas;
    private UnityEngine.UI.Image fadeImage;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        savePath = Path.Combine(Application.persistentDataPath, saveFileName);

        // Cargar desde disco al inicio
        if (HasSaveFile())
            LoadFromDiskIntoProfile();
        else if (runtimeProfile != null)
        {
            runtimeProfile.hasSave = false;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    #region Fade System

    private void CreateFadeCanvas()
    {
        if (fadeCanvas != null) return;

        GameObject canvasGO = new GameObject("SaveManager_FadeCanvas");
        DontDestroyOnLoad(canvasGO);
        
        fadeCanvas = canvasGO.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 9999;
        
        canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        GameObject imageGO = new GameObject("FadeImage");
        imageGO.transform.SetParent(canvasGO.transform, false);
        
        fadeImage = imageGO.AddComponent<UnityEngine.UI.Image>();
        fadeImage.color = Color.black;
        
        RectTransform rt = fadeImage.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;

        fadeCanvas.gameObject.SetActive(false);
    }

    private void ShowFade()
    {
        CreateFadeCanvas();
        fadeCanvas.gameObject.SetActive(true);
        fadeImage.color = Color.black;
    }

    private void HideFade()
    {
        if (fadeCanvas != null)
            fadeCanvas.gameObject.SetActive(false);
    }

    #endregion

    #region Save / Load disk

    public void SaveCurrentPlayerState()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("[SaveManager] No se encontró GameObject con tag 'Player'.");
            return;
        }

        // Obtener posición
        Vector3 pos = player.transform.position;

        // Obtener ThirdPersonController para estadísticas
        var tpc = player.GetComponent<StarterAssets.ThirdPersonController>();
        if (tpc == null)
        {
            Debug.LogWarning("[SaveManager] No se encontró ThirdPersonController en el Player.");
            return;
        }

        // Crear datos de guardado con toda la info
        SaveData data = new SaveData(
            pos,
            openWorldSceneName,
            tpc.health,
            tpc.maxHealth,
            tpc.force,
            tpc.maxForce
        );

        try
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(savePath, json);
            Debug.Log($"[SaveManager] ✅ Guardado completo en {savePath}\n" +
                      $"Posición: {pos}\n" +
                      $"Salud: {data.health}/{data.maxHealth}\n" +
                      $"Stamina: {data.force}/{data.maxForce}\n" +
                      $"Equipado: {data.isEquipped}");

            // Actualizar runtimeProfile
            if (runtimeProfile != null)
            {
                runtimeProfile.playerPosition = pos;
                runtimeProfile.sceneName = data.sceneName;
                runtimeProfile.health = data.health;
                runtimeProfile.maxHealth = data.maxHealth;
                runtimeProfile.force = data.force;
                runtimeProfile.maxForce = data.maxForce;
    
                runtimeProfile.hasSave = true;
                
                #if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(runtimeProfile);
                #endif
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("[SaveManager] Error guardando: " + ex.Message);
        }
    }

    public bool HasSaveFile()
    {
        return File.Exists(savePath);
    }

    public void LoadFromDiskIntoProfile()
    {
        if (!HasSaveFile())
        {
            if (runtimeProfile != null) runtimeProfile.hasSave = false;
            return;
        }

        try
        {
            string json = File.ReadAllText(savePath);
            SaveData d = JsonUtility.FromJson<SaveData>(json);
            
            if (runtimeProfile != null)
            {
                runtimeProfile.playerPosition = new Vector3(d.px, d.py, d.pz);
                runtimeProfile.sceneName = d.sceneName;
                runtimeProfile.health = d.health;
                runtimeProfile.maxHealth = d.maxHealth;
                runtimeProfile.force = d.force;
                runtimeProfile.maxForce = d.maxForce;
                runtimeProfile.hasSave = true;
                
                #if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(runtimeProfile);
                #endif

                Debug.Log($"[SaveManager] 📂 Cargado desde disco\n" +
                          $"Posición: {runtimeProfile.playerPosition}\n" +
                          $"Salud: {runtimeProfile.health}/{runtimeProfile.maxHealth}\n" +
                          $"Stamina: {runtimeProfile.force}/{runtimeProfile.maxForce}\n");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("[SaveManager] Error cargando save a profile: " + ex.Message);
            if (runtimeProfile != null) runtimeProfile.hasSave = false;
        }
    }

    #endregion

    #region Scene loaded handling

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != openWorldSceneName) return;
        if (!loadOnNextOpenWorld) return;

        loadOnNextOpenWorld = false;

        // Siempre recargar desde disco antes de aplicar
        LoadFromDiskIntoProfile();

        if (runtimeProfile != null && runtimeProfile.hasSave)
        {
            Debug.Log($"[SaveManager] 🎯 Preparando carga completa");
            StartCoroutine(ApplyPlayerStateWhenExists());
        }
    }

    private IEnumerator ApplyPlayerStateWhenExists()
    {
        // Mostrar pantalla negra
        ShowFade();

        const float timeout = 8f;
        const float pollInterval = 0.1f;
        float elapsed = 0f;

        // 1) Buscar al player
        GameObject player = null;
        while (elapsed < timeout)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) break;
            elapsed += pollInterval;
            yield return new WaitForSecondsRealtime(pollInterval);
        }

        if (player == null)
        {
            Debug.LogWarning("[SaveManager] ❌ No se encontró Player (timeout).");
            HideFade();
            yield break;
        }

        // 2) Obtener componentes necesarios
        var tpc = player.GetComponent<StarterAssets.ThirdPersonController>();
        if (tpc == null)
        {
            Debug.LogWarning("[SaveManager] ❌ No se encontró ThirdPersonController.");
            HideFade();
            yield break;
        }

        Vector3 targetPos = runtimeProfile.playerPosition;
        Debug.Log($"[SaveManager] 🎯 Aplicando estado guardado...");

        // 3) Recolectar componentes de movimiento para desactivar
        var toDisable = new System.Collections.Generic.List<Behaviour>();
        
        if (tpc != null) toDisable.Add(tpc);
        
        var sai = player.GetComponent<StarterAssets.StarterAssetsInputs>();
        if (sai != null) toDisable.Add(sai);
        
        var playerInput = player.GetComponent<UnityEngine.InputSystem.PlayerInput>();
        if (playerInput != null) toDisable.Add(playerInput);
        
        var playerController = player.GetComponent<PlayerController>();
        if (playerController != null) toDisable.Add(playerController);

        // 4) Desactivar Cinemachine Virtual Cameras
        var allVCams = FindObjectsOfType<Cinemachine.CinemachineVirtualCamera>();
        var vCamStates = new System.Collections.Generic.List<bool>();
        foreach (var vcam in allVCams)
        {
            vCamStates.Add(vcam.enabled);
            vcam.enabled = false;
        }

        // 5) Manejar física
        var rb = player.GetComponent<Rigidbody>();
        var cc = player.GetComponent<CharacterController>();
        
        bool wasKinematic = false;
        bool ccWasEnabled = false;

        if (rb != null)
        {
            wasKinematic = rb.isKinematic;
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        if (cc != null)
        {
            ccWasEnabled = cc.enabled;
            cc.enabled = false;
        }

        // 6) Desactivar componentes
        var origStates = new System.Collections.Generic.List<bool>();
        foreach (var b in toDisable)
        {
            origStates.Add(b.enabled);
            b.enabled = false;
        }

        yield return null;

        // 7) Aplicar ESTADÍSTICAS primero (mientras está desactivado)
        tpc.health = runtimeProfile.health;
        tpc.maxHealth = runtimeProfile.maxHealth;
        tpc.force = runtimeProfile.force;
        tpc.maxForce = runtimeProfile.maxForce;


        // 8) Aplicar posición múltiples veces
        for (int i = 0; i < 5; i++)
        {
            player.transform.position = targetPos;
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            yield return null;
        }

        // 9) Mover CinemachineCameraTarget
        var camTarget = player.transform.Find("CinemachineCameraTarget");
        if (camTarget != null)
        {
            camTarget.position = targetPos;
        }

        // 10) Reactivar CharacterController primero
        if (cc != null && ccWasEnabled)
        {
            cc.enabled = true;
            yield return null;
        }

        // 11) Reactivar componentes de movimiento
        for (int i = 0; i < toDisable.Count; i++)
        {
            if (toDisable[i] != null)
                toDisable[i].enabled = origStates[i];
        }

        // 12) Restaurar Rigidbody
        if (rb != null)
        {
            rb.isKinematic = wasKinematic;
        }

        // 13) Forzar posición final
        player.transform.position = targetPos;
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // 14) Actualizar animator si está equipado
        if (tpc._animator != null)
        {
            tpc._animator.SetBool(Animator.StringToHash("Equipped"), true);
        }

        // 15) Reactivar Cinemachine y forzar update
        for (int i = 0; i < allVCams.Length; i++)
        {
            allVCams[i].enabled = vCamStates[i];
            if (allVCams[i].enabled)
            {
                allVCams[i].PreviousStateIsValid = false;
            }
        }

        var brain = Camera.main?.GetComponent<Cinemachine.CinemachineBrain>();
        if (brain != null)
        {
            brain.ManualUpdate();
        }

        yield return null;
        
        // Ocultar pantalla negra
        HideFade();

        Debug.Log($"[SaveManager] ✅ Estado completo restaurado\n" +
                  $"Posición: {targetPos}\n" +
                  $"Salud: {tpc.health}/{tpc.maxHealth}\n" +
                  $"Stamina: {tpc.force}/{tpc.maxForce}\n" +
                  $"Equipado: {tpc.isEquipped}");
    }

    #endregion
}