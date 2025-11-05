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

        // If no fadeImage assigned, try to find one on children
        if (fadeImage == null)
            fadeImage = GetComponentInChildren<Image>();

        // make sure there's an (optional) CanvasGroup to block input
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // init as invisible and non-blocking
        if (fadeImage != null)
        {
            var c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
        }
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

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

        // 🔹 Destruir el fade una vez que ya cambió de escena
        // (solo si ya no estamos en el MainMenu)
        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            Destroy(gameObject);
        }
    }

    public IEnumerator FadeFromBlack(float durationOverride = -1f)
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
