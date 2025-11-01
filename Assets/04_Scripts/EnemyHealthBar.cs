using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MonoBehaviour))]
public class EnemyHealthBar : MonoBehaviour
{
    [Tooltip("Offset in local space where the bar will be placed (y is vertical)")]
    public Vector3 localOffset = new Vector3(0f, 2f, 0f);

    [Tooltip("Width of the full bar in world units")]
    public float barWidth = 1.2f;
    [Tooltip("Height of the bar rect in world units")]
    public float barHeight = 0.12f;

    [Tooltip("Background color of the bar")]
    public Color backgroundColor = new Color(0f, 0f, 0f, 0.6f);
    [Tooltip("Fill color of the health portion")]
    public Color fillColor = new Color(0.8f, 0.1f, 0.1f, 1f);

    // internal
    Canvas worldCanvas;
    RectTransform bgRect;
    RectTransform fillRect;
    Enemy enemy;
    int maxHealth;

    void Start()
    {
        enemy = GetComponent<Enemy>();
        if (enemy == null)
        {
            Debug.LogWarning("EnemyHealthBar: no Enemy component found on " + gameObject.name + ". Disabling.");
            enabled = false;
            return;
        }

        // determine max health (if there's no explicit max, use initial health)
        maxHealth = Mathf.Max(1, enemy.health);

        CreateWorldSpaceBar();
        UpdateFillImmediate();
    }

    void CreateWorldSpaceBar()
    {
        // Parent an empty GameObject to this enemy
        GameObject canvasGO = new GameObject("HealthCanvas");
        canvasGO.transform.SetParent(transform);
        canvasGO.transform.localPosition = localOffset;
        canvasGO.transform.localRotation = Quaternion.identity;
        canvasGO.transform.localScale = Vector3.one * 0.01f; // scale down so that UI sizes in px look reasonable

        worldCanvas = canvasGO.AddComponent<Canvas>();
        worldCanvas.renderMode = RenderMode.WorldSpace;
        worldCanvas.worldCamera = Camera.main; // optional; allows proper sorting

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10;

        // Background
        GameObject bg = new GameObject("BG");
        bg.transform.SetParent(canvasGO.transform, false);
        bgRect = bg.AddComponent<RectTransform>();
        bgRect.sizeDelta = new Vector2(barWidth * 100f, barHeight * 100f);
        Image bgImage = bg.AddComponent<Image>();
        bgImage.color = backgroundColor;

        // Fill
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(bg.transform, false);
        fillRect = fill.AddComponent<RectTransform>();
        // anchor to left so scaling looks natural
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(0f, 1f);
        fillRect.pivot = new Vector2(0f, 0.5f);
        fillRect.anchoredPosition = new Vector2(0f, 0f);
        fillRect.sizeDelta = new Vector2(barWidth * 100f, 0f);
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = fillColor;
    }

    void LateUpdate()
    {
        if (enemy == null) return;

        // Face camera
        Camera cam = Camera.main;
        if (cam != null && worldCanvas != null)
        {
            worldCanvas.transform.rotation = Quaternion.LookRotation(worldCanvas.transform.position - cam.transform.position);
        }

        UpdateFillImmediate();

        // hide when dead
        if (enemy.health <= 0 && worldCanvas != null)
        {
            worldCanvas.gameObject.SetActive(false);
        }
    }

    void UpdateFillImmediate()
    {
        if (fillRect == null || bgRect == null || enemy == null) return;

        float percent = Mathf.Clamp01((float)enemy.health / (float)maxHealth);

        // update width of fill rect
        float fullWidth = barWidth * 100f;
        fillRect.sizeDelta = new Vector2(fullWidth * percent, 0f);
    }
}
