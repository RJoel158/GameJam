#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Lista todas las escenas del proyecto
/// Ejecuta desde: Tools > Show All Scenes in Project
/// </summary>
public class ShowAllScenes
{
    [MenuItem("Tools/Show All Scenes in Project")]
    public static void ListAllScenes()
    {
        Debug.Log("<color=cyan>==========================================</color>");
        Debug.Log("<color=cyan>TODAS LAS ESCENAS EN TU PROYECTO:</color>");
        Debug.Log("<color=cyan>==========================================</color>");
        
        string[] allSceneGuids = AssetDatabase.FindAssets("t:Scene");
        
        if (allSceneGuids.Length == 0)
        {
            Debug.LogError("<color=red>NO SE ENCONTRARON ESCENAS!</color>");
            return;
        }

        Debug.Log($"<color=green>Total: {allSceneGuids.Length} escenas encontradas</color>\n");

        for (int i = 0; i < allSceneGuids.Length; i++)
        {
            string guid = allSceneGuids[i];
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            string folder = System.IO.Path.GetDirectoryName(path);
            
            Debug.Log($"<color=yellow>[{i + 1}]</color> <color=white><b>{name}</b></color>");
            Debug.Log($"    Carpeta: {folder}");
        }

        Debug.Log("\n<color=cyan>==========================================</color>");
        Debug.Log("<color=yellow>Copia el nombre exacto de la escena que quieres cargar</color>");
        Debug.Log("<color=cyan>==========================================</color>");
    }
}
#endif
