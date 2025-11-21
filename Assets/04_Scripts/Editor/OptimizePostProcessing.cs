#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using System;

public static class OptimizePostProcessing
{
    [MenuItem("Tools/GameJam/Optimize/Apply Performance Profile (Post-Processing)")]
    public static void ApplyPerformanceProfile()
    {
        if (!EditorUtility.DisplayDialog("Apply performance optimizations?",
            "This will disable Volume components in open scenes and apply safer Quality/URP settings (shadow distance, MSAA, render scale). Continue?", "Yes", "No"))
            return;

        int modifiedScenes = 0;

        // 1) Disable Volume components in all open scenes
        var volumes = UnityEngine.Object.FindObjectsOfType<Volume>(true);
        foreach (var v in volumes)
        {
            if (v == null) continue;
            if (v.enabled)
            {
                Undo.RecordObject(v, "Disable Volume");
                v.enabled = false;
                EditorUtility.SetDirty(v);
                modifiedScenes++;
            }
        }

        // 2) Apply QualitySettings changes
        Undo.RecordObject(QualitySettingsRenderHelper.Instance, "QualitySettings Change");
        QualitySettings.antiAliasing = 0; // disable MSAA in built-in QA
        QualitySettings.shadowDistance = Mathf.Min(QualitySettings.shadowDistance, 30f);
        QualitySettings.globalTextureMipmapLimit = Mathf.Max(0, QualitySettings.globalTextureMipmapLimit); // keep but ensure it's valid

        // 3) Attempt to find and modify UniversalRenderPipelineAsset (URP)
        try
        {
            string[] guids = AssetDatabase.FindAssets("t:UniversalRenderPipelineAsset");
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                if (asset == null) continue;

                var urpType = asset.GetType();
                // try to set common properties via reflection if available
                var shadowProp = urpType.GetProperty("shadowDistance");
                if (shadowProp != null && shadowProp.CanWrite)
                {
                    Undo.RecordObject(asset, "URP shadowDistance");
                    shadowProp.SetValue(asset, 30f, null);
                    EditorUtility.SetDirty(asset);
                }

                var msaaProp = urpType.GetProperty("msaaSampleCount");
                if (msaaProp != null && msaaProp.CanWrite)
                {
                    Undo.RecordObject(asset, "URP MSAA");
                    msaaProp.SetValue(asset, 1, null); // 1 == none
                    EditorUtility.SetDirty(asset);
                }

                var renderScaleProp = urpType.GetProperty("renderScale");
                if (renderScaleProp != null && renderScaleProp.CanWrite)
                {
                    float current = (float)renderScaleProp.GetValue(asset, null);
                    float target = Mathf.Clamp(current, 0.7f, 1f);
                    if (current > 0.9f) target = 0.9f; // reduce if > 0.9
                    Undo.RecordObject(asset, "URP renderScale");
                    renderScaleProp.SetValue(asset, target, null);
                    EditorUtility.SetDirty(asset);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("URP asset adjustments skipped or failed: " + e.Message);
        }

        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog("Optimization applied", $"Volumes disabled: {modifiedScenes}. Quality/URP settings adjusted where available. Please profile in Editor to confirm impact.", "OK");
    }

    [MenuItem("Tools/GameJam/Optimize/Revert - Enable Volumes (Manual)")]
    public static void ReenableVolumes()
    {
        if (!EditorUtility.DisplayDialog("Re-enable volumes?",
            "This will re-enable all Volume components in open scenes. Continue?", "Yes", "No"))
            return;

        int reenabled = 0;
        var volumes = UnityEngine.Object.FindObjectsOfType<Volume>(true);
        foreach (var v in volumes)
        {
            if (v == null) continue;
            if (!v.enabled)
            {
                Undo.RecordObject(v, "Enable Volume");
                v.enabled = true;
                EditorUtility.SetDirty(v);
                reenabled++;
            }
        }

        EditorUtility.DisplayDialog("Re-enabled Volumes", $"Volumes re-enabled: {reenabled}", "OK");
    }

    // Helper singleton to store a dummy ScriptableObject so we can record Undo for QualitySettings changes
    class QualitySettingsRenderHelper : ScriptableObject
    {
        private static QualitySettingsRenderHelper _instance;
        public static QualitySettingsRenderHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = ScriptableObject.CreateInstance<QualitySettingsRenderHelper>();
                    _instance.hideFlags = HideFlags.HideAndDontSave;
                }
                return _instance;
            }
        }
    }
}
#endif
