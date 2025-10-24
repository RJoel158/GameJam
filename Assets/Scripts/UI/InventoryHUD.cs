using UnityEngine;
using UnityEngine.UI;

// Controla la ventana HUD de inventario/estadísticas.
public class InventoryHUD : MonoBehaviour
{
    [Header("References")]
    public GameObject hudPanel; // panel raíz que se muestra/oculta
    public Text nameText;
    public Text levelText;
    public Text pointsText;

    [Header("Stat Rows")]
    public StatRow[] statRows; // asignar 3 filas en inspector

    [Header("Player Stats")]
    public PlayerStats playerStats;

    private void Awake()
    {
        if (hudPanel != null) hudPanel.SetActive(false);

        // Auto-populate statRows if none assigned (conveniencia en la escena)
        if (statRows == null || statRows.Length == 0)
        {
            statRows = GetComponentsInChildren<StatRow>(true);
            if (statRows != null && statRows.Length > 0)
                Debug.Log($"InventoryHUD: auto-detectadas {statRows.Length} StatRow(s) en hijos.");
        }
    }

    private void OnEnable()
    {
        if (playerStats != null)
            playerStats.OnStatsChanged += UpdateUI;
    }

    private void OnDisable()
    {
        if (playerStats != null)
            playerStats.OnStatsChanged -= UpdateUI;
    }

    // Llamar desde un botón o tecla para alternar la ventana
    public void ToggleHUD()
    {
        if (hudPanel == null) return;
        bool active = !hudPanel.activeSelf;
        hudPanel.SetActive(active);
        if (active) UpdateUI();
    }

    public void OpenHUD()
    {
        if (hudPanel == null) return;
        hudPanel.SetActive(true);
        UpdateUI();
    }

    public void CloseHUD()
    {
        if (hudPanel == null) return;
        hudPanel.SetActive(false);
    }

    // Actualiza la UI con los valores del player
    public void UpdateUI()
    {
        if (playerStats == null) return;

        if (nameText != null) nameText.text = playerStats.playerName;
        if (levelText != null) levelText.text = "Lvl." + playerStats.level.ToString();
        if (pointsText != null) pointsText.text = playerStats.availablePoints.ToString();

        if (statRows != null && statRows.Length > 0)
        {
            foreach (var row in statRows)
            {
                if (row != null)
                    row.Setup(playerStats, this);
            }
        }
        else
        {
            // Intentar autocompletar en caso de falta de asignación
            var found = GetComponentsInChildren<StatRow>(true);
            if (found != null && found.Length > 0)
            {
                statRows = found;
                foreach (var row in statRows)
                    if (row != null)
                        row.Setup(playerStats, this);
                Debug.Log($"InventoryHUD: statRows auto-registradas ({statRows.Length}) en UpdateUI.");
            }
            else
            {
                Debug.LogWarning("InventoryHUD: no hay StatRow asignadas ni encontradas como hijos.");
            }
        }
    }

    // Editor convenient: menú de contexto para buscar filas manualmente desde el Inspector
    [ContextMenu("Auto Find StatRows")]
    public void AutoFindStatRows()
    {
        statRows = GetComponentsInChildren<StatRow>(true);
        Debug.Log($"InventoryHUD: AutoFind StatRows found {(statRows == null ? 0 : statRows.Length)} rows.");
    }

    // Petición desde una StatRow para incrementar. Aquí se valida y aplican efectos adicionales si es necesario.
    public void RequestIncreaseStat(PlayerStats.StatType stat)
    {
        if (playerStats == null) return;

        bool ok = playerStats.IncreaseStat(stat);
        if (ok)
        {
            // Si deseas agregar efectos (sonido, animación), hacerlo aquí.
            UpdateUI();
        }
        else
        {
            // Opcional: feedback (no hay puntos)
            Debug.Log("No hay puntos disponibles para gastar.");
        }
    }
}
