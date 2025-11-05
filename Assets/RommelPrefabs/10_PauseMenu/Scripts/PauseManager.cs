using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Cinemachine; // importante si usas Cinemachine

public class PauseManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pausePanel;
    public GameObject optionsPanel;

    [Header("Settings")]
    public KeyCode toggleKey = KeyCode.Escape;
    public bool pauseTimescale = true;

    [Header("Camera Control")]
    public CinemachineBrain[] cameraBrains; // arrastra tus cámaras con CinemachineBrain aquí

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

        // 🔹 Detener cámaras
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

        // 🔹 Reactivar cámaras
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
    public void OnSaveButton() => SaveGame();

    public void OnQuitButton()
    {
                // 🔹 Reanudar el tiempo antes de cambiar de escena
        Time.timeScale = 1f;

        // 🔹 Guardar antes de salir (opcional)

        // o si quieres volver al menú principal:
        SceneManager.LoadScene("MainMenu");
    }

    void SaveGame()
    {
        PlayerPrefs.SetInt("dummy_saved", 1);
        PlayerPrefs.Save();
        Debug.Log("Implementación del guardado aquí.");
        SaveManager.Instance?.SaveCurrentPlayerPosition();
    }
}
