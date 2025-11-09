using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FadeTransition : MonoBehaviour
{
    public static FadeTransition Instance { get; private set; }

    [Header("References")]
    public Image fadeImage; // Fullscreen black Image (alpha 0..1)
    public float fadeDuration = 1.0f;
    public bool blockInputDuringFade = true;

    CanvasGroup canvasGroup;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (fadeImage == null)
            fadeImage = GetComponentInChildren<Image>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (fadeImage != null)
        {
            var c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
        }
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        
        // Suscribirse a cambios de escena para auto-destruirse
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Si llegamos a LoadingScreen, esperar 1 segundo y luego hacer fade out y destruirse
        if (scene.name == "LoadingScreen_1")
        {
            StartCoroutine(FadeOutAndDestroy());
        }
    }

    IEnumerator FadeOutAndDestroy()
    {
        Debug.Log("FadeTransition: En LoadingScreen, esperando 1 segundo antes de fade out...");
        
        // Esperar 1 segundo con el fade negro
        yield return new WaitForSecondsRealtime(1f);
        
        // Hacer fade a transparente rápidamente
        Debug.Log("FadeTransition: Haciendo fade out...");
        yield return FadeToTransparent(0.5f);
        
        // Destruirse
        Debug.Log("FadeTransition: Destruyendo...");
        Destroy(gameObject);
    }

    /// <summary>Fade to given scene (fades to black, loads, stays black).</summary>
    public void FadeToScene(string sceneName)
    {
        StartCoroutine(FadeAndLoad(sceneName));
    }

    public IEnumerator FadeAndLoad(string sceneName)
    {
        if (blockInputDuringFade)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        // Fade to black
        if (fadeImage != null)
        {
            float elapsed = 0f;
            Color start = fadeImage.color;
            start.a = Mathf.Clamp01(start.a);
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                Color c = fadeImage.color;
                c.a = Mathf.Lerp(start.a, 1f, t);
                fadeImage.color = c;
                yield return null;
            }
            Color end = fadeImage.color; end.a = 1f; fadeImage.color = end;
        }
        else
        {
            yield return null;
        }

        // Load the scene
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        while (!op.isDone)
            yield return null;

        // destroy fade if not MainMenu
        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            Destroy(gameObject);
        }
    }

    /// <summary>Fade to black (no scene load) — usable externally as IEnumerator.</summary>
    public IEnumerator FadeToBlack(float durationOverride = -1f)
    {
        if (blockInputDuringFade)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        float dur = (durationOverride > 0f) ? durationOverride : fadeDuration;
        if (fadeImage == null) yield break;

        float elapsed = 0f;
        Color start = fadeImage.color;
        start.a = Mathf.Clamp01(start.a);
        while (elapsed < dur)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / dur);
            Color c = fadeImage.color;
            c.a = Mathf.Lerp(start.a, 1f, t);
            fadeImage.color = c;
            yield return null;
        }
        Color end = fadeImage.color; end.a = 1f; fadeImage.color = end;
    }

    /// <summary>Fade from black to transparent (no scene load) — ya lo tenías como FadeFromBlack.</summary>
    public IEnumerator FadeToTransparent(float durationOverride = -1f)
    {
        float dur = (durationOverride > 0f) ? durationOverride : fadeDuration;
        if (fadeImage == null) yield break;

        float elapsed = 0f;
        Color start = fadeImage.color;
        start.a = Mathf.Clamp01(start.a);
        while (elapsed < dur)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / dur);
            Color c = fadeImage.color;
            c.a = Mathf.Lerp(start.a, 0f, t);
            fadeImage.color = c;
            yield return null;
        }
        Color end = fadeImage.color; end.a = 0f; fadeImage.color = end;

        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }
}
