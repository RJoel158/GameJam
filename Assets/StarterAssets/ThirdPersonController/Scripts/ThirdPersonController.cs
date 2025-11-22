using UnityEngine;
#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{


    [RequireComponent(typeof(CharacterController))]

    [RequireComponent(typeof(PlayerInput))]

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
        public bool inHitAnimation = false;

        [Space(10)]
        public bool hardModeEnabled = false;
        public float hardModeTime = 30f;
        public float hardModeTimer = 0f;
        public bool inHardModeAnimation = false;

        [Space(10)]
        [Range(0f, 100f)]
        public float staminaPercent = 100f;
        public int stamina = 100;
        public int maxStamina = 2000;

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
        [Space(4)]
        [Tooltip("Tiempo mínimo entre acciones de sacar/guardar espada (segundos)")]
        public float drawCooldown = 0.35f;
        private float _drawTimer = 0f;
        private bool _canDraw = true;

        [Header("Attack")]
        public bool isAttacking;
        public bool inAttackAnimation = false;
        public bool canAttack = true;
        [Tooltip("Cooldown entre ataques para evitar spam (segundos)")]
        public float attackCooldown = 0.25f;
        private float _attackTimer = 0f;

        [Header("Block")]
        public bool isBlocking;
        public float timeBtwResetBlock = 2f;
        public float resetBlockTimer = 0f;
        public bool canBlock = true;
        public bool block = false;

        [Header("Materials")]
        public Material hardModeMaterial;
        public Material originalMaterial;
        public GameObject playerTextureObject;

        [Header("UI & Systems")]
        private FaseColorController faseColorController;

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
        private int _animIDDamage;
        private int _animIDDeath;
        private int _animIDHitting;
        public int _animIDBlock;
        private int _animIDBlocked;
        private int _animIDStamina;
        private int _animIDHardMode;
        private int _animIDForce;
        private int _animIDBlocking;

        private PlayerInput _playerInput;

        public Animator _animator;
        private CharacterController _controller;
        public StarterAssetsInputs _input;
        private GameObject _mainCamera;

        private const float _threshold = 0.01f;

        private bool _hasAnimator;

        private bool IsCurrentDeviceMouse
        {
            get
            {
                return _playerInput.currentControlScheme == "KeyboardMouse";
            }
        }

        // PlayerControllerParameters
        private PlayerController playerController;

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();

            // get a reference to our main camera
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        private void Start()
        {
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

            // Initialize required components early to avoid NullReferenceExceptions
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
            _playerInput = GetComponent<PlayerInput>();
            faseColorController = FindFirstObjectByType<FaseColorController>();

            // Assign animator reference and hashes before any animator calls
            _hasAnimator = TryGetComponent(out _animator);
            AssignAnimationIDs();

            // Ensure cooldown timers are initialized
            _attackTimer = 0f;
            canAttack = true;
            _drawTimer = 0f;
            _canDraw = true;

            // Call initial handlers after initialization
            Attack();
            DrawSheathSword();

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }

        private void Update()
        {
            // avoid TryGetComponent each frame - components are cached in Start()
            Attack();
            DrawSheathSword();
            JumpAndGravity();
            GroundedCheck();
            Move();
            HandleStadistics();
            HandleBlock();
            HandleHardMode();
            HandleResetBlock();
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
            _animIDDamage = Animator.StringToHash("Damage");
            _animIDDeath = Animator.StringToHash("Dead");
            _animIDHitting = Animator.StringToHash("Hitting");
            _animIDBlock = Animator.StringToHash("Block");
            _animIDBlocked = Animator.StringToHash("Blocked");
            _animIDStamina = Animator.StringToHash("Stamina");
            _animIDHardMode = Animator.StringToHash("HardMode");
            _animIDForce = Animator.StringToHash("Force");
            _animIDBlocking = Animator.StringToHash("Blocking");
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
            if (dead || isBlocking || inHardModeAnimation)
            {
                return;
            }

            // ELIMINAR, RESTRINGE EL MOVIMIENTO EN LA EQUIPACION Y BLOQUEO
            //if (playerController.isEquipping || playerController.isBlocking)
            //{
            //    return;
            //}

            // Verificar si estamina llegó a 0 mientras estaba corriendo - forzar a caminar
            bool canSprint = faseColorController != null ? faseColorController.CanSprint() : true;
            float targetSpeed = (_input.sprint && canSprint && staminaPercent > 0) ? SprintSpeed : MoveSpeed;

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

            // Si no puede correr más, forzar animación de walk inmediatamente
            if (!canSprint && _input.sprint && _animationBlend > MoveSpeed)
            {
                _animationBlend = MoveSpeed;
            }
            else
            {
                _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            }
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
            _animator.SetBool(_animIDDeath, dead);
            _animator.SetBool(_animIDHitting, inHitAnimation);
            _animator.SetInteger(_animIDStamina, (int)staminaPercent);
            _animator.SetInteger(_animIDForce, (int)forcePercent);

            healthPercent = (health * 100) / maxHealth;

            staminaPercent = (stamina * 100) / maxStamina;

            forcePercent = (force * 100) / maxForce;

            if (hardModeEnabled)
            {
                stamina = maxStamina;
                health = maxHealth;
            }
        }

        public void TakeDamage(int damageAmount)
        {
            if (!dead && !isBlocking)
            {
                health -= damageAmount;
                _animator.SetTrigger(_animIDDamage);
                //CameraShake.Instance.ShakeCamera(2f, 0.2f);

                // Reproducir sonido de quejido al recibir daño
                // if (PlayerAudioManager.Instance != null)
                //     PlayerAudioManager.Instance.PlayHurtSound();
            }
            else if (!dead && isBlocking && staminaPercent > 0)
            {
                // Consumir estamina al bloquear/recibir daño
                if (faseColorController != null)
                {
                    faseColorController.ConsumeStaminaForDamage();
                }

                _animator.SetTrigger(_animIDBlocked);

                // Reproducir sonido de bloqueo
                // if (PlayerAudioManager.Instance != null)
                //     PlayerAudioManager.Instance.PlayBlockSound();
            }

            if (health <= 0)
            {
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
                // Draw
                if (_input.draw && !isEquipped)
                {
                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetTrigger(_animIDDrawSword);
                        _animator.SetBool(_animIDEquipped, true);
                        _input.draw = false;

                        //Reproducir sonido de sacar espada
                         if (PlayerAudioManager.Instance != null)
                            PlayerAudioManager.Instance.PlayDrawSword();
                    }
                }
                else if (_input.draw && isEquipped)
                {
                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetTrigger(_animIDSheathSword);
                        _animator.SetBool(_animIDEquipped, false);
                        _input.draw = false;

                        // Reproducir sonido de guardar espada
                        if (PlayerAudioManager.Instance != null)
                            PlayerAudioManager.Instance.PlaySheathSword();
                    }
                }
            }
        }

        private void Attack()
        {
            _animator.SetBool(_animIDAttacking, inAttackAnimation);

            if (Grounded && faseColorController != null)
            {
                // Attack
                if (_input.attack && isEquipped && !isAttacking && !inHitAnimation)
                {
                    // update animator if using character
                    if (canAttack && staminaPercent > 0)
                    {
                        // Intentar atacar - FaseColorController verifica estamina
                        bool canPerformAttack = faseColorController.TryAttack();
                        if (canPerformAttack)
                        {
                            _animator.SetTrigger(_animIDAttack);
                            _animator.SetFloat(_animIDSpeed, 0);
                            _input.attack = false;
                            canAttack = false;
                        }
                        else
                        {
                            Debug.Log("[Ataque] No hay suficiente estamina para atacar");
                            _input.attack = false;
                        }
                    }
                }
            }

            if (!canAttack)
            {
                if (_attackTimer <= attackCooldown)
                {
                    _attackTimer += Time.deltaTime;
                }
                else
                {
                    canAttack = true;
                    _attackTimer = 0;
                }
            }
        }

        private void HandleBlock()
        {   
            _animator.SetBool(_animIDBlocking, isBlocking);
            _animator.SetBool(_animIDBlock, block);

            if (faseColorController == null) return;
            
            // Reportar estado de bloqueo a FaseColorController
            if (_input.block && !isBlocking && Grounded && !isAttacking && !isEquipping && _animationBlend <= 0.01f)
            {
                // Intentar iniciar bloqueo
                if (faseColorController.TryStartBlock())
                {
                    if (_hasAnimator && staminaPercent > 0)
                    {
                        block = true;
                    }
                    //isBlocking = true;
                }
            }
            else if (!_input.block && isBlocking)
            {
                // Soltar bloqueo
                faseColorController.StopBlock();
                if (_hasAnimator)
                {
                    block = false;
                }
                isBlocking = false;
            }
            
            // Verificar si FaseColorController dice que debe detener el bloqueo (por falta de estamina)
            if (!faseColorController.IsBlockingActive())
            {
                if (_hasAnimator)
                {
                    block = false;
                    faseColorController.StopBlock();
                }
                isBlocking = false;
            }
        }

        private void HandleResetBlock()
        {
            if (!canBlock)
            {
                if (resetBlockTimer <= timeBtwResetBlock)
                {
                    isBlocking = false;
                    resetBlockTimer += Time.deltaTime;
                }
                else
                {
                    canBlock = true;
                    resetBlockTimer = 0;
                }
            }
        }

        private void HandleHardMode()
        {
            if (Grounded && !isAttacking && !isEquipping && _animationBlend <= 0.01f)
            {
                if (_input.hardMode && !hardModeEnabled && forcePercent == 100)
                {
                    if (_hasAnimator)
                    {
                        _animator.SetTrigger(_animIDHardMode);
                        hardModeEnabled = true;
                        
                        // Consumir mana al activar Hard Mode desde FaseColorController
                        if (faseColorController != null)
                        {
                            faseColorController.ConsumeManForHardMode();
                        }
                    }
                }
            }

            if (hardModeEnabled)
            {
                // Verificar si el mana se agotó - si es así, desactivar Hard Mode
                if (faseColorController != null && faseColorController.mana <= 0)
                {
                    DeactivateHardMode();
                }
                else if (forcePercent >= 0)
                {
                    force -= 1;
                }
                else
                {
                    DeactivateHardMode();
                }
            }
            else if (force < maxForce)
            {
                force += 1;
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
            Gizmos.DrawWireSphere(
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
            inHitAnimation = true;
            if (PlayerAudioManager.Instance != null)
            {
                PlayerAudioManager.Instance.PlayPlayerHurtSound();
            }
        }

        public void EndPlayerDamage()
        {
            inHitAnimation = false;
        }

        public void StartEquipping()
        {
            isEquipping = true;
        }

        public void EndEquipping()
        {
            isEquipping = false;
        }

        public void StartBlocking()
        {
            isBlocking = true;
        }

        public void BlockSound()
        {
            if (PlayerAudioManager.Instance != null)
            {
                PlayerAudioManager.Instance.PlayBlockSound();
            }
        }

        public void StartHardMode()
        {
            inHardModeAnimation = true;

            if (PlayerAudioManager.Instance != null)
            {
                PlayerAudioManager.Instance.PlayHardModeSound();
            }
        }

        public void EndHardMode()
        {
            inHardModeAnimation = false;
        }

        public void ActiveHardMode()
        {
            Debug.Log("Hard Mode Active");
            _input.hardMode = false;

            // Guardar el material original la primera vez
            if (originalMaterial == null && playerTextureObject != null)
            {
                var renderer = playerTextureObject.GetComponent<Renderer>();
                if (renderer != null)
                    originalMaterial = renderer.material;
            }

            // Cambiar al material de modo difícil
            if (playerTextureObject != null && hardModeMaterial != null)
            {
                var renderer = playerTextureObject.GetComponent<Renderer>();
                if (renderer != null)
                    renderer.material = hardModeMaterial;
            }
        }

        public void DeactivateHardMode()
        {
            if (playerTextureObject != null && originalMaterial != null)
            {
                var renderer = playerTextureObject.GetComponent<Renderer>();
                if (renderer != null)
                    renderer.material = originalMaterial;
            }

            hardModeEnabled = false;
            Debug.Log("Hard Mode Deactivated");
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                // Usar PlayerAudioManager si está disponible
                // if (PlayerAudioManager.Instance != null)
                // {
                //     PlayerAudioManager.Instance.PlayFootstepSound(transform.TransformPoint(_controller.center));
                // }
                // Fallback al sistema antiguo
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
                // Usar PlayerAudioManager si está disponible
                // if (PlayerAudioManager.Instance != null)
                // {
                //     PlayerAudioManager.Instance.PlayLandingSound(transform.TransformPoint(_controller.center));
                // }
                // Fallback al sistema antiguo
                if (LandingAudioClip != null)
                {
                    AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
                }
            }
        }

        // Método público para que FaseColorController actualice la estamina
        public void UpdateStaminaFromUI(int newStamina, float newStaminaPercent)
        {
            stamina = newStamina;
            staminaPercent = newStaminaPercent;
        }
    }
}