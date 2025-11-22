using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class DeathScreenManager : MonoBehaviour
{
    public static DeathScreenManager Instance;

    [Header("UI")]
    public Image deathImage;           // asigna tu imagen de derrota aquí
    public float fadeDuration = 1.2f;  // tiempo del fade
    public float delayBeforeRestart = 1.5f;

    [Header("Audio")]
    public AudioClip defeatSound;      // sonido de derrota
    [Range(0f, 1f)]
    public float defeatSoundVolume = 1f;
    [Range(0.5f, 1.5f)]
    [Tooltip("Pitch del audio (0.7 = más lento y grave, 1.0 = normal, 1.5 = más rápido y agudo)")]
    public float defeatSoundPitch = 0.7f;

    bool isShowing = false;

    void Awake()
    {
        Instance = this;

        if (deathImage != null)
        {
            var c = deathImage.color;
            c.a = 0f;
            deathImage.color = c;
            // Mantener la imagen oculta pero el GameObject activo
            deathImage.gameObject.SetActive(true);
        }

        // NO desactivar el GameObject, solo la imagen empieza transparente
    }

    public void TriggerDeathScreen()
    {
        if (isShowing) return;
        isShowing = true;

        Debug.Log("DeathScreenManager: TriggerDeathScreen llamado - iniciando DeathFlow");
        
        // Asegurar que la imagen esté visible
        if (deathImage != null)
            deathImage.gameObject.SetActive(true);
            
        StartCoroutine(DeathFlow());
    }

    IEnumerator DeathFlow()
    {
        Debug.Log("DeathScreenManager: Iniciando fade in de pantalla de muerte");
        
        // Reproducir sonido de derrota al inicio con pitch ajustable
        if (defeatSound != null)
        {
            GameObject tempAudio = new GameObject("TempDefeatAudio");
            AudioSource audioSource = tempAudio.AddComponent<AudioSource>();
            audioSource.clip = defeatSound;
            audioSource.volume = defeatSoundVolume;
            audioSource.pitch = defeatSoundPitch;
            audioSource.Play();
            
            Destroy(tempAudio, defeatSound.length / defeatSoundPitch + 0.1f);
            
            Debug.Log($"DeathScreenManager: Reproduciendo sonido de derrota (pitch: {defeatSoundPitch})");
        }
        
        // Fade in (imagen de derrota)
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);

            if (deathImage != null)
            {
                var c = deathImage.color;
                c.a = alpha;
                deathImage.color = c;
            }

            yield return null;
        }

        // Asegurar que la imagen esté completamente opaca
        if (deathImage != null)
        {
            var c = deathImage.color;
            c.a = 1f;
            deathImage.color = c;
        }

        Debug.Log($"DeathScreenManager: Fade completado, esperando {delayBeforeRestart} segundos");
        yield return new WaitForSecondsRealtime(delayBeforeRestart);

        // Reiniciar escena directamente (sin fade out)
        Debug.Log("DeathScreenManager: Reiniciando escena");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
