using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

[Serializable]
public class SaveData
{
    public float px, py, pz;
    public string sceneName;
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

        // 🔹 CRÍTICO: Siempre cargar desde disco al inicio
        if (HasSaveFile())
            LoadFromDiskIntoProfile();
        else if (runtimeProfile != null)
        {
            // Limpiar el profile si no hay archivo (importante para el Editor)
            runtimeProfile.hasSave = false;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    #region Save / Load disk

    public void SaveCurrentPlayerPosition()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("[SaveManager] No se encontró GameObject con tag 'Player'.");
            return;
        }

        Vector3 pos = player.transform.position;

        SaveData data = new SaveData
        {
            px = pos.x,
            py = pos.y,
            pz = pos.z,
            sceneName = openWorldSceneName
        };

        try
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(savePath, json);
            Debug.Log($"[SaveManager] ✅ Guardado en {savePath} → Posición: {pos}");

            // 🔹 Actualizar runtimeProfile INMEDIATAMENTE después de guardar
            if (runtimeProfile != null)
            {
                runtimeProfile.playerPosition = pos;
                runtimeProfile.sceneName = data.sceneName;
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

    public Vector3 LoadSavedPosition()
    {
        if (!HasSaveFile())
        {
            Debug.LogWarning("[SaveManager] No hay archivo de guardado encontrado.");
            return Vector3.zero;
        }

        try
        {
            string json = File.ReadAllText(savePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            return new Vector3(data.px, data.py, data.pz);
        }
        catch (Exception ex)
        {
            Debug.LogError("[SaveManager] Error leyendo save: " + ex.Message);
            return Vector3.zero;
        }
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
                runtimeProfile.hasSave = true;
                #if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(runtimeProfile);
                #endif
            }
            Debug.Log($"[SaveManager] 📂 Cargado desde disco → Posición: {runtimeProfile.playerPosition}");
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

        // 🔹 SIEMPRE recargar desde disco antes de aplicar (ignora lo que tenga el ScriptableObject)
        LoadFromDiskIntoProfile();

        if (runtimeProfile != null && runtimeProfile.hasSave)
        {
            Debug.Log($"[SaveManager] 🎯 Preparando carga en posición: {runtimeProfile.playerPosition}");
            StartCoroutine(ApplyPositionWhenPlayerExists());
        }
    }

    private IEnumerator ApplyPositionWhenPlayerExists()
    {
        const float timeout = 8f;
        const float pollInterval = 0.1f;
        float elapsed = 0f;

        // 1) Buscar al player (usualmente ya existe)
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
            yield break;
        }

        // 2) 🔹 OCULTAR la cámara principal durante el proceso
        Camera mainCam = Camera.main;
        bool camWasActive = false;
        if (mainCam != null)
        {
            camWasActive = mainCam.enabled;
            mainCam.enabled = false; // Pantalla negra mientras reposicionamos
        }

        // 3) Obtener posición objetivo (siempre desde runtimeProfile, que acabamos de recargar)
        Vector3 target = runtimeProfile.playerPosition;
        Debug.Log($"[SaveManager] 🎯 Aplicando posición: {target}");

        // 4) Recolectar componentes de movimiento
        var toDisable = new System.Collections.Generic.List<Behaviour>();
        
        var tpc = player.GetComponent<StarterAssets.ThirdPersonController>();
        if (tpc != null) toDisable.Add(tpc);
        
        var sai = player.GetComponent<StarterAssets.StarterAssetsInputs>();
        if (sai != null) toDisable.Add(sai);
        
        var playerInput = player.GetComponent<UnityEngine.InputSystem.PlayerInput>();
        if (playerInput != null) toDisable.Add(playerInput);
        
        var playerController = player.GetComponent<PlayerController>();
        if (playerController != null) toDisable.Add(playerController);

        // 5) Desactivar Cinemachine Virtual Cameras
        var allVCams = FindObjectsOfType<Cinemachine.CinemachineVirtualCamera>();
        var vCamStates = new System.Collections.Generic.List<bool>();
        foreach (var vcam in allVCams)
        {
            vCamStates.Add(vcam.enabled);
            vcam.enabled = false;
        }

        // 6) Manejar física
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

        // 7) Desactivar componentes
        var origStates = new System.Collections.Generic.List<bool>();
        foreach (var b in toDisable)
        {
            origStates.Add(b.enabled);
            b.enabled = false;
        }

        // 8) Esperar un frame completo
        yield return null;

        // 9) Aplicar posición múltiples veces
        for (int i = 0; i < 5; i++)
        {
            player.transform.position = target;
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            yield return null;
        }

        // 10) Mover CinemachineCameraTarget
        var camTarget = player.transform.Find("CinemachineCameraTarget");
        if (camTarget != null)
        {
            camTarget.position = target;
        }

        // 11) Reactivar CharacterController primero
        if (cc != null && ccWasEnabled)
        {
            cc.enabled = true;
            yield return null;
        }

        // 12) Reactivar componentes de movimiento
        for (int i = 0; i < toDisable.Count; i++)
        {
            if (toDisable[i] != null)
                toDisable[i].enabled = origStates[i];
        }

        // 13) Restaurar Rigidbody
        if (rb != null)
        {
            rb.isKinematic = wasKinematic;
        }

        // 14) Forzar posición final
        player.transform.position = target;
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
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

        // 16) 🔹 Esperar un frame y reactivar la cámara
        yield return null;
        if (mainCam != null && camWasActive)
        {
            mainCam.enabled = true;
        }

        Debug.Log($"[SaveManager] ✅ Carga completada en: {target}");
    }

    #endregion
}