using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;
		public bool draw;
		public bool attack;
		public bool block;

		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

#if ENABLE_INPUT_SYSTEM
		private InputAction blockAction;

        private void Awake()
        {
            var playerInput = GetComponent<PlayerInput>();
            blockAction = playerInput.actions["Block"]; // Usa el mismo nombre de la acción en tu Input Actions

            // Escucha los eventos del sistema de entrada
            blockAction.started += OnBlockStarted;
            blockAction.canceled += OnBlockCanceled;
        }

        private void OnDestroy()
        {
            // Limpieza de eventos
            blockAction.started -= OnBlockStarted;
            blockAction.canceled -= OnBlockCanceled;
        }


        public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if(cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}
		#endif


		public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}

		public void OnDraw(InputValue value)
		{
			DrawInput(value.isPressed);
		}

		public void DrawInput(bool newDrawState)
        {
            draw = newDrawState;
        }

        public void OnAttack(InputValue value)
        {
            AttackInput(value.isPressed);
        }

        public void AttackInput(bool newDrawState)
        {
            attack = newDrawState;
        }

        private void OnBlockStarted(InputAction.CallbackContext context)
        {
            BlockInput(true); // Mantiene tu misma estructura
        }

        private void OnBlockCanceled(InputAction.CallbackContext context)
        {
            BlockInput(false);
        }

        public void OnBlock(InputValue value)
        {
            BlockInput(value.isPressed);
        }

        public void BlockInput(bool newBlockState)
        {
            block = newBlockState;
        }


        private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}
	
}