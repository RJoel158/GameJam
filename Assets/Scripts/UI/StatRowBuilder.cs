using UnityEngine;
using UnityEngine.UI;

// Crea filas StatRow en tiempo de edición/ejecución y las asigna a InventoryHUD.
// Útil si no quieres crear prefabs manualmente.
public class StatRowBuilder : MonoBehaviour
{
    public InventoryHUD inventoryHUD;
    public Font uiFont; // opcional: asigna una fuente desde el inspector

    [ContextMenu("Build StatRows")]
    public void BuildStatRows()
    {
        if (inventoryHUD == null)
        {
            Debug.LogError("StatRowBuilder: inventoryHUD no asignado.");
            return;
        }

        var panel = inventoryHUD.hudPanel;
        if (panel == null)
        {
            Debug.LogError("StatRowBuilder: inventoryHUD.hudPanel no asignado.");
            return;
        }

        // Obtener fuente por defecto si no hay
        if (uiFont == null)
            uiFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

        // Borrar filas anteriores temporales (no destruye assets, sólo hijos creados por este builder)
        var existing = panel.GetComponentsInChildren<StatRow>(true);
        foreach (var e in existing)
        {
            if (Application.isPlaying)
                Destroy(e.gameObject);
            else
                DestroyImmediate(e.gameObject);
        }

        var types = System.Enum.GetValues(typeof(PlayerStats.StatType));
        StatRow[] rows = new StatRow[types.Length];

        for (int i = 0; i < types.Length; i++)
        {
            var t = (PlayerStats.StatType)types.GetValue(i);

            // Crear raiz
            GameObject rowGO = new GameObject(t.ToString() + "Row", typeof(RectTransform));
            rowGO.transform.SetParent(panel.transform, false);

            // Label
            GameObject labelGO = new GameObject("Label", typeof(RectTransform));
            labelGO.transform.SetParent(rowGO.transform, false);
            var label = labelGO.AddComponent<Text>();
            label.font = uiFont;
            label.fontSize = 18;
            label.alignment = TextAnchor.MiddleLeft;
            label.text = t.ToString();

            // Value
            GameObject valueGO = new GameObject("Value", typeof(RectTransform));
            valueGO.transform.SetParent(rowGO.transform, false);
            var value = valueGO.AddComponent<Text>();
            value.font = uiFont;
            value.fontSize = 18;
            value.alignment = TextAnchor.MiddleCenter;
            value.text = "0";

            // Button
            GameObject btnGO = new GameObject("PlusButton", typeof(RectTransform));
            btnGO.transform.SetParent(rowGO.transform, false);
            var image = btnGO.AddComponent<Image>();
            image.color = Color.white;
            var button = btnGO.AddComponent<Button>();

            // Button text
            GameObject btnTextGO = new GameObject("Text", typeof(RectTransform));
            btnTextGO.transform.SetParent(btnGO.transform, false);
            var btnText = btnTextGO.AddComponent<Text>();
            btnText.font = uiFont;
            btnText.fontSize = 20;
            btnText.alignment = TextAnchor.MiddleCenter;
            btnText.text = "+";

            // Añadir StatRow
            var statRow = rowGO.AddComponent<StatRow>();
            statRow.labelText = label;
            statRow.valueText = value;
            statRow.plusButton = button;
            statRow.statType = t;

            // Opcional: ajustar layout básico
            var h = rowGO.AddComponent<HorizontalLayoutGroup>();
            h.childForceExpandHeight = false;
            h.childForceExpandWidth = false;
            h.spacing = 8;

            rows[i] = statRow;
        }

        inventoryHUD.statRows = rows;
        inventoryHUD.UpdateUI();

        Debug.Log($"StatRowBuilder: creadas {rows.Length} filas y asignadas a InventoryHUD.");
    }
}
