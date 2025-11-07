using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Player")]
        public float MoveSpeed = 2.0f;
        public float SprintSpeed = 5.335f;
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;
        public float SpeedChangeRate = 10.0f;

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Space(10)]
        public AudioClip DrawSwordSound;
        public float DrawSwordVolume = 1f;
        
        [Header("Draw & Sheath Sounds")]
        public AudioClip DrawSound;
        [Range(0f, 1f)]
        public float DrawSoundVolume = 1f;
        public AudioClip SheathSound;
        [Range(0f, 1f)]
        public float SheathSoundVolume = 1f;

        [Header("Ambient Sound")]
        public AudioClip AmbientSound;
        [Range(0f, 1f)]
        public float AmbientSoundVolume = 0.3f;
        private AudioSource ambientAudioSource;

       [Header("F Key Sound")]
[Tooltip("Array de clips de audio para reproducir")]
public AudioClip[] FSoundClips;
[Range(0f, 1f)]
public float FSoundVolume = 1f;

[Tooltip("Modo de selección: 0=Random, 1=Sequential, 2=RandomNoRepeat")]
public int soundPlayMode = 0; // 0: Random, 1: Sequential, 2: RandomNoRepeat

private int currentSoundIndex = 0;
private int lastPlayedSoundIndex = -1;

        [Space(10)]
        public float JumpHeight = 1.2f;
        public float Gravity = -15.0f;

        [Space(10)]
        public float JumpTimeout = 0.50f;
        public float FallTimeout = 0.15f;

        [Header("Stadistics")]
        [Range(0f, 100f)]
        public float healthPercent = 100f;
        public int health = 1000;
        public int maxHealth = 2000;
        public bool death = false;
        public bool dead = false;
        public bool hitting = false;

        [Space(10)]
        [Range(0f, 100f)]
        public float forcePercent = 100f;
        public int force = 200;
        public int maxForce = 2000;

        [Header("Player Grounded")]
        public bool Grounded = true;
        public float GroundedOffset = -0.14f;
        public float GroundedRadius = 0.28f;
        public LayerMask GroundLayers;

        [Header("Equipment")]
        public bool isEquipping;
        public bool isEquipped;

        [Header("Attack")]
        public bool isAttacking;
        public bool inAttackAnimation = false;

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f;

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false;

        // cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        // player
        private float _speed;
        public float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;
        private int _animIDDrawSword;
        private int _animIDSheathSword;
        private int _animIDEquipped;
        private int _animIDAttack;
        private int _animIDAttacking;
        private int _animIDInAir;
        private int _animIDDeath;
        private int _animIDHitting;

#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
#endif

        public Animator _animator;
        private CharacterController _controller;
        public StarterAssetsInputs _input;
        private GameObject _mainCamera;
        private AudioSource audioSource;

        private const float _threshold = 0.01f;

        private bool _hasAnimator;

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
                return true;
#endif
            }
        }

        // PlayerControllerParameters
        private PlayerController playerController;
        private FaseColorController faseColorController;

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();
            faseColorController = FindFirstObjectByType<FaseColorController>();

            // get a reference to our main camera
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        private void Start()
        {
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

            _hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
            audioSource = GetComponent<AudioSource>();
            
            // Si no hay AudioSource, crear uno automáticamente
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                Debug.Log("[AUDIO] AudioSource creado automáticamente en el jugador");
            }
            
            // Crear un segundo AudioSource para el sonido ambiental
            ambientAudioSource = gameObject.AddComponent<AudioSource>();
            ambientAudioSource.loop = true; // Loop infinito
            ambientAudioSource.spatialBlend = 0f; // 2D, no 3D
            
            // Iniciar el sonido ambiental
            StartAmbientSound();
            
#if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<PlayerInput>();
#endif

            AssignAnimationIDs();

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }

        private void Update()
        {
            _hasAnimator = TryGetComponent(out _animator);

            Attack();
            DrawSheathSword();
            HandleFKeySound();
            JumpAndGravity();
            GroundedCheck();
            Move();
            HandleStadistics();
        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
            _animIDDrawSword = Animator.StringToHash("DrawSword");
            _animIDSheathSword = Animator.StringToHash("SheathSword");
            _animIDEquipped = Animator.StringToHash("Equipped");
            _animIDAttack = Animator.StringToHash("Attack");
            _animIDAttacking = Animator.StringToHash("Attacking");
            _animIDInAir = Animator.StringToHash("InAir");
            _animIDDeath = Animator.StringToHash("Dead");
            _animIDHitting = Animator.StringToHash("Hitting");
        }

        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }

        private void CameraRotation()
        {
            // if there is an input and camera position is not fixed
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                //Don't multiply mouse input by Time.deltaTime;
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
            }

            // clamp our rotations so our values are limited 360 degrees
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // Cinemachine will follow this target
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw, 0.0f);
        }

        private void Move()
        {
            // Prevenir movimiento si está muerto
            if (dead || death)
            {
                return;
            }

            // ELIMINAR, RESTRINGE EL MOVIMIENTO EN LA EQUIPACION Y BLOQUEO
            //if (playerController.isEquipping || playerController.isBlocking)
            //{
            //    return;
            //}

            // set target speed based on move speed, sprint speed and if sprint is pressed
            // NO permite correr si el stamina está en 0
            bool canSprint = (faseColorController != null) ? faseColorController.CanSprint() : true;
            
            // Si stamina llegó a 0, fuerza cancelar sprint
            if (!canSprint)
            {
                _input.sprint = false;
            }
            
            float targetSpeed = (_input.sprint && canSprint) ? SprintSpeed : MoveSpeed;

            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

            // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // normalise input direction
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if (_input.move != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    RotationSmoothTime);

                // rotate to face input direction relative to camera position
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }


            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            // move the player
            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }

        private void JumpAndGravity()
        {
            if (Grounded)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;
                _animator.SetBool(_animIDInAir, false);

                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // Jump
                if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, true);
                        _animator.SetBool(_animIDInAir, true);
                    }
                }

                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // reset the jump timeout timer
                _jumpTimeoutDelta = JumpTimeout;

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFreeFall, true);
                        _animator.SetBool(_animIDInAir, true);
                    }
                }

                // if we are not grounded, do not jump
                _input.jump = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        private void HandleStadistics()
        {
            // Actualizar animator con estados de muerte y golpe
            _animator.SetBool(_animIDDeath, dead || death);
            _animator.SetBool(_animIDHitting, hitting);

            healthPercent = (health * 100) / maxHealth;

            forcePercent = (force * 100) / maxForce;
        }

        public void TakeDamage(int damageAmount)
        {
            if (!dead && !death)
            {
                health -= damageAmount;
                _animator.SetTrigger("Damage");
                //CameraShake.Instance.ShakeCamera(2f, 0.2f);
            }

            if (health <= 0)
            {
                death = true;
                dead = true;
                Die();
            }
        }

        void Die()
        {
            //Instantiate(ragdoll, transform.position, transform.rotation);
            _animator.SetTrigger("Death");
            //Destroy(this.gameObject);
        }

        private void DrawSheathSword()
        {
            if (Grounded)
            {
                // Draw (Tecla 1 - Sacar espada)
                if (_input.draw && !isEquipped)
                {
                    // update animator if using character
                    if (_hasAnimator)
                    {
                        isEquipping = true;
                        _animator.SetTrigger(_animIDDrawSword);
                        _animator.SetBool(_animIDEquipped, true);
                        _input.draw = false;
                        
                        // Reproducir sonido personalizado de Draw (tecla 1)
                        PlayDrawSound();
                    }
                }
                else if (_input.draw && isEquipped)
                {
                    // update animator if using character
                    if (_hasAnimator)
                    {
                        isEquipping = false;
                        _animator.SetTrigger(_animIDSheathSword);
                        _animator.SetBool(_animIDEquipped, false);
                        _input.draw = false;
                        
                        // Reproducir sonido personalizado de Sheath (guardar)
                        PlaySheathSound();
                    }
                }
            }
        }

        private void HandleFKeySound()
{
    // Click izquierdo del mouse solo si tiene la espada equipada
    if (Input.GetMouseButtonDown(0) && isEquipped)
    {
        PlayFSound();
    }
}

        private void PlayDrawSound()
        {
            // Intentar obtener o crear AudioSource
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                    Debug.Log("[DRAW SOUND] AudioSource creado en tiempo de ejecución");
                }
            }

            if (audioSource != null)
            {
                if (DrawSound != null)
                {
                    audioSource.PlayOneShot(DrawSound, Mathf.Clamp01(DrawSoundVolume));
                    Debug.Log($"[DRAW SOUND] ✓ Reproduciendo Draw Sound con volumen {DrawSoundVolume}");
                }
                else if (DrawSwordSound != null)
                {
                    audioSource.PlayOneShot(DrawSwordSound, DrawSwordVolume);
                    Debug.Log($"[DRAW SOUND] ✓ Reproduciendo DrawSwordSound (fallback) con volumen {DrawSwordVolume}");
                }
                else
                {
                    Debug.LogWarning("[DRAW SOUND] ❌ No hay sonido asignado para Draw");
                }
            }
            else
            {
                Debug.LogError("[DRAW SOUND] ❌ No se pudo crear AudioSource");
            }
        }

        private void PlaySheathSound()
        {
            // Intentar obtener o crear AudioSource
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                    Debug.Log("[SHEATH SOUND] AudioSource creado en tiempo de ejecución");
                }
            }

            if (audioSource != null)
            {
                if (SheathSound != null)
                {
                    audioSource.PlayOneShot(SheathSound, Mathf.Clamp01(SheathSoundVolume));
                    Debug.Log($"[SHEATH SOUND] ✓ Reproduciendo Sheath Sound con volumen {SheathSoundVolume}");
                }
                else if (DrawSwordSound != null)
                {
                    audioSource.PlayOneShot(DrawSwordSound, DrawSwordVolume);
                    Debug.Log($"[SHEATH SOUND] ✓ Reproduciendo DrawSwordSound (fallback) con volumen {DrawSwordVolume}");
                }
                else
                {
                    Debug.LogWarning("[SHEATH SOUND] ❌ No hay sonido asignado para Sheath");
                }
            }
            else
            {
                Debug.LogError("[SHEATH SOUND] ❌ No se pudo crear AudioSource");
            }
        }

        private void PlayFSound()
{
    // Intentar obtener o crear AudioSource
    if (audioSource == null)
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            Debug.Log("[F SOUND] AudioSource creado en tiempo de ejecución");
        }
    }

    // Verificar que hay sonidos en el array
    if (FSoundClips == null || FSoundClips.Length == 0)
    {
        Debug.LogWarning("[F SOUND] ❌ No hay sonidos asignados en el array");
        return;
    }

    // Seleccionar el índice según el modo
    int selectedIndex = SelectSoundIndex();

    // Verificar que el clip no es nulo
    if (FSoundClips[selectedIndex] != null)
    {
        audioSource.PlayOneShot(FSoundClips[selectedIndex], Mathf.Clamp01(FSoundVolume));
        Debug.Log($"[F SOUND] ✓ Reproduciendo sonido {selectedIndex + 1}/{FSoundClips.Length} - Modo: {soundPlayMode}");
    }
    else
    {
        Debug.LogWarning($"[F SOUND] ❌ El clip en índice {selectedIndex} es nulo");
    }
}

private int SelectSoundIndex()
{
    switch (soundPlayMode)
    {
        case 0: // Random
            return Random.Range(0, FSoundClips.Length);

        case 1: // Sequential
            int seqIndex = currentSoundIndex;
            currentSoundIndex = (currentSoundIndex + 1) % FSoundClips.Length;
            return seqIndex;

        case 2: // RandomNoRepeat
            if (FSoundClips.Length == 1)
                return 0;
            
            int randomIndex;
            do
            {
                randomIndex = Random.Range(0, FSoundClips.Length);
            }
            while (randomIndex == lastPlayedSoundIndex);
            
            lastPlayedSoundIndex = randomIndex;
            return randomIndex;

        default:
            return 0;
    }
}

        private void StartAmbientSound()
        {
            if (ambientAudioSource == null)
            {
                ambientAudioSource = gameObject.AddComponent<AudioSource>();
                ambientAudioSource.loop = true;
                ambientAudioSource.spatialBlend = 0f;
                Debug.Log("[AMBIENT] AudioSource para sonido ambiental creado");
            }

            if (ambientAudioSource != null && AmbientSound != null)
            {
                ambientAudioSource.clip = AmbientSound;
                ambientAudioSource.volume = Mathf.Clamp01(AmbientSoundVolume);
                ambientAudioSource.Play();
                Debug.Log($"[AMBIENT] ✓ Sonido ambiental iniciado con volumen {AmbientSoundVolume}");
            }
            else if (AmbientSound == null)
            {
                Debug.LogWarning("[AMBIENT] ⚠️ No hay sonido ambiental asignado. Asigna un clip en el Inspector.");
            }
            else
            {
                Debug.LogError("[AMBIENT] ❌ No se pudo crear AudioSource para sonido ambiental");
            }
        }

        public void StopAmbientSound()
        {
            if (ambientAudioSource != null && ambientAudioSource.isPlaying)
            {
                ambientAudioSource.Stop();
                Debug.Log("[AMBIENT] Sonido ambiental detenido");
            }
        }

        public void PauseAmbientSound()
        {
            if (ambientAudioSource != null && ambientAudioSource.isPlaying)
            {
                ambientAudioSource.Pause();
                Debug.Log("[AMBIENT] Sonido ambiental pausado");
            }
        }

        public void ResumeAmbientSound()
        {
            if (ambientAudioSource != null && !ambientAudioSource.isPlaying)
            {
                ambientAudioSource.Play();
                Debug.Log("[AMBIENT] Sonido ambiental reanudado");
            }
        }

        private void Attack()
        {
            _animator.SetBool(_animIDAttacking, inAttackAnimation);

            if (Grounded)
            {
                // NO permite atacar si stamina es 0 o no hay suficiente o está siendo golpeado
                if (_input.attack && isEquipped && !isAttacking && !hitting)
                {
                    // update animator if using character
                    if (_hasAnimator)
                    {
                        // Intentar consumir stamina para atacar
                        if (faseColorController != null && faseColorController.TryConsumeStaminaForAttack())
                        {
                            _animator.SetTrigger(_animIDAttack);
                            _animator.SetFloat(_animIDSpeed, 0);
                            _input.attack = false;
                        }
                        else
                        {
                            // No hay stamina suficiente - cancelar ataque
                            _input.attack = false;
                        }
                    }
                }
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }

        // Animation Events
        public void EndLandAnimation()
        {
            _animator.SetBool(_animIDInAir, false);
        }

        public void StartPlayerDamage()
        {
            hitting = true;
        }

        public void EndPlayerDamage()
        {
            hitting = false;
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }
    }
}