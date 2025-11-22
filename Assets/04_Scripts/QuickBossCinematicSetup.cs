using UnityEngine;
using StarterAssets;

/// <summary>
/// Script simple para configuración rápida de cinemática del boss
/// Adjunta este script a cualquier GameObject y usa los botones en el Inspector
/// </summary>
public class QuickBossCinematicSetup : MonoBehaviour
{
    [Header("⚠️ CONFIGURACIÓN RÁPIDA")]
    [Tooltip("El prefab del boss que aparecerá")]
    public GameObject bossPrefab;

    [Tooltip("Posición donde aparecerá el boss")]
    public Vector3 bossSpawnPosition = new Vector3(0, 0, 50);

    [Header("Audio Opcional")]
    public AudioClip bossRoarSound;
    public AudioClip groundImpactSound;
    public AudioClip dramaticMusic;

    [Header("Estado")]
    [SerializeField] private bool setupCompleted = false;

    /// <summary>
    /// Configura todo automáticamente
    /// </summary>
    [ContextMenu("🚀 SETUP COMPLETO (1 CLICK)")]
    public void SetupEverything()
    {
        if (bossPrefab == null)
        {
            Debug.LogError("<color=red>[Quick Setup] ERROR: Boss Prefab no asignado!</color>");
            return;
        }

        Debug.Log("<color=cyan>========================================</color>");
        Debug.Log("<color=cyan>[Quick Setup] Iniciando configuración automática...</color>");
        Debug.Log("<color=cyan>========================================</color>");

        // 1. Crear BossSpawner
        GameObject spawner = CreateBossSpawner();

        // 2. Crear punto de spawn
        CreateSpawnPoint(spawner);

        // 3. Buscar o crear MissionManager
        ConnectToMissionManager(spawner);

        setupCompleted = true;

        Debug.Log("<color=green>========================================</color>");
        Debug.Log("<color=green>[Quick Setup] ¡Configuración completada! ✅</color>");
        Debug.Log("<color=green>Ahora cuando completes la misión, el boss aparecerá</color>");
        Debug.Log("<color=green>========================================</color>");
        Debug.Log("<color=yellow>SIGUIENTE PASO: Si quieres cinemática, usa Tools > Boss Cinematic Setup Wizard</color>");
    }

    private GameObject CreateBossSpawner()
    {
        // Buscar si ya existe
        BossSpawner existingSpawner = FindAnyObjectByType<BossSpawner>();
        if (existingSpawner != null)
        {
            Debug.Log("<color=yellow>[Quick Setup] BossSpawner ya existe, reutilizando...</color>");
            return existingSpawner.gameObject;
        }

        // Crear nuevo spawner
        GameObject spawnerObj = new GameObject("BossSpawner");
        BossSpawner spawner = spawnerObj.AddComponent<BossSpawner>();

        // Configuración básica sin cinemática
        spawner.bossGameObject = bossPrefab;
        spawner.instantiateBoss = true;
        spawner.playCinematicBeforeSpawn = false; // Sin cinemática por defecto
        spawner.spawnDelay = 1f;
        spawner.disablePlayerControls = false;

        Debug.Log("<color=green>[Quick Setup] BossSpawner creado ✅</color>");
        return spawnerObj;
    }

    private void CreateSpawnPoint(GameObject spawner)
    {
        BossSpawner spawnerScript = spawner.GetComponent<BossSpawner>();

        if (spawnerScript.spawnPoint == null)
        {
            GameObject spawnPoint = new GameObject("BossSpawnPoint");
            spawnPoint.transform.position = bossSpawnPosition;
            spawnPoint.transform.SetParent(spawner.transform);

            spawnerScript.spawnPoint = spawnPoint.transform;

            Debug.Log($"<color=green>[Quick Setup] Spawn Point creado en {bossSpawnPosition} ✅</color>");
        }
        else
        {
            Debug.Log("<color=yellow>[Quick Setup] Spawn Point ya existe</color>");
        }
    }

    private void ConnectToMissionManager(GameObject spawner)
    {
        MissionManager missionManager = FindAnyObjectByType<MissionManager>();

        if (missionManager == null)
        {
            Debug.LogWarning("<color=orange>[Quick Setup] ⚠️ MissionManager no encontrado en la escena!</color>");
            Debug.LogWarning("<color=orange>El boss spawner se conectará automáticamente cuando exista un MissionManager</color>");
            return;
        }

        Debug.Log("<color=green>[Quick Setup] MissionManager encontrado, el evento se conectará automáticamente ✅</color>");
        Debug.Log("<color=cyan>[Quick Setup] El BossSpawner escuchará el evento OnMissionCompleted</color>");
    }

    /// <summary>
    /// Solo crea el spawner básico
    /// </summary>
    [ContextMenu("1️⃣ Crear Solo Boss Spawner")]
    public void CreateSpawnerOnly()
    {
        if (bossPrefab == null)
        {
            Debug.LogError("<color=red>[Quick Setup] Boss Prefab no asignado!</color>");
            return;
        }

        GameObject spawner = CreateBossSpawner();
        CreateSpawnPoint(spawner);
        Debug.Log("<color=green>[Quick Setup] Boss Spawner creado ✅</color>");
    }

    /// <summary>
    /// Testear spawn del boss manualmente
    /// </summary>
    [ContextMenu("🧪 TEST: Forzar Spawn del Boss")]
    public void TestSpawnBoss()
    {
        BossSpawner spawner = FindAnyObjectByType<BossSpawner>();

        if (spawner == null)
        {
            Debug.LogError("<color=red>[Quick Setup] No se encontró BossSpawner. Ejecuta Setup primero.</color>");
            return;
        }

        Debug.Log("<color=yellow>[Quick Setup] Testeando spawn del boss...</color>");
        spawner.ForceSpawnBoss();
    }

    /// <summary>
    /// Muestra información del setup
    /// </summary>
    [ContextMenu("ℹ️ Mostrar Info del Setup")]
    public void ShowSetupInfo()
    {
        Debug.Log("<color=cyan>========================================</color>");
        Debug.Log("<color=cyan>[Quick Setup] INFORMACIÓN DEL SETUP</color>");
        Debug.Log("<color=cyan>========================================</color>");

        BossSpawner spawner = FindAnyObjectByType<BossSpawner>();
        if (spawner != null)
        {
            Debug.Log($"<color=green>✅ BossSpawner: Encontrado</color>");
            Debug.Log($"   - Boss GameObject: {(spawner.bossGameObject != null ? spawner.bossGameObject.name : "NO ASIGNADO")}");
            Debug.Log($"   - Spawn Point: {(spawner.spawnPoint != null ? spawner.spawnPoint.position.ToString() : "NO ASIGNADO")}");
            Debug.Log($"   - Cinemática: {(spawner.playCinematicBeforeSpawn ? "SÍ" : "NO")}");
        }
        else
        {
            Debug.Log("<color=red>❌ BossSpawner: No encontrado</color>");
        }

        MissionManager mm = FindAnyObjectByType<MissionManager>();
        if (mm != null)
        {
            Debug.Log($"<color=green>✅ MissionManager: Encontrado</color>");
            Debug.Log($"   - Misiones disponibles: {mm.availableMissions.Count}");
            Debug.Log($"   - Misión activa: {(mm.currentMission != null ? mm.currentMission.missionName : "Ninguna")}");
        }
        else
        {
            Debug.Log("<color=red>❌ MissionManager: No encontrado</color>");
        }

        ThirdPersonController player = FindAnyObjectByType<ThirdPersonController>();
        if (player != null)
        {
            Debug.Log($"<color=green>✅ Player: Encontrado en {player.transform.position}</color>");
        }
        else
        {
            Debug.Log("<color=orange>⚠️ Player: No encontrado</color>");
        }

        Debug.Log("<color=cyan>========================================</color>");
    }

    /// <summary>
    /// Limpia el setup
    /// </summary>
    [ContextMenu("🗑️ Limpiar Setup")]
    public void CleanupSetup()
    {
        BossSpawner spawner = FindAnyObjectByType<BossSpawner>();
        if (spawner != null)
        {
            DestroyImmediate(spawner.gameObject);
            Debug.Log("<color=yellow>[Quick Setup] BossSpawner eliminado</color>");
        }

        setupCompleted = false;
        Debug.Log("<color=green>[Quick Setup] Limpieza completada ✅</color>");
    }

    // Mostrar ayuda en el Inspector
    private void OnValidate()
    {
        if (bossPrefab == null)
        {
            Debug.LogWarning("[Quick Setup] Asigna el Boss Prefab en el Inspector y luego usa el menú contextual (click derecho en el script) para configurar.");
        }
    }

    // Visualizar spawn point en Scene view
    private void OnDrawGizmos()
    {
        // Dibujar el spawn point
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(bossSpawnPosition, 2f);
        Gizmos.DrawLine(bossSpawnPosition, bossSpawnPosition + Vector3.up * 5f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(bossSpawnPosition + Vector3.up * 5.5f, Vector3.one * 0.5f);

#if UNITY_EDITOR
        // Mostrar label
        UnityEditor.Handles.Label(bossSpawnPosition + Vector3.up * 6f, "BOSS SPAWN POINT");
#endif
    }
}
