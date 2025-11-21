using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Representa un campamento de enemigos. Cuenta muertes de los `Enemy` asignados
/// y dispara `OnCampCleared` cuando se alcanza el umbral (`enemiesNeeded`).
/// </summary>
public class EnemyCamp : MonoBehaviour
{
    [Header("Auto-sink (simple)")]
    [Tooltip("Si true, cuando el campamento quede limpio se desactivarán colliders, se hundirá y se destruirá automáticamente.")]
    public bool autoSinkOnClear = true;
    [Tooltip("Distancia en unidades a hundir (hacia abajo) cuando se limpia el campamento")]
    public float sinkDepth = 5f;
    [Tooltip("Velocidad de hundimiento en unidades/segundo")]
    public float sinkSpeed = 2f;
    [Tooltip("Segundos que esperar después de hundirse antes de destruir")]
    public float destroyDelayAfterSink = 5f;
    [Tooltip("Si true, se desactivan los Colliders en los hijos antes de hundir")]
    public bool disableCollidersWhenSinking = true;

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
            // Invoke event so other systems still react
            OnCampCleared?.Invoke(this);

            // Simple built-in sink behavior: optionally disable colliders, translate down and destroy
            if (autoSinkOnClear)
            {
                StartCoroutine(SimpleSinkAndDestroy());
            }
        }
    }

    IEnumerator SimpleSinkAndDestroy()
    {
        // disable colliders if requested
        Collider[] cols = null;
        bool[] prev = null;
        if (disableCollidersWhenSinking)
        {
            cols = GetComponentsInChildren<Collider>(true);
            if (cols != null && cols.Length > 0)
            {
                prev = new bool[cols.Length];
                for (int i = 0; i < cols.Length; i++)
                {
                    prev[i] = cols[i].enabled;
                    cols[i].enabled = false;
                }
            }
        }

        float targetY = transform.position.y - sinkDepth;
        while (transform.position.y > targetY)
        {
            float step = sinkSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(transform.position.x, targetY, transform.position.z), step);
            yield return null;
        }

        // wait a bit then destroy
        yield return new WaitForSeconds(destroyDelayAfterSink);

        Destroy(gameObject);
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
