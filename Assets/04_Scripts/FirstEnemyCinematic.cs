using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

// Attach to a GameObject in the scene (e.g. GameManager) and assign a VideoClip.
// On the first Enemy.OnEnemyDefeated event this script will:
//  - show a fullscreen UI with the video
//  - pause gameplay (Time.timeScale = 0) and disable player controller
//  - play the video and wait until it finishes
//  - hide the UI and resume gameplay
public class FirstEnemyCinematic : MonoBehaviour
{
    [Tooltip("Video clip to play when the first enemy is defeated")]
    public VideoClip clip;

    [Tooltip("Optional: Player root/object. If set, this script will disable the component of type `ThirdPersonController` during the cinematic.")]
    public GameObject player;

    [Tooltip("If set, this GameObject's AudioListener will be disabled while the cinematic plays (to prevent audio doubling).")]
    public AudioListener optionalAudioListenerToDisable;

    [Tooltip("If true the cinematic will mute the global AudioListener (if present)")]
    public bool muteGlobalAudioListener = false;
    [Tooltip("Tiempo (segundos, sin escalado) para las transiciones de fade in/out")]
    public float fadeDuration = 0.6f;
    [Tooltip("Permitir saltar el cinematic con una tecla")]
    public bool allowSkip = true;
    [Tooltip("Tecla para saltar el cinematic si `allowSkip` es true")]
    public KeyCode skipKey = KeyCode.Space;

    bool played = false;

    Canvas cinematicCanvas;
    RawImage rawImage;
    VideoPlayer videoPlayer;
    AudioSource videoAudioSource;
    CanvasGroup panelCanvasGroup;
    CanvasGroup videoCanvasGroup;
    float prevAudioListenerVolume = 1f;

    void OnEnable()
    {
        Enemy.OnEnemyDefeated += OnEnemyDefeated;
    }

    void OnDisable()
    {
        Enemy.OnEnemyDefeated -= OnEnemyDefeated;
    }

    void OnEnemyDefeated(Vector3 pos)
    {
        if (played) return; // only once
        played = true;
        StartCoroutine(PlayCinematicRoutine());
    }

    IEnumerator PlayCinematicRoutine()
    {
        if (clip == null)
        {
            Debug.LogWarning("FirstEnemyCinematic: No VideoClip assigned. Skipping cinematic.");
            yield break;
        }

        // Build UI
        BuildCinematicUI();

        // Pause game logic (we'll set Time.timeScale after the scene fade)
        float prevTimeScale = Time.timeScale;

        // Disable player controller if provided
        MonoBehaviour playerController = null;
        if (player != null)
        {
            // try to find ThirdPersonController on player
            playerController = player.GetComponent<MonoBehaviour>();
            if (playerController != null)
            {
                playerController.enabled = false;
            }
        }

        // Optionally mute AudioListener
        AudioListener globalListener = null;
        if (muteGlobalAudioListener)
        {
            globalListener = FindObjectOfType<AudioListener>();
        }
        if (optionalAudioListenerToDisable != null)
        {
            optionalAudioListenerToDisable.enabled = false;
        }

        // Record previous global volume and fade it down while fading to black
        prevAudioListenerVolume = AudioListener.volume;
        float targetAmbientVolume = 0f; // silenciar ambient durante cinematic

        // Fade panel to black and lower global audio (unscaled time)
        float t = 0f;
        panelCanvasGroup.alpha = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float f = Mathf.Clamp01(t / fadeDuration);
            panelCanvasGroup.alpha = Mathf.Lerp(0f, 1f, f);
            AudioListener.volume = Mathf.Lerp(prevAudioListenerVolume, targetAmbientVolume, f);
            yield return null;
        }
        panelCanvasGroup.alpha = 1f;
        AudioListener.volume = targetAmbientVolume;

        // Now pause gameplay
        Time.timeScale = 0f;

        // Prepare VideoPlayer
        videoPlayer.clip = clip;
        videoPlayer.isLooping = false;
        videoPlayer.playOnAwake = false;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.SetTargetAudioSource(0, videoAudioSource);

        // Prepare render texture
        RenderTexture rt = new RenderTexture(1024, 576, 0);
        rt.Create();
        videoPlayer.targetTexture = rt;
        rawImage.texture = rt;
        // ensure video canvas group starts transparent and audio at 0
        videoCanvasGroup.alpha = 0f;
        videoAudioSource.volume = 0f;

        // Prepare and play
        videoPlayer.Prepare();
        while (!videoPlayer.isPrepared)
        {
            yield return null; // wait until prepared (unscaled)
        }


        videoPlayer.Play();
        videoAudioSource.Play();

        // Fade in video visual + video audio
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float f = Mathf.Clamp01(t / fadeDuration);
            videoCanvasGroup.alpha = Mathf.Lerp(0f, 1f, f);
            videoAudioSource.volume = Mathf.Lerp(0f, 1f, f);
            // allow skip
            if (allowSkip && Input.GetKeyDown(skipKey)) break;
            yield return null;
        }
        videoCanvasGroup.alpha = 1f;
        videoAudioSource.volume = 1f;

        // Wait until the clip finishes or user skips
        bool finished = false;
        videoPlayer.loopPointReached += (vp) => finished = true;

        while (!finished)
        {
            if (allowSkip && Input.GetKeyDown(skipKey)) break;
            yield return null;
        }

        // Fade out video visual + audio
        t = 0f;
        float endFade = Mathf.Min(fadeDuration, 0.8f);
        while (t < endFade)
        {
            t += Time.unscaledDeltaTime;
            float f = Mathf.Clamp01(t / endFade);
            videoCanvasGroup.alpha = Mathf.Lerp(1f, 0f, f);
            videoAudioSource.volume = Mathf.Lerp(1f, 0f, f);
            yield return null;
        }

        videoPlayer.Stop();
        videoAudioSource.Stop();
        videoPlayer.targetTexture = null;
        rawImage.texture = null;
        Destroy(rt);

        // Fade panel back to full then fade out both panel and restore ambient audio
        t = 0f;
        float restoreDuration = fadeDuration;
        while (t < restoreDuration)
        {
            t += Time.unscaledDeltaTime;
            float f = Mathf.Clamp01(t / restoreDuration);
            panelCanvasGroup.alpha = Mathf.Lerp(1f, 0f, f);
            AudioListener.volume = Mathf.Lerp(0f, prevAudioListenerVolume, f);
            yield return null;
        }
        panelCanvasGroup.alpha = 0f;
        AudioListener.volume = prevAudioListenerVolume;

        // Restore audio listener(s)
        if (muteGlobalAudioListener && globalListener != null)
        {
            // previously we didn't disable globalListener, just adjusted AudioListener.volume
        }
        if (optionalAudioListenerToDisable != null)
        {
            optionalAudioListenerToDisable.enabled = true;
        }

        // Restore player controller
        if (playerController != null)
        {
            playerController.enabled = true;
        }

        // Hide and destroy cinematic UI
        if (cinematicCanvas != null)
        {
            Destroy(cinematicCanvas.gameObject);
        }

        Time.timeScale = prevTimeScale;
    }

    void BuildCinematicUI()
    {
        // Create Canvas
        GameObject canvasGO = new GameObject("CinematicCanvas");
        cinematicCanvas = canvasGO.AddComponent<Canvas>();
        cinematicCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler cs = canvasGO.AddComponent<CanvasScaler>();
        cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.AddComponent<GraphicRaycaster>();

        // Fullscreen panel
        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(canvasGO.transform, false);
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = Color.black;
        RectTransform panelRT = panel.GetComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;
        // add CanvasGroup for fade control
        panelCanvasGroup = panel.AddComponent<CanvasGroup>();

        // RawImage for video
        GameObject rawGO = new GameObject("VideoRaw");
        rawGO.transform.SetParent(panel.transform, false);
        rawImage = rawGO.AddComponent<RawImage>();
        RectTransform rt = rawGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(Screen.width * 0.8f, Screen.height * 0.8f);
        // canvas group for the video area so we can fade it independently
        videoCanvasGroup = rawGO.AddComponent<CanvasGroup>();

        // Add VideoPlayer and AudioSource components to canvas GO
        videoPlayer = canvasGO.AddComponent<VideoPlayer>();
        videoAudioSource = canvasGO.AddComponent<AudioSource>();
        videoAudioSource.playOnAwake = false;
        videoAudioSource.spatialBlend = 0f; // 2D

        // Bring canvas to front
        canvasGO.transform.SetAsLastSibling();
    }
}
