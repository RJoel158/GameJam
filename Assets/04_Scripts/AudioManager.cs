using UnityEngine;
using StarterAssets;

/// <summary>
/// AudioManager - Gestor centralizado de sonidos del juego
/// Proporciona métodos para controlar sonidos ambientales y efectos de sonido
/// </summary>
public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;
    private ThirdPersonController playerController;

    private void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Obtener referencia al jugador
        playerController = FindFirstObjectByType<ThirdPersonController>();
        if (playerController == null)
        {
            Debug.LogError("[AudioManager] ThirdPersonController no encontrado en la escena");
        }
    }

    /// <summary>
    /// Pausar el sonido ambiental
    /// </summary>
    public static void PauseAmbient()
    {
        if (instance != null && instance.playerController != null)
        {
            instance.playerController.PauseAmbientSound();
        }
    }

    /// <summary>
    /// Reanudar el sonido ambiental
    /// </summary>
    public static void ResumeAmbient()
    {
        if (instance != null && instance.playerController != null)
        {
            instance.playerController.ResumeAmbientSound();
        }
    }

    /// <summary>
    /// Detener el sonido ambiental
    /// </summary>
    public static void StopAmbient()
    {
        if (instance != null && instance.playerController != null)
        {
            instance.playerController.StopAmbientSound();
        }
    }

    /// <summary>
    /// Cambiar el volumen del sonido ambiental
    /// </summary>
    /// <param name="volume">Volumen entre 0 y 1</param>
    public static void SetAmbientVolume(float volume)
    {
        if (instance != null && instance.playerController != null)
        {
            instance.playerController.AmbientSoundVolume = Mathf.Clamp01(volume);
            Debug.Log($"[AudioManager] Volumen ambiental cambiado a {volume}");
        }
    }
}
