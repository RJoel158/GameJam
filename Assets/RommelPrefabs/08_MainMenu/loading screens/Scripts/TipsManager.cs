using UnityEngine;
using TMPro;
using System.Collections;

public class TipsManager : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI tipText; // ← TextMeshProUGUI, no Text

    [Header("Tips")]
    [TextArea(3,10)]
    public string[] tips;

    [Header("Timing")]
    public float tipChangeInterval = 6f;
    public float textFadeDuration = 0.6f;

    CanvasGroup cg;
    int lastIndex = -1;

    void Awake()
    {
        if (tipText == null)
            Debug.LogError("TipsManager: asigna el campo tipText en el Inspector.");

        cg = tipText.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = tipText.gameObject.AddComponent<CanvasGroup>();
    }

    void Start()
    {
        if (tips == null || tips.Length == 0)
        {
            tipText.text = "";
            return;
        }
        StartCoroutine(TipCycleCoroutine());
    }

    IEnumerator TipCycleCoroutine()
    {
        yield return null;

        while (true)
        {
            // Elegir un tip aleatorio, evitando repetir el último
            int idx = Random.Range(0, tips.Length);
            if (tips.Length > 1)
            {
                int tries = 0;
                while (idx == lastIndex && tries < 6)
                {
                    idx = Random.Range(0, tips.Length);
                    tries++;
                }
            }
            lastIndex = idx;

            // Fade out
            yield return StartCoroutine(FadeText(0f, textFadeDuration * 0.25f));

            // Cambiar texto
            tipText.text = tips[idx];

            // Fade in
            yield return StartCoroutine(FadeText(1f, textFadeDuration));

            // Esperar antes de cambiar al siguiente
            float holdTime = Mathf.Max(0.1f, tipChangeInterval - textFadeDuration * 0.5f);
            yield return new WaitForSeconds(holdTime);
        }
    }

    IEnumerator FadeText(float targetAlpha, float duration)
    {
        if (cg == null) yield break;

        float start = cg.alpha;
        float elapsed = 0f;
        if (duration <= 0f)
        {
            cg.alpha = targetAlpha;
            yield break;
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            cg.alpha = Mathf.Lerp(start, targetAlpha, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        cg.alpha = targetAlpha;
    }
}
