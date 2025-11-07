using UnityEngine;
using UnityEngine.UI;
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

    public void OnContinueButton() => ResumeGame();
    public void OnOptionsButton() => optionsPanel?.SetActive(true);
    
    // 🔹 Método actualizado para guardar el estado completo
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