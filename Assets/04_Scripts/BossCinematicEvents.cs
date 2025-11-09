using UnityEngine;
using UnityEngine.Playables;
using StarterAssets;

/// <summary>
/// Helper script that can be called from Timeline using Signals or Animation Events
/// to trigger specific actions during the boss cinematic
/// </summary>
public class BossCinematicEvents : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the Boss Spawner")]
    public BossSpawner bossSpawner;
    
    [Tooltip("Reference to the boss GameObject")]
    public GameObject boss;
    
    [Header("Camera Shake")]
    [Tooltip("Enable camera shake on boss roar")]
    public bool enableCameraShake = true;
    
    [Tooltip("Shake intensity")]
    public float shakeIntensity = 0.5f;
    
    [Tooltip("Shake duration")]
    public float shakeDuration = 0.3f;

    [Header("Effects")]
    [Tooltip("Particle effect to play when boss appears")]
    public ParticleSystem bossAppearEffect;
    
    [Tooltip("Ground impact effect")]
    public ParticleSystem groundImpactEffect;

    [Header("Audio")]
    [Tooltip("Boss roar sound")]
    public AudioClip bossRoarSound;
    
    [Tooltip("Ground impact sound")]
    public AudioClip groundImpactSound;
    
    [Tooltip("Dramatic music")]
    public AudioClip dramaticMusic;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    #region Timeline Event Methods
    // These methods can be called from Timeline using Signals or Animation Events

    /// <summary>
    /// Called when the boss should spawn (can be called from Timeline)
    /// </summary>
    public void SpawnBoss()
    {
        Debug.Log("<color=yellow>[BossCinematicEvents] Spawning boss from Timeline event...</color>");
        
        if (bossSpawner != null)
        {
            bossSpawner.SpawnBoss();
        }
        else
        {
            Debug.LogError("<color=red>[BossCinematicEvents] BossSpawner reference not set!</color>");
        }
    }

    /// <summary>
    /// Plays the boss appear effect
    /// </summary>
    public void PlayBossAppearEffect()
    {
        Debug.Log("<color=yellow>[BossCinematicEvents] Playing boss appear effect...</color>");
        
        if (bossAppearEffect != null)
        {
            bossAppearEffect.Play();
        }
    }

    /// <summary>
    /// Plays ground impact effect
    /// </summary>
    public void PlayGroundImpactEffect()
    {
        Debug.Log("<color=yellow>[BossCinematicEvents] Playing ground impact effect...</color>");
        
        if (groundImpactEffect != null)
        {
            groundImpactEffect.Play();
        }

        if (groundImpactSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(groundImpactSound);
        }

        // Camera shake
        if (enableCameraShake)
        {
            ShakeCamera();
        }
    }

    /// <summary>
    /// Plays boss roar sound
    /// </summary>
    public void PlayBossRoar()
    {
        Debug.Log("<color=yellow>[BossCinematicEvents] Playing boss roar...</color>");
        
        if (bossRoarSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(bossRoarSound);
        }

        // Trigger boss animation if it has an Animator
        if (boss != null)
        {
            Animator animator = boss.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetTrigger("Roar");
            }
        }
    }

    /// <summary>
    /// Plays dramatic music
    /// </summary>
    public void PlayDramaticMusic()
    {
        Debug.Log("<color=yellow>[BossCinematicEvents] Playing dramatic music...</color>");
        
        if (dramaticMusic != null && audioSource != null)
        {
            audioSource.clip = dramaticMusic;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    /// <summary>
    /// Stops all music
    /// </summary>
    public void StopMusic()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    /// <summary>
    /// Shows text message (requires TextMeshPro UI)
    /// </summary>
    public void ShowMessage(string message)
    {
        Debug.Log($"<color=cyan>[BossCinematicEvents] MESSAGE: {message}</color>");
        
        // You can implement UI text display here
        // Example: Find a TextMeshProUGUI component and update its text
        var textUI = FindAnyObjectByType<TMPro.TextMeshProUGUI>();
        if (textUI != null)
        {
            textUI.text = message;
        }
    }

    /// <summary>
    /// Makes the boss look at the player
    /// </summary>
    public void BossLookAtPlayer()
    {
        if (boss != null)
        {
            var player = FindAnyObjectByType<ThirdPersonController>();
            if (player != null)
            {
                Vector3 direction = (player.transform.position - boss.transform.position).normalized;
                direction.y = 0; // Keep on horizontal plane
                boss.transform.rotation = Quaternion.LookRotation(direction);
                
                Debug.Log("<color=yellow>[BossCinematicEvents] Boss looking at player</color>");
            }
        }
    }

    /// <summary>
    /// Triggers camera shake effect
    /// </summary>
    private void ShakeCamera()
    {
        // Find Cinemachine camera and apply impulse
        var impulseSource = GetComponent<Cinemachine.CinemachineImpulseSource>();
        if (impulseSource != null)
        {
            impulseSource.GenerateImpulse(shakeIntensity);
        }
        else
        {
            Debug.LogWarning("<color=orange>[BossCinematicEvents] No CinemachineImpulseSource found for camera shake!</color>");
        }
    }

    /// <summary>
    /// Enables boss AI/combat
    /// </summary>
    public void EnableBossAI()
    {
        Debug.Log("<color=yellow>[BossCinematicEvents] Enabling boss AI...</color>");
        
        if (boss != null)
        {
            // Enable enemy script or AI component
            var enemy = boss.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.enabled = true;
            }

            // Enable NavMeshAgent if present
            var navAgent = boss.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (navAgent != null)
            {
                navAgent.enabled = true;
            }
        }
    }

    /// <summary>
    /// Freezes the boss (for beginning of cinematic)
    /// </summary>
    public void FreezeBoss()
    {
        Debug.Log("<color=yellow>[BossCinematicEvents] Freezing boss...</color>");
        
        if (boss != null)
        {
            // Disable enemy script or AI component
            var enemy = boss.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.enabled = false;
            }

            // Disable NavMeshAgent if present
            var navAgent = boss.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (navAgent != null)
            {
                navAgent.enabled = false;
            }
        }
    }

    /// <summary>
    /// Debug log message (useful for testing timeline timing)
    /// </summary>
    public void DebugLog(string message)
    {
        Debug.Log($"<color=magenta>[BossCinematicEvents - Timeline] {message}</color>");
    }

    #endregion

    #region Context Menu (For Testing)

    [ContextMenu("Test Spawn Boss")]
    private void TestSpawnBoss()
    {
        SpawnBoss();
    }

    [ContextMenu("Test Boss Roar")]
    private void TestBossRoar()
    {
        PlayBossRoar();
    }

    [ContextMenu("Test Ground Impact")]
    private void TestGroundImpact()
    {
        PlayGroundImpactEffect();
    }

    #endregion
}
