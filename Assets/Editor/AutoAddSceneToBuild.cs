using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.Linq;

[InitializeOnLoad]
public class AutoAddSceneToBuild
{
    static AutoAddSceneToBuild()
    {
        // Esto se ejecuta cuando Unity carga/compila
        EditorApplication.delayCall += AddAllGameScenes;
    }

    static void AddAllGameScenes()
    {
        // Agregar todas las escenas del juego
        string[] sceneNames = new string[] 
        { 
            "OpenWorldSceneMerged1",
            "Game",
            "MainMenu",
            "LoadingScreen"
        };
        
        bool anyAdded = false;
        
        foreach (string sceneName in sceneNames)
        {
            if (AddSpecificSceneToBuild(sceneName))
            {
                anyAdded = true;
            }
        }
        
        if (anyAdded)
        {
            Debug.Log("<color=cyan>========================================</color>");
            Debug.Log("<color=green>[Auto Build] Escenas agregadas al Build Settings!</color>");
            Debug.Log("<color=cyan>========================================</color>");
        }
    }

    static bool AddSpecificSceneToBuild(string sceneName)
    {
        // Buscar la escena especifica
        string[] guids = AssetDatabase.FindAssets($"t:Scene {sceneName}");
        
        if (guids.Length == 0)
        {
            // No mostrar warning, puede que la escena no exista
            return false;
        }

        string scenePath = AssetDatabase.GUIDToAssetPath(guids[0]);
        
        // Obtener escenas actuales
        var scenes = EditorBuildSettings.scenes.ToList();
        
        // Verificar si ya existe
        if (scenes.Any(x => x.path == scenePath))
        {
            // Ya existe, no hacer nada
            return false;
        }

        // Agregar la escena
        scenes.Add(new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
        
        Debug.Log($"<color=green>[Auto Build] '{sceneName}' AGREGADA!</color>");
        return true;
    }
}
#endif
