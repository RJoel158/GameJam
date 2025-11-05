using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class LoadingScreenManager : MonoBehaviour
{
    [Header("Scene Settings")]
    public string sceneToLoad = "OpenWorldSceneMerged"; // cambia por tu escena
    [Header("Fade References")]
    public Image fadeImage;                  // ← arrastra tu FadeImage aquí
    public CanvasGroup canvasGroup;          // ← el CanvasGroup de LoadingElements
    [Header("Durations")]
    public float fadeDuration = 1.2f;
    public float minLoadingTime = 2.5f;

    void Start()
    {
        StartCoroutine(LoadingSequence());
    }

    IEnumerator LoadingSequence()
    {
        // 1️⃣ Asegurar que empiece totalmente negro
        fadeImage.color = new Color(0, 0, 0, 1);
        canvasGroup.alpha = 1f;

        // 2️⃣ Fade del negro hacia la pantalla de carga
        yield return StartCoroutine(FadeImageAlpha(1, 0));

        // 3️⃣ Iniciar carga asíncrona
        AsyncOperation async = SceneManager.LoadSceneAsync("OpenWorldSceneMerged");
        async.allowSceneActivation = false;

        float timer = 0f;
        while (async.progress < 0.9f || timer < minLoadingTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        // 4️⃣ Fade hacia negro antes de activar la escena
        yield return StartCoroutine(FadeImageAlpha(0, 1));

        // 5️⃣ Activar escena
        async.allowSceneActivation = true;
    }

    IEnumerator FadeImageAlpha(float start, float end)
    {
        float elapsed = 0f;
        Color color = fadeImage.color;

        while (elapsed < fadeDuration)
        {
            float a = Mathf.Lerp(start, end, elapsed / fadeDuration);
            color.a = a;
            fadeImage.color = color;
            elapsed += Time.deltaTime;
            yield return null;
        }

        color.a = end;
        fadeImage.color = color;
    }
}
