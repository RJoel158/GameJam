using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using Cinemachine;
using StarterAssets;

/// <summary>
/// Controls cinematic sequences using Unity Timeline
/// Can be used for boss introductions, cutscenes, etc.
/// </summary>
public class CinematicController : MonoBehaviour
{
    [Header("Timeline Settings")]
    [Tooltip("The main Playable Director (Timeline)")]
    public PlayableDirector timelineDirector;

    [Header("Camera Settings")]
    [Tooltip("Cinemachine brain (usually on Main Camera)")]
    public CinemachineBrain cinemachineBrain;

    [Tooltip("Virtual cameras used in the cinematic")]
    public CinemachineVirtualCamera[] cinematicCameras;

    [Tooltip("Player camera to disable during cinematic")]
    public CinemachineVirtualCamera playerCamera;

    [Header("Player Control Settings")]
    [Tooltip("Disable player input during cinematic")]
    public bool disablePlayerInput = true;

    [Tooltip("Disable player movement during cinematic")]
    public bool disablePlayerMovement = true;

    [Tooltip("Hide player during cinematic")]
    public bool hidePlayer = false;

    [Header("UI Settings")]
    [Tooltip("Canvas or UI elements to hide during cinematic")]
    public GameObject[] uiElementsToHide;

    [Tooltip("Show cinematic bars (letterbox)")]
    public bool showLetterbox = true;

    [Tooltip("Letterbox bars (top and bottom black bars)")]
    public GameObject letterboxBars;

    [Header("Audio Settings")]
    [Tooltip("Audio to play when cinematic starts")]
    public AudioClip cinematicMusic;

    [Tooltip("Fade in duration for music")]
    public float musicFadeInDuration = 1f;

    private AudioSource audioSource;
    private GameObject player;
    private ThirdPersonController playerController;
    private UnityEngine.InputSystem.PlayerInput playerInput;
    private bool cinematicPlaying = false;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.loop = false;
        audioSource.playOnAwake = false;
    }

    private void Start()
    {
        // Find player references
        FindPlayerReferences();

        // Setup timeline callbacks
        if (timelineDirector != null)
        {
            timelineDirector.played += OnTimelinePlayed;
            timelineDirector.stopped += OnTimelineStopped;
            timelineDirector.paused += OnTimelinePaused;
        }

        // Hide letterbox initially
        if (letterboxBars != null)
        {
            letterboxBars.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (timelineDirector != null)
        {
            timelineDirector.played -= OnTimelinePlayed;
            timelineDirector.stopped -= OnTimelineStopped;
            timelineDirector.paused -= OnTimelinePaused;
        }
    }

    /// <summary>
    /// Finds references to player components
    /// </summary>
    private void FindPlayerReferences()
    {
        playerController = FindAnyObjectByType<ThirdPersonController>();
        if (playerController != null)
        {
            player = playerController.gameObject;
        }

        playerInput = FindAnyObjectByType<UnityEngine.InputSystem.PlayerInput>();
    }

    /// <summary>
    /// Plays the cinematic
    /// </summary>
    public void PlayCinematic()
    {
        if (timelineDirector == null)
        {
            Debug.LogError("<color=red>[CinematicController] No Timeline Director assigned!</color>");
            return;
        }

        if (cinematicPlaying)
        {
            Debug.LogWarning("<color=yellow>[CinematicController] Cinematic already playing!</color>");
            return;
        }

        Debug.Log("<color=magenta>[CinematicController] Starting cinematic...</color>");

        // Prepare for cinematic
        PrepareForCinematic();

        // Play the timeline
        timelineDirector.Play();
    }

    /// <summary>
    /// Stops the cinematic
    /// </summary>
    public void StopCinematic()
    {
        if (timelineDirector != null)
        {
            timelineDirector.Stop();
        }
    }

    /// <summary>
    /// Prepares the scene for the cinematic
    /// </summary>
    private void PrepareForCinematic()
    {
        cinematicPlaying = true;

        // Disable player controls
        if (disablePlayerInput && playerInput != null)
        {
            playerInput.DeactivateInput();
            Debug.Log("<color=cyan>[CinematicController] Player input disabled</color>");
        }

        if (disablePlayerMovement && playerController != null)
        {
            playerController.enabled = false;
            Debug.Log("<color=cyan>[CinematicController] Player movement disabled</color>");
        }

        // Hide player if needed
        if (hidePlayer && player != null)
        {
            player.SetActive(false);
            Debug.Log("<color=cyan>[CinematicController] Player hidden</color>");
        }

        // Disable player camera
        if (playerCamera != null)
        {
            playerCamera.Priority = 0;
            Debug.Log("<color=cyan>[CinematicController] Player camera priority set to 0</color>");
        }

        // Enable cinematic cameras
        foreach (var cam in cinematicCameras)
        {
            if (cam != null)
            {
                cam.Priority = 10; // Higher priority than player camera
            }
        }

        // Hide UI elements
        foreach (var ui in uiElementsToHide)
        {
            if (ui != null)
            {
                ui.SetActive(false);
            }
        }

        // Show letterbox
        if (showLetterbox && letterboxBars != null)
        {
            letterboxBars.SetActive(true);
        }

        // Play music
        if (cinematicMusic != null && audioSource != null)
        {
            StartCoroutine(FadeInMusic());
        }
    }

    /// <summary>
    /// Restores the scene after the cinematic
    /// </summary>
    private void RestoreAfterCinematic()
    {
        cinematicPlaying = false;

        Debug.Log("<color=magenta>[CinematicController] Cinematic ended, restoring scene...</color>");

        // Re-enable player controls
        if (disablePlayerInput && playerInput != null)
        {
            playerInput.ActivateInput();
            Debug.Log("<color=cyan>[CinematicController] Player input re-enabled</color>");
        }

        if (disablePlayerMovement && playerController != null)
        {
            playerController.enabled = true;
            Debug.Log("<color=cyan>[CinematicController] Player movement re-enabled</color>");
        }

        // Show player
        if (hidePlayer && player != null)
        {
            player.SetActive(true);
            Debug.Log("<color=cyan>[CinematicController] Player shown</color>");
        }

        // Re-enable player camera
        if (playerCamera != null)
        {
            playerCamera.Priority = 10;
            Debug.Log("<color=cyan>[CinematicController] Player camera priority restored</color>");
        }

        // Disable cinematic cameras
        foreach (var cam in cinematicCameras)
        {
            if (cam != null)
            {
                cam.Priority = 0;
            }
        }

        // Show UI elements
        foreach (var ui in uiElementsToHide)
        {
            if (ui != null)
            {
                ui.SetActive(true);
            }
        }

        // Hide letterbox
        if (letterboxBars != null)
        {
            letterboxBars.SetActive(false);
        }

        // Stop music
        if (audioSource != null)
        {
            StartCoroutine(FadeOutMusic());
        }
    }

    /// <summary>
    /// Fades in the cinematic music
    /// </summary>
    private IEnumerator FadeInMusic()
    {
        audioSource.clip = cinematicMusic;
        audioSource.volume = 0f;
        audioSource.Play();

        float elapsed = 0f;
        while (elapsed < musicFadeInDuration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, 1f, elapsed / musicFadeInDuration);
            yield return null;
        }

        audioSource.volume = 1f;
    }

    /// <summary>
    /// Fades out the cinematic music
    /// </summary>
    private IEnumerator FadeOutMusic()
    {
        float elapsed = 0f;
        float startVolume = audioSource.volume;

        while (elapsed < musicFadeInDuration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / musicFadeInDuration);
            yield return null;
        }

        audioSource.volume = 0f;
        audioSource.Stop();
    }

    #region Timeline Callbacks

    private void OnTimelinePlayed(PlayableDirector director)
    {
        Debug.Log("<color=magenta>[CinematicController] Timeline started playing</color>");
    }

    private void OnTimelineStopped(PlayableDirector director)
    {
        Debug.Log("<color=magenta>[CinematicController] Timeline stopped</color>");
        RestoreAfterCinematic();
    }

    private void OnTimelinePaused(PlayableDirector director)
    {
        Debug.Log("<color=magenta>[CinematicController] Timeline paused</color>");
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Skips the cinematic
    /// </summary>
    public void SkipCinematic()
    {
        if (cinematicPlaying && timelineDirector != null)
        {
            timelineDirector.time = timelineDirector.duration;
            timelineDirector.Evaluate();
            StopCinematic();
        }
    }

    /// <summary>
    /// Pauses the cinematic
    /// </summary>
    public void PauseCinematic()
    {
        if (timelineDirector != null)
        {
            timelineDirector.Pause();
        }
    }

    /// <summary>
    /// Resumes the cinematic
    /// </summary>
    public void ResumeCinematic()
    {
        if (timelineDirector != null)
        {
            timelineDirector.Resume();
        }
    }

    /// <summary>
    /// Checks if cinematic is currently playing
    /// </summary>
    public bool IsCinematicPlaying()
    {
        return cinematicPlaying;
    }

    #endregion

    #region Context Menu Methods (For Testing)

    [ContextMenu("Test Play Cinematic")]
    private void TestPlayCinematic()
    {
        PlayCinematic();
    }

    [ContextMenu("Test Stop Cinematic")]
    private void TestStopCinematic()
    {
        StopCinematic();
    }

    [ContextMenu("Test Skip Cinematic")]
    private void TestSkipCinematic()
    {
        SkipCinematic();
    }

    #endregion
}
