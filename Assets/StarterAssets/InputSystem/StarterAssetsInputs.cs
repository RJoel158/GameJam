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
		public bool hardMode;
		public bool interactF; // Tecla F para interactuar/hacer sonido

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
            if (playerInput != null && playerInput.actions != null)
            {
                blockAction = playerInput.actions["Block"];
                if (blockAction != null)
                {
                    blockAction.started += OnBlockStarted;
                    blockAction.canceled += OnBlockCanceled;
                }
            }
        }

        private void OnDestroy()
        {
            if (blockAction != null)
            {
                blockAction.started -= OnBlockStarted;
                blockAction.canceled -= OnBlockCanceled;
            }
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

#if ENABLE_INPUT_SYSTEM
	public void OnDraw(InputValue value)
	{
		DrawInput(value.isPressed);
	}

	public void OnAttack(InputValue value)
	{
		AttackInput(value.isPressed);
	}

	public void OnInteractF(InputValue value)
	{
		InteractFInput(value.isPressed);
	}
#endif

		public void DrawInput(bool newDrawState)
		{
			draw = newDrawState;
		}

		public void AttackInput(bool newDrawState)
		{
			attack = newDrawState;
		}

	public void InteractFInput(bool newInteractFState)
	{
		interactF = newInteractFState;
	}

    private void OnBlockStarted(InputAction.CallbackContext context)
    {
        BlockInput(true);
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

    public void OnHardMode(InputValue value)
    {
        HardModeInput(value.isPressed);
    }

    public void HardModeInput(bool newDrawState)
    {
        hardMode = newDrawState;
    }

	private void OnApplicationFocus(bool hasFocus)
	{
		SetCursorState(cursorLocked);
	}		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}

}