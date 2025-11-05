using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class GameSetupHelper : MonoBehaviour
{
    [MenuItem("Tools/Setup Complete Game (Scenes + Fonts + Pause)")]
    public static void SetupEverything()
    {
        SetupBuildScenes();
        Debug.Log("════════════════════════════════════");
        // Las fuentes y el pause menu se configurarán manualmente
        Debug.Log("<color=cyan>Paso 2: Ahora cierra Unity y vuélvelo a abrir</color>");
        Debug.Log("<color=cyan>Paso 3: En MainMenu, regenera fuentes con: Tools → Regenerate TMP Fonts</color>");
        Debug.Log("<color=cyan>Paso 4: Prueba el juego desde MainMenu</color>");
    }

    [MenuItem("Tools/1. Setup Build Settings Scenes")]
    public static void SetupBuildScenes()
    {
        string[] requiredScenes = new string[]
        {
            "Assets/RommelPrefabs/08_MainMenu/MainMenu.unity",
            "Assets/RommelPrefabs/08_MainMenu/loading screens/LoadingScreen_1.unity",
            "Assets/00_Scenes/OpenWorldSceneMerged.unity"
        };

        List<EditorBuildSettingsScene> buildScenes = new List<EditorBuildSettingsScene>();

        foreach (string scenePath in requiredScenes)
        {
            if (System.IO.File.Exists(scenePath))
            {
                buildScenes.Add(new EditorBuildSettingsScene(scenePath, true));
                Debug.Log($"✅ Agregada: {scenePath}");
            }
            else
            {
                Debug.LogWarning($"⚠️ Escena no encontrada: {scenePath}");
            }
        }

        EditorBuildSettings.scenes = buildScenes.ToArray();
        Debug.Log($"<color=green>✅ Build Settings actualizado con {buildScenes.Count} escenas</color>");
    }

    [MenuItem("Tools/2. Regenerate TMP Fonts")]
    public static void RegenerateFonts()
    {
        // Solo mostrar instrucciones - las fuentes se regenerarán en la próxima apertura
        string message = @"Para regenerar las fuentes TextMeshPro:

1. Window → TextMeshPro → Font Asset Creator
2. Arrastra: Assets/RommelPrefabs/08_MainMenu/Fonts/newton_howard/Newton Howard Font.ttf
3. Click 'Generate Font Atlas'
4. Save As: Newton Howard Font SDF (en la misma carpeta)

Repite para las otras ubicaciones de fuentes.";

        EditorUtility.DisplayDialog("Regenerar Fuentes TMP", message, "OK");
        Debug.Log("<color=yellow>📖 Instrucciones de fuentes mostradas</color>");
    }
}
