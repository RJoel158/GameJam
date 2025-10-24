using UnityEngine;

// Simple toggle script that calls InventoryHUD.ToggleHUD() cuando se presiona la tecla configurada.
public class HUDToggle : MonoBehaviour
{
    public InventoryHUD inventoryHUD;
    public KeyCode toggleKey = KeyCode.Tab;

    void Update()
    {
        if (inventoryHUD == null) return;

        if (Input.GetKeyDown(toggleKey))
        {
            inventoryHUD.ToggleHUD();
        }
    }
}
