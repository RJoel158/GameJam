using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public class MenuAssetsFixHelper
{
    [MenuItem("Tools/Fix Menu Assets References")]
    public static void FixMenuAssets()
    {
        Debug.Log("🔧 Verificando assets de menús...");
        
        // Verificar imágenes de LoadingScreen
        string logoPath = "Assets/RommelPrefabs/08_MainMenu/loading screens/Imagenes_usadas/logo.png";
        Sprite logoSprite = AssetDatabase.LoadAssetAtPath<Sprite>(logoPath);
        
        if (logoSprite != null)
        {
            Debug.Log($"✅ Logo encontrado: {logoPath}");
        }
        else
        {
            Debug.LogWarning($"⚠️ Logo NO encontrado: {logoPath}");
        }
        
        // Verificar imágenes de PauseMenu
        string buttonPath = "Assets/RommelPrefabs/10_PauseMenu/Images/button.png";
        Sprite buttonSprite = AssetDatabase.LoadAssetAtPath<Sprite>(buttonPath);
        
        if (buttonSprite != null)
        {
            Debug.Log($"✅ Button encontrado: {buttonPath}");
        }
        else
        {
            Debug.LogWarning($"⚠️ Button NO encontrado: {buttonPath}");
        }
        
        string bgPath = "Assets/RommelPrefabs/10_PauseMenu/Images/Gemini_Generated_Image_daxaq2daxaq2daxa-Photoroom.png";
        Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(bgPath);
        
        if (bgSprite != null)
        {
            Debug.Log($"✅ Background encontrado: {bgPath}");
        }
        else
        {
            Debug.LogWarning($"⚠️ Background NO encontrado: {bgPath}");
        }
        
        // Reimportar todas las imágenes
        AssetDatabase.ImportAsset(logoPath, ImportAssetOptions.ForceUpdate);
        AssetDatabase.ImportAsset(buttonPath, ImportAssetOptions.ForceUpdate);
        AssetDatabase.ImportAsset(bgPath, ImportAssetOptions.ForceUpdate);
        
        AssetDatabase.Refresh();
        
        Debug.Log("<color=green>✅ Assets reimportados. Si siguen sin aparecer, verifica que las imágenes tengan Texture Type = Sprite (2D and UI)</color>");
        
        EditorUtility.DisplayDialog("Fix Menu Assets", 
            "Assets reimportados.\n\nSi las imágenes siguen sin verse:\n1. Selecciona cada imagen en Project\n2. En Inspector: Texture Type = Sprite (2D and UI)\n3. Click Apply", 
            "OK");
    }
    
    [MenuItem("Tools/Open LoadingScreen Scene")]
    public static void OpenLoadingScreen()
    {
        EditorSceneManager.OpenScene("Assets/RommelPrefabs/08_MainMenu/loading screens/LoadingScreen_1.unity");
    }
    
    [MenuItem("Tools/Open PauseMenu Scene")]
    public static void OpenPauseMenuScene()
    {
        EditorSceneManager.OpenScene("Assets/RommelPrefabs/10_PauseMenu/PauseMenuScene.unity");
    }
}
