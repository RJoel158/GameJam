using UnityEngine;
using UnityEngine.UI;

// Muestra la cantidad de un consumible en la HUD
public class ConsumableUI : MonoBehaviour
{
    public Text countText;
    public int currentCount = 0;

    public void SetCount(int c)
    {
        currentCount = c;
        if (countText != null)
            countText.text = currentCount.ToString();
    }

    // Método de utilidad para sumar/restar
    public void Add(int delta)
    {
        currentCount += delta;
        if (currentCount < 0) currentCount = 0;
        SetCount(currentCount);
    }
}
