using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class LoadingScreenManager : MonoBehaviour
{
    [Header("Scene Settings")]
    public string sceneToLoad = "Game"; // cambia por tu escena
    [Header("Fade References")]
    public Image fadeImage;                  // ← arrastra tu FadeImage aquí
    public CanvasGroup canvasGroup;          // ← el CanvasGroup de LoadingElements
    [Header("Durations")]
    public float fadeDuration = 1.2f;
    public float minLoadingTime = 2.5f;

    void Start()
    {
        // Verificar que las referencias esten asignadas
        if (fadeImage == null)
        {
            Debug.LogError("[LoadingScreen] fadeImage no esta asignado!");
            return;
        }

        if (canvasGroup == null)
        {
            Debug.LogError("[LoadingScreen] canvasGroup no esta asignado!");
            return;
        }

        // Verificar que haya escenas en Build Settings
        if (SceneManager.sceneCountInBuildSettings == 0)
        {
            Debug.LogError("[LoadingScreen] NO HAY ESCENAS en Build Settings!");
            Debug.LogError("[LoadingScreen] Ve a File -> Build Settings y agrega la escena");
            ListAvailableScenes();
            return;
        }

        Debug.Log($"[LoadingScreen] Iniciando carga de escena: {sceneToLoad}");
        StartCoroutine(LoadingSequence());
    }

    void ListAvailableScenes()
    {
        Debug.Log("<color=cyan>========================================</color>");
        Debug.Log("<color=cyan>ESCENAS EN BUILD SETTINGS:</color>");

        int sceneCount = SceneManager.sceneCountInBuildSettings;

        if (sceneCount == 0)
        {
            Debug.LogWarning("<color=yellow>NO HAY ESCENAS en Build Settings!</color>");
            Debug.LogWarning("<color=yellow>Ve a File -> Build Settings y agrega OpenWorldSceneMerged1</color>");
        }
        else
        {
            Debug.Log($"<color=green>Total de escenas: {sceneCount}</color>");
            Debug.Log("<color=yellow>Tip: Ve a File -> Build Settings para ver la lista completa</color>");
        }

        Debug.Log("<color=cyan>========================================</color>");
    }

    IEnumerator LoadingSequence()
    {
        Debug.Log($"[LoadingScreen] Iniciando carga de escena: {sceneToLoad}");

        // 1️⃣ Asegurar que empiece totalmente negro
        if (fadeImage != null)
        {
            fadeImage.color = new Color(0, 0, 0, 1);
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }

        // 2️⃣ Fade del negro hacia la pantalla de carga
        yield return StartCoroutine(FadeImageAlpha(1, 0));

        // 3️⃣ Iniciar carga asíncrona
        AsyncOperation async = null;

        try
        {
            async = SceneManager.LoadSceneAsync(sceneToLoad);

            if (async == null)
            {
                Debug.LogError($"[LoadingScreen] No se pudo cargar la escena '{sceneToLoad}'");
                yield break;
            }

            async.allowSceneActivation = false;
            Debug.Log("[LoadingScreen] Carga asincrona iniciada...");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[LoadingScreen] Error al cargar escena: {e.Message}");
            yield break;
        }

        float timer = 0f;
        while (async.progress < 0.9f || timer < minLoadingTime)
        {
            timer += Time.deltaTime;

            // Mostrar progreso
            float progress = Mathf.Clamp01(async.progress / 0.9f);
            if (timer % 0.5f < 0.1f) // Log cada medio segundo
            {
                Debug.Log($"[LoadingScreen] Progreso: {progress * 100f:F0}%");
            }

            yield return null;
        }

        Debug.Log("[LoadingScreen] Carga completada! Activando escena...");

        // 4️⃣ Fade hacia negro antes de activar la escena
        yield return StartCoroutine(FadeImageAlpha(0, 1));

        // 5️⃣ Activar escena
        async.allowSceneActivation = true;

        Debug.Log("[LoadingScreen] Escena activada!");
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
