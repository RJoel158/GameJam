using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using StarterAssets;

/// <summary>
/// Spawns the boss and triggers cinematic when mission is completed
/// </summary>
public class BossSpawner : MonoBehaviour
{
    [Header("Boss Settings")]
    [Tooltip("The boss prefab or GameObject to activate")]
    public GameObject bossPrefab;

    [Tooltip("Position where the boss will spawn")]
    public Transform spawnPoint;

    [Tooltip("If true, uses the prefab. If false, just activates the boss GameObject")]
    public bool instantiateBoss = true;

    [Header("Mission Trigger Settings")]
    [Tooltip("The mission that must be completed before the boss spawns")]
    public DefeatEnemiesMission requiredMission;

    [Tooltip("If true, checks if the mission is completed. If false, spawns when any mission completes")]
    public bool requireSpecificMission = true;

    [Header("Cinematic Settings")]
    [Tooltip("The Timeline Playable Director for the boss introduction")]
    public PlayableDirector cinematicTimeline;

    [Tooltip("If true, plays the cinematic before spawning the boss")]
    public bool playCinematicBeforeSpawn = true;

    [Tooltip("Delay before spawning boss (if no cinematic)")]
    public float spawnDelay = 1f;

    [Tooltip("Delay after mission completed before starting cinematic (to show mission complete UI)")]
    [Range(1f, 10f)]
    public float missionCompleteDelay = 3f;

    [Header("Camera Settings")]
    [Tooltip("Reference to the player camera to disable during cinematic")]
    public GameObject playerCamera;

    [Tooltip("If true, disables player controls during cinematic")]
    public bool disablePlayerControls = true;

    [Header("Boss Teleport Settings")]
    [Tooltip("If true, teleports boss near player after cinematic")]
    public bool teleportBossAfterCinematic = true;

    [Tooltip("Distance from player to teleport the boss")]
    [Range(5f, 20f)]
    public float teleportDistance = 8f;

    [Tooltip("Height offset for boss teleport (to prevent clipping into ground)")]
    [Range(0f, 5f)]
    public float teleportHeightOffset = 1.5f;

    [Header("Audio Settings")]
    [Tooltip("Audio to play when boss appears")]
    public AudioClip bossAppearSound;

    private AudioSource audioSource;
    private GameObject spawnedBoss;
    private bool bossSpawned = false;

    // Lista para guardar los Canvas que desactivamos
    private List<Canvas> disabledCanvases = new List<Canvas>();

    // Referencia a la UI del boss
    private BossHealthBarUI bossUI;

    // Estados guardados del jugador antes de la cinemática
    private bool playerWasEquipped = false;
    private bool playerWasBlocking = false;
    private PlayerController savedPlayerController = null;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void Start()
    {
        // Subscribe to mission completed event
        if (MissionManager.Instance != null)
        {
            MissionManager.Instance.OnMissionCompleted.AddListener(OnMissionCompleted);
            Debug.Log("<color=cyan>[BossSpawner] Subscribed to MissionManager.OnMissionCompleted</color>");
        }
        else
        {
            Debug.LogError("<color=red>[BossSpawner] MissionManager.Instance not found!</color>");
        }

        // Ensure boss is inactive at start
        if (bossPrefab != null && !instantiateBoss)
        {
            bossPrefab.SetActive(false);
        }

        // Setup cinematic timeline callbacks if available
        if (cinematicTimeline != null)
        {
            cinematicTimeline.stopped += OnCinematicFinished;
        }

        // Buscar la UI del boss
        bossUI = FindAnyObjectByType<BossHealthBarUI>();
        if (bossUI != null)
        {
            // Ocultar la UI al inicio
            bossUI.HideBossUI();
            Debug.Log("<color=cyan>[BossSpawner] Boss UI found and hidden</color>");
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (MissionManager.Instance != null)
        {
            MissionManager.Instance.OnMissionCompleted.RemoveListener(OnMissionCompleted);
        }

        if (cinematicTimeline != null)
        {
            cinematicTimeline.stopped -= OnCinematicFinished;
        }
    }

    /// <summary>
    /// Called when a mission is completed
    /// </summary>
    private void OnMissionCompleted(DefeatEnemiesMission mission)
    {
        if (bossSpawned)
        {
            Debug.Log("<color=yellow>[BossSpawner] Boss already spawned, ignoring mission completion</color>");
            return;
        }

        // Check if this is the required mission (if specific mission check is enabled)
        if (requireSpecificMission && requiredMission != null && mission != requiredMission)
        {
            Debug.Log($"<color=yellow>[BossSpawner] Mission '{mission.missionName}' completed, but waiting for '{requiredMission.missionName}'</color>");
            return;
        }

        Debug.Log($"<color=green>[BossSpawner] Required mission '{mission.missionName}' completed! Preparing boss spawn...</color>");

        // IMPORTANTE: Esperar un momento para que la UI de misión completada se muestre
        StartCoroutine(DelayedCinematicStart());
    }

    /// <summary>
    /// Espera un momento antes de iniciar la cinemática para que se muestre la UI de misión completada
    /// </summary>
    private IEnumerator DelayedCinematicStart()
    {
        // Esperar el tiempo configurado para que la animación de "Misión Completada" termine
        Debug.Log($"<color=cyan>[BossSpawner] Waiting {missionCompleteDelay}s for mission complete UI...</color>");
        yield return new WaitForSeconds(missionCompleteDelay);

        if (playCinematicBeforeSpawn && cinematicTimeline != null)
        {
            StartCinematic();
        }
        else
        {
            StartCoroutine(SpawnBossDelayed());
        }
    }

    /// <summary>
    /// Starts the cinematic timeline
    /// </summary>
    private void StartCinematic()
    {
        Debug.Log("<color=magenta>[BossSpawner] Starting boss introduction cinematic...</color>");

        // OPCIÓN: Spawnear el boss ANTES de la cinemática (para que sea visible)
        // Comentar esto si prefieres spawnearlo durante el Timeline con señales
        if (!bossSpawned)
        {
            SpawnBoss();
            Debug.Log("<color=magenta>[BossSpawner] Boss spawned before cinematic</color>");
        }

        // IMPORTANTE: Configurar el Timeline para usar Unscaled Time
        if (cinematicTimeline != null)
        {
            cinematicTimeline.timeUpdateMode = DirectorUpdateMode.UnscaledGameTime;
            Debug.Log("<color=magenta>[BossSpawner] Timeline set to UnscaledGameTime mode</color>");
        }

        // Disable player controls if needed
        if (disablePlayerControls)
        {
            DisablePlayerControls();
        }

        // Play the timeline
        if (cinematicTimeline != null)
        {
            cinematicTimeline.Play();
        }
        else
        {
            Debug.LogWarning("<color=orange>[BossSpawner] No cinematic timeline assigned!</color>");
            StartCoroutine(SpawnBossDelayed());
        }
    }

    /// <summary>
    /// Called when the cinematic finishes
    /// </summary>
    private void OnCinematicFinished(PlayableDirector director)
    {
        Debug.Log("<color=magenta>[BossSpawner] Cinematic finished!</color>");

        // Teletransportar el boss cerca del jugador (si está habilitado)
        if (teleportBossAfterCinematic)
        {
            TeleportBossToPlayer();
        }

        // IMPORTANTE: Habilitar el combate del boss
        EnableBossCombat();

        // Re-enable player controls
        if (disablePlayerControls)
        {
            EnablePlayerControls();
        }

        // Spawn boss if it wasn't spawned during the timeline
        if (!bossSpawned)
        {
            SpawnBoss();
        }
    }

    /// <summary>
    /// Habilita el combate del boss (NavMeshAgent, Enemy script, etc.)
    /// </summary>
    private void EnableBossCombat()
    {
        if (spawnedBoss == null)
        {
            Debug.LogWarning("<color=orange>[BossSpawner] No boss to enable combat!</color>");
            return;
        }

        // Habilitar el componente Enemy
        var enemy = spawnedBoss.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.enabled = true;
            Debug.Log("<color=cyan>[BossSpawner] Boss Enemy component enabled</color>");
        }

        // Habilitar NavMeshAgent
        var navAgent = spawnedBoss.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (navAgent != null)
        {
            navAgent.enabled = true;
            Debug.Log("<color=cyan>[BossSpawner] Boss NavMeshAgent enabled</color>");
        }

        // Habilitar Animator
        var animator = spawnedBoss.GetComponent<Animator>();
        if (animator != null)
        {
            animator.enabled = true;
            Debug.Log("<color=cyan>[BossSpawner] Boss Animator enabled</color>");
        }

        // NUEVO: Mostrar la UI del boss
        if (bossUI == null)
        {
            bossUI = FindAnyObjectByType<BossHealthBarUI>();
        }

        if (bossUI != null)
        {
            // Conectar el boss con la UI si no está conectado
            BossController bossController = spawnedBoss.GetComponent<BossController>();
            if (bossController != null)
            {
                bossUI.bossController = bossController;
            }

            bossUI.ShowBossUI();
            Debug.Log("<color=cyan>[BossSpawner] Boss UI shown</color>");
        }
        else
        {
            Debug.LogWarning("<color=orange>[BossSpawner] No BossHealthBarUI found! Run 'Game Jam > Quick Setup > Boss Health & Energy'</color>");
        }

        Debug.Log("<color=green>[BossSpawner] Boss combat systems enabled!</color>");
    }

    /// <summary>
    /// Teletransporta el boss cerca del jugador después de la cinemática
    /// </summary>
    private void TeleportBossToPlayer()
    {
        if (spawnedBoss == null)
        {
            Debug.LogWarning("<color=orange>[BossSpawner] No boss to teleport!</color>");
            return;
        }

        // Encontrar al jugador
        var player = FindAnyObjectByType<ThirdPersonController>();
        if (player == null)
        {
            Debug.LogWarning("<color=orange>[BossSpawner] Player not found for teleport!</color>");
            return;
        }

        // Calcular posición cerca del jugador
        Vector3 playerPos = player.transform.position;
        Vector3 direction = (spawnedBoss.transform.position - playerPos).normalized;

        // Si el boss está muy cerca o la dirección es inválida, usar dirección hacia adelante del jugador
        if (direction.magnitude < 0.1f)
        {
            direction = player.transform.forward;
        }

        Vector3 teleportPosition = playerPos + direction * teleportDistance;

        // ARREGLO: Elevar el boss para que no se entierre
        // Usar la altura del jugador + un offset adicional para bosses grandes
        teleportPosition.y = playerPos.y + teleportHeightOffset;

        // Teletransportar el boss
        spawnedBoss.transform.position = teleportPosition;

        // Hacer que el boss mire al jugador
        Vector3 lookDirection = (playerPos - teleportPosition).normalized;
        lookDirection.y = 0; // Mantener en plano horizontal
        if (lookDirection.magnitude > 0.1f)
        {
            spawnedBoss.transform.rotation = Quaternion.LookRotation(lookDirection);
        }

        Debug.Log($"<color=magenta>[BossSpawner] Boss teleported near player! Distance: {Vector3.Distance(playerPos, teleportPosition):F2}m, Height: {teleportPosition.y}</color>");

        // Efecto visual de teletransporte (opcional)
        PlaySpawnEffects(teleportPosition);
    }

    /// <summary>
    /// Spawns the boss with a delay
    /// </summary>
    private IEnumerator SpawnBossDelayed()
    {
        yield return new WaitForSeconds(spawnDelay);
        SpawnBoss();
    }

    /// <summary>
    /// Spawns the boss at the designated spawn point
    /// </summary>
    public void SpawnBoss()
    {
        if (bossSpawned)
        {
            Debug.LogWarning("<color=yellow>[BossSpawner] Boss already spawned!</color>");
            return;
        }

        if (bossPrefab == null)
        {
            Debug.LogError("<color=red>[BossSpawner] No boss prefab assigned!</color>");
            return;
        }

        Vector3 spawnPosition;
        Quaternion spawnRotation;

        // If spawn near player is enabled, calculate position near player
        if (spawnPoint != null)
        {
            spawnPosition = spawnPoint.position;
            spawnRotation = spawnPoint.rotation;
        }
        else
        {
            // Spawn near player if no spawn point is set
            var player = FindAnyObjectByType<ThirdPersonController>();
            if (player != null)
            {
                Vector3 playerPos = player.transform.position;
                Vector3 direction = player.transform.forward;
                spawnPosition = playerPos + direction * teleportDistance;
                spawnPosition.y = playerPos.y + teleportHeightOffset;
                spawnRotation = Quaternion.LookRotation(-direction);
            }
            else
            {
                spawnPosition = transform.position;
                spawnRotation = transform.rotation;
            }
        }

        if (instantiateBoss)
        {
            // Instantiate a new boss
            spawnedBoss = Instantiate(bossPrefab, spawnPosition, spawnRotation);
            Debug.Log($"<color=green>[BossSpawner] Boss instantiated at {spawnPosition}</color>");
        }
        else
        {
            // Just activate the existing boss
            bossPrefab.transform.position = spawnPosition;
            bossPrefab.transform.rotation = spawnRotation;
            bossPrefab.SetActive(true);
            spawnedBoss = bossPrefab;
            Debug.Log($"<color=green>[BossSpawner] Boss activated at {spawnPosition}</color>");
        }

        // Play spawn sound
        if (bossAppearSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(bossAppearSound);
        }

        bossSpawned = true;

        // Optional: Add spawn effects
        PlaySpawnEffects(spawnPosition);
    }

    /// <summary>
    /// Play visual/audio effects when boss spawns
    /// </summary>
    private void PlaySpawnEffects(Vector3 position)
    {
        // You can add particle effects, screen shake, etc. here
        Debug.Log($"<color=yellow>[BossSpawner] Playing spawn effects at {position}</color>");

        // Example: Find and play a particle system
        // ParticleSystem spawnVFX = GetComponentInChildren<ParticleSystem>();
        // if (spawnVFX != null)
        // {
        //     spawnVFX.transform.position = position;
        //     spawnVFX.Play();
        // }
    }

    /// <summary>
    /// Disables player controls during cinematic
    /// </summary>
    private void DisablePlayerControls()
    {
        Debug.Log("<color=yellow>[BossSpawner] Disabling player controls...</color>");

        // Limpiar la lista anterior
        disabledCanvases.Clear();

        // Desactivar TODAS las UI Canvas y guardar referencias
        Canvas[] allCanvas = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (Canvas canvas in allCanvas)
        {
            // Solo desactivar canvas de UI (no de World Space)
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay || canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                if (canvas.gameObject.activeSelf) // Solo guardar si estaba activo
                {
                    disabledCanvases.Add(canvas);
                    canvas.gameObject.SetActive(false);
                    Debug.Log($"<color=yellow>[BossSpawner] UI Canvas disabled: {canvas.gameObject.name}</color>");
                }
            }
        }

        Debug.Log($"<color=yellow>[BossSpawner] Disabled {disabledCanvases.Count} UI Canvas</color>");

        // NO desactivamos la Main Camera porque Cinemachine la necesita activa
        // En su lugar, desactivamos la cámara virtual del jugador si existe
        if (playerCamera != null)
        {
            // Si es una Virtual Camera, reducir su prioridad
            var vcam = playerCamera.GetComponent<Cinemachine.CinemachineVirtualCamera>();
            if (vcam != null)
            {
                vcam.Priority = 0; // Baja prioridad para que las cámaras cinemáticas tomen control
                Debug.Log("<color=yellow>[BossSpawner] Player virtual camera priority set to 0</color>");
            }
            else
            {
                // Si es una cámara normal, NO la desactivamos (Cinemachine necesita Main Camera activa)
                Debug.Log("<color=yellow>[BossSpawner] Player camera found but NOT disabling (Cinemachine needs it active)</color>");
            }
        }

        // Find and disable player controller
        var playerController = FindAnyObjectByType<ThirdPersonController>();
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        // Disable player input
        var playerInput = FindAnyObjectByType<UnityEngine.InputSystem.PlayerInput>();
        if (playerInput != null)
        {
            playerInput.DeactivateInput();
        }

        // Deshabilitar el sistema de combate del jugador
        var attackSystem = FindAnyObjectByType<AttackSystem>();
        if (attackSystem != null)
        {
            attackSystem.enabled = false;
            Debug.Log("<color=yellow>[BossSpawner] Player AttackSystem disabled</color>");
        }

        // Deshabilitar el PlayerController (equipamiento y bloqueo)
        var playerCtrl = FindAnyObjectByType<PlayerController>();
        if (playerCtrl != null)
        {
            // GUARDAR el estado del equipamiento del PlayerController
            savedPlayerController = playerCtrl;
            playerWasEquipped = playerCtrl.isEquipped;
            playerWasBlocking = playerCtrl.isBlocking;

            Debug.Log($"<color=cyan>[BossSpawner] Saved PlayerController state: isEquipped={playerWasEquipped}, isBlocking={playerWasBlocking}</color>");

            playerCtrl.enabled = false;
            Debug.Log("<color=yellow>[BossSpawner] PlayerController disabled</color>");
        }

        // MEJORADO: Guardar estado del jugador ANTES de resetear, luego resetear parámetros
        var playerAnimator = FindAnyObjectByType<ThirdPersonController>()?.GetComponent<Animator>();
        if (playerAnimator != null)
        {
            // GUARDAR estado actual del equipamiento y bloqueo desde el Animator
            // (ya guardamos desde PlayerController arriba, esto es redundancia por seguridad)
            if (savedPlayerController == null)
            {
                playerWasEquipped = playerAnimator.GetBool("Equipped");
                playerWasBlocking = playerAnimator.GetBool("Block");
            }

            Debug.Log($"<color=cyan>[BossSpawner] Saved animator state: Equipped={playerWasEquipped}, Blocking={playerWasBlocking}</color>");

            // Resetear TODOS los triggers
            playerAnimator.ResetTrigger("Attack");
            playerAnimator.ResetTrigger("Damage");
            playerAnimator.ResetTrigger("Death");
            playerAnimator.ResetTrigger("DrawSword");

            // Resetear TODOS los bools a estado idle
            playerAnimator.SetBool("Block", false);
            playerAnimator.SetBool("Equipped", false);
            playerAnimator.SetBool("Grounded", true);

            // Resetear floats a 0 (sin movimiento)
            playerAnimator.SetFloat("Speed", 0f);
            playerAnimator.SetFloat("MotionSpeed", 0f);

            // Forzar estado Idle/Locomotion
            playerAnimator.Play("Idle", 0, 0f);

            Debug.Log("<color=yellow>[BossSpawner] Player Animator reset to idle state</color>");
        }

        // IMPORTANTE: Desactivar TODOS los enemigos durante la cinemática
        Enemy[] allEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (Enemy enemy in allEnemies)
        {
            enemy.enabled = false;

            // También desactivar el NavMeshAgent si existe
            var navAgent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (navAgent != null)
            {
                navAgent.enabled = false;
            }

            // Desactivar animadores para congelar animaciones
            var animator = enemy.GetComponent<Animator>();
            if (animator != null)
            {
                animator.enabled = false;
            }
        }

        Debug.Log($"<color=cyan>[BossSpawner] {allEnemies.Length} enemies frozen for cinematic</color>");

        // TEMPORAL: Comentado para probar si Timeline funciona sin congelar tiempo
        // Si el jugador y enemigos se mueven, descomentar esta línea
        // Time.timeScale = 0f;
        // Debug.Log("<color=cyan>[BossSpawner] Time frozen (timeScale = 0)</color>");
    }

    /// <summary>
    /// Re-enables player controls after cinematic
    /// </summary>
    private void EnablePlayerControls()
    {
        Debug.Log("<color=cyan>[BossSpawner] Enabling player controls...</color>");

        // Reactivar SOLO las UI Canvas que desactivamos (usando la lista guardada)
        foreach (Canvas canvas in disabledCanvases)
        {
            if (canvas != null) // Verificar que no se haya destruido
            {
                canvas.gameObject.SetActive(true);
                Debug.Log($"<color=cyan>[BossSpawner] UI Canvas enabled: {canvas.gameObject.name}</color>");
            }
        }

        Debug.Log($"<color=cyan>[BossSpawner] Re-enabled {disabledCanvases.Count} UI Canvas</color>");
        disabledCanvases.Clear();

        // Restaurar el tiempo (comentado porque no lo estamos congelando ahora)
        // Time.timeScale = 1f;
        // Debug.Log("<color=cyan>[BossSpawner] Time restored (timeScale = 1)</color>");

        // Re-enable player camera priority
        if (playerCamera != null)
        {
            // Si es una Virtual Camera, restaurar su prioridad
            var vcam = playerCamera.GetComponent<Cinemachine.CinemachineVirtualCamera>();
            if (vcam != null)
            {
                vcam.Priority = 10; // Restaurar prioridad normal
                Debug.Log("<color=cyan>[BossSpawner] Player virtual camera priority restored to 10</color>");
            }
            else
            {
                // Si era una cámara normal, reactivarla
                playerCamera.SetActive(true);
            }
        }

        // Find and enable player controller
        var playerController = FindAnyObjectByType<ThirdPersonController>();
        if (playerController != null)
        {
            playerController.enabled = true;

            // NUEVO: Resetear velocidades y estados físicos
            var characterController = playerController.GetComponent<CharacterController>();
            if (characterController != null)
            {
                // Reset velocity (no podemos acceder directamente, pero moverlo 0 ayuda)
                characterController.Move(Vector3.zero);
            }

            Debug.Log("<color=cyan>[BossSpawner] Player controller enabled and physics reset</color>");
        }

        // Re-enable player input
        var playerInput = FindAnyObjectByType<UnityEngine.InputSystem.PlayerInput>();
        if (playerInput != null)
        {
            playerInput.ActivateInput();
        }

        // NUEVO: Resetear el StarterAssetsInputs para limpiar estados de input
        var starterInput = FindAnyObjectByType<StarterAssets.StarterAssetsInputs>();
        if (starterInput != null)
        {
            // Resetear todos los inputs a false/zero
            starterInput.move = Vector2.zero;
            starterInput.look = Vector2.zero;
            starterInput.jump = false;
            starterInput.sprint = false;
            starterInput.attack = false;

            Debug.Log("<color=cyan>[BossSpawner] StarterAssetsInputs reset</color>");
        }

        // Reactivar el sistema de combate del jugador
        var attackSystem = FindAnyObjectByType<AttackSystem>();
        if (attackSystem != null)
        {
            attackSystem.enabled = true;
            Debug.Log("<color=cyan>[BossSpawner] Player AttackSystem enabled</color>");
        }

        // Reactivar el PlayerController (equipamiento y bloqueo)
        var playerCtrl = FindAnyObjectByType<PlayerController>();
        if (playerCtrl != null)
        {
            // RESTAURAR el estado del equipamiento
            playerCtrl.isEquipped = playerWasEquipped;
            playerCtrl.isBlocking = playerWasBlocking;
            playerCtrl.isEquipping = false; // Asegurar que no está en medio de equipar

            Debug.Log($"<color=green>[BossSpawner] Restored PlayerController state: isEquipped={playerWasEquipped}</color>");

            playerCtrl.enabled = true;
            Debug.Log("<color=cyan>[BossSpawner] PlayerController enabled</color>");
        }

        // MEJORADO: Reactivar el Animator y RESTAURAR el estado guardado
        var playerAnimator = FindAnyObjectByType<ThirdPersonController>()?.GetComponent<Animator>();
        if (playerAnimator != null)
        {
            // El Animator ya está activo, solo necesitamos resetear parámetros

            // Resetear TODOS los triggers para evitar animaciones atascadas
            playerAnimator.ResetTrigger("Attack");
            playerAnimator.ResetTrigger("Damage");
            playerAnimator.ResetTrigger("Death");
            playerAnimator.ResetTrigger("DrawSword");
            playerAnimator.ResetTrigger("Jump");
            playerAnimator.ResetTrigger("FreeFall");

            // RESTAURAR el estado guardado de equipamiento y bloqueo
            playerAnimator.SetBool("Block", playerWasBlocking);
            playerAnimator.SetBool("Equipped", playerWasEquipped);
            playerAnimator.SetBool("Grounded", true);

            Debug.Log($"<color=green>[BossSpawner] Restored player state: Equipped={playerWasEquipped}, Blocking={playerWasBlocking}</color>");

            // Resetear floats
            playerAnimator.SetFloat("Speed", 0f);
            playerAnimator.SetFloat("MotionSpeed", 0f);

            // Forzar transición al estado correcto (Idle o IdleEquipped)
            if (playerWasEquipped)
            {
                // Si tenía la espada, volver a IdleEquipped
                playerAnimator.Play("IdleEquipped", 0, 0f);
                Debug.Log("<color=green>[BossSpawner] Player restored to IdleEquipped state</color>");
            }
            else
            {
                // Si no tenía la espada, volver a Idle normal
                playerAnimator.Play("Idle", 0, 0f);
                Debug.Log("<color=green>[BossSpawner] Player restored to Idle state</color>");
            }

            // También actualizar el animator para forzar la transición inmediata
            playerAnimator.Update(0f);

            Debug.Log("<color=cyan>[BossSpawner] Player Animator parameters reset to clean state</color>");
        }

        // Reactivar TODOS los enemigos después de la cinemática
        Enemy[] allEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (Enemy enemy in allEnemies)
        {
            enemy.enabled = true;

            // Reactivar NavMeshAgent
            var navAgent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (navAgent != null)
            {
                navAgent.enabled = true;
            }

            // Reactivar animadores
            var animator = enemy.GetComponent<Animator>();
            if (animator != null)
            {
                animator.enabled = true;
            }
        }

        Debug.Log($"<color=cyan>[BossSpawner] {allEnemies.Length} enemies unfrozen, player combat enabled</color>");
    }

    /// <summary>
    /// Public method to force spawn the boss (for testing)
    /// </summary>
    [ContextMenu("Force Spawn Boss")]
    public void ForceSpawnBoss()
    {
        bossSpawned = false;
        SpawnBoss();
    }

    /// <summary>
    /// Resets the spawner (for testing)
    /// </summary>
    [ContextMenu("Reset Spawner")]
    public void ResetSpawner()
    {
        bossSpawned = false;
        if (spawnedBoss != null)
        {
            Destroy(spawnedBoss);
        }
        Debug.Log("<color=cyan>[BossSpawner] Spawner reset</color>");
    }

    private void OnDrawGizmosSelected()
    {
        if (spawnPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(spawnPoint.position, 2f);
            Gizmos.DrawLine(spawnPoint.position, spawnPoint.position + spawnPoint.forward * 3f);

            // Draw direction arrow
            Gizmos.color = Color.blue;
            Vector3 arrowEnd = spawnPoint.position + spawnPoint.forward * 3f;
            Gizmos.DrawLine(arrowEnd, arrowEnd + (-spawnPoint.forward + spawnPoint.right) * 0.5f);
            Gizmos.DrawLine(arrowEnd, arrowEnd + (-spawnPoint.forward - spawnPoint.right) * 0.5f);
        }
    }
}
