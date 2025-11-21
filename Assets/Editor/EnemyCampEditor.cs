using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// Custom inspector and utility menu for EnemyCamp to make setup easier
[CustomEditor(typeof(EnemyCamp))]
public class EnemyCampInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EnemyCamp camp = (EnemyCamp)target;

        EditorGUILayout.Space();

        if (GUILayout.Button("Collect Children Enemies"))
        {
            Undo.RecordObject(camp, "Collect Children Enemies");
            camp.enemies = new List<Enemy>(camp.GetComponentsInChildren<Enemy>(true));
            EditorUtility.SetDirty(camp);
        }

        if (GUILayout.Button("Clear Enemies List"))
        {
            Undo.RecordObject(camp, "Clear Enemies List");
            camp.enemies = new List<Enemy>();
            EditorUtility.SetDirty(camp);
        }
    }

    [MenuItem("Tools/Enemy Camps/Collect All Camps")]
    private static void CollectAllCamps()
    {
        var camps = Object.FindObjectsOfType<EnemyCamp>(true);
        int count = 0;
        foreach (var camp in camps)
        {
            Undo.RecordObject(camp, "Collect Children Enemies");
            camp.enemies = new List<Enemy>(camp.GetComponentsInChildren<Enemy>(true));
            EditorUtility.SetDirty(camp);
            count++;
        }
        Debug.Log($"[EnemyCampEditor] Collected children for {count} camp(s)");
    }

    [MenuItem("Tools/Enemy Camps/Clear All Camps Lists")]
    private static void ClearAllCamps()
    {
        var camps = Object.FindObjectsOfType<EnemyCamp>(true);
        int count = 0;
        foreach (var camp in camps)
        {
            Undo.RecordObject(camp, "Clear Enemies List");
            camp.enemies = new List<Enemy>();
            EditorUtility.SetDirty(camp);
            count++;
        }
        Debug.Log($"[EnemyCampEditor] Cleared enemies list for {count} camp(s)");
    }
}
