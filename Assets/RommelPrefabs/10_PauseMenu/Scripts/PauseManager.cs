using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Reflection;
using UnityEngine.SceneManagement;
using Cinemachine;

public class PauseManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pausePanel;
    public GameObject optionsPanel;

    [Header("Settings")]
    public KeyCode toggleKey = KeyCode.Escape;
    public bool pauseTimescale = true;

    [Header("Camera Control")]
    public CinemachineBrain[] cameraBrains;

    bool isPaused = false;

    void Start()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        // Asegurar que exista un EventSystem en la escena para que los botones funcionen
        if (FindObjectOfType<EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<EventSystem>();

            // Intentar usar Input System UI module si está disponible (soporte para hover/point con PlayerInput)
            Type uiModuleType = null;
            try
            {
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    try
                    {
                        var t = asm.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule");
                        if (t != null)
                        {
                            uiModuleType = t;
                            break;
                        }
                    }
                    catch { }
                }
            }
            catch { }

            if (uiModuleType != null && typeof(Component).IsAssignableFrom(uiModuleType))
            {
                esGO.AddComponent(uiModuleType);
                Debug.Log("[PauseManager] EventSystem creado con InputSystemUIInputModule.");
            }
            else
            {
                esGO.AddComponent<StandaloneInputModule>();
                Debug.Log("[PauseManager] EventSystem creado con StandaloneInputModule.");
            }

            DontDestroyOnLoad(esGO);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (isPaused) ResumeGame();
        else PauseGame();
    }

    public void PauseGame()
    {
        if (pausePanel != null)
            pausePanel.SetActive(true);

        if (pauseTimescale)
            Time.timeScale = 0f;

        SetCamerasEnabled(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Asegurar que el panel de pausa pueda recibir clicks y esté al frente
        EnsureInteractableCanvas(pausePanel);

        isPaused = true;
    }

    public void ResumeGame()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (pauseTimescale)
            Time.timeScale = 1f;

        SetCamerasEnabled(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Restaurar bloqueo de raycasts si fue modificado
        DisablePanelRaycasts(pausePanel);

        isPaused = false;
    }

    void SetCamerasEnabled(bool state)
    {
        if (cameraBrains == null || cameraBrains.Length == 0) return;

        foreach (var cam in cameraBrains)
        {
            if (cam != null)
                cam.enabled = state;
        }
    }

    // Asegura que un panel de UI tenga Canvas con overrideSorting, GraphicRaycaster y CanvasGroup
    void EnsureInteractableCanvas(GameObject panel)
    {
        if (panel == null) return;

        // Si no existe Canvas en los padres, crear uno (caso excepcional)
        var parentCanvas = panel.GetComponentInParent<Canvas>();
        if (parentCanvas == null)
        {
            parentCanvas = panel.AddComponent<Canvas>();
            parentCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            parentCanvas.overrideSorting = true;
            parentCanvas.sortingOrder = 1000;
        }

        // Asegurar que el panel tenga su propio Canvas y priorizarlo
        var ownCanvas = panel.GetComponent<Canvas>();
        if (ownCanvas == null)
        {
            ownCanvas = panel.AddComponent<Canvas>();
        }
        ownCanvas.overrideSorting = true;
        ownCanvas.sortingOrder = 10001; // por encima del fade (9999)

        // Asegurar GraphicRaycaster en el panel directamente
        var gr = panel.GetComponent<UnityEngine.UI.GraphicRaycaster>();
        if (gr == null)
        {
            panel.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }

        // Asegurar CanvasGroup y que permita raycasts
        var cg = panel.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            cg = panel.AddComponent<CanvasGroup>();
        }
        cg.interactable = true;
        cg.blocksRaycasts = true;

        // Traer al frente en la jerarquía
        try { panel.transform.SetAsLastSibling(); } catch { }
    }

    void DisablePanelRaycasts(GameObject panel)
    {
        if (panel == null) return;
        var cg = panel.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.blocksRaycasts = false;
        }
    }

    public void OnContinueButton() => ResumeGame();
    public void OnOptionsButton() => optionsPanel?.SetActive(true);
    public void OnSaveButton() => SaveGame();

    public void OnQuitButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    void SaveGame()
    {
        if (SaveManager.Instance != null)
        {
            // 🔹 Ahora guarda posición Y estadísticas
            SaveManager.Instance.SaveCurrentPlayerState();
            Debug.Log("[PauseManager] ✅ Juego guardado (posición + stats)");
        }
        else
        {
            Debug.LogWarning("[PauseManager] SaveManager no encontrado.");
        }
    }
}