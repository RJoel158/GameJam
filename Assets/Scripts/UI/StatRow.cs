using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Componente para una fila de estadística (label, valor, botón +)
// Ahora soporta tanto UnityEngine.UI.Text como TextMeshPro (TMP_Text).
public class StatRow : MonoBehaviour
{
    [Header("Text (Unity UI)")]
    public Text labelText;
    public Text valueText;

    [Header("Text (TextMeshPro)")]
    public TMP_Text labelTMP;
    public TMP_Text valueTMP;

    public Button plusButton;

    [Tooltip("Qué stat representa esta fila")]
    public PlayerStats.StatType statType;

    private PlayerStats playerStats;
    private InventoryHUD parentHud;

    public void Setup(PlayerStats stats, InventoryHUD parent)
    {
        playerStats = stats;
        parentHud = parent;
        if (playerStats != null)
            UpdateRow();

        if (plusButton != null)
        {
            plusButton.onClick.RemoveAllListeners();
            plusButton.onClick.AddListener(() => OnPlusClicked());
        }
    }

    public void UpdateRow()
    {
        if (playerStats == null) return;

        string label = statType.ToString();
        string value = playerStats.GetStat(statType).ToString();

        if (labelText != null) labelText.text = label;
        if (valueText != null) valueText.text = value;
        if (labelTMP != null) labelTMP.text = label;
        if (valueTMP != null) valueTMP.text = value;

        if (plusButton != null)
            plusButton.interactable = playerStats.availablePoints > 0;
    }

    private void OnPlusClicked()
    {
        if (parentHud != null)
            parentHud.RequestIncreaseStat(statType);
    }
}
