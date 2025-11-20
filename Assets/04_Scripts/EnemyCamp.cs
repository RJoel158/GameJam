using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Representa un campamento de enemigos. Cuenta muertes de los `Enemy` asignados
/// y dispara `OnCampCleared` cuando se alcanza el umbral (`enemiesNeeded`).
/// </summary>
public class EnemyCamp : MonoBehaviour
{
    [Header("Camp Settings")]
    [Tooltip("Número de enemigos que deben morir para considerar el campamento limpio")]
    public int enemiesNeeded = 3;

    [Tooltip("Si está activo, buscará automáticamente Enemy en los hijos del GameObject")]
    public bool autoFindChildrenEnemies = true;

    [Tooltip("Lista de enemigos que pertenecen a este campamento (puede llenarse en inspector)")]
    public List<Enemy> enemies = new List<Enemy>();

    [Header("Runtime")]
    public int deadCount = 0;
    public bool isCleared = false;

    // Event fired when this camp is cleared
    public static event System.Action<EnemyCamp> OnCampCleared;

    // Keep per-enemy handlers so we can unsubscribe cleanly
    private readonly Dictionary<Enemy, System.Action> handlers = new Dictionary<Enemy, System.Action>();

    private void Awake()
    {
        if (autoFindChildrenEnemies && (enemies == null || enemies.Count == 0))
        {
            enemies = new List<Enemy>(GetComponentsInChildren<Enemy>(true));
        }
    }

    private void OnEnable()
    {
        SubscribeAll();
    }

    private void OnDisable()
    {
        UnsubscribeAll();
    }

    [ContextMenu("Subscribe All Enemies")]
    public void SubscribeAll()
    {
        UnsubscribeAll();

        if (enemies == null) enemies = new List<Enemy>();

        foreach (var e in enemies)
        {
            if (e == null) continue;
            System.Action handler = () => OnEnemyDied(e);
            handlers[e] = handler;
            e.OnDied += handler;
        }
    }

    [ContextMenu("Unsubscribe All Enemies")]
    public void UnsubscribeAll()
    {
        foreach (var kv in handlers)
        {
            if (kv.Key != null)
            {
                kv.Key.OnDied -= kv.Value;
            }
        }
        handlers.Clear();
    }

    private void OnEnemyDied(Enemy e)
    {
        if (isCleared) return;

        deadCount++;
        Debug.Log($"<color=cyan>[EnemyCamp] Enemy died in camp '{name}'. deadCount={deadCount}/{enemiesNeeded}</color>");

        if (deadCount >= enemiesNeeded)
        {
            isCleared = true;
            Debug.Log($"<color=green>[EnemyCamp] Camp cleared: {name}</color>");
            OnCampCleared?.Invoke(this);
        }
    }

    [ContextMenu("Reset Camp")]
    public void ResetCamp()
    {
        deadCount = 0;
        isCleared = false;
    }

    private void OnValidate()
    {
        if (enemies == null) enemies = new List<Enemy>();

        // In editor, if autoFindChildrenEnemies is enabled, populate the list
        // with Enemy components found in the children so it's easy to configure.
#if UNITY_EDITOR
        if (autoFindChildrenEnemies)
        {
            var found = new List<Enemy>(GetComponentsInChildren<Enemy>(true));
            // Only overwrite if different to avoid disturbing manual edits
            bool different = found.Count != enemies.Count;
            if (!different)
            {
                for (int i = 0; i < found.Count; i++)
                {
                    if (found[i] != enemies[i]) { different = true; break; }
                }
            }

            if (different)
            {
                enemies = found;
                // Mark dirty so Unity saves the change in the inspector/prefab
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
#endif
    }

    public Vector3 GetCampPosition()
    {
        return transform.position;
    }
}
