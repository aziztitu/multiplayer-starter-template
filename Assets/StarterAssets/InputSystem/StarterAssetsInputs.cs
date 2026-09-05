using Azeesoft.Multiplayer;
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

		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

		public bool CanProcessInput => PauseMenu.Instance == null || !PauseMenu.Instance.isPaused;

		void Start()
		{
            SetCursorState(cursorLocked);
		}

        private void Update()
        {
            if (!CanProcessInput)
			{
				ResetAllInputStates();
            }
        }

#if ENABLE_INPUT_SYSTEM
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
			if (!CanProcessInput) return;
            move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
        {
            if (!CanProcessInput) return;
            look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
        {
            if (!CanProcessInput) return;
            jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
        {
            if (!CanProcessInput) return;
            sprint = newSprintState;
		}

		public void ResetAllInputStates()
		{
			move = Vector2.zero;
            look = Vector2.zero;
            jump = false;
            sprint = false;
        }

		private void OnApplicationFocus(bool hasFocus)
		{
			//SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}
	
}