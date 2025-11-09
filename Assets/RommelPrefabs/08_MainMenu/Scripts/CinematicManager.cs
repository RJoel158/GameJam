using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CinematicManager : MonoBehaviour
{
    public static CinematicManager Instance;

    [Header("Video UI")]
    public GameObject videoContainer; // GameObject que contiene RawImage + VideoPlayer (desactivado por defecto)
    public VideoPlayer videoPlayer;   // VideoPlayer apuntando al RenderTexture o a RawImage
    public RawImage rawImageTarget;   // RawImage donde se mostrará el video (opcional)

    [Header("Flow")]
    public string nextSceneAfterCinematic = "LoadingScreen_1";
    public float preFadeDuration = 0.5f; // tiempo del fade in previo a la reproducción
    public float postFadeDuration = 0.4f; // fade tras terminar video antes de cargar

    bool isPlaying = false;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }

        if (videoContainer != null)
            videoContainer.SetActive(false);

        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false;
            // Configurar para evitar warnings de timestamps
            videoPlayer.skipOnDrop = true;
            
            Debug.Log($"CinematicManager: VideoPlayer configurado. Clip: {videoPlayer.clip?.name ?? "null"}");
        }
    }

    /// <summary>
    /// Llamar desde MenuActions.NewGame()
    /// </summary>
    public void PlayCinematicThenLoad()
    {
        if (isPlaying) return;
        StartCoroutine(PlayFlow());
    }

    IEnumerator PlayFlow()
    {
        isPlaying = true;

        // 0) pause música del menú (no Stop, Pause para poder resume)
        UIAudioManager.Instance?.PauseMusic();

        // 1) Fade to black (pre fade) — usamos FadeTransition FadeToBlack
        if (FadeTransition.Instance != null)
        {
            yield return StartCoroutine(FadeTransition.Instance.FadeToBlack(preFadeDuration));
        }

        // 2) Mostrar contenedor del video
        if (videoContainer != null)
            videoContainer.SetActive(true);

        // 3) Preparar y reproducir video
        if (videoPlayer != null)
        {
            Debug.Log("CinematicManager: Preparando video...");
            videoPlayer.Prepare();
            
            // Esperar a que el video esté preparado
            float timeout = 10f;
            float elapsed = 0f;
            while (!videoPlayer.isPrepared && elapsed < timeout)
            {
                yield return null;
                elapsed += Time.unscaledDeltaTime;
            }

            if (!videoPlayer.isPrepared)
            {
                Debug.LogError("CinematicManager: Video no se pudo preparar. Saltando a siguiente escena.");
                SceneManager.LoadScene(nextSceneAfterCinematic);
                yield break;
            }

            Debug.Log("CinematicManager: Video preparado, haciendo fade a transparente...");
            
            // Fade a transparente para ver el video
            if (FadeTransition.Instance != null)
            {
                yield return StartCoroutine(FadeTransition.Instance.FadeToTransparent(0.5f));
            }

            Debug.Log("CinematicManager: Reproduciendo video...");
            videoPlayer.Play();
            
            // Esperar un poco para que se active isPlaying
            yield return new WaitForSecondsRealtime(0.2f);
        }

        // 4) Iniciar corrutina para fade a negro a los 46 segundos
        StartCoroutine(FadeAtSpecificTime(46f));

        // 5) Esperar hasta que termine el video
        if (videoPlayer != null)
        {
            while (videoPlayer.isPlaying)
            {
                yield return null;
            }
            Debug.Log("CinematicManager: Video terminado.");
        }

        // 6) Apagar video visual
        if (videoPlayer != null)
            videoPlayer.Stop();

        if (videoContainer != null)
            videoContainer.SetActive(false);

        // 7) Asegurar que estamos completamente en negro
        if (FadeTransition.Instance != null && FadeTransition.Instance.fadeImage != null)
        {
            Color c = FadeTransition.Instance.fadeImage.color;
            c.a = 1f; // Opacidad al 100%
            FadeTransition.Instance.fadeImage.color = c;
            Debug.Log("CinematicManager: Fade image forzado a opacidad 100%");
        }

        // 8) Pequeña espera
        yield return new WaitForSecondsRealtime(0.3f);

        // 9) Cargar la siguiente escena (FadeTransition se auto-destruirá cuando llegue a LoadingScreen)
        Debug.Log($"CinematicManager: Cargando escena {nextSceneAfterCinematic}");
        SceneManager.LoadScene(nextSceneAfterCinematic);

        isPlaying = false;
        Destroy(gameObject);
    }

    IEnumerator FadeAtSpecificTime(float seconds)
    {
        Debug.Log($"CinematicManager: Esperando {seconds} segundos para activar fade a negro...");
        yield return new WaitForSecondsRealtime(seconds);
        
        if (FadeTransition.Instance != null)
        {
            Debug.Log("CinematicManager: ¡Activando fade a negro al 100%!");
            
            // Hacer fade a negro más rápido y asegurar 100% opacidad
            float fadeDuration = 1.5f;
            float elapsed = 0f;
            
            if (FadeTransition.Instance.fadeImage != null)
            {
                Color startColor = FadeTransition.Instance.fadeImage.color;
                
                while (elapsed < fadeDuration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t = Mathf.Clamp01(elapsed / fadeDuration);
                    Color c = FadeTransition.Instance.fadeImage.color;
                    c.a = Mathf.Lerp(startColor.a, 1f, t);
                    FadeTransition.Instance.fadeImage.color = c;
                    yield return null;
                }
                
                // Asegurar 100% al final
                Color finalColor = FadeTransition.Instance.fadeImage.color;
                finalColor.a = 1f;
                FadeTransition.Instance.fadeImage.color = finalColor;
                
                Debug.Log("CinematicManager: Fade completado - Opacidad al 100%");
            }
        }
    }
}
