using System.Collections;
using UnityEngine;

/// <summary>
/// Controls the boss "edge walk" phase: makes the boss walk around a circle
/// near its spawn position, disables damage (invulnerable) during the phase,
/// and then ends the phase.
/// </summary>
public class GodController : MonoBehaviour
{
    [Header("Edge Phase Settings")]
    [Tooltip("Duration in seconds for the edge-walking phase")]
    public float edgePhaseDuration = 120f;

    [Tooltip("Radius (meters) around the spawn center to walk; adjust to match platform edges")]
    public float edgeRadius = 4f;

    [Tooltip("Walking speed (m/s) while patrolling the edge")]
    public float walkSpeed = 1.8f;

    [Tooltip("If true, boss will face the player while walking (recommended)")]
    public bool facePlayerDuringWalk = true;
    [Tooltip("How much the boss should blend facing the player vs facing movement direction (0 = only movement, 1 = only player)")]
    [Range(0f, 1f)]
    public float facePlayerWeight = 0.3f;

    [Tooltip("How quickly the boss rotates to face the movement direction (degrees per second)")]
    public float rotationSpeed = 360f;

    [Header("Ground Sampling")]
    [Tooltip("Layers considered ground when sampling height")]
    public LayerMask groundLayer = ~0;

    [Tooltip("How high above target to start the ground raycast")]
    public float groundRayStartHeight = 5f;

    [Tooltip("YOffset to add above the sampled ground hit to place the boss correctly")]
    public float groundYOffset = 0.05f;

    [Header("Animator Smoothing")]
    [Tooltip("How quickly the MotionSpeed/Speed parameter interpolates to the real movement (higher = snappier)")]
    [Range(1f, 20f)]
    public float speedSmooth = 8f;

    [Header("Follow Aura (Burn)")]
    [Tooltip("Optional aura prefab (e.g. Star aura). If null, will try to find a child named 'Star aura'.")]
    public GameObject followAuraPrefab;

    [Tooltip("Local scale multiplier applied to instantiated aura")]
    public float followAuraScale = 1f;

    [Tooltip("Radius (meters) of the burning aura that deals damage to the player")]
    public float followAuraRadius = 2.5f;

    [Tooltip("Burn damage per second applied to the player while inside the aura")]
    public float burnDPS = 5f;

    [Header("Aura Sound")]
    [Tooltip("Sound played once when player enters the aura")]
    public AudioClip followAuraEnterSfx;

    [Tooltip("Loop sound played while player remains inside the aura (optional)")]
    public AudioClip followAuraLoopSfx;

    [Tooltip("Sound played once when player exits the aura")]
    public AudioClip followAuraExitSfx;

    [Header("Teleport / Projectiles Phase")]
    [Tooltip("Duration of the teleport+projectile phase (seconds). Placeholder — replace with real logic later.")]
    public float teleportPhaseDuration = 60f;

    [Tooltip("Delay before starting teleport phase after follow ends (seconds)")]
    public float teleportPhaseDelay = 0.3f;

    [Tooltip("If true, the phase will start automatically when told to StartPhase(); otherwise start manually")]
    public bool startOnEnable = false;

    [Header("Jump Hit VFX")]
    [Tooltip("Name of the child GameObject to activate when the jump animation finishes (e.g. 'Electro hit')")]
    public string electroHitChildName = "Electro hit";

    [Tooltip("How long (seconds) to keep the Electro hit GameObject active after the jump finishes (0 = don't auto-disable)")]
    public float electroHitActiveDuration = 1.0f;

    [Header("Jump Attack Settings")]
    [Tooltip("Sound to play when the jump attack lands")]
    public AudioClip jumpAttackSfx;

    [Tooltip("Radius (meters) of the area damage when the jump attack lands")]
    public float jumpAttackRadius = 3f;

    [Tooltip("Damage applied to the player if inside the radius when the jump attack lands")]
    public int jumpAttackDamage = 10;

    [Tooltip("Volume multiplier applied when playing the jump attack SFX (Inspector can use >1 to boost)")]
    public float jumpAttackVolume = 1.2f;

    [Tooltip("Scale multiplier applied to Electro hit ParticleSystems as a visual fallback")]
    public float electroParticleScale = 1.5f;

    [Header("Electro Hit Fallback")]
    [Tooltip("Optional fallback particle prefab to spawn at the Electro hit position if the child's particles do not show. Drag a prefab here.")]
    public GameObject electroHitPrefab;

    [Tooltip("How long (seconds) to keep the fallback prefab active before destroying it")]
    public float electroHitPrefabDuration = 1f;

    [Tooltip("Priority for the temporary jump SFX AudioSource (lower = higher priority). Use 0 for highest priority.")]
    public int jumpAttackAudioPriority = 64;
    [Tooltip("Priority for teleport audio one-shot (lower = higher priority). Default 64.")]
    public int teleportAudioPriority = 64;

    [Header("Teleport Visual / SFX")]
    [Tooltip("Optional teleport prefab to spawn when teleport phase starts. Drag the prefab (e.g. Teleport Variant.prefab) here.")]
    public GameObject teleportPrefab;

    [Tooltip("How long (seconds) to keep the teleport prefab alive before destroying it")]
    public float teleportPrefabDuration = 2f;

    [Tooltip("Sound to play when the teleport prefab is spawned")]
    public AudioClip teleportSfx;

    [Header("Boss Music")]
    [Tooltip("Looping music clip to play while the boss is in the follow phase")]
    public AudioClip bossLoopMusic;

    [Tooltip("Volume for the boss loop music (0..1)")]
    [Range(0f, 1f)]
    public float bossMusicVolume = 0.7f;

    [Tooltip("If true, the boss loop music will start when the follow phase begins")]
    public bool bossMusicLoopOnFollow = true;

    private AudioSource bossMusicSource;

    [Header("UI")]
    [Tooltip("Display name to show on the boss health UI (overrides default)")]
    public string bossDisplayName = "BOSS";

    [Header("Meteor Rain (Second Phase)")]
    [Tooltip("Prefab for the meteor object. If null, a simple sphere will be used.")]
    public GameObject meteorPrefab;

    [Tooltip("Particle prefab to spawn on meteor impact (e.g. Laser AOE prefab). Assign the prefab from Assets/Hovl Studio/.../Laser AOE.prefab.")]
    public GameObject meteorImpactPrefab;

    [Tooltip("Height (world units) above the player where meteors spawn")]
    public float meteorSpawnHeight = 12f;

    [Tooltip("Interval (seconds) between spawned meteors during the rain")]
    public float meteorSpawnInterval = 0.5f;

    [Tooltip("Downward speed for meteors (m/s)")]
    public float meteorFallSpeed = 12f;

    [Tooltip("Damage per second applied by the impact AoE while the player stays inside")]
    public float meteorAoEDamagePerSecond = 4f;

    [Tooltip("Radius (meters) of the impact AoE")]
    public float meteorAoERadius = 1.5f;

    [Tooltip("How long (seconds) the impact AoE persists and applies damage")]
    public float meteorAoEDuration = 2f;

    // Internal cancellation token for meteor coroutine
    private Coroutine meteorRainCoroutine = null;
    [Tooltip("Sound to play when a meteor spawns (appears)")]
    public AudioClip meteorSpawnSfx;

    [Tooltip("Volume for meteor spawn SFX")]
    public float meteorSpawnSfxVolume = 1f;

    [Tooltip("Priority for meteor spawn SFX (lower = higher priority). Use 0 for highest priority so it won't be interrupted.")]
    public int meteorSpawnSfxPriority = 0;
    [Tooltip("Sound to play when a meteor impacts (assign the clip used for each impact)")]
    public AudioClip meteorImpactSfx;

    [Tooltip("Volume for meteor impact SFX")]
    public float meteorImpactSfxVolume = 1f;

    [Tooltip("Priority for meteor impact SFX (lower = higher priority). Use 0 for highest priority so other sounds won't interrupt it.")]
    public int meteorImpactSfxPriority = 0;

    private Vector3 centerPosition;
    private float currentAngle = 0f;
    private bool isWalking = false;
    private God god;
    private Animator animator;
    private GameObject player;
    private StarterAssets.ThirdPersonController playerController;
    private Vector3 prevPosition;
    private float smoothedSpeed = 0f;
    private AudioSource audioSource;
    private bool playerWasInAura = false;
    private GameObject activeAuraInstance = null;
    private bool electroActivatedThisJump = false;
    // Cached renderer/collider states for Hide/Show
    private Renderer[] _cachedRenderers;
    private bool[] _cachedRenderersEnabled;
    private Collider[] _cachedColliders;
    private bool[] _cachedCollidersEnabled;
    private bool _animatorWasEnabled = true;

    private void Awake()
    {
        centerPosition = transform.position;
        // Try to resolve God component robustly: self, children, or any in scene
        god = GetComponent<God>();
        if (god == null)
        {
            god = GetComponentInChildren<God>(true);
            if (god != null)
                Debug.Log($"[GodController] Auto-resolved God via GetComponentInChildren -> {god.gameObject.name}");
        }
        if (god == null)
        {
            var anyGod = FindAnyObjectByType<God>();
            if (anyGod != null)
            {
                god = anyGod;
                Debug.Log($"[GodController] Fallback: found God via FindAnyObjectByType -> {god.gameObject.name}");
            }
        }
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        // Ensure animator does not apply root motion by default (we drive movement manually)
        if (animator != null)
        {
            animator.applyRootMotion = false;
        }

        // Ensure an AudioSource exists for aura SFX (separate from music)
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f; // 3D sound
            audioSource.priority = 128; // Normal priority for SFX
        }

        // Create a dedicated AudioSource for boss music (never interrupted)
        if (bossMusicSource == null && bossLoopMusic != null)
        {
            bossMusicSource = gameObject.AddComponent<AudioSource>();
            bossMusicSource.playOnAwake = false;
            bossMusicSource.loop = true;
            bossMusicSource.spatialBlend = 0f; // 2D music
            bossMusicSource.priority = 0; // Highest priority - NEVER gets interrupted
            bossMusicSource.ignoreListenerPause = true;
            bossMusicSource.clip = bossLoopMusic;
            bossMusicSource.volume = Mathf.Clamp01(bossMusicVolume);
            Debug.Log("<color=green>[GodController] Dedicated boss music AudioSource created with priority 0 (highest)</color>");
        }

        // Try find player by tag, fallback to StarterAssets controller
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            var p = FindAnyObjectByType<StarterAssets.ThirdPersonController>();
            if (p != null) player = p.gameObject;
        }
        if (god == null)
        {
            Debug.LogWarning("<color=orange>[GodController] No God component found on boss; invulnerability API will be unavailable. Will try to resolve again before applying phase damage.</color>");
        }
    }

    private void OnEnable()
    {
        if (startOnEnable)
        {
            StartPhase();
        }
    }

    /// <summary>
    /// Starts the edge-walk phase: sets invulnerable and begins walking for configured duration
    /// </summary>
    public void StartPhase()
    {
        // Recompute center at start in case boss was moved by spawner
        centerPosition = transform.position;

        StopAllCoroutines();
        StartCoroutine(EdgePhaseCoroutine());
    }

    /// <summary>
    /// Starts a follow-player phase where the boss follows the player maintaining a distance.
    /// </summary>
    /// <param name="duration">How long to follow (seconds)</param>
    /// <param name="distance">Desired following distance in meters</param>
    /// <param name="followSpeed">Speed of following movement</param>
    /// <param name="jitterAmount">Max lateral jitter in meters to make movement unsettling</param>
    public void StartFollowPhase(float duration = 60f, float distance = 6f, float followSpeed = 3f, float jitterAmount = 0.6f)
    {
        StopAllCoroutines();
        StartCoroutine(FollowPhaseCoroutine(duration, distance, followSpeed, jitterAmount));
    }

    private IEnumerator FollowPhaseCoroutine(float duration, float distance, float followSpeed, float jitterAmount)
    {
        // Find player if needed
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                var p = FindAnyObjectByType<StarterAssets.ThirdPersonController>();
                if (p != null) player = p.gameObject;
            }
        }

        if (player == null)
        {
            Debug.LogWarning("<color=orange>[GodController] No player found for follow phase.</color>");
            yield break;
        }

        // Invulnerable while following
        if (god != null) god.SetInvulnerable(true);

        float timer = 0f;
        smoothedSpeed = 0f;
        prevPosition = transform.position;

        // Try to enable or create aura visual if requested
        activeAuraInstance = null;
        if (followAuraPrefab != null)
        {
            activeAuraInstance = Instantiate(followAuraPrefab, transform.position, Quaternion.identity, transform);
            activeAuraInstance.transform.localPosition = Vector3.zero;
            // Optional scale control
            activeAuraInstance.transform.localScale = Vector3.one * followAuraScale;
            // start boss music if configured
            try { if (bossMusicLoopOnFollow) StartBossMusic(); } catch { }
        }
        else
        {
            // Try find existing child by name (common name: "Star aura") and enable it
            var child = transform.Find("Star aura");
            if (child != null)
            {
                activeAuraInstance = child.gameObject;
                activeAuraInstance.SetActive(true);
                try { if (bossMusicLoopOnFollow) StartBossMusic(); } catch { }
            }
        }

        // Try to show boss UI (if present) and set display name
        try
        {
            var bossComp = GetComponent<Boss>();
            var bossUI = FindObjectOfType<SimpleBossHealthBar>(true);
            if (bossUI != null)
            {
                if (bossComp != null) bossUI.boss = bossComp;
                bossUI.bossName = string.IsNullOrEmpty(bossDisplayName) ? (bossComp != null ? bossComp.gameObject.name : bossUI.bossName) : bossDisplayName;
                bossUI.ShowBossUI();
                Debug.Log("[GodController] SimpleBossHealthBar found and shown.");
            }
        }
        catch { }

        float burnAccumulator = 0f;
        playerWasInAura = false;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            // Compute desired position behind player at given distance
            Vector3 playerPos = player.transform.position;
            Vector3 behind = -player.transform.forward.normalized * distance;
            Vector3 baseTarget = playerPos + behind;

            // Add small lateral jitter using Perlin noise for an unsettling movement
            float noise = (Mathf.PerlinNoise(Time.time * 0.8f, 0f) - 0.5f) * 2f; // -1..1
            Vector3 lateral = Vector3.Cross(Vector3.up, player.transform.forward).normalized * (noise * jitterAmount);
            Vector3 target = baseTarget + lateral;

            // Sample ground for Y
            Vector3 rayStart = new Vector3(target.x, centerPosition.y + groundRayStartHeight, target.z);
            float sampledY = centerPosition.y;
            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, groundRayStartHeight * 2f, groundLayer))
            {
                sampledY = hit.point.y + groundYOffset;
            }
            target.y = sampledY;

            // Rotate to face player (creepy stare)
            Vector3 toPlayer = player.transform.position - transform.position;
            toPlayer.y = 0f;
            if (toPlayer.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(toPlayer.normalized);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }

            // Move toward target but stop at a small threshold so it doesn't bump into player
            float step = followSpeed * Time.deltaTime;
            Vector3 newPos = Vector3.MoveTowards(transform.position, target, step);
            transform.position = new Vector3(newPos.x, Mathf.MoveTowards(transform.position.y, target.y, (followSpeed * 0.5f) * Time.deltaTime), newPos.z);

            // Animator update
            if (animator != null)
            {
                float currentSpeed = 0f;
                if (Time.deltaTime > 0f) currentSpeed = Vector3.Distance(transform.position, prevPosition) / Time.deltaTime;
                smoothedSpeed = Mathf.Lerp(smoothedSpeed, currentSpeed, Mathf.Clamp01(speedSmooth * Time.deltaTime));
                animator.SetFloat("Speed", smoothedSpeed);
                animator.SetFloat("MotionSpeed", smoothedSpeed);
                animator.SetBool("Moving", smoothedSpeed > 0.05f);
            }
            // Apply burn damage if player inside aura radius
            if (player != null && burnDPS > 0f)
            {
                float dist = Vector3.Distance(player.transform.position, transform.position);
                if (dist <= followAuraRadius)
                {
                    // Enter aura
                    if (!playerWasInAura)
                    {
                        playerWasInAura = true;
                        if (followAuraEnterSfx != null)
                            audioSource.PlayOneShot(followAuraEnterSfx);

                        if (followAuraLoopSfx != null)
                        {
                            audioSource.clip = followAuraLoopSfx;
                            audioSource.loop = true;
                            audioSource.Play();
                        }
                    }

                    burnAccumulator += burnDPS * Time.deltaTime;
                    if (playerController == null)
                        playerController = player.GetComponent<StarterAssets.ThirdPersonController>();

                    if (playerController != null && burnAccumulator >= 1f)
                    {
                        int dmg = Mathf.FloorToInt(burnAccumulator);
                        playerController.TakeDamage(dmg);
                        burnAccumulator -= dmg;
                    }
                }
                else
                {
                    // Exited aura
                    if (playerWasInAura)
                    {
                        playerWasInAura = false;
                        // stop loop
                        if (audioSource != null && audioSource.isPlaying && audioSource.clip == followAuraLoopSfx)
                        {
                            audioSource.Stop();
                            audioSource.loop = false;
                        }
                        if (followAuraExitSfx != null)
                            audioSource.PlayOneShot(followAuraExitSfx);
                    }

                    // if outside aura, slowly decay accumulator (optional)
                    burnAccumulator = Mathf.Max(0f, burnAccumulator - (burnDPS * 0.5f * Time.deltaTime));
                }
            }

            prevPosition = transform.position;
            yield return null;
        }

        // End follow phase
        if (god != null) god.SetInvulnerable(false);
        if (animator != null)
        {
            animator.SetBool("Moving", false);
            animator.SetFloat("Speed", 0f);
            animator.SetFloat("MotionSpeed", 0f);
        }

        // Disable or destroy aura visual (ensure done even if coroutines were interrupted)
        DisableActiveAura();

        // NO detenemos la música aquí - debe continuar durante todas las fases del boss
        // try { StopBossMusic(); } catch { }

        Debug.Log("<color=green>[GodController] Follow phase complete. Boss vulnerable again.</color>");

        // Play JumpAttack animation, then start teleport/projectile phase
        StartCoroutine(PlayJumpThenTeleportCoroutine(teleportPhaseDelay));
    }

    private IEnumerator PlayJumpThenTeleportCoroutine(float delayBefore)
    {
        // Optional small delay before starting the jump
        if (delayBefore > 0f)
            yield return new WaitForSeconds(delayBefore);

        float waitTime = 0.5f; // fallback
        string animSubstring = "JUMP"; // search for any clip that contains JUMP

        string chosenClipName = null;
        if (animator != null)
        {
            var controller = animator.runtimeAnimatorController;
            if (controller != null)
            {
                // prefer exact JUMPATTACK (case-insensitive)
                foreach (var clip in controller.animationClips)
                {
                    if (clip == null) continue;
                    if (clip.name.Equals("JUMPATTACK", System.StringComparison.OrdinalIgnoreCase))
                    {
                        chosenClipName = clip.name;
                        waitTime = clip.length;
                        break;
                    }
                }

                // If not found, try any clip that contains 'JUMP' or 'ATTACK'
                if (chosenClipName == null)
                {
                    foreach (var clip in controller.animationClips)
                    {
                        if (clip == null) continue;
                        if (clip.name.IndexOf(animSubstring, System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                            clip.name.IndexOf("ATTACK", System.StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            chosenClipName = clip.name;
                            waitTime = clip.length;
                            break;
                        }
                    }
                }

                // If we found a candidate clip, verify the animator actually has a state with that name
                if (!string.IsNullOrEmpty(chosenClipName))
                {
                    bool hasState = false;
                    try
                    {
                        int hash = Animator.StringToHash(chosenClipName);
                        hasState = animator.HasState(0, hash);
                    }
                    catch { hasState = false; }

                    if (hasState)
                    {
                        Debug.Log($"[GodController] Playing jump state by name: {chosenClipName}");
                        try { animator.Play(chosenClipName, 0, 0f); }
                        catch (System.Exception ex) { Debug.LogWarning($"[GodController] Failed to Play animator state '{chosenClipName}': {ex.Message}"); }
                    }
                    else
                    {
                        // Try playing by clip name anyway (common when state name == clip name)
                        Debug.Log($"[GodController] Candidate clip '{chosenClipName}' found but animator.HasState returned false. Attempting Play by clip name anyway.");
                        try { animator.Play(chosenClipName, 0, 0f); }
                        catch (System.Exception ex) { Debug.LogWarning($"[GodController] Play fallback failed for '{chosenClipName}': {ex.Message}"); }
                    }
                }
                else
                {
                    Debug.LogWarning("[GodController] Jump clip not found by name; will try animator parameters/triggers next.");
                }
            }

            // If we still didn't play anything, try to find animator parameters that look like a jump/attack trigger and set them
            bool triggered = false;
            foreach (var p in animator.parameters)
            {
                var nameLower = p.name.ToLowerInvariant();
                if (nameLower.Contains("jump") || nameLower.Contains("attack"))
                {
                    if (p.type == AnimatorControllerParameterType.Trigger)
                    {
                        animator.SetTrigger(p.name);
                        Debug.Log($"[GodController] Set trigger parameter '{p.name}' to attempt jump animation.");
                        triggered = true;
                    }
                    else if (p.type == AnimatorControllerParameterType.Bool)
                    {
                        animator.SetBool(p.name, true);
                        Debug.Log($"[GodController] Set bool parameter '{p.name}' = true to attempt jump animation.");
                        triggered = true;
                    }
                }
            }

            if (!triggered)
            {
                // Last resort: attempt to Play common state names to give more tries before giving up
                string[] commonNames = new string[] { "JUMPATTACK", "JumpAttack", "Jump Attack", "Jump", "Attack" };
                foreach (var n in commonNames)
                {
                    try
                    {
                        if (animator.HasState(0, Animator.StringToHash(n)))
                        {
                            Debug.Log($"[GodController] Playing common state name '{n}' via HasState check.");
                            animator.Play(n, 0, 0f);
                            triggered = true;
                            break;
                        }
                    }
                    catch { }
                }

                if (!triggered)
                {
                    Debug.LogWarning("[GodController] Could not find jump/attack state or parameter. Please verify the Animator has a state or a trigger parameter for the jump attack.");
                }
            }

            // Deterministic attempt: if the Animator has a trigger param named 'JumpAttack', set it
            if (animator != null)
            {
                foreach (var p in animator.parameters)
                {
                    if (p.name == "JumpAttack" && p.type == AnimatorControllerParameterType.Trigger)
                    {
                        animator.SetTrigger("JumpAttack");
                        Debug.Log("[GodController] Set trigger 'JumpAttack' on animator as deterministic fallback.");
                        // small yield to give animator a frame to respond
                        yield return null;
                        Debug.Log($"[GodController] After SetTrigger: current stateInfo.nameHash={animator.GetCurrentAnimatorStateInfo(0).shortNameHash}, normalizedTime={animator.GetCurrentAnimatorStateInfo(0).normalizedTime}");
                        break;
                    }
                }
            }

            // Activate Electro hit immediately when we trigger/play the jump animation
            try
            {
                electroActivatedThisJump = false;
                if (!string.IsNullOrEmpty(electroHitChildName))
                {
                    var earlyElectro = FindDeepChild(transform, electroHitChildName);
                    if (earlyElectro != null)
                    {
                        ActivateElectroHit(earlyElectro, Mathf.Max(0.05f, electroHitActiveDuration));
                        electroActivatedThisJump = true;
                    }
                }
            }
            catch { }
        }

        // Wait until the animator reports a clip containing 'JUMP' as current, then wait for that state's normalized time >= 1
        float waitTimer = 0f;
        float maxWait = Mathf.Max(1f, waitTime * 2f);
        bool foundJumpClip = false;

        while (waitTimer < maxWait)
        {
            waitTimer += Time.deltaTime;

            if (animator != null)
            {
                var clips = animator.GetCurrentAnimatorClipInfo(0);
                if (clips != null && clips.Length > 0)
                {
                    foreach (var ci in clips)
                    {
                        if (ci.clip != null && ci.clip.name.IndexOf("JUMP", System.StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            foundJumpClip = true;
                            // wait until state normalized time >= 1 (clip finished)
                            var state = animator.GetCurrentAnimatorStateInfo(0);
                            if (state.normalizedTime >= 1f)
                            {
                                waitTimer = maxWait; // exit outer loop
                            }
                            break;
                        }
                    }
                }
            }

            if (foundJumpClip == false)
            {
                // if not yet found, fallback small wait to allow animator to enter state
                yield return null;
            }
            else
            {
                // keep waiting for the state to complete
                yield return null;
            }
        }

        // If we never detected the jump clip, fallback to waitTime
        if (!foundJumpClip)
        {
            yield return new WaitForSeconds(waitTime + 0.05f);
        }

        // Log current animator clips for debugging
        if (animator != null)
        {
            var cur = animator.GetCurrentAnimatorClipInfo(0);
            if (cur != null && cur.Length > 0)
            {
                foreach (var ci in cur)
                {
                    if (ci.clip != null)
                        Debug.Log($"[GodController] Current animator clip after wait: {ci.clip.name}");
                }
            }
            else
            {
                Debug.Log("[GodController] No current animator clip info after wait.");
            }
        }

        // Ensure aura is disabled before the jump hit
        DisableActiveAura();

        // Play jump attack sound if assigned (controller-level fallback)
        if (jumpAttackSfx != null && audioSource != null)
        {
            audioSource.PlayOneShot(jumpAttackSfx);
        }

        // Activate Electro hit child (if present) right when the jump animation ends
        Transform electroT = null;
        if (!string.IsNullOrEmpty(electroHitChildName))
        {
            // Use a recursive find in case the child is nested
            electroT = FindDeepChild(transform, electroHitChildName);
            if (electroT != null && electroT.gameObject != null)
            {
                try
                {
                    // Attach or get a TimedAutoDisable helper on the target so scheduled disable survives StopAllCoroutines()
                    var tad = electroT.gameObject.GetComponent<TimedAutoDisable>();
                    if (tad == null)
                        tad = electroT.gameObject.AddComponent<TimedAutoDisable>();

                    // Ensure the object is active then schedule auto-disable
                    try { Debug.Log($"[GodController] Electro hit target found: {GetTransformPath(electroT)}; activeBefore={electroT.gameObject.activeSelf}; position={electroT.position}"); } catch { }
                    try { electroT.gameObject.SetActive(true); } catch { }
                    // Force Electro hit active for exactly electroHitPrefabDuration (user-configurable)
                    float forceDuration = Mathf.Max(0.05f, electroHitPrefabDuration);
                    tad.ActivateForSeconds(forceDuration);

                    // If a fallback prefab is assigned, instantiate it now so the visual is guaranteed
                    try
                    {
                        if (electroHitPrefab != null && electroT != null)
                        {
                            var prefabGo = Instantiate(electroHitPrefab, electroT.position, Quaternion.identity);
                            if (prefabGo != null)
                            {
                                try { prefabGo.transform.localScale *= Mathf.Max(0.01f, electroParticleScale); } catch { }
                                Destroy(prefabGo, Mathf.Max(0.1f, forceDuration));
                                Debug.Log("[GodController] Instantiated electroHitPrefab immediately as guaranteed visual fallback.");
                            }
                        }
                    }
                    catch (System.Exception ex) { Debug.LogWarning($"[GodController] electroHitPrefab instantiation error: {ex.Message}"); }

                    // Play any AudioSource(s) on the Electro hit child so its own SFX are heard
                    try
                    {
                        var electroAudioSources = electroT.gameObject.GetComponentsInChildren<AudioSource>(true);
                        bool audioPlayed = false;
                        foreach (var ea in electroAudioSources)
                        {
                            try
                            {
                                if (ea == null) continue;
                                if (ea.clip != null)
                                {
                                    ea.Play();
                                    audioPlayed = true;
                                }
                                else if (jumpAttackSfx != null)
                                {
                                    ea.PlayOneShot(jumpAttackSfx);
                                    audioPlayed = true;
                                }
                            }
                            catch { }
                        }
                        // Fallback: if no child audio played, play the configured SFX at the electro position so it is audible
                        if (!audioPlayed && jumpAttackSfx != null)
                        {
                            try
                            {
                                Debug.Log("[GodController] No child audio played - using PlayOneShot/PlayClipAtPoint fallback for jumpAttackSfx.");
                                // Prefer this controller's AudioSource so volumeScale can exceed 1 via PlayOneShot
                                if (audioSource != null)
                                {
                                    audioSource.PlayOneShot(jumpAttackSfx, jumpAttackVolume);
                                }
                                else
                                {
                                    // Last resort: PlayClipAtPoint; clamp volume for PlayClipAtPoint
                                    AudioSource.PlayClipAtPoint(jumpAttackSfx, electroT.position, Mathf.Clamp(jumpAttackVolume, 0f, 1f));
                                }

                                // Also play a forced high-priority AudioSource at the electro position so the sound is heard above others
                                PlayLoudOneShotAt(jumpAttackSfx, electroT.position, jumpAttackVolume, jumpAttackAudioPriority);
                            }
                            catch { }
                        }
                    }
                    catch { }

                    // Start any particle systems on the Electro hit so the VFX play reliably
                    try
                    {
                        var parts = electroT.gameObject.GetComponentsInChildren<ParticleSystem>(true);
                        foreach (var p in parts)
                        {
                            try
                            {
                                // scale particle size as visual fallback
                                var main = p.main;
                                main.startSizeMultiplier *= Mathf.Max(0.01f, electroParticleScale);
                                p.Play(true);
                            }
                            catch { }
                            try { var rend = p.GetComponent<ParticleSystemRenderer>(); if (rend != null) rend.enabled = true; } catch { }
                        }
                        try { Debug.Log($"[GodController] Electro hit started {parts.Length} particle systems (scale x{electroParticleScale})."); } catch { }

                        // If we started particle systems but none appear to be playing, force an emit as a last-resort visual fallback
                        try
                        {
                            bool anyPlaying = false;
                            foreach (var p in parts)
                            {
                                try { if (p != null && (p.isPlaying || p.IsAlive(true))) { anyPlaying = true; break; } } catch { }
                            }
                            if (!anyPlaying && parts.Length > 0)
                            {
                                Debug.Log("[GodController] No particle was playing - forcing Emit on Electro hit particle systems as fallback.");
                                foreach (var p in parts)
                                {
                                    try { p.Clear(true); p.Emit(24); p.Play(true); } catch { }
                                }

                                // As last resort, spawn a fallback prefab (if assigned) at the electro position so the effect is visible
                                try
                                {
                                    if (electroHitPrefab != null && electroT != null)
                                    {
                                        var go = Instantiate(electroHitPrefab, electroT.position, Quaternion.identity);
                                        if (go != null)
                                        {
                                            try { go.transform.localScale *= Mathf.Max(0.01f, electroParticleScale); } catch { }
                                            Destroy(go, Mathf.Max(0.1f, forceDuration));
                                            Debug.Log("[GodController] Spawned electroHitPrefab fallback at electro position.");
                                        }
                                    }
                                }
                                catch (System.Exception ex) { Debug.LogWarning($"[GodController] Fallback prefab spawn error: {ex.Message}"); }
                            }
                        }
                        catch { }
                    }
                    catch { }

                    Debug.Log($"<color=cyan>[GodController] Activated '{electroHitChildName}' (for {electroHitActiveDuration}s) and triggered its audio/particles. activeAfter={electroT.gameObject.activeSelf}</color>");
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[GodController] Failed to activate '{electroHitChildName}': {ex.Message}");
                }
            }
            else
            {
                Debug.LogWarning($"[GodController] Child '{electroHitChildName}' not found on boss. Cannot activate Electro hit.");
            }
        }


        // Determine center for damage: prefer Electro hit position if available (handles offsets)
        Vector3 damageCenter = (electroT != null) ? electroT.position : transform.position;

        // Apply area damage at damageCenter
        if (jumpAttackRadius > 0f && jumpAttackDamage > 0)
        {
            Collider[] hits = null;
            try
            {
                hits = Physics.OverlapSphere(damageCenter, jumpAttackRadius);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[GodController] OverlapSphere error: {ex.Message}");
                yield break;
            }

            if (hits == null) yield break;

            Debug.Log($"[GodController] OverlapSphere found {hits.Length} colliders within radius {jumpAttackRadius} at damageCenter={damageCenter}.");
            foreach (var c in hits)
            {
                if (c == null || c.gameObject == null) continue;
                float dist = Vector3.Distance(c.transform.position, damageCenter);
                Debug.Log($"[GodController] Overlap hit: name={c.gameObject.name}, dist={dist}, layer={c.gameObject.layer}");

                // Prefer StarterAssets controller
                var tpc = c.GetComponent<StarterAssets.ThirdPersonController>() ?? c.GetComponentInParent<StarterAssets.ThirdPersonController>();
                if (tpc != null)
                {
                    try { tpc.TakeDamage(jumpAttackDamage); Debug.Log($"<color=red>[GodController] Applied {jumpAttackDamage} jump damage to player via TPC.</color>"); }
                    catch { Debug.LogWarning("[GodController] Failed to call TakeDamage on StarterAssets.ThirdPersonController."); }
                    continue;
                }

                // Fallback: attempt a SendMessage to 'TakeDamage' (will call if component supports it)
                try
                {
                    c.gameObject.SendMessage("TakeDamage", jumpAttackDamage, SendMessageOptions.DontRequireReceiver);
                    Debug.Log($"[GodController] SentMessage TakeDamage to {c.gameObject.name} (fallback).");
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[GodController] SendMessage TakeDamage failed on {c.gameObject.name}: {ex.Message}");
                }
            }

            // Extra fallback: if player GameObject exists but wasn't inside the OverlapSphere, apply direct check
            try
            {
                var playerGo = GameObject.FindGameObjectWithTag("Player");
                if (playerGo != null)
                {
                    Vector3 playerPos = playerGo.transform.position;
                    float pdist3D = Vector3.Distance(playerPos, damageCenter);
                    float pdistXZ = Vector2.Distance(new Vector2(playerPos.x, playerPos.z), new Vector2(damageCenter.x, damageCenter.z));
                    Debug.Log($"[GodController] Player distance to damageCenter 3D = {pdist3D}, XZ = {pdistXZ}");

                    // Prefer horizontal (XZ) distance check because Electro hit VFX may be elevated.
                    if (pdistXZ <= jumpAttackRadius || pdist3D <= jumpAttackRadius)
                    {
                        try
                        {
                            var ptpc = playerGo.GetComponent<StarterAssets.ThirdPersonController>();
                            if (ptpc != null)
                            {
                                ptpc.TakeDamage(jumpAttackDamage);
                                Debug.Log($"<color=red>[GodController] Applied {jumpAttackDamage} damage to Player via direct component fallback.</color>");
                            }
                            else
                            {
                                playerGo.SendMessage("TakeDamage", jumpAttackDamage, SendMessageOptions.DontRequireReceiver);
                                Debug.Log($"[GodController] SentMessage TakeDamage to Player (direct fallback).");
                            }
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogWarning($"[GodController] Direct damage fallback failed on Player: {ex.Message}");
                        }
                    }
                    else
                    {
                        Debug.Log("[GodController] Player was outside the horizontal/jump radius; no direct damage applied.");
                    }
                }
            }
            catch { }
        }

        // Start teleport/projectile phase
        StartTeleportProjectilePhase();
    }

    private IEnumerator StartTeleportPhaseAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartTeleportProjectilePhase();
    }

    /// <summary>
    /// Placeholder teleport+projectile phase. Replace with real teleport/projectile behaviour later.
    /// At the end of the phase this will apply damage equal to 1/3 (33%) of the boss' max health.
    /// </summary>
    public void StartTeleportProjectilePhase()
    {
        StopAllCoroutines();
        StartCoroutine(TeleportProjectilePhaseCoroutine());
    }

    private IEnumerator TeleportProjectilePhaseCoroutine()
    {
        Debug.Log("<color=cyan>[GodController] Teleport+Projectile phase START</color>");

        // Ensure any electro hit left active from previous step is turned off before starting the teleport phase
        if (!string.IsNullOrEmpty(electroHitChildName))
        {
            Transform electroT = FindDeepChild(transform, electroHitChildName);
            if (electroT != null && electroT.gameObject != null)
            {
                try
                {
                    // If TimedAutoDisable exists, cancel its invokes and disable immediately
                    var tad = electroT.gameObject.GetComponent<TimedAutoDisable>();
                    if (tad != null)
                    {
                        tad.DisableNow();
                    }
                    else
                    {
                        electroT.gameObject.SetActive(false);
                    }
                }
                catch { }
                // Spawn teleport prefab and play SFX at the electro position when the electro hit is disabled
                try
                {
                    Vector3 spawnPos = electroT.position;
                    if (teleportPrefab != null)
                    {
                        var go = Instantiate(teleportPrefab, spawnPos, Quaternion.identity);
                        try { go.transform.localScale *= 1f; } catch { }
                        Destroy(go, Mathf.Max(0.1f, teleportPrefabDuration));
                        Debug.Log("[GodController] Spawned teleport prefab at electro position.");
                    }

                    if (teleportSfx != null)
                    {
                        // Prefer controller AudioSource if available
                        if (audioSource != null)
                            audioSource.PlayOneShot(teleportSfx, 1f);
                        // Also fire a loud one-shot at the position for spatial clarity (use teleportAudioPriority)
                        PlayLoudOneShotAt(teleportSfx, spawnPos, 1f, teleportAudioPriority);
                    }
                }
                catch { }
                // Hide the boss visually to create the disappearance illusion
                try { HideBoss(); } catch { }
            }
        }

        // Ensure aura audio is stopped
        if (audioSource != null && audioSource.isPlaying && audioSource.clip == followAuraLoopSfx)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }

        // Here you would implement teleporting and projectile attacks. For now, wait the duration.
        // Start meteor rain as part of the teleport/projectile phase
        if (meteorRainCoroutine != null) StopCoroutine(meteorRainCoroutine);
        meteorRainCoroutine = StartCoroutine(MeteorRainCoroutine());

        float timer = 0f;
        while (timer < teleportPhaseDuration)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        // Stop the meteor rain when this phase finishes
        if (meteorRainCoroutine != null)
        {
            try { StopCoroutine(meteorRainCoroutine); } catch { }
            meteorRainCoroutine = null;
        }

        // Apply 1/3 max health damage to the boss (preferencing Boss component if present)
        try
        {
            // Try to locate a Boss component first (preferred for UI sync)
            Boss bossComp = null;
            try { bossComp = GetComponent<Boss>(); } catch { bossComp = null; }
            if (bossComp == null)
            {
                try { bossComp = GetComponentInChildren<Boss>(true); } catch { bossComp = null; }
            }
            if (bossComp == null)
            {
                try { bossComp = FindObjectOfType<Boss>(); } catch { bossComp = null; }
            }

            if (bossComp != null)
            {
                int third = Mathf.Max(1, bossComp.maxHealth / 3);
                bossComp.TakeDamage(third);
                Debug.Log($"<color=red>[GodController] Teleport phase complete — applied {third} damage to Boss component '{bossComp.gameObject.name}' (1/3 maxHealth = 33%). New boss health={bossComp.health}/{bossComp.maxHealth}</color>");

                // Update any boss UI immediately
                try
                {
                    var sb = FindObjectOfType<SimpleBossHealthBar>(true);
                    if (sb != null)
                    {
                        sb.boss = bossComp; // ensure UI is bound to the damaged boss
                        sb.RefreshUI();
                        Debug.Log($"[GodController] Refreshed SimpleBossHealthBar for '{bossComp.gameObject.name}'.");
                    }
                }
                catch { }
            }
            else
            {
                // No Boss component found; try to use God if available
                if (god == null)
                {
                    god = GetComponentInChildren<God>(true) ?? FindObjectOfType<God>();
                    if (god != null)
                        Debug.Log($"[GodController] Late-resolved God -> {god.gameObject.name}");
                }

                if (god != null)
                {
                    int third = Mathf.Max(1, god.maxHealth / 3);
                    god.SetInvulnerable(false);
                    god.TakeDamage(third);
                    Debug.Log($"<color=red>[GodController] Teleport phase complete — applied {third} damage to god (1/3 maxHealth = 33%). New god health={god.health}/{god.maxHealth}</color>");

                    // Update UI even when using God component
                    try
                    {
                        var sb = FindObjectOfType<SimpleBossHealthBar>(true);
                        if (sb != null)
                        {
                            // Force UI to refresh with current God health
                            sb.RefreshUI();
                            Debug.Log($"[GodController] Refreshed SimpleBossHealthBar after God damage. God health={god.health}/{god.maxHealth}");
                        }
                    }
                    catch { }

                    Debug.Log($"<color=yellow>[GodController] Note: applied damage to God because no Boss component was found on this object.</color>");
                }
                else
                {
                    Debug.LogWarning("<color=orange>[GodController] No God or Boss component found; cannot apply phase damage (1/3 maxHealth).</color>");
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[GodController] Exception while applying phase damage: {ex.Message}");
        }

        // Reveal the boss again after teleport/projectile phase
        try { ShowBoss(); } catch { }

        // After the teleport/projectile phase ends you can transition to next behaviour (not implemented)
        Debug.Log("<color=cyan>[GodController] Teleport+Projectile phase END</color>");
    }

    private IEnumerator MeteorRainCoroutine()
    {
        Debug.Log("[GodController] Meteor rain START");
        while (true)
        {
            // spawn meteor targeted at player XZ
            try
            {
                GameObject playerGo = GameObject.FindGameObjectWithTag("Player");
                Vector3 spawnPos = Vector3.zero;
                Vector3 targetPos = transform.position;
                if (playerGo != null)
                {
                    targetPos = playerGo.transform.position;
                    spawnPos = new Vector3(targetPos.x, targetPos.y + meteorSpawnHeight, targetPos.z);
                }
                else
                {
                    // fallback above boss
                    spawnPos = transform.position + Vector3.up * meteorSpawnHeight;
                    targetPos = transform.position;
                }

                GameObject meteor = null;
                if (meteorPrefab != null)
                {
                    meteor = Instantiate(meteorPrefab, spawnPos, Quaternion.identity);
                }
                else
                {
                    meteor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    meteor.transform.position = spawnPos;
                    meteor.transform.localScale = Vector3.one * 0.6f;
                    var mr = meteor.GetComponent<Renderer>(); if (mr != null) { try { mr.material.color = Color.red; } catch { } }
                    // remove collider (we'll handle damage via AoE on impact)
                    var col = meteor.GetComponent<Collider>(); if (col != null) Destroy(col);
                }

                if (meteor != null)
                {
                    var mp = meteor.GetComponent<MeteorProjectile>();
                    if (mp == null) mp = meteor.AddComponent<MeteorProjectile>();
                    mp.fallSpeed = meteorFallSpeed;
                    mp.targetPosition = targetPos;
                    mp.impactPrefab = meteorImpactPrefab;
                    mp.impactPrefabScale = 1f;
                    mp.impactRadius = meteorAoERadius;
                    mp.impactDPS = meteorAoEDamagePerSecond;
                    mp.impactDuration = meteorAoEDuration;
                    // pass SFX settings so meteor can play a high-priority impact sound
                    mp.impactSfx = meteorImpactSfx;
                    mp.impactSfxVolume = meteorImpactSfxVolume;
                    mp.impactSfxPriority = meteorImpactSfxPriority;
                    // Play spawn SFX for this meteor (so a sound plays when it appears)
                    try
                    {
                        if (meteorSpawnSfx != null)
                        {
                            if (audioSource != null) audioSource.PlayOneShot(meteorSpawnSfx, meteorSpawnSfxVolume);
                            PlayLoudOneShotAt(meteorSpawnSfx, spawnPos, meteorSpawnSfxVolume, meteorSpawnSfxPriority);
                        }
                    }
                    catch { }
                }

                // wait interval
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("[GodController] Meteor spawn failed: " + ex.Message);
            }

            yield return new WaitForSeconds(Mathf.Max(0.05f, meteorSpawnInterval));
        }
    }

    private void DisableActiveAura()
    {
        if (activeAuraInstance != null)
        {
            Debug.Log($"[GodController] DisableActiveAura called. activeAuraInstance={activeAuraInstance.name}, followAuraPrefab={(followAuraPrefab != null)}");
            // If the aura was instantiated from a prefab, destroy it shortly after to allow exit VFX to play
            if (followAuraPrefab != null)
            {
                // Stop any audio in the aura before destroying
                var childAudio = activeAuraInstance.GetComponentsInChildren<AudioSource>(true);
                foreach (var a in childAudio)
                {
                    try { a.Stop(); a.loop = false; } catch { }
                }

                var parts = activeAuraInstance.GetComponentsInChildren<ParticleSystem>(true);
                foreach (var p in parts)
                {
                    try { p.Stop(true, ParticleSystemStopBehavior.StopEmitting); } catch { }
                }

                Destroy(activeAuraInstance, 0.2f);
            }
            else
            {
                try
                {
                    // Stop audio and particles inside the existing child aura as well
                    var childAudio = activeAuraInstance.GetComponentsInChildren<AudioSource>(true);
                    foreach (var a in childAudio) { try { a.Stop(); a.loop = false; } catch { } }
                    var parts = activeAuraInstance.GetComponentsInChildren<ParticleSystem>(true);
                    foreach (var p in parts) { try { p.Stop(true, ParticleSystemStopBehavior.StopEmitting); } catch { } }
                    activeAuraInstance.SetActive(false);
                }
                catch { }
            }

            // Clear reference after handling
            activeAuraInstance = null;
        }

        // stop audio loop played by this controller (fallback)
        if (audioSource != null)
        {
            if (audioSource.isPlaying && audioSource.clip == followAuraLoopSfx)
            {
                audioSource.Stop();
                audioSource.loop = false;
            }
        }

        playerWasInAura = false;
        // Additional safety sweep
        StopAnyLingeringAuraEffects();
    }

    // Extra safety: stop any lingering aura audio/particles under the boss by name match
    private void StopAnyLingeringAuraEffects()
    {
        try
        {
            foreach (var a in GetComponentsInChildren<AudioSource>(true))
            {
                if (a == null || a.clip == null) continue;
                var name = a.gameObject.name.ToLowerInvariant();
                if (name.Contains("aura") || name.Contains("star aura") || name.Contains("follow"))
                {
                    try { a.Stop(); a.loop = false; } catch { }
                }
            }

            foreach (var p in GetComponentsInChildren<ParticleSystem>(true))
            {
                if (p == null) continue;
                var name = p.gameObject.name.ToLowerInvariant();
                if (name.Contains("aura") || name.Contains("star aura") || name.Contains("follow"))
                {
                    try { p.Stop(true, ParticleSystemStopBehavior.StopEmitting); } catch { }
                }
            }
        }
        catch { }
    }

    // Recursive child search
    private Transform FindDeepChild(Transform parent, string name)
    {
        if (parent == null) return null;
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            var found = FindDeepChild(child, name);
            if (found != null) return found;
        }
        return null;
    }

    // Returns a slash-separated hierarchy path for a Transform (for debugging)
    private string GetTransformPath(Transform t)
    {
        if (t == null) return "<null>";
        string path = t.name;
        var cur = t.parent;
        while (cur != null)
        {
            path = cur.name + "/" + path;
            cur = cur.parent;
        }
        return path;
    }

    private IEnumerator DeactivateAfterDelay(GameObject go, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (go != null)
        {
            try { go.SetActive(false); }
            catch { }
        }
    }

    // Play a loud one-shot audio clip at position using a temporary AudioSource with high priority.
    private void PlayLoudOneShotAt(AudioClip clip, Vector3 pos, float volume = 1f, int priority = 64)
    {
        if (clip == null) return;
        try
        {
            var go = new GameObject("_JumpSfxOneShot");
            go.transform.position = pos;
            var src = go.AddComponent<AudioSource>();
            src.clip = clip;
            src.spatialBlend = 1f; // 3D
            src.priority = Mathf.Clamp(priority, 0, 256);
            src.volume = Mathf.Max(0f, volume);
            src.playOnAwake = false;
            src.minDistance = 0.1f;
            src.maxDistance = Mathf.Max(10f, src.maxDistance);
            src.rolloffMode = AudioRolloffMode.Linear;
            try { src.Play(); } catch { src.PlayOneShot(clip, volume); }
            Destroy(go, clip.length + 0.2f);
        }
        catch { }
    }

    private void StartBossMusic()
    {
        if (bossLoopMusic == null)
        {
            Debug.LogWarning("[GodController] No boss loop music assigned.");
            return;
        }

        try
        {
            // If bossMusicSource wasn't created in Awake, create it now
            if (bossMusicSource == null)
            {
                bossMusicSource = gameObject.AddComponent<AudioSource>();
                bossMusicSource.playOnAwake = false;
                bossMusicSource.loop = true;
                bossMusicSource.spatialBlend = 0f; // 2D music
                bossMusicSource.priority = 0; // Highest priority - NEVER interrupted
                bossMusicSource.ignoreListenerPause = true;
                bossMusicSource.clip = bossLoopMusic;
                bossMusicSource.volume = Mathf.Clamp01(bossMusicVolume);
                Debug.Log("<color=yellow>[GodController] Boss music AudioSource created on-demand</color>");
            }

            // Only play if not already playing
            if (!bossMusicSource.isPlaying)
            {
                bossMusicSource.Play();
                Debug.Log("<color=green>[GodController] ♫ Boss loop music STARTED (priority 0, never interrupted)</color>");
            }
            else
            {
                Debug.Log("[GodController] Boss music already playing, continuing...");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[GodController] Error starting boss music: {ex.Message}");
        }
    }

    private void StopBossMusic()
    {
        try
        {
            if (bossMusicSource != null && bossMusicSource.isPlaying)
            {
                bossMusicSource.Stop();
                Debug.Log("<color=red>[GodController] ♫ Boss loop music STOPPED</color>");
            }
            else if (bossMusicSource != null)
            {
                Debug.Log("[GodController] Boss music source exists but wasn't playing.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[GodController] Error stopping boss music: {ex.Message}");
        }
    }

    // Hides the boss visually and disables its colliders/animator without deactivating the GameObject
    private void HideBoss()
    {
        // cache renderers
        _cachedRenderers = GetComponentsInChildren<Renderer>(true);
        if (_cachedRenderers != null)
        {
            _cachedRenderersEnabled = new bool[_cachedRenderers.Length];
            for (int i = 0; i < _cachedRenderers.Length; ++i)
            {
                try { _cachedRenderersEnabled[i] = _cachedRenderers[i].enabled; _cachedRenderers[i].enabled = false; } catch { _cachedRenderersEnabled[i] = false; }
            }
        }

        // cache colliders
        _cachedColliders = GetComponentsInChildren<Collider>(true);
        if (_cachedColliders != null)
        {
            _cachedCollidersEnabled = new bool[_cachedColliders.Length];
            for (int i = 0; i < _cachedColliders.Length; ++i)
            {
                try { _cachedCollidersEnabled[i] = _cachedColliders[i].enabled; _cachedColliders[i].enabled = false; } catch { _cachedCollidersEnabled[i] = false; }
            }
        }

        // disable animator so it doesn't update visuals
        if (animator != null)
        {
            _animatorWasEnabled = animator.enabled;
            animator.enabled = false;
        }

        // as extra, stop any audio and particles on the boss root
        try { StopAnyLingeringAuraEffects(); } catch { }

        Debug.Log("[GodController] Boss hidden for teleport illusion.");
    }

    // Restores previously hidden renderers/colliders/animator
    private void ShowBoss()
    {
        if (_cachedRenderers != null && _cachedRenderersEnabled != null)
        {
            for (int i = 0; i < _cachedRenderers.Length && i < _cachedRenderersEnabled.Length; ++i)
            {
                try { if (_cachedRenderers[i] != null) _cachedRenderers[i].enabled = _cachedRenderersEnabled[i]; } catch { }
            }
        }

        if (_cachedColliders != null && _cachedCollidersEnabled != null)
        {
            for (int i = 0; i < _cachedColliders.Length && i < _cachedCollidersEnabled.Length; ++i)
            {
                try { if (_cachedColliders[i] != null) _cachedColliders[i].enabled = _cachedCollidersEnabled[i]; } catch { }
            }
        }

        if (animator != null)
        {
            try { animator.enabled = _animatorWasEnabled; } catch { }
        }

        Debug.Log("[GodController] Boss revealed after teleport phase.");
    }

    // Activates the Electro hit visual/audio for a given transform for the requested duration.
    private void ActivateElectroHit(Transform electroT, float duration)
    {
        if (electroT == null || electroT.gameObject == null) return;
        try
        {
            var go = electroT.gameObject;
            if (go == null) return;

            // Attach or get TimedAutoDisable to survive StopAllCoroutines
            var tad = go.GetComponent<TimedAutoDisable>();
            if (tad == null) tad = go.AddComponent<TimedAutoDisable>();

            try { Debug.Log($"[GodController] (early) Activating Electro hit: {GetTransformPath(electroT)}; activeBefore={go.activeSelf}; position={electroT.position}"); } catch { }
            try { go.SetActive(true); } catch { }
            if (tad != null) tad.ActivateForSeconds(duration);

            // Play child audio if available
            try
            {
                var electroAudioSources = go.GetComponentsInChildren<AudioSource>(true);
                bool played = false;
                if (electroAudioSources != null)
                {
                    foreach (var ea in electroAudioSources)
                    {
                        try
                        {
                            if (ea == null || ea.gameObject == null) continue;
                            if (ea.clip != null) { ea.Play(); played = true; }
                            else if (jumpAttackSfx != null) { ea.PlayOneShot(jumpAttackSfx); played = true; }
                        }
                        catch { }
                    }
                }
                if (!played && jumpAttackSfx != null)
                {
                    if (audioSource != null) audioSource.PlayOneShot(jumpAttackSfx, jumpAttackVolume);
                    PlayLoudOneShotAt(jumpAttackSfx, electroT.position, jumpAttackVolume, jumpAttackAudioPriority);
                }
            }
            catch { }

            // Start child particles and scale them
            try
            {
                var parts = go.GetComponentsInChildren<ParticleSystem>(true);
                if (parts != null)
                {
                    foreach (var p in parts)
                    {
                        if (p == null || p.gameObject == null) continue;
                        try { var main = p.main; main.startSizeMultiplier *= Mathf.Max(0.01f, electroParticleScale); p.Play(true); } catch { }
                        try { var rend = p.GetComponent<ParticleSystemRenderer>(); if (rend != null) rend.enabled = true; } catch { }
                    }
                    Debug.Log($"[GodController] (early) Started {parts.Length} particle systems for Electro hit.");
                }
            }
            catch { }

            // Instantiate fallback prefab immediately if assigned (guaranteed visual)
            try
            {
                if (electroHitPrefab != null && electroT != null)
                {
                    var prefabGo = Instantiate(electroHitPrefab, electroT.position, Quaternion.identity);
                    if (prefabGo != null)
                    {
                        try { prefabGo.transform.localScale *= Mathf.Max(0.01f, electroParticleScale); } catch { }
                        Destroy(prefabGo, Mathf.Max(0.1f, duration));
                        Debug.Log("[GodController] (early) Instantiated electroHitPrefab fallback at electro position.");
                    }
                }
            }
            catch (System.Exception ex) { Debug.LogWarning($"[GodController] electroHitPrefab early instantiation error: {ex.Message}"); }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[GodController] ActivateElectroHit failed: {ex.Message}");
        }
    }

    private IEnumerator EdgePhaseCoroutine()
    {
        // Set invulnerable if possible
        if (god != null)
            god.SetInvulnerable(true);

        isWalking = true;
        if (animator != null)
        {
            animator.SetBool("Moving", true);
            // initialize smoothed speed
            smoothedSpeed = 0f;
            animator.SetFloat("Speed", 0f);
            animator.SetFloat("MotionSpeed", 0f);
        }

        prevPosition = transform.position;

        float timer = 0f;
        while (timer < edgePhaseDuration)
        {
            timer += Time.deltaTime;
            // Compute target horizontal position around center
            currentAngle += (walkSpeed / Mathf.Max(0.001f, edgeRadius)) * Time.deltaTime * Mathf.Rad2Deg; // approximate angular velocity
            float rad = currentAngle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * edgeRadius;
            Vector3 desiredXZ = new Vector3(centerPosition.x + offset.x, 0f, centerPosition.z + offset.z);

            // Sample ground height at desired position
            Vector3 rayStart = new Vector3(desiredXZ.x, centerPosition.y + groundRayStartHeight, desiredXZ.z);
            float sampledY = centerPosition.y; // fallback
            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, groundRayStartHeight * 2f, groundLayer))
            {
                sampledY = hit.point.y + groundYOffset;
            }

            Vector3 targetPos = new Vector3(desiredXZ.x, sampledY, desiredXZ.z);

            // Move horizontally toward target, but set Y using sampled ground to avoid falling through irregular geometry
            Vector3 nextPos = Vector3.MoveTowards(transform.position, new Vector3(targetPos.x, transform.position.y, targetPos.z), walkSpeed * Time.deltaTime);
            // Update Y smoothly to sampled ground
            float newY = Mathf.MoveTowards(transform.position.y, targetPos.y, (walkSpeed * 0.5f) * Time.deltaTime);
            transform.position = new Vector3(nextPos.x, newY, nextPos.z);

            // Update animator speeds based on actual movement, smoothed to avoid jumps
            if (animator != null)
            {
                // defensively ensure root motion is off (some animator assets may toggle it)
                if (animator.applyRootMotion)
                    animator.applyRootMotion = false;

                float currentSpeed = 0f;
                if (Time.deltaTime > 0f)
                {
                    currentSpeed = Vector3.Distance(transform.position, prevPosition) / Time.deltaTime;
                }

                smoothedSpeed = Mathf.Lerp(smoothedSpeed, currentSpeed, Mathf.Clamp01(speedSmooth * Time.deltaTime));
                animator.SetFloat("Speed", smoothedSpeed);
                animator.SetFloat("MotionSpeed", smoothedSpeed);
            }
            prevPosition = transform.position;

            // Determine desired facing direction: primarily towards movement target, optionally blended towards player
            Vector3 moveDir = (new Vector3(targetPos.x, transform.position.y, targetPos.z) - transform.position);
            moveDir.y = 0f;
            if (moveDir.sqrMagnitude < 0.0001f)
                moveDir = transform.forward;
            else
                moveDir.Normalize();

            Vector3 finalFacing = moveDir;
            if (facePlayerDuringWalk && player != null)
            {
                Vector3 toPlayer = player.transform.position - transform.position;
                toPlayer.y = 0f;
                if (toPlayer.sqrMagnitude > 0.0001f)
                    toPlayer.Normalize();
                // Blend between move direction and player direction
                finalFacing = Vector3.Slerp(moveDir, toPlayer, facePlayerWeight).normalized;
            }

            // Rotate towards finalFacing using rotationSpeed
            Quaternion targetRot = Quaternion.LookRotation(finalFacing);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);

            // Move forward along the forward vector so animation matches movement
            Vector3 forwardMove = transform.forward * walkSpeed * Time.deltaTime;
            Vector3 proposedPos = transform.position + forwardMove;

            // Keep Y controlled by ground sampling (we already computed targetPos.y)
            proposedPos.y = Mathf.MoveTowards(transform.position.y, targetPos.y, (walkSpeed * 0.5f) * Time.deltaTime);
            transform.position = proposedPos;

            yield return null;
        }

        isWalking = false;

        // End phase: restore vulnerability
        if (god != null)
            god.SetInvulnerable(false);

        // Reset animator movement params
        if (animator != null)
        {
            animator.SetBool("Moving", false);
            animator.SetFloat("Speed", 0f);
            animator.SetFloat("MotionSpeed", 0f);
        }

        Debug.Log("<color=green>[GodController] Edge phase complete. Boss vulnerable again. Starting follow phase...</color>");
        
        // Automatically transition to follow phase after edge phase (60 seconds)
        StartFollowPhase(60f, 6f, 2f, 0.6f);
    }

    /// <summary>
    /// Llamado por Wizard cuando muere. Aplica daño al boss.
    /// </summary>
    public void OnWizardKilled()
    {
        Debug.Log("<color=cyan>[GodController] OnWizardKilled() - Un wizard ha muerto, aplicando daño al boss</color>");

        // Hay 4 wizards en la tercera fase
        // Fase 1: -33%, Fase 2: -33%, Fase 3: -34% (4 wizards × 8.5% cada uno)
        // 34% / 4 wizards = 8.5% por wizard
        int damagePerWizard = 0;

        try
        {
            // Intentar usar el componente Boss primero
            Boss bossComp = GetComponent<Boss>() ?? GetComponentInChildren<Boss>(true) ?? FindAnyObjectByType<Boss>();

            if (bossComp != null)
            {
                // Daño = 8.5% de la vida máxima (34% de la tercera fase / 4 wizards)
                damagePerWizard = Mathf.Max(1, Mathf.RoundToInt(bossComp.maxHealth * 0.085f));
                bossComp.TakeDamage(damagePerWizard);

                Debug.Log($"<color=red>[GodController] ✓ Wizard muerto - aplicado {damagePerWizard} daño al Boss. Vida actual: {bossComp.health}/{bossComp.maxHealth}</color>");

                // Actualizar la UI inmediatamente
                try
                {
                    SimpleBossHealthBar healthBar = FindAnyObjectByType<SimpleBossHealthBar>();
                    if (healthBar != null)
                    {
                        healthBar.boss = bossComp;
                        healthBar.RefreshUI();
                        Debug.Log("[GodController] UI del boss actualizada tras muerte de wizard");
                    }
                }
                catch { }
            }
            else
            {
                // Fallback: usar componente God
                if (god == null)
                {
                    god = GetComponentInChildren<God>(true) ?? FindAnyObjectByType<God>();
                }

                if (god != null)
                {
                    damagePerWizard = Mathf.Max(1, Mathf.RoundToInt(god.maxHealth * 0.085f));
                    god.SetInvulnerable(false); // Asegurar que no esté invulnerable
                    god.TakeDamage(damagePerWizard);

                    Debug.Log($"<color=red>[GodController] ✓ Wizard muerto - aplicado {damagePerWizard} daño a God. Vida actual: {god.health}/{god.maxHealth}</color>");

                    // Actualizar UI
                    try
                    {
                        SimpleBossHealthBar healthBar = FindAnyObjectByType<SimpleBossHealthBar>();
                        if (healthBar != null)
                        {
                            healthBar.RefreshUI();
                            Debug.Log("[GodController] UI del boss actualizada tras muerte de wizard (God)");
                        }
                    }
                    catch { }
                }
                else
                {
                    Debug.LogError("<color=red>[GodController] ERROR: No se encontró componente Boss ni God para aplicar daño por wizard!</color>");
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[GodController] Excepción al aplicar daño por wizard: {ex.Message}");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, edgeRadius);
    }
}
