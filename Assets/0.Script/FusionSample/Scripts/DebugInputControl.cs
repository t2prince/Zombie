using Fusion;
using UnityEngine;

namespace Projectiles
{
	public class DebugInputControl : NetworkBehaviour
	{
		// PUBLIC MEMBERS

		public bool IsLocked => Cursor.lockState == CursorLockMode.Locked;

		// PRIVATE MEMBERS

		private static int _lastSingleInputChange;
		private static int _cursorLockRequests;

		// PUBLIC METHODS

		public void RequestCursorLock()
		{
			// Static requests count is used for multi-peer setup
			_cursorLockRequests++;

			if (_cursorLockRequests == 1)
			{
				// First lock request, let's lock
				SetLockedState(true);
			}
		}

		public void RequestCursorRelease()
		{
			_cursorLockRequests--;

			Assert.Check(_cursorLockRequests >= 0, "Cursor lock requests are negative, this should not happen");

			if (_cursorLockRequests == 0)
			{
				SetLockedState(false);
			}
		}

		// NetworkBehaviour INTERFACE

		public override void Render()
		{
			// Only one single input change per frame is possible (important for multi-peer multi-input game)
			if (_lastSingleInputChange == Time.frameCount)
				return;

			if (Input.GetKeyDown(KeyCode.Return) == true || Input.GetKeyDown(KeyCode.KeypadEnter) == true || Input.GetKeyDown(KeyCode.Escape) == true)
			{
				SetLockedState(Cursor.lockState != CursorLockMode.Locked);
				_lastSingleInputChange = Time.frameCount;
			}

			if (Input.GetKeyDown(KeyCode.Keypad0) == true || Input.GetKeyDown(KeyCode.U) == true)
			{
				SetActiveRunner(-1);
			}
			else if (Input.GetKeyDown(KeyCode.Keypad1) == true || Input.GetKeyDown(KeyCode.I) == true)
			{
				SetActiveRunner(0);
			}
			else if (Input.GetKeyDown(KeyCode.Keypad2) == true || Input.GetKeyDown(KeyCode.O) == true)
			{
				SetActiveRunner(1);
			}
			else if (Input.GetKeyDown(KeyCode.Keypad3) == true || Input.GetKeyDown(KeyCode.P) == true)
			{
				SetActiveRunner(2);
			}
		}

		// PRIVATE METHODS

		private void SetLockedState(bool value)
		{
			Cursor.lockState = value == true ? CursorLockMode.Locked : CursorLockMode.None;
			Cursor.visible = !value;

			//Debug.Log($"Cursor lock state {Cursor.lockState}, visibility {Cursor.visible}");
		}

		private void SetActiveRunner(int index)
		{
			var enumerator = NetworkRunner.GetInstancesEnumerator();

			int currentIndex = -1;
			while (enumerator.MoveNext() == true)
			{
				var runner = enumerator.Current;

				// Skip temporary runner
				if (runner.LocalPlayer.IsRealPlayer == false)
					continue;

				currentIndex++;

				runner.SetVisible(index < 0 || currentIndex == index);
				runner.ProvideInput = index < 0 || currentIndex == index;
			}
		}
	}
}
