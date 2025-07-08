using UnityEngine;
using Fusion;
using Fusion.Addons.SimpleKCC;

namespace Projectiles
{
	[DefaultExecutionOrder(-5)]
	public class Player : NetworkBehaviour
	{
		// PRIVATE MEMBERS

		[SerializeField]
		private float _moveSpeed = 10f;
		[SerializeField]
		private Transform _cameraPivot;
		[SerializeField]
		private MeshRenderer[] _thirdPersonRenderers;

		[Networked]
		private NetworkButtons _lastButtonsInput { get; set; }

		private SimpleKCC _kcc;
		private WeaponBase _weapon;
		private Transform _cameraTransform;

		// NetworkBehaviour INTERFACE

		public override void Spawned()
		{
			if (HasInputAuthority == true && _cameraTransform == null)
			{
				var scene = Runner.SimulationUnityScene.GetComponent<Scene>();
				_cameraTransform = scene.Camera.transform;
			}
		}

		public override void FixedUpdateNetwork()
		{
			var input = GetInput<GameplayInput>();
			ProcessInput(input.GetValueOrDefault());
		}

		public override void Render()
		{
			if (HasInputAuthority == false)
				return;

			// Update camera pitch
			Vector2 pitchRotation = _kcc.GetLookRotation(true, false);
			_cameraPivot.localRotation = Quaternion.Euler(pitchRotation);
		}

		// MONOBEHAVIOUR

		protected void Awake()
		{
			_weapon = GetComponentInChildren<WeaponBase>();
			_kcc = GetComponent<SimpleKCC>();
		}

		protected void LateUpdate()
		{
			if (HasInputAuthority == false)
				return;

			if (_cameraTransform != null)
			{
				_cameraTransform.position = _cameraPivot.position;
				_cameraTransform.rotation = _cameraPivot.rotation;
			}

			if (Object != null)
			{
				// Hide meshes that should be visible only for third person players
				for (int i = 0; i < _thirdPersonRenderers.Length; i++)
				{
					_thirdPersonRenderers[i].enabled = Runner.GetVisible() == true && Object.HasInputAuthority == false;
				}
			}
		}

		// PRIVATE METHODS

		private void ProcessInput(GameplayInput input)
		{
			_kcc.AddLookRotation(input.LookRotationDelta);

			// Calculate input direction based on recently updated look rotation (the change propagates internally also to KCC.TransformRotation)
			Vector3 inputDirection = _kcc.TransformRotation * new Vector3(input.MoveDirection.x, 0f, input.MoveDirection.y);
			_kcc.Move(inputDirection * _moveSpeed);

			// Update fire transform before fire
			Vector2 pitchRotation = _kcc.GetLookRotation(true, false);
			_cameraPivot.localRotation = Quaternion.Euler(pitchRotation);

			// KCC position is not updated yet at this point and positions of all
			// enemies are not updated as well so fire will be one tick off.
			// Check PlayerAgent in AdvancedSample where Fire is initiated precisely.
			if (input.Buttons.WasPressed(_lastButtonsInput, EInputButtons.Fire) == true)
			{
				_weapon.Fire();
			}

			_lastButtonsInput = input.Buttons;
		}
	}
}
