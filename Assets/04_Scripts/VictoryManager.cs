using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Collections;

public class VictoryManager : MonoBehaviour
{
    public static VictoryManager Instance;

    [Header("Video Settings")]
    public string videoPath = "Assets/00_Scenes/CinematicaFinal.mp4";
    public VideoPlayer videoPlayer;
    public RawImage videoDisplay;

    [Header("Victory Screen")]
    public Sprite youWinSprite;
    public Image youWinImage;
    public Image youWinBackground;
    public float youWinFadeDuration = 1.5f;
    public float youWinDisplayTime = 3f;

    [Header("Audio")]
    public AudioClip victoryMusic;
    [Range(0f, 1f)]
    public float victoryMusicVolume = 0.8f;

    [Header("Scene")]
    public string mainMenuSceneName = "Game";

    private bool isPlaying = false;
    private AudioSource audioSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Auto-setup VideoPlayer
        if (videoPlayer == null)
        {
            videoPlayer = gameObject.AddComponent<VideoPlayer>();
            videoPlayer.playOnAwake = false;
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
        }

        // Auto-create UI if missing
        SetupUIElements();

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    private void SetupUIElements()
    {
        // Find or create Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("VictoryCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }

        // Create VideoDisplay if missing
        if (videoDisplay == null)
        {
            GameObject videoDisplayObj = new GameObject("VideoDisplay");
            videoDisplayObj.transform.SetParent(canvas.transform, false);
            videoDisplay = videoDisplayObj.AddComponent<RawImage>();

            RectTransform rt = videoDisplay.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            videoDisplay.color = Color.white;
            videoDisplayObj.SetActive(false);
        }

        // Create YouWin panel with background and image
        if (youWinImage == null || youWinBackground == null)
        {
            GameObject youWinPanel = new GameObject("YouWinPanel");
            youWinPanel.transform.SetParent(canvas.transform, false);

            // Background
            youWinBackground = youWinPanel.AddComponent<Image>();
            RectTransform bgRt = youWinBackground.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;
            youWinBackground.color = new Color(0, 0, 0, 0);

            // Image
            GameObject imageObj = new GameObject("YouWinImage");
            imageObj.transform.SetParent(youWinPanel.transform, false);
            youWinImage = imageObj.AddComponent<Image>();

            RectTransform imageRt = youWinImage.GetComponent<RectTransform>();
            imageRt.anchorMin = new Vector2(0.5f, 0.5f);
            imageRt.anchorMax = new Vector2(0.5f, 0.5f);
            imageRt.sizeDelta = new Vector2(800, 400);
            imageRt.anchoredPosition = Vector2.zero;

            youWinImage.color = new Color(1, 1, 1, 0);
            youWinImage.preserveAspect = true;

            youWinPanel.SetActive(false);
        }

        // Asignar sprite si fue configurado
        if (youWinSprite != null && youWinImage != null)
        {
            youWinImage.sprite = youWinSprite;
            Debug.Log($"[VictoryManager] Sprite '{youWinSprite.name}' asignado a youWinImage");
        }
        else if (youWinSprite != null)
        {
            Debug.LogWarning("[VictoryManager] youWinSprite configurado pero youWinImage es null!");
        }
        else
        {
            Debug.LogWarning("[VictoryManager] No hay youWinSprite asignado - se verá imagen en blanco");
        }
    }

    public void TriggerVictory()
    {
        if (isPlaying)
        {
            Debug.LogWarning("Victoria ya en progreso");
            return;
        }

        isPlaying = true;
        Debug.Log("¡VICTORIA! Iniciando secuencia...");

        StartCoroutine(VictorySequence());
    }

    private IEnumerator VictorySequence()
    {
        Time.timeScale = 1f;

        yield return StartCoroutine(PlayVictoryVideo());
        yield return StartCoroutine(ShowYouWinScreen());

        LoadMainMenu();
    }

    private IEnumerator PlayVictoryVideo()
    {
        Debug.Log("Reproduciendo video de cinemática final...");

        if (videoPlayer == null)
        {
            Debug.LogError("VideoPlayer no asignado!");
            yield break;
        }

        videoPlayer.url = System.IO.Path.GetFullPath(videoPath);

        if (videoDisplay != null)
        {
            RenderTexture renderTexture = new RenderTexture(1920, 1080, 24);
            videoPlayer.targetTexture = renderTexture;
            videoDisplay.texture = renderTexture;
            videoDisplay.gameObject.SetActive(true);
        }

        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }

        videoPlayer.Play();
        Debug.Log("Video iniciado");

        while (videoPlayer.isPlaying)
        {
            yield return null;
        }

        Debug.Log("Video terminado");

        if (videoDisplay != null)
        {
            videoDisplay.gameObject.SetActive(false);
        }
    }

    private IEnumerator ShowYouWinScreen()
    {
        Debug.Log("Mostrando pantalla YOU WIN");

        if (youWinImage == null || youWinBackground == null)
        {
            Debug.LogError("[VictoryManager] YOU WIN elements no asignados! youWinImage=" + (youWinImage != null) + ", youWinBackground=" + (youWinBackground != null));
            yield return new WaitForSeconds(2f);
            yield break;
        }

        Debug.Log($"[VictoryManager] Activando panel YOU WIN. Sprite asignado: {youWinImage.sprite != null}");

        // Activar panel
        youWinBackground.transform.parent.gameObject.SetActive(true);

        Debug.Log($"[VictoryManager] Panel activado. GameObject activo: {youWinBackground.transform.parent.gameObject.activeSelf}");

        // Reproducir música de victoria si está asignada
        if (victoryMusic != null)
        {
            audioSource.clip = victoryMusic;
            audioSource.volume = victoryMusicVolume;
            audioSource.Play();
        }

        // Fade in background y image
        float elapsed = 0f;
        while (elapsed < youWinFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(0f, 0.8f, elapsed / youWinFadeDuration);

            Color bgColor = youWinBackground.color;
            bgColor.a = alpha;
            youWinBackground.color = bgColor;

            // Fade in image
            Color imageColor = youWinImage.color;
            imageColor.a = Mathf.Lerp(0f, 1f, elapsed / youWinFadeDuration);
            youWinImage.color = imageColor;

            yield return null;
        }

        // Asegurar opacidad completa
        Color finalBg = youWinBackground.color;
        finalBg.a = 0.8f;
        youWinBackground.color = finalBg;

        Color finalImage = youWinImage.color;
        finalImage.a = 1f;
        youWinImage.color = finalImage;

        Debug.Log($"[VictoryManager] YOU WIN completamente visible. Color final alpha: {youWinImage.color.a}");
        Debug.Log("YOU WIN visible por " + youWinDisplayTime + " segundos");
        yield return new WaitForSecondsRealtime(youWinDisplayTime);
    }

    private void LoadMainMenu()
    {
        Debug.Log("Cargando menú principal: " + mainMenuSceneName);
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
