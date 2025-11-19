using UnityEngine;

namespace StarterAssets
{
    /// <summary>
    /// AudioManager Singleton - Maneja todos los sonidos del jugador
    /// </summary>
    public class AudioManager
    {
        private static AudioManager _instance;
        public static AudioManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AudioManager();
                }
                return _instance;
            }
        }

        // Draw & Sheath Sounds
        public AudioClip DrawSound;
        public float DrawSoundVolume = 1f;
        public AudioClip SheathSound;
        public float SheathSoundVolume = 1f;

        // Legacy Draw Sword Sound
        public AudioClip DrawSwordSound;
        public float DrawSwordVolume = 1f;

        // Ambient Sound
        public AudioClip AmbientSound;
        public float AmbientSoundVolume = 0.3f;

        // Attack Sounds
        public AudioClip[] AttackSoundClips;
        public float AttackSoundVolume = 1f;
        public int soundPlayMode = 0; // 0: Random, 1: Sequential, 2: RandomNoRepeat

        // Footstep Sounds
        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        public float FootstepAudioVolume = 0.5f;

        // AudioSources - se pasan desde ThirdPersonController
        private AudioSource generalAudioSource;
        private AudioSource ambientAudioSource;

        // Cooldowns para evitar spam de sonidos
        private float drawSoundCooldown = 0f;
        private float sheathSoundCooldown = 0f;
        private const float SOUND_COOLDOWN_DURATION = 1f;

        // Variables para selección de sonidos
        private int currentSoundIndex = 0;
        private int lastPlayedSoundIndex = -1;

        // Constructor privado para singleton
        private AudioManager()
        {
        }

        public void Initialize(AudioSource general, AudioSource ambient)
        {
            generalAudioSource = general;
            ambientAudioSource = ambient;

            if (ambientAudioSource != null)
            {
                ambientAudioSource.loop = true;
                ambientAudioSource.spatialBlend = 0f;
            }
        }
        public void UpdateCooldowns(float deltaTime)
        {
            if (drawSoundCooldown > 0f)
                drawSoundCooldown -= deltaTime;
            if (sheathSoundCooldown > 0f)
                sheathSoundCooldown -= deltaTime;
        }

        #region Draw & Sheath Sounds

        public void PlayDrawSound()
        {
            if (drawSoundCooldown > 0f)
            {
                Debug.Log("[AudioManager] Draw sound en cooldown");
                return;
            }

            if (generalAudioSource != null)
            {
                if (DrawSound != null)
                {
                    generalAudioSource.PlayOneShot(DrawSound, Mathf.Clamp01(DrawSoundVolume));
                    Debug.Log($"[AudioManager] ✓ Draw Sound reproducido con volumen {DrawSoundVolume}");
                }
                else if (DrawSwordSound != null)
                {
                    generalAudioSource.PlayOneShot(DrawSwordSound, DrawSwordVolume);
                    Debug.Log($"[AudioManager] ✓ DrawSwordSound (fallback) reproducido");
                }
                else
                {
                    Debug.LogWarning("[AudioManager] ❌ No hay sonido asignado para Draw");
                }

                drawSoundCooldown = SOUND_COOLDOWN_DURATION;
            }
        }

        public void PlaySheathSound()
        {
            if (sheathSoundCooldown > 0f)
            {
                Debug.Log("[AudioManager] Sheath sound en cooldown");
                return;
            }

            if (generalAudioSource != null)
            {
                if (SheathSound != null)
                {
                    generalAudioSource.PlayOneShot(SheathSound, Mathf.Clamp01(SheathSoundVolume));
                    Debug.Log($"[AudioManager] ✓ Sheath Sound reproducido con volumen {SheathSoundVolume}");
                }
                else if (DrawSwordSound != null)
                {
                    generalAudioSource.PlayOneShot(DrawSwordSound, DrawSwordVolume);
                    Debug.Log($"[AudioManager] ✓ DrawSwordSound (fallback) para Sheath");
                }
                else
                {
                    Debug.LogWarning("[AudioManager] ❌ No hay sonido asignado para Sheath");
                }

                sheathSoundCooldown = SOUND_COOLDOWN_DURATION;
            }
        }

        #endregion

        #region Attack Sounds

        public void PlayAttackSound()
        {
            if (AttackSoundClips == null || AttackSoundClips.Length == 0)
            {
                Debug.LogWarning("[AudioManager] ❌ No hay sonidos de ataque asignados");
                return;
            }

            int selectedIndex = SelectSoundIndex();

            if (AttackSoundClips[selectedIndex] != null)
            {
                generalAudioSource.PlayOneShot(AttackSoundClips[selectedIndex], Mathf.Clamp01(AttackSoundVolume));
                Debug.Log($"[AudioManager] ✓ Attack sound {selectedIndex + 1}/{AttackSoundClips.Length} - Modo: {soundPlayMode}");
            }
            else
            {
                Debug.LogWarning($"[AudioManager] ❌ Clip en índice {selectedIndex} es nulo");
            }
        }

        private int SelectSoundIndex()
        {
            switch (soundPlayMode)
            {
                case 0: // Random
                    return Random.Range(0, AttackSoundClips.Length);

                case 1: // Sequential
                    int seqIndex = currentSoundIndex;
                    currentSoundIndex = (currentSoundIndex + 1) % AttackSoundClips.Length;
                    return seqIndex;

                case 2: // RandomNoRepeat
                    if (AttackSoundClips.Length == 1)
                        return 0;

                    int randomIndex;
                    do
                    {
                        randomIndex = Random.Range(0, AttackSoundClips.Length);
                    }
                    while (randomIndex == lastPlayedSoundIndex);

                    lastPlayedSoundIndex = randomIndex;
                    return randomIndex;

                default:
                    return 0;
            }
        }

        #endregion

        #region Ambient Sound

        public void StartAmbientSound()
        {
            if (ambientAudioSource != null && AmbientSound != null)
            {
                ambientAudioSource.clip = AmbientSound;
                ambientAudioSource.volume = Mathf.Clamp01(AmbientSoundVolume);
                ambientAudioSource.Play();
                Debug.Log($"[AudioManager] ✓ Sonido ambiental iniciado con volumen {AmbientSoundVolume}");
            }
        }

        public void StopAmbientSound()
        {
            if (ambientAudioSource != null && ambientAudioSource.isPlaying)
            {
                ambientAudioSource.Stop();
                Debug.Log("[AudioManager] Sonido ambiental detenido");
            }
        }

        public void PauseAmbientSound()
        {
            if (ambientAudioSource != null && ambientAudioSource.isPlaying)
            {
                ambientAudioSource.Pause();
                Debug.Log("[AudioManager] Sonido ambiental pausado");
            }
        }

        public void ResumeAmbientSound()
        {
            if (ambientAudioSource != null && !ambientAudioSource.isPlaying)
            {
                ambientAudioSource.Play();
                Debug.Log("[AudioManager] Sonido ambiental reanudado");
            }
        }

        #endregion

        #region Footstep Sounds

        public void PlayFootstepSound(Vector3 position)
        {
            if (FootstepAudioClips != null && FootstepAudioClips.Length > 0)
            {
                var index = Random.Range(0, FootstepAudioClips.Length);
                AudioSource.PlayClipAtPoint(FootstepAudioClips[index], position, FootstepAudioVolume);
            }
        }

        public void PlayLandingSound(Vector3 position)
        {
            if (LandingAudioClip != null)
            {
                AudioSource.PlayClipAtPoint(LandingAudioClip, position, FootstepAudioVolume);
            }
        }

        #endregion
    }
}

