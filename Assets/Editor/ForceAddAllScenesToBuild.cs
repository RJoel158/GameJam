#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;
using UnityEngine;

/// <summary>
/// Este script se ejecuta automaticamente cuando se guarda
/// Agrega TODAS las escenas al Build Settings
/// </summary>
[InitializeOnLoad]
public class ForceAddAllScenesToBuild
{
    static ForceAddAllScenesToBuild()
    {
        EditorApplication.delayCall += () =>
        {
            AddAllScenesToBuild();
        };
    }

    static void AddAllScenesToBuild()
    {
        // Buscar TODAS las escenas en el proyecto
        string[] allSceneGuids = AssetDatabase.FindAssets("t:Scene");
        
        if (allSceneGuids.Length == 0)
        {
            Debug.LogWarning("[Force Build] No se encontraron escenas en el proyecto!");
            return;
        }

        var currentScenes = EditorBuildSettings.scenes.ToList();
        bool anyAdded = false;

        foreach (string guid in allSceneGuids)
        {
            string scenePath = AssetDatabase.GUIDToAssetPath(guid);
            
            // Verificar si ya existe
            if (currentScenes.Any(x => x.path == scenePath))
            {
                continue;
            }

            // Agregar la escena
            currentScenes.Add(new EditorBuildSettingsScene(scenePath, true));
            
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            Debug.Log($"<color=green>[Force Build] AGREGADA: {sceneName}</color>");
            anyAdded = true;
        }

        if (anyAdded)
        {
            EditorBuildSettings.scenes = currentScenes.ToArray();
            
            Debug.Log("<color=cyan>========================================</color>");
            Debug.Log($"<color=green>[Force Build] {currentScenes.Count} escenas en Build Settings</color>");
            Debug.Log("<color=cyan>========================================</color>");
            
            // Listar todas
            Debug.Log("<color=cyan>ESCENAS EN BUILD:</color>");
            for (int i = 0; i < currentScenes.Count; i++)
            {
                string name = System.IO.Path.GetFileNameWithoutExtension(currentScenes[i].path);
                Debug.Log($"  [{i}] {name}");
            }
        }
    }
}
#endif
