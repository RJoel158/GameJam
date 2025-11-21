using System.Collections;
using UnityEngine;

/// <summary>
/// Reproduce partículas y anima (fade/scale) el child del campamento llamado "Marquer"
/// cuando el campamento se limpia (EnemyCamp.OnCampCleared).
/// Coloca este script en un GameObject manager en la escena y asigna el prefab de partículas.
/// </summary>
public class CampClearFX : MonoBehaviour
{
    [Tooltip("Prefab de ParticleSystem que se reproducirá en la posición del campamento")]
    public ParticleSystem clearParticlesPrefab;

    [Tooltip("Nombre exacto del child que actúa como marcador dentro del campamento (por defecto 'Marquer')")]
    public string markerChildName = "Marquer";

    [Tooltip("Duración total de la animación de desaparición del marcador (segundos)")]
    public float markerFadeDuration = 1.0f;

    [Tooltip("Delay antes de destruir el campamento (da tiempo para las partículas)")]
    public float destroyDelay = 1.1f;

    [Tooltip("Si true, el script destruirá el GameObject del campamento al terminar la animación")]
    public bool destroyCampOnClear = true;

    void OnEnable()
    {
        EnemyCamp.OnCampCleared += HandleCampCleared;
    }

    void OnDisable()
    {
        EnemyCamp.OnCampCleared -= HandleCampCleared;
    }

    void HandleCampCleared(EnemyCamp camp)
    {
        if (camp == null) return;
        StartCoroutine(PlayClearEffect(camp));
    }

    IEnumerator PlayClearEffect(EnemyCamp camp)
    {
        // 1) instantiate particles
        if (clearParticlesPrefab != null)
        {
            var ps = Instantiate(clearParticlesPrefab, camp.transform.position, Quaternion.identity);
            ps.Play();
            // destroy particle gameobject after its duration
            Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax + 0.5f);
        }

        // 2) find marker child
        Transform marker = camp.transform.Find(markerChildName);
        if (marker != null)
        {
            // try several fade strategies: CanvasGroup (UI), Image/Text, SpriteRenderer, or generic scale
            CanvasGroup cg = marker.GetComponent<CanvasGroup>();
            UnityEngine.UI.Graphic uiGraphic = marker.GetComponent<UnityEngine.UI.Graphic>();
            SpriteRenderer sr = marker.GetComponent<SpriteRenderer>();

            float t = 0f;
            Vector3 startScale = marker.localScale;
            Vector3 targetScale = Vector3.zero;

            // if UI has CanvasGroup or Graphic, ensure we can fade
            if (cg == null && uiGraphic != null)
            {
                // add temporary CanvasGroup if needed
                cg = marker.gameObject.AddComponent<CanvasGroup>();
            }

            // If neither cg nor sr nor uiGraphic exist, we will scale the transform

            while (t < markerFadeDuration)
            {
                t += Time.deltaTime;
                float alpha = 1f - Mathf.Clamp01(t / markerFadeDuration);

                // scale down
                marker.localScale = Vector3.Lerp(startScale, targetScale, t / markerFadeDuration);

                if (cg != null)
                {
                    cg.alpha = alpha;
                }
                else if (uiGraphic != null)
                {
                    var c = uiGraphic.color;
                    c.a = alpha;
                    uiGraphic.color = c;
                }
                else if (sr != null)
                {
                    var c = sr.color;
                    c.a = alpha;
                    sr.color = c;
                }

                yield return null;
            }

            // after loop ensure invisible
            marker.localScale = targetScale;
            if (cg != null) cg.alpha = 0f;
            if (uiGraphic != null) { var c = uiGraphic.color; c.a = 0f; uiGraphic.color = c; }
            if (sr != null) { var c = sr.color; c.a = 0f; sr.color = c; }

            // optionally destroy the marker object
            Destroy(marker.gameObject, 0.01f);
        }

        // wait a bit to let particles finish
        yield return new WaitForSeconds(destroyDelay);

        // destroy camp root if desired
        if (destroyCampOnClear && camp != null)
        {
            Destroy(camp.gameObject);
        }
    }
}
