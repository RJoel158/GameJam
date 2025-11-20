using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Herramienta para agregar escenas al Build Settings automaticamente
/// </summary>
public class AddScenesToBuild : MonoBehaviour
{
    [ContextMenu("Agregar TODAS las escenas al Build Settings")]
    public static void AddAllScenesToBuildSettings()
    {
        // Buscar todas las escenas en el proyecto
        string[] guids = AssetDatabase.FindAssets("t:Scene");
        
        List<EditorBuildSettingsScene> editorBuildSettingsScenes = new List<EditorBuildSettingsScene>();
        
        Debug.Log("<color=cyan>========================================</color>");
        Debug.Log("<color=cyan>AGREGANDO ESCENAS AL BUILD SETTINGS</color>");
        Debug.Log("<color=cyan>========================================</color>");
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(path);
            
            // Agregar solo si no esta ya en la lista
            if (!editorBuildSettingsScenes.Any(x => x.path == path))
            {
                EditorBuildSettingsScene scene = new EditorBuildSettingsScene(path, true);
                editorBuildSettingsScenes.Add(scene);
                Debug.Log($"<color=green>+ Agregada: {sceneName}</color>");
            }
        }
        
        // Aplicar los cambios
        EditorBuildSettings.scenes = editorBuildSettingsScenes.ToArray();
        
        Debug.Log("<color=cyan>========================================</color>");
        Debug.Log($"<color=green>TOTAL: {editorBuildSettingsScenes.Count} escenas agregadas</color>");
        Debug.Log("<color=cyan>========================================</color>");
    }

    [ContextMenu("Agregar SOLO OpenWorldSceneMerged1")]
    public static void AddMainSceneToBuild()
    {
        AddSpecificSceneToBuild("OpenWorldSceneMerged1");
    }

    public static void AddSpecificSceneToBuild(string sceneName)
    {
        // Buscar la escena especifica
        string[] guids = AssetDatabase.FindAssets($"t:Scene {sceneName}");
        
        if (guids.Length == 0)
        {
            Debug.LogError($"[Build Settings] No se encontro la escena '{sceneName}'");
            return;
        }

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        
        // Obtener escenas actuales
        List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes.ToList();
        
        // Verificar si ya existe
        if (scenes.Any(x => x.path == path))
        {
            Debug.Log($"<color=yellow>[Build Settings] '{sceneName}' ya esta en Build Settings</color>");
            return;
        }

        // Agregar la escena
        EditorBuildSettingsScene newScene = new EditorBuildSettingsScene(path, true);
        scenes.Add(newScene);
        
        // Aplicar cambios
        EditorBuildSettings.scenes = scenes.ToArray();
        
        Debug.Log($"<color=green>[Build Settings] '{sceneName}' agregada exitosamente!</color>");
        Debug.Log($"<color=cyan>Ruta: {path}</color>");
    }

    [ContextMenu("Listar escenas en Build Settings")]
    public static void ListScenesInBuild()
    {
        Debug.Log("<color=cyan>========================================</color>");
        Debug.Log("<color=cyan>ESCENAS EN BUILD SETTINGS:</color>");
        Debug.Log("<color=cyan>========================================</color>");
        
        var scenes = EditorBuildSettings.scenes;
        
        if (scenes.Length == 0)
        {
            Debug.LogWarning("<color=yellow>NO HAY ESCENAS en Build Settings!</color>");
        }
        else
        {
            for (int i = 0; i < scenes.Length; i++)
            {
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenes[i].path);
                string status = scenes[i].enabled ? "HABILITADA" : "deshabilitada";
                string color = scenes[i].enabled ? "green" : "gray";
                
                Debug.Log($"<color={color}>[{i}] {sceneName} - {status}</color>");
                Debug.Log($"    Path: {scenes[i].path}");
            }
        }
        
        Debug.Log("<color=cyan>========================================</color>");
    }

    [MenuItem("Tools/Build Settings/Agregar OpenWorldSceneMerged1")]
    public static void MenuAddMainScene()
    {
        AddMainSceneToBuild();
    }

    [MenuItem("Tools/Build Settings/Agregar TODAS las escenas")]
    public static void MenuAddAllScenes()
    {
        AddAllScenesToBuildSettings();
    }

    [MenuItem("Tools/Build Settings/Listar escenas")]
    public static void MenuListScenes()
    {
        ListScenesInBuild();
    }
}
#endif
