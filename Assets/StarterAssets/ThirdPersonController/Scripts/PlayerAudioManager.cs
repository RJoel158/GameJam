using UnityEngine;

namespace StarterAssets
{
    /// <summary>
    /// PlayerAudioManager Singleton - Maneja todos los sonidos del jugador
    /// Simplemente arrastra este componente a un GameObject en la escena y configura los clips
    /// </summary>
    public class PlayerAudioManager : MonoBehaviour
    {
        public static PlayerAudioManager Instance { get; private set; }

        [Header("▼ EQUIPAMIENTO - Draw & Sheath")]
        [Tooltip("Sonido al sacar la espada (tecla 1)")]
        public AudioClip drawSwordSound;
        [Tooltip("Sonido al guardar la espada")]
        public AudioClip sheathSwordSound;
        [Range(0f, 1f)]
        public float equipmentVolume = 0.8f;

        [Header("▼ COMBATE - Ataques")]
        [Tooltip("Array de sonidos de ataque - se reproducen aleatoriamente")]
        public AudioClip[] attackSounds;
        [Range(0f, 1f)]
        public float attackVolume = 0.7f;
        [Tooltip("0=Random, 1=Sequential, 2=Random sin repetir")]
        public int attackSoundMode = 2;

        [Header("▼ DAÑO - Recibir golpes")]
        [Tooltip("Sonidos de quejido al recibir daño")]
        public AudioClip[] hurtSounds;
        [Range(0f, 1f)]
        public float hurtVolume = 0.9f;

        [Header("▼ MOVIMIENTO - Pasos y Saltos")]
        [Tooltip("Sonidos de pasos (llamados desde Animation Events)")]
        public AudioClip[] footstepSounds;
        [Tooltip("Sonido al aterrizar después de saltar")]
        public AudioClip landingSound;
        [Range(0f, 1f)]
        public float footstepVolume = 0.5f;

        [Header("▼ AMBIENTE - Música de fondo")]
        [Tooltip("Música o sonido ambiental en loop")]
        public AudioClip ambientSound;
        [Range(0f, 1f)]
        public float ambientVolume = 0.3f;

        [Header("▼ BLOQUEO")]
        [Tooltip("Sonido al bloquear un ataque")]
        public AudioClip blockSound;
        [Range(0f, 1f)]
        public float blockVolume = 0.8f;

        // AudioSources privados
        private AudioSource effectsSource;
        private AudioSource ambientSource;

        // Control de reproducción
        private int lastAttackIndex = -1;
        private int currentAttackIndex = 0;

        private void Awake()
        {
            // Implementación Singleton
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[PlayerAudioManager] Ya existe una instancia. Destruyendo duplicado.");
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeAudioSources();
        }

        private void Start()
        {
            StartAmbientMusic();
        }

        private void InitializeAudioSources()
        {
            // AudioSource para efectos de sonido
            effectsSource = gameObject.GetComponent<AudioSource>();
            if (effectsSource == null)
            {
                effectsSource = gameObject.AddComponent<AudioSource>();
            }
            effectsSource.playOnAwake = false;
            effectsSource.spatialBlend = 0f; // efectos por defecto en 2D (no posicional)

            // AudioSource separado para música ambiental
            ambientSource = gameObject.AddComponent<AudioSource>();
            ambientSource.loop = true;
            ambientSource.playOnAwake = false;
            ambientSource.spatialBlend = 0f; // 2D sound

            Debug.Log("[PlayerAudioManager] ✓ AudioManager inicializado correctamente");
        }

        #region Métodos Públicos - Llamados desde ThirdPersonController

        /// <summary>
        /// Reproduce sonido al sacar la espada
        /// </summary>
        public void PlayDrawSword()
        {
            PlayEffectClip(drawSwordSound, equipmentVolume, interrupt: true, allowOverlap: false);
            Debug.Log("[Audio] 🗡️ Draw Sword");
        }

        /// <summary>
        /// Reproduce sonido al guardar la espada
        /// </summary>
        public void PlaySheathSword()
        {
            PlayEffectClip(sheathSwordSound, equipmentVolume, interrupt: true, allowOverlap: false);
            Debug.Log("[Audio] 📦 Sheath Sword");
        }

        /// <summary>
        /// Reproduce sonido de ataque (aleatorio o secuencial según configuración)
        /// </summary>
        public void PlayAttackSound()
        {
            if (attackSounds == null || attackSounds.Length == 0)
            {
                Debug.LogWarning("[Audio] ⚠️ No hay sonidos de ataque configurados");
                return;
            }
            int index = GetAttackSoundIndex();
            // Interrumpe y reproduce la versión más reciente (más responsivo para combos rápidos)
            PlayEffectClip(attackSounds[index], attackVolume, interrupt: true, allowOverlap: false);
            Debug.Log($"[Audio] ⚔️ Attack Sound {index + 1}/{attackSounds.Length}");
        }

        /// <summary>
        /// Reproduce sonido de quejido al recibir daño
        /// </summary>
        public void PlayHurtSound()
        {
            if (hurtSounds == null || hurtSounds.Length == 0)
            {
                Debug.LogWarning("[Audio] ⚠️ No hay sonidos de daño configurados");
                return;
            }

            int randomIndex = Random.Range(0, hurtSounds.Length);
            // Para que el feedback de recibir daño sea inmediato, interrumpimos cualquier efecto actual
            PlayEffectClip(hurtSounds[randomIndex], hurtVolume, interrupt: true, allowOverlap: false);
            Debug.Log($"[Audio] 😖 Hurt Sound {randomIndex + 1}/{hurtSounds.Length}");
        }

        /// <summary>
        /// Reproduce sonido de paso (llamado desde Animation Event)
        /// </summary>
        public void PlayFootstepSound(Vector3 position)
        {
            if (footstepSounds == null || footstepSounds.Length == 0)
                return;

            int randomIndex = Random.Range(0, footstepSounds.Length);

            if (footstepSounds[randomIndex] != null)
            {
                AudioSource.PlayClipAtPoint(footstepSounds[randomIndex], position, footstepVolume);
            }
        }

        /// <summary>
        /// Reproduce sonido al aterrizar (llamado desde Animation Event)
        /// </summary>
        public void PlayLandingSound(Vector3 position)
        {
            if (landingSound != null)
            {
                AudioSource.PlayClipAtPoint(landingSound, position, footstepVolume);
                Debug.Log("[Audio] 🦶 Landing");
            }
        }

        /// <summary>
        /// Reproduce sonido al bloquear un ataque
        /// </summary>
        public void PlayBlockSound()
        {
            PlayEffectClip(blockSound, blockVolume, interrupt: true, allowOverlap: false);
            Debug.Log("[Audio] 🛡️ Block");
        }

        /// <summary>
        /// Reproduce un clip de efecto con reglas de prioridad.
        /// - allowOverlap = true -> usa PlayOneShot (permitir solapamiento)
        /// - interrupt = true -> detiene el clip actual y reproduce el nuevo
        /// - interrupt = false && !allowOverlap -> solo reproduce si no hay nada sonando
        /// </summary>
        private void PlayEffectClip(AudioClip clip, float volume, bool interrupt = true, bool allowOverlap = false)
        {
            if (clip == null || effectsSource == null) return;

            if (allowOverlap)
            {
                effectsSource.PlayOneShot(clip, volume);
                return;
            }

            if (interrupt)
            {
                effectsSource.Stop();
                effectsSource.clip = clip;
                effectsSource.volume = volume;
                effectsSource.Play();
                return;
            }

            // No interrumpir: solo reproducir si no está sonando nada
            if (!effectsSource.isPlaying)
            {
                effectsSource.clip = clip;
                effectsSource.volume = volume;
                effectsSource.Play();
            }
        }

        #endregion

        #region Control de Música Ambiental

        /// <summary>
        /// Inicia la música ambiental
        /// </summary>
        public void StartAmbientMusic()
        {
            if (ambientSound != null && ambientSource != null && !ambientSource.isPlaying)
            {
                ambientSource.clip = ambientSound;
                ambientSource.volume = ambientVolume;
                ambientSource.Play();
                Debug.Log("[Audio] 🎵 Ambient music started");
            }
        }

        /// <summary>
        /// Pausa la música ambiental
        /// </summary>
        public void PauseAmbientMusic()
        {
            if (ambientSource != null && ambientSource.isPlaying)
            {
                ambientSource.Pause();
                Debug.Log("[Audio] ⏸️ Ambient music paused");
            }
        }

        /// <summary>
        /// Reanuda la música ambiental
        /// </summary>
        public void ResumeAmbientMusic()
        {
            if (ambientSource != null && !ambientSource.isPlaying && ambientSource.clip != null)
            {
                ambientSource.UnPause();
                Debug.Log("[Audio] ▶️ Ambient music resumed");
            }
        }

        /// <summary>
        /// Detiene la música ambiental
        /// </summary>
        public void StopAmbientMusic()
        {
            if (ambientSource != null && ambientSource.isPlaying)
            {
                ambientSource.Stop();
                Debug.Log("[Audio] ⏹️ Ambient music stopped");
            }
        }

        /// <summary>
        /// Cambia el volumen de la música ambiental
        /// </summary>
        public void SetAmbientVolume(float volume)
        {
            if (ambientSource != null)
            {
                ambientSource.volume = Mathf.Clamp01(volume);
            }
        }

        #endregion

        #region Métodos Privados

        private int GetAttackSoundIndex()
        {
            switch (attackSoundMode)
            {
                case 0: // Random
                    return Random.Range(0, attackSounds.Length);

                case 1: // Sequential
                    int seqIndex = currentAttackIndex;
                    currentAttackIndex = (currentAttackIndex + 1) % attackSounds.Length;
                    return seqIndex;

                case 2: // Random sin repetir
                    if (attackSounds.Length == 1)
                        return 0;

                    int randomIndex;
                    do
                    {
                        randomIndex = Random.Range(0, attackSounds.Length);
                    }
                    while (randomIndex == lastAttackIndex);

                    lastAttackIndex = randomIndex;
                    return randomIndex;

                default:
                    return 0;
            }
        }

        #endregion

        #region Debugging

        private void OnValidate()
        {
            // Validación en el Inspector
            if (attackSounds != null && attackSounds.Length > 0)
            {
                int nullCount = 0;
                foreach (var clip in attackSounds)
                {
                    if (clip == null) nullCount++;
                }

                if (nullCount > 0)
                {
                    Debug.LogWarning($"[PlayerAudioManager] {nullCount} sonidos de ataque están vacíos");
                }
            }
        }

        #endregion
    }
}
