using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonSound : MonoBehaviour, IPointerDownHandler
{
    public bool playClickFromManager = true; // si quieres reproducir via UIAudioManager
    public AudioClip overrideClickClip; // opcional: clip local (si lo pones, ignora manager)
    public float volume = 1f;

    public void OnPointerDown(PointerEventData eventData)
    {
        // prioridad: overrideClip -> manager clip
        if (overrideClickClip != null)
        {
            // reproducir local
            var src = gameObject.AddComponent<AudioSource>();
            src.spatialBlend = 0f;
            src.playOnAwake = false;
            src.volume = volume;
            src.PlayOneShot(overrideClickClip, volume);
            Destroy(src, overrideClickClip.length + 0.1f);
        }
        else if (UIAudioManager.Instance != null && playClickFromManager)
        {
            UIAudioManager.Instance.PlayClick();
        }
    }
}
