using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(TMP_Text))]
public class UnderlineOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private TMP_Text tmproText;
    private Color originalColor;
    private Vector3 originalScale;
    
    [Header("Hover Settings")]
    public Color hoverColor = new Color(1f, 0.8f, 0.3f); // Dorado suave
    public float hoverScale = 1.1f; // 5% más grande
    public float transitionSpeed = 8f; // Velocidad de animación

    private bool isHovered = false;

    void Awake()
    {
        tmproText = GetComponent<TMP_Text>();
        originalColor = tmproText.color;
        originalScale = transform.localScale;
    }

    void Update()
    {
        // Animación suave (lerp) entre estados
        Color targetColor = isHovered ? hoverColor : originalColor;
        Vector3 targetScale = isHovered ? originalScale * hoverScale : originalScale;

        tmproText.color = Color.Lerp(tmproText.color, targetColor, Time.deltaTime * transitionSpeed);
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * transitionSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        // reproducir SFX de hover (si existe)
        if (UIAudioManager.Instance != null)
            UIAudioManager.Instance.PlayHover();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
    }
}
