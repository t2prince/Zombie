using Fusion;
using UnityEngine;

namespace Projectiles
{
	public enum EInputButtons
	{
		Fire = 0,
	}

	public struct GameplayInput : INetworkInput
	{
		public Vector2        MoveDirection;
		public Vector2        LookRotationDelta;
		public NetworkButtons Buttons;
	}

	public class PlayerInput : NetworkBehaviour, IBeforeUpdate
	{
		// PRIVATE MEMBERS

		[SerializeField]
		private DebugInputControl _inputControl;

		// Accumulated input holds combined input for all render frames from last fixed update
		private GameplayInput _accumulatedInput;

		private bool _resetCachedInput;

		// NetworkBehaviour INTERFACE

		public override void Spawned()
		{
			// Reset to default state (in case this object was cached)
			_accumulatedInput = default;

			if (Runner.LocalPlayer == Object.InputAuthority)
			{
				var events = Runner.GetComponent<NetworkEvents>();

				events.OnInput.RemoveListener(OnInput);
				events.OnInput.AddListener(OnInput);

				_inputControl.RequestCursorLock();
			}
		}

		public override void Despawned(NetworkRunner runner, bool hasState)
		{
			var events = Runner.GetComponent<NetworkEvents>();
			events.OnInput.RemoveListener(OnInput);

			if (Runner.LocalPlayer == Object.InputAuthority)
			{
				_inputControl.RequestCursorRelease();
			}
		}

		// IBeforeUpdate INTERFACE

		void IBeforeUpdate.BeforeUpdate()
		{
			if (HasInputAuthority == false)
				return;

			if (_resetCachedInput == true)
			{
				_accumulatedInput = default;
				_resetCachedInput = false;
			}

			// Input is tracked only if the runner should provide input (important in multipeer mode)
			if (Runner.ProvideInput == false || _inputControl.IsLocked == false)
				return;

			ProcessKeyboardInput();
		}

		// PRIVATE METHODS

		private void OnInput(NetworkRunner runner, NetworkInput networkInput)
		{
			// Input is polled for single fixed update, but at this time we don't know how many times in a row OnInput() will be executed.
			// This is the reason for having a reset flag instead of resetting input immediately, otherwise we could lose input for next
			// fixed updates (for example move direction).
			_resetCachedInput = true;

			networkInput.Set(_accumulatedInput);

			// Input consumed by OnInput() call will be read in OnFixedUpdate() and immediately propagated to KCC.
			// Here we should reset RELATIVE input properties so they are not applied twice (fixed + render update)
			_accumulatedInput.LookRotationDelta = default;
		}

		private void ProcessKeyboardInput()
		{
			if (Input.GetKey(KeyCode.Space) == true || Input.GetMouseButton(0) == true)
			{
				_accumulatedInput.Buttons.Set(EInputButtons.Fire, true);
			}

			Vector2 moveDirection = default;

			if (Input.GetKey(KeyCode.W) == true) { moveDirection += Vector2.up;    }
			if (Input.GetKey(KeyCode.S) == true) { moveDirection += Vector2.down;  }
			if (Input.GetKey(KeyCode.A) == true) { moveDirection += Vector2.left;  }
			if (Input.GetKey(KeyCode.D) == true) { moveDirection += Vector2.right; }

			_accumulatedInput.MoveDirection = moveDirection.normalized;

			var lookDelta = new Vector2(-Input.GetAxisRaw("Mouse Y"), Input.GetAxisRaw("Mouse X"));
			_accumulatedInput.LookRotationDelta += lookDelta;
		}
	}
}
