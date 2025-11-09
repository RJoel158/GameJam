using UnityEngine;

/// <summary>
/// Makes an AudioSource play independently of Time.timeScale
/// Useful for cinematics that freeze time
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class UnscaledAudioSource : MonoBehaviour
{
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        // Asegurar que el audio ignore timeScale
        if (audioSource != null)
        {
            audioSource.ignoreListenerPause = true;
            audioSource.ignoreListenerVolume = false;
            Debug.Log($"<color=cyan>[UnscaledAudioSource] AudioSource on {gameObject.name} configured to ignore timeScale</color>");
        }
    }

    /// <summary>
    /// Play a sound that ignores timeScale
    /// </summary>
    public void PlayUnscaled(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
