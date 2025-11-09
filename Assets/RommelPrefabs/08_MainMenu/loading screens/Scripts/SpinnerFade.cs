using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class SpinnerFade : MonoBehaviour
{
    [Header("Rotation")]
    public float rotateSpeed = 180f; // grados por segundo

    [Header("Fade (smooth)")]
    [Tooltip("Tiempo en segundos que tarda en un ciclo completo (aparecer -> desaparecer -> aparecer)")]
    public float fadeCycleDuration = 1.6f;

    [Range(0.0f, 1.0f)]
    public float minAlpha = 0.05f;   // alpha mínimo cuando casi invisible
    [Range(0.0f, 1.0f)]
    public float maxAlpha = 1.0f;    // alpha máximo

    [Header("Pulse scale (opcional)")]
    public bool usePulse = true;
    public float pulseMin = 0.9f;
    public float pulseMax = 1.05f;
    public float pulseSpeed = 2.0f;

    CanvasGroup cg;
    RectTransform rt;
    float timeOffset;
    Vector3 baseScale; // <<--- escala original

    void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        rt = GetComponent<RectTransform>();
        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
        // Guardamos la escala original del objeto
        baseScale = rt.localScale;

        // Pequeño offset aleatorio para que varios spinners no se sincronicen
        timeOffset = Random.Range(0f, 100f);
    }

    void Update()
    {
        // 1) Rotación
        transform.Rotate(Vector3.forward, -rotateSpeed * Time.deltaTime);

        // 2) Fade suave con seno
        float t = (Time.time + timeOffset) / fadeCycleDuration;
        float tOsc = (Mathf.Sin(t * Mathf.PI * 2f) + 1f) * 0.5f;
        float eased = Mathf.SmoothStep(0f, 1f, tOsc);
        cg.alpha = Mathf.Lerp(minAlpha, maxAlpha, eased);

        // 3) Pulse opcional (sin perder escala base)
        if (usePulse && rt != null)
        {
            float p = (Mathf.Sin((Time.time + timeOffset) * pulseSpeed) + 1f) * 0.5f;
            float s = Mathf.Lerp(pulseMin, pulseMax, p);
            rt.localScale = baseScale * s; // <<--- multiplicar por la escala base
        }
    }
}
