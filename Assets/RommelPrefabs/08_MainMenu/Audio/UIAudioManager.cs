using UnityEngine;
using UnityEngine.SceneManagement;

public class UIAudioManager : MonoBehaviour
{
    public static UIAudioManager Instance { get; private set; }

    [Header("SFX")]
    public AudioClip hoverClip;
    public AudioClip clickClip;
    public float sfxVolume = 1f;
    public float hoverCooldown = 0.12f;

    [Header("Music (optional)")]
    public AudioClip backgroundMusic;
    public float musicVolume = 0.5f;
    public bool musicLoop = true;
    public bool playMusicOnAwake = true;

    // Nombre de la escena donde QUEREMOS que la música se apague
    [Header("Scenes")]
    public string stopMusicOnScene = "OpenWorldSceneMerged";
    public string mainMenuSceneName = "MainMenu";

    AudioSource sfxSource;
    AudioSource musicSource;
    float lastHoverTime = -10f;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.spatialBlend = 0f;
        sfxSource.volume = sfxVolume;

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = musicLoop;
        musicSource.spatialBlend = 0f;
        musicSource.volume = musicVolume;

        if (playMusicOnAwake && backgroundMusic != null)
            PlayMusic(backgroundMusic, musicVolume, musicLoop);

        // Suscribirse a cambios de escena
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // llamado cuando cualquier escena termina de cargarse
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
{
    if (scene.name == stopMusicOnScene)
    {
        StopMusic();
        return;
    }

    // Si volvemos al menú principal, reproducir la música del menú
    if (scene.name == mainMenuSceneName)
    {
        // Si ya está sonando no hacemos nada
        if (!musicSource.isPlaying && backgroundMusic != null)
            PlayMusic(backgroundMusic, musicVolume, musicLoop);
    }
}

    public void PlayHover()
    {
        if (hoverClip == null) return;
        if (Time.unscaledTime - lastHoverTime < hoverCooldown) return;
        lastHoverTime = Time.unscaledTime;
        sfxSource.PlayOneShot(hoverClip, sfxVolume);
    }

    public void PlayClick()
    {
        if (clickClip == null) return;
        sfxSource.PlayOneShot(clickClip, sfxVolume);
    }

    public void PlayMusic(AudioClip clip, float volume = 0.5f, bool loop = true)
    {
        if (clip == null) return;
        musicSource.clip = clip;
        musicSource.volume = volume;
        musicSource.loop = loop;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource.isPlaying) musicSource.Stop();
    }
}
