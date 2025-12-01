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
    [Tooltip("The boss GameObject in the scene (should be inactive at start)")]
    public GameObject bossGameObject;

    [Tooltip("Optional spawn point. If null, teleports near player")]
    public Transform spawnPoint;

    [Tooltip("If true, instantiates a prefab. If false, activates existing GameObject (recommended)")]
    public bool instantiateBoss = false;

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

    [Tooltip("Time (seconds) between portal spawn and boss appearing")]
    [Range(0f, 3f)]
    public float portalLeadTime = 0.6f;

    [Tooltip("Multiplier applied to the spawn effect (portal) scale")]
    [Range(0.1f, 5f)]
    public float spawnEffectScale = 1.6f;

    [Tooltip("Vertical offset applied to the boss when it appears relative to the portal position (negative = lower)")]
    public float bossSpawnYOffset = -0.5f;

    [Header("Spawn Follow Mode")]
    [Tooltip("If true, after appearing the boss will follow the player for a short phase instead of edge-walking")]
    public bool spawnFollowPlayer = true;

    [Tooltip("Duration of the follow phase in seconds")]
    public float followPhaseDuration = 8f;

    [Tooltip("Desired follow distance from player (meters)")]
    public float followDistance = 6f;

    [Tooltip("Follow movement speed (m/s)")]
    public float followSpeed = 2f;

    [Tooltip("Lateral jitter amount (meters) to make following unsettling")]
    public float followJitter = 0.6f;

    [Header("Audio Settings")]
    [Tooltip("Audio to play when boss appears")]
    public AudioClip bossAppearSound;

    [Header("Visual Spawn Effect")]
    [Tooltip("Optional visual effect prefab (e.g. Hovl Studio portal) to play when boss teleports/spawns")]
    public GameObject spawnEffectPrefab;

    [Tooltip("How long (seconds) to keep the spawned effect before destroying it (0 = keep)")]
    public float spawnEffectDuration = 5f;

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
        if (bossGameObject != null && !instantiateBoss)
        {
            bossGameObject.SetActive(false);
            Debug.Log("<color=cyan>[BossSpawner] Boss GameObject set to inactive</color>");
        }

        // Setup cinematic timeline callbacks if available
        if (cinematicTimeline != null)
        {
            cinematicTimeline.stopped += OnCinematicFinished;
        }

        // Buscar la UI del boss (primero intenta BossHealthBarUI, luego SimpleBossHealthBar)
        bossUI = FindAnyObjectByType<BossHealthBarUI>();
        if (bossUI != null)
        {
            // Ocultar la UI al inicio
            bossUI.HideBossUI();
            Debug.Log("<color=cyan>[BossSpawner] BossHealthBarUI found and hidden</color>");
        }
        else
        {
            // Intentar con SimpleBossHealthBar
            var simpleBossUI = FindAnyObjectByType<SimpleBossHealthBar>();
            if (simpleBossUI != null)
            {
                simpleBossUI.HideBossUI();
                Debug.Log("<color=cyan>[BossSpawner] SimpleBossHealthBar found and hidden</color>");
            }
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

        // Activar y posicionar el boss ANTES de la cinemática
        if (!bossSpawned)
        {
            ActivateAndPositionBoss();
            Debug.Log("<color=magenta>[BossSpawner] Boss activated and positioned before cinematic</color>");
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

        // SIEMPRE teletransportar cerca del jugador después de la cinemática
        TeleportBossToPlayer();

        // IMPORTANTE: Habilitar el combate del boss
        EnableBossCombat();

        // Re-enable player controls
        if (disablePlayerControls)
        {
            EnablePlayerControls();
        }

        // Activar boss si no se activó durante el timeline
        if (!bossSpawned)
        {
            ActivateAndPositionBoss();
            TeleportBossToPlayer();
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
            Debug.Log("<color=cyan>[BossSpawner] BossHealthBarUI shown</color>");
        }
        else
        {
            // Intentar con SimpleBossHealthBar
            var simpleBossUI = FindAnyObjectByType<SimpleBossHealthBar>();
            if (simpleBossUI != null)
            {
                // Conectar el boss si no está conectado
                Boss boss = spawnedBoss.GetComponent<Boss>();
                if (boss != null)
                {
                    simpleBossUI.boss = boss;
                }

                simpleBossUI.ShowBossUI();
                Debug.Log("<color=cyan>[BossSpawner] SimpleBossHealthBar shown</color>");
            }
            else
            {
                Debug.LogWarning("<color=orange>[BossSpawner] No se encontró UI del boss (BossHealthBarUI o SimpleBossHealthBar)</color>");
            }
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

        // Calcular posición FRENTE al jugador
        Vector3 playerPos = player.transform.position;
        Vector3 playerForward = player.transform.forward;

        // Proyectar solo en el plano horizontal (ignorar rotación vertical)
        playerForward.y = 0;
        playerForward.Normalize();

        // Posicionar el boss FRENTE al jugador a la distancia configurada
        Vector3 teleportPosition = playerPos + playerForward * teleportDistance;

        // Usar la misma altura del jugador + offset para evitar que se entierre
        teleportPosition.y = playerPos.y + teleportHeightOffset;

        // Primero: crear el portal/efecto en la posición objetivo (portal aparece antes)
        if (spawnEffectPrefab != null)
        {
            Vector3 portalPos = teleportPosition;
            GameObject vfx = Instantiate(spawnEffectPrefab, portalPos, Quaternion.identity);

            // Escalar el portal para dar más presencia
            vfx.transform.localScale = Vector3.one * spawnEffectScale;

            // Orientar hacia el jugador
            Vector3 lookDir = (playerPos - portalPos);
            lookDir.y = 0f;
            if (lookDir.sqrMagnitude > 0.01f)
                vfx.transform.rotation = Quaternion.LookRotation(lookDir);

            vfx.transform.SetParent(this.transform, true);

            if (spawnEffectDuration > 0f)
            {
                Destroy(vfx, spawnEffectDuration);
            }

            Debug.Log($"<color=cyan>[BossSpawner] Spawned portal VFX at {portalPos} (scale x{spawnEffectScale})</color>");
        }

        // Si queremos que el portal aparezca antes, ocultamos al boss y lo posicionamos después de un delay
        if (portalLeadTime > 0f)
        {
            // Temporarily hide boss so it appears emerging from portal
            bool wasActive = spawnedBoss.activeSelf;
            spawnedBoss.SetActive(false);

            StartCoroutine(DelayedPlaceBoss(teleportPosition, playerPos, portalLeadTime, wasActive));
        }
        else
        {
            // Teletransportar el boss inmediatamente (ajustando su Y por bossSpawnYOffset)
            Vector3 finalPos = teleportPosition + Vector3.up * bossSpawnYOffset;
            spawnedBoss.transform.position = finalPos;

            // Hacer que el boss mire AL JUGADOR (cara a cara)
            Vector3 lookDirection = (playerPos - finalPos).normalized;
            lookDirection.y = 0; // Mantener en plano horizontal
            if (lookDirection.magnitude > 0.1f)
            {
                spawnedBoss.transform.rotation = Quaternion.LookRotation(lookDirection);
            }

            Debug.Log($"<color=magenta>[BossSpawner] Boss teleported in front of player! Distance: {Vector3.Distance(playerPos, finalPos):F2}m, Position: {finalPos}</color>");

            // Efecto visual de teletransporte (opcional)
            PlaySpawnEffects(teleportPosition);
        }
    }

    /// <summary>
    /// Spawns the boss with a delay
    /// </summary>
    private IEnumerator SpawnBossDelayed()
    {
        yield return new WaitForSeconds(spawnDelay);
        ActivateAndPositionBoss();
    }

    /// <summary>
    /// Activa el boss en la escena y lo posiciona (para la cinemática)
    /// </summary>
    public void ActivateAndPositionBoss()
    {
        if (bossSpawned)
        {
            Debug.LogWarning("<color=yellow>[BossSpawner] Boss already spawned!</color>");
            return;
        }

        if (bossGameObject == null)
        {
            Debug.LogError("<color=red>[BossSpawner] No boss GameObject assigned!</color>");
            return;
        }

        Vector3 initialPosition;
        Quaternion initialRotation;

        // Usar spawn point si está asignado (para la cinemática)
        if (spawnPoint != null)
        {
            initialPosition = spawnPoint.position;
            initialRotation = spawnPoint.rotation;
            Debug.Log($"<color=green>[BossSpawner] Using spawn point: {spawnPoint.name}</color>");
        }
        else
        {
            // Si no hay spawn point, dejar en su posición actual
            initialPosition = bossGameObject.transform.position;
            initialRotation = bossGameObject.transform.rotation;
            Debug.Log("<color=yellow>[BossSpawner] No spawn point, using boss current position</color>");
        }

        // Posicionar el boss
        bossGameObject.transform.position = initialPosition;
        bossGameObject.transform.rotation = initialRotation;

        // Activar el boss GameObject PRIMERO
        bossGameObject.SetActive(true);
        spawnedBoss = bossGameObject;

        // Ensure the boss has a God component (used by GodController for invulnerability/damage)
        var godComp = spawnedBoss.GetComponent<God>();
        if (godComp == null)
        {
            godComp = spawnedBoss.AddComponent<God>();
            Debug.Log("<color=yellow>[BossSpawner] Added missing 'God' component to boss at spawn time.</color>");
        }

        // Configurar el Animator DESPUÉS de activar (para evitar crash de memoria)
        StartCoroutine(ConfigureBossAnimatorNextFrame(bossGameObject));

        // Play spawn sound
        if (bossAppearSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(bossAppearSound);
        }

        bossSpawned = true;
        Debug.Log($"<color=green>[BossSpawner] Boss activated at {initialPosition}</color>");
    }

    /// <summary>
    /// Configura el Animator del boss en el siguiente frame (para evitar crashes de memoria)
    /// </summary>
    private IEnumerator ConfigureBossAnimatorNextFrame(GameObject boss)
    {
        // Wait until next frame and ensure the boss Animator is active and initialized
        float timeout = 0.5f;
        float timer = 0f;

        while (timer < timeout)
        {
            if (boss == null) yield break;
            if (boss.activeInHierarchy)
            {
                var bossAnimator = boss.GetComponent<Animator>();
                if (bossAnimator != null && bossAnimator.enabled && bossAnimator.runtimeAnimatorController != null)
                {
                    // Asegurar que está en el aire
                    bossAnimator.SetBool("Grounded", false);
                    bossAnimator.SetFloat("Speed", 0f);
                    bossAnimator.SetFloat("MotionSpeed", 0f);
                    bossAnimator.SetBool("Moving", false);

                    // Forzar el estado Fall explícitamente (guardado en try)
                    try { bossAnimator.Play("Fall", 0, 0f); }
                    catch { Debug.LogWarning("<color=yellow>[BossSpawner] Unable to Play 'Fall' state - state may not exist.</color>"); }

                    Debug.Log("<color=cyan>[BossSpawner] Boss animator configured: Grounded=false, State=Fall</color>");
                    yield break;
                }
            }

            timer += Time.deltaTime;
            yield return null;
        }

        Debug.LogWarning("<color=yellow>[BossSpawner] Timed out waiting for boss Animator to initialize.</color>");
    }

    /// <summary>
    /// Espera un tiempo y coloca/activa al boss para que parezca salir del portal
    /// </summary>
    private IEnumerator DelayedPlaceBoss(Vector3 teleportPosition, Vector3 playerPos, float delay, bool restoreActive)
    {
        yield return new WaitForSeconds(delay);

        if (spawnedBoss == null)
        {
            Debug.LogWarning("<color=orange>[BossSpawner] spawnedBoss lost before delayed place.</color>");
            yield break;
        }

        Vector3 finalPos = teleportPosition + Vector3.up * bossSpawnYOffset;
        spawnedBoss.transform.position = finalPos;

        // Hacer que el boss mire AL JUGADOR (cara a cara)
        Vector3 lookDirection = (playerPos - finalPos).normalized;
        lookDirection.y = 0; // Mantener en plano horizontal
        if (lookDirection.magnitude > 0.1f)
        {
            spawnedBoss.transform.rotation = Quaternion.LookRotation(lookDirection);
        }

        // Reactivar el boss si estaba activo originalmente
        spawnedBoss.SetActive(restoreActive);

        Debug.Log($"<color=magenta>[BossSpawner] Boss placed after portal delay. Position: {finalPos}</color>");

        // Play spawn effects at the same position (in case not spawned earlier)
        PlaySpawnEffects(teleportPosition);

        // Configure animator next frame to avoid race conditions
        StartCoroutine(ConfigureBossAnimatorNextFrame(spawnedBoss));

        // Start the chosen boss post-spawn phase (follow or edge-walk) if the boss has a GodController
        var godCtrl = spawnedBoss.GetComponent<GodController>();
        if (godCtrl != null)
        {
            if (spawnFollowPlayer)
            {
                godCtrl.StartFollowPhase(followPhaseDuration, followDistance, followSpeed, followJitter);
                Debug.Log("<color=cyan>[BossSpawner] Started boss follow-player phase via GodController.</color>");
            }
            else
            {
                godCtrl.StartPhase();
                Debug.Log("<color=cyan>[BossSpawner] Started boss edge-walk phase via GodController.</color>");
            }
        }
    }

    /// <summary>
    /// Play visual/audio effects when boss spawns
    /// </summary>
    private void PlaySpawnEffects(Vector3 position)
    {
        // You can add particle effects, screen shake, etc. here
        Debug.Log($"<color=yellow>[BossSpawner] Playing spawn effects at {position}</color>");

        // If a spawnEffectPrefab is assigned in the inspector (e.g. Hovl Studio portal), instantiate it
        if (spawnEffectPrefab != null)
        {
            GameObject vfx = Instantiate(spawnEffectPrefab, position, Quaternion.identity);

            // Orient the VFX to face the player if possible
            var player = FindAnyObjectByType<ThirdPersonController>();
            if (player != null)
            {
                Vector3 lookDir = player.transform.position - position;
                lookDir.y = 0f;
                if (lookDir.sqrMagnitude > 0.01f)
                    vfx.transform.rotation = Quaternion.LookRotation(lookDir);
            }

            // Parent to spawner for scene cleanliness
            vfx.transform.SetParent(this.transform, true);

            if (spawnEffectDuration > 0f)
            {
                Destroy(vfx, spawnEffectDuration);
            }
        }
        else
        {
            // Example fallback: play any ParticleSystem child
            ParticleSystem spawnVFX = GetComponentInChildren<ParticleSystem>();
            if (spawnVFX != null)
            {
                spawnVFX.transform.position = position;
                spawnVFX.Play();
                if (spawnEffectDuration > 0f)
                {
                    Destroy(spawnVFX.gameObject, spawnEffectDuration);
                }
            }
        }
    }

    /// <summary>
    /// Public helper: teleports the boss in front of the player and optionally plays a given effect prefab at the boss position.
    /// This is intended for use by separate scripts (e.g. a Player spawn notifier).
    /// </summary>
    public void TeleportBossToPlayerWithEffect(GameObject effectPrefab, float effectDuration = 0f)
    {
        // Ensure boss is active/instantiated
        if (!bossSpawned)
        {
            ActivateAndPositionBoss();
        }

        // Teleport using existing logic (this will also call PlaySpawnEffects using the configured spawnEffectPrefab)
        TeleportBossToPlayer();

        // If an explicit effectPrefab was provided, instantiate it at the boss's position
        if (effectPrefab != null && spawnedBoss != null)
        {
            GameObject vfx = Instantiate(effectPrefab, spawnedBoss.transform.position, Quaternion.identity);

            // orient toward player
            var player = FindAnyObjectByType<ThirdPersonController>();
            if (player != null)
            {
                Vector3 lookDir = player.transform.position - spawnedBoss.transform.position;
                lookDir.y = 0f;
                if (lookDir.sqrMagnitude > 0.01f)
                    vfx.transform.rotation = Quaternion.LookRotation(lookDir);
            }

            vfx.transform.SetParent(this.transform, true);

            if (effectDuration > 0f)
                Destroy(vfx, effectDuration);
        }
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
        ActivateAndPositionBoss();
        TeleportBossToPlayer();
        EnableBossCombat();
    }

    [ContextMenu("Force Spawn Boss With VFX")]
    public void ForceSpawnBossWithVFX()
    {
        bossSpawned = false;
        ActivateAndPositionBoss();

        // Use the configured spawnEffectPrefab and duration
        TeleportBossToPlayerWithEffect(spawnEffectPrefab, spawnEffectDuration);

        EnableBossCombat();
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
