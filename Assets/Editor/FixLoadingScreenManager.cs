#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Busca y actualiza automaticamente el LoadingScreenManager
/// Ejecuta desde: Tools > Fix LoadingScreenManager Scene Name
/// </summary>
public class FixLoadingScreenManager
{
    [MenuItem("Tools/Fix LoadingScreenManager Scene Name")]
    public static void FixSceneName()
    {
        Debug.Log("<color=cyan>========================================</color>");
        Debug.Log("<color=cyan>BUSCANDO LoadingScreenManager EN ESCENA ACTUAL...</color>");
        
        // Buscar LoadingScreenManager en la escena actual
        var managers = Object.FindObjectsByType<LoadingScreenManager>(FindObjectsSortMode.None);
        
        if (managers.Length == 0)
        {
            Debug.LogWarning("<color=yellow>No se encontro LoadingScreenManager en la escena actual!</color>");
            Debug.LogWarning("<color=yellow>Abre la escena del LoadingScreen primero (probablemente en RommelPrefabs/08_MainMenu/loading screens/)</color>");
            return;
        }
        
        bool changed = false;
        
        foreach (var manager in managers)
        {
            Debug.Log($"<color=cyan>Encontrado en: {manager.gameObject.name}</color>");
            Debug.Log($"  Scene To Load actual: '<color=yellow>{manager.sceneToLoad}</color>'");
            
            if (manager.sceneToLoad != "OpenWorldSceneMerged1")
            {
                manager.sceneToLoad = "OpenWorldSceneMerged1";
                EditorUtility.SetDirty(manager);
                changed = true;
                
                Debug.Log($"<color=green>  CAMBIADO a: 'OpenWorldSceneMerged1'</color>");
            }
            else
            {
                Debug.Log($"<color=green>  Ya esta configurado correctamente!</color>");
            }
        }
        
        if (changed)
        {
            // Guardar la escena
            EditorSceneManager.SaveOpenScenes();
            
            Debug.Log("<color=cyan>========================================</color>");
            Debug.Log("<color=green>ARREGLADO Y GUARDADO!</color>");
            Debug.Log("<color=cyan>========================================</color>");
        }
    }

    [MenuItem("Tools/Show Current LoadingScreenManager Settings")]
    public static void ShowCurrentSettings()
    {
        var managers = Object.FindObjectsByType<LoadingScreenManager>(FindObjectsSortMode.None);
        
        Debug.Log("<color=cyan>========================================</color>");
        Debug.Log("<color=cyan>LoadingScreenManager EN ESCENA ACTUAL:</color>");
        
        if (managers.Length == 0)
        {
            Debug.LogWarning("<color=yellow>No hay LoadingScreenManager en la escena actual</color>");
        }
        else
        {
            foreach (var manager in managers)
            {
                Debug.Log($"<color=green>GameObject: {manager.gameObject.name}</color>");
                Debug.Log($"  Scene To Load: '<color=yellow>{manager.sceneToLoad}</color>'");
                Debug.Log($"  Fade Image: {(manager.fadeImage != null ? "Asignado" : "NULL")}");
                Debug.Log($"  Canvas Group: {(manager.canvasGroup != null ? "Asignado" : "NULL")}");
            }
        }
        
        Debug.Log("<color=cyan>========================================</color>");
    }
}
#endif
