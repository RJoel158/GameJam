#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

// Editor helper: busca InventoryHUD en la escena y añade/configura HUDToggle para bindear la tecla.
public static class InventoryHUDInstaller
{
    [MenuItem("Tools/Bind HUD Toggle (Tab)")]
    public static void BindHudToggle()
    {
        var hud = Object.FindObjectOfType<InventoryHUD>();
        if (hud == null)
        {
            Debug.LogWarning("No InventoryHUD encontrado en la escena. Añade InventoryHUD a tu HUD panel primero.");
            return;
        }

        var go = hud.gameObject;
        var toggle = go.GetComponent<HUDToggle>();
        if (toggle == null)
        {
            toggle = Undo.AddComponent<HUDToggle>(go);
            Debug.Log("HUDToggle añadido al GameObject con InventoryHUD.");
        }
        else
        {
            Debug.Log("HUDToggle ya existe en el GameObject.");
        }

        // Assign reference
        if (toggle.inventoryHUD != hud)
        {
            Undo.RecordObject(toggle, "Asignar InventoryHUD a HUDToggle");
            toggle.inventoryHUD = hud;
            EditorUtility.SetDirty(toggle);
            Debug.Log("InventoryHUD asignado al HUDToggle.");
        }

        // Seleccionar el objeto en la jerarquía para que el usuario pueda ver el componente
        Selection.activeGameObject = go;
    }
}
#endif
