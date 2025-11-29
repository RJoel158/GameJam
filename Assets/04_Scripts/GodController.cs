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
    public float edgePhaseDuration = 8f;

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

    [Tooltip("If true, the phase will start automatically when told to StartPhase(); otherwise start manually")]
    public bool startOnEnable = false;

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

    private void Awake()
    {
        centerPosition = transform.position;
        god = GetComponent<God>();
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

        // Ensure an AudioSource exists for aura SFX
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f; // 3D sound
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
            Debug.LogWarning("<color=orange>[GodController] No God component found on boss; invulnerability API will be unavailable.</color>");
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
    public void StartFollowPhase(float duration = 8f, float distance = 6f, float followSpeed = 2f, float jitterAmount = 0.6f)
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
        GameObject auraInstance = null;
        if (followAuraPrefab != null)
        {
            auraInstance = Instantiate(followAuraPrefab, transform.position, Quaternion.identity, transform);
            auraInstance.transform.localPosition = Vector3.zero;
            // Optional scale control
            auraInstance.transform.localScale = Vector3.one * followAuraScale;
        }
        else
        {
            // Try find existing child by name (common name: "Star aura") and enable it
            var child = transform.Find("Star aura");
            if (child != null)
            {
                auraInstance = child.gameObject;
                auraInstance.SetActive(true);
            }
        }

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

        // Disable or destroy aura visual
        if (auraInstance != null)
        {
            // If it was instantiated by us, destroy; if it was existing child, just deactivate
            if (followAuraPrefab != null)
                Destroy(auraInstance, 0.2f);
            else
                auraInstance.SetActive(false);
        }

        Debug.Log("<color=green>[GodController] Follow phase complete. Boss vulnerable again.</color>");
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

        Debug.Log("<color=green>[GodController] Edge phase complete. Boss vulnerable again.</color>");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, edgeRadius);
    }
}
