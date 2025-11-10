using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.Linq;

/// <summary>
/// Script para agregar manualmente la escena Game al Build Settings
/// Ejecuta desde el menu: Tools > Add Game Scene to Build
/// </summary>
public class AddGameSceneToBuild : MonoBehaviour
{
    [MenuItem("Tools/Add Game Scene to Build NOW")]
    public static void AddGameScene()
    {
        string sceneName = "Game";
        
        Debug.Log($"<color=cyan>[Add Scene] Buscando escena '{sceneName}'...</color>");
        
        // Buscar la escena
        string[] guids = AssetDatabase.FindAssets($"t:Scene {sceneName}");
        
        if (guids.Length == 0)
        {
            Debug.LogError($"<color=red>[Add Scene] NO SE ENCONTRO la escena '{sceneName}'!</color>");
            Debug.LogError("<color=red>Asegurate de que existe una escena llamada 'Game.unity' en tu proyecto</color>");
            
            // Buscar todas las escenas y listarlas
            ListAllScenes();
            return;
        }

        string scenePath = AssetDatabase.GUIDToAssetPath(guids[0]);
        Debug.Log($"<color=green>[Add Scene] Escena encontrada: {scenePath}</color>");
        
        // Obtener escenas actuales
        var scenes = EditorBuildSettings.scenes.ToList();
        
        // Verificar si ya existe
        if (scenes.Any(x => x.path == scenePath))
        {
            Debug.Log($"<color=yellow>[Add Scene] '{sceneName}' YA ESTA en Build Settings</color>");
            ListBuildScenes();
            return;
        }

        // Agregar la escena
        scenes.Add(new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
        
        Debug.Log("<color=cyan>========================================</color>");
        Debug.Log($"<color=green>[Add Scene] '{sceneName}' AGREGADA EXITOSAMENTE!</color>");
        Debug.Log($"<color=cyan>Ruta: {scenePath}</color>");
        Debug.Log($"<color=cyan>Index: {scenes.Count - 1}</color>");
        Debug.Log("<color=cyan>========================================</color>");
        
        ListBuildScenes();
    }

    static void ListAllScenes()
    {
        Debug.Log("<color=cyan>========================================</color>");
        Debug.Log("<color=cyan>TODAS LAS ESCENAS EN EL PROYECTO:</color>");
        
        string[] guids = AssetDatabase.FindAssets("t:Scene");
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            Debug.Log($"  - {name} ({path})");
        }
        
        Debug.Log("<color=cyan>========================================</color>");
    }

    static void ListBuildScenes()
    {
        Debug.Log("<color=cyan>========================================</color>");
        Debug.Log("<color=cyan>ESCENAS EN BUILD SETTINGS:</color>");
        
        var scenes = EditorBuildSettings.scenes;
        
        if (scenes.Length == 0)
        {
            Debug.LogWarning("<color=yellow>NO HAY ESCENAS en Build Settings!</color>");
        }
        else
        {
            for (int i = 0; i < scenes.Length; i++)
            {
                string name = System.IO.Path.GetFileNameWithoutExtension(scenes[i].path);
                string status = scenes[i].enabled ? "✓" : "✗";
                Debug.Log($"  [{i}] {status} {name}");
            }
        }
        
        Debug.Log("<color=cyan>========================================</color>");
    }

    [MenuItem("Tools/List All Scenes in Project")]
    public static void MenuListAllScenes()
    {
        ListAllScenes();
    }

    [MenuItem("Tools/List Build Settings Scenes")]
    public static void MenuListBuildScenes()
    {
        ListBuildScenes();
    }
}
#endif
