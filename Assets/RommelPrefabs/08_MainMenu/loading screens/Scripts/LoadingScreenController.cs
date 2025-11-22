
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class LoadingScreenController : MonoBehaviour
{
    [Header("Scene Settings")]
    public string sceneToLoad = "Game";  // nombre de la escena a cargar

    [Header("Fade Settings")]
    public float fadeDuration = 0.5f;           // duración del fade (negro)
    public float loadingDisplayTime = 3f;     // tiempo mínimo que se muestra la pantalla de carga

    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f; // empieza completamente negro
    }

    void Start()
    {
        StartCoroutine(LoadSequence());
    }

    IEnumerator LoadSequence()
    {
        // 1️⃣ Fade desde negro a visible (aparece la pantalla de carga)
        yield return StartCoroutine(FadeCanvas(1f, 0f, fadeDuration));

        // 2️⃣ Cargar la escena en segundo plano
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad);
        asyncLoad.allowSceneActivation = false;

        float timer = 0f;
        while (asyncLoad.progress < 0.9f || timer < loadingDisplayTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        // 3️⃣ Fade a negro antes de activar la escena cargada
        yield return StartCoroutine(FadeCanvas(0f, 1f, fadeDuration));

        // 4️⃣ Activar la escena cuando el fade negro esté completo
        asyncLoad.allowSceneActivation = true;
    }

    IEnumerator FadeCanvas(float start, float end, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(start, end, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = end;
    }
}
