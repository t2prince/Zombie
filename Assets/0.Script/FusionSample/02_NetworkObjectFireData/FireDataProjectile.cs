using Fusion;
using Jamcat.Ingame.Character;
using UnityEngine;

namespace Projectiles.NetworkObjectFireData
{
	// FireDataProjectile is still a NetworkObject spawned in the scene, but it does not use NetworkRigidbody
	// or NetworkTransform to constantly synchronize position and rotation to all peers. Instead only fire data
	// (fire position, fire rotation) is saved on the start and it's position for specific time is calculated based
	// on this data separately on all peers.
	// This approach is more suitable than to use NetworkTransform but there is still overhead when spawning new
	// NetworkObject for each projectile. For hitscan projectiles, solutions from Example 03 and 04 are much more
	// efficient and easier. For kinematic projectiles use this solution only when the projectile needs to live for
	// a long time or simplicity is a key. Otherwise kinematic projectile data buffer (Example 05) is a better option.
	public class FireDataProjectile : NetworkBehaviour
	{
		// PRIVATE MEMBERS
		
		[SerializeField]
		private float _damage = 10f;
		
		private BaseCharacter _owner;

		[SerializeField]
		private float _speed = 50f;
		
		[SerializeField]
		private float _lifeTime = 4f;
		[SerializeField]
		private LayerMask _hitMask;
		[SerializeField]
		private float _hitImpulse = 50f;
		[SerializeField]
		private GameObject _hitEffect;
		[SerializeField]
		private float _lifeTimeAfterHit = 2f;
		[SerializeField]
		private GameObject _visualsRoot;
		[SerializeField]
		private TrailRenderer _trail;

		[Networked]
		private int _fireTick { get; set; }
		[Networked]
		private Vector3 _firePosition { get; set; }
		[Networked]
		private Vector3 _fireVelocity { get; set; }
		[Networked]
		private NetworkBool _isDestroyed { get; set; }
		[Networked]
		private TickTimer _lifeCooldown { get; set; }
		[Networked]
		private Vector3 _hitPosition { get; set; }

		private bool _isInitializedRender;
		private bool _isDestroyedRender;

		// PUBLIC METHODS

		public void Fire(Vector3 position, Vector3 direction)
		{
			// Save fire data
			_fireTick = Runner.Tick;
			_firePosition = position;
			_fireVelocity = direction * _speed;

			if (_lifeTime > 0f)
			{
				_lifeCooldown = TickTimer.CreateFromSeconds(Runner, _lifeTime);
			}
			
			// Fire 시점에서 디버그 정보 출력
			Debug.Log($"[FireDataProjectile] Fire called:");
			Debug.Log($"  Fire Position: {position}");
			Debug.Log($"  Fire Direction: {direction}");
			Debug.Log($"  Fire Velocity: {_fireVelocity}");
			Debug.Log($"  Fire Tick: {_fireTick}");
			Debug.Log($"  HitMask configured: {_hitMask.value}");
			Debug.Log($"  HitMask layers: {GetLayerNames(_hitMask)}");
			Debug.Log($"  Object.InputAuthority at Fire: {Object.InputAuthority}");
			Debug.Log($"  Runner.LocalPlayer: {Runner.LocalPlayer}");
		}
		
		private string GetLayerNames(LayerMask mask)
		{
			var layers = new System.Collections.Generic.List<string>();
			for (int i = 0; i < 32; i++)
			{
				if ((mask.value & (1 << i)) != 0)
				{
					layers.Add($"{i}({LayerMask.LayerToName(i)})");
				}
			}
			return layers.Count > 0 ? string.Join(", ", layers) : "None";
		}

		// NetworkBehaviour INTERFACE

		public override void FixedUpdateNetwork()
		{
			if (_lifeCooldown.IsRunning == true && _lifeCooldown.Expired(Runner) == true)
			{
				Runner.Despawn(Object);
				return;
			}

			if (_isDestroyed == true)
				return;

			// Previous and next position is calculated based on the initial parameters.
			// There is no point in actually moving the object in FUN.
			var previousPosition = GetMovePosition(Runner.Tick - 1);
			var nextPosition = GetMovePosition(Runner.Tick);

			var direction = nextPosition - previousPosition;

			var distance = direction.magnitude;
			
			if (distance <= 0f)
			{
				Debug.LogWarning("[FireDataProjectile] Distance is zero or negative, skipping raycast");
				return;
			}
			
			direction /= distance; // Normalize
			var hit = new LagCompensatedHit();
			var hitOptions = HitOptions.IncludePhysX | HitOptions.IgnoreInputAuthority;
			
			
			bool lagCompHit = false;
			
			// InputAuthority가 없으면 LagCompensation.Raycast 사용 불가
			if (Object.HasInputAuthority)
			{
				lagCompHit = Runner.LagCompensation.Raycast(previousPosition, direction, distance,
					    Object.InputAuthority, out hit, _hitMask, hitOptions);
				Debug.Log($"  LagCompensation.Raycast result: {lagCompHit}");
			}
			else
			{
				Debug.LogWarning("[FireDataProjectile] No InputAuthority - cannot use LagCompensation.Raycast");
			}
			
			// InputAuthority가 없거나 LagCompensation이 실패하면 Physics.Raycast 사용
			if (!lagCompHit)
			{
				RaycastHit physicsHitInfo;
				if (Physics.Raycast(previousPosition, direction, out physicsHitInfo, distance, _hitMask))
				{
					Debug.Log("[FireDataProjectile] Using Physics.Raycast");
					
					// LagCompensatedHit 형태로 변환
					hit = new LagCompensatedHit();
					hit.Point = physicsHitInfo.point;
					hit.Normal = physicsHitInfo.normal;
					hit.Collider = physicsHitInfo.collider;
					hit.GameObject = physicsHitInfo.collider.gameObject;
					lagCompHit = true;
				}
			}
			
			if (lagCompHit)
			{
				_isDestroyed = true;
				_lifeCooldown = TickTimer.CreateFromSeconds(Runner, _lifeTimeAfterHit);

				// Save hit position so hit effects are at correct position on proxies
				_hitPosition = hit.Point;

				if (hit.Collider != null && hit.Collider.attachedRigidbody != null)
				{
					hit.Collider.attachedRigidbody.AddForce(direction * _hitImpulse, ForceMode.Impulse);
				}

				var target = hit.GameObject.GetComponent<BaseCharacter>();
				if (target == null) return;
				
				target.TakeDamage(_owner, _damage);
			}
		}

		public override void Render()
		{
			InitializeRender();

			if (_isDestroyed == true && _isDestroyedRender == false)
			{
				_isDestroyedRender = true;
				ShowDestroyEffect();
			}

			if (_isDestroyed == true)
				return;

			// For proxies we move projectiles in remote time frame, for input/state authority we use local time frame
			float renderTime = Object.IsProxy == true ? Runner.RemoteRenderTime : Runner.LocalRenderTime;
			float floatTick = renderTime / Runner.DeltaTime;

			// It is enough to move the object only in Render.
			// In FUN previous and next position can be calculated from initial parameters.
			transform.position = GetMovePosition(floatTick);
		}

		// MONOBEHAVIOUR

		protected void Awake()
		{
			if (_hitEffect != null)
			{
				_hitEffect.SetActive(false);
			}

			if (_trail != null)
			{
				_trail.gameObject.SetActive(false);
			}
		}

		// PRIVATE METHODS

		private void InitializeRender()
		{
			if (_isInitializedRender == true)
				return;

			// Usually we would do following functionality in Spawned
			// but since this projectile can be used in NetworkObjectBuffer
			// Spawned is called too soon (when adding object to buffer)
			// so we do it in first Render instead

			// Set initial position and rotation on proxies
			if (IsProxy == true)
			{
				transform.position = _firePosition;
				transform.rotation = Quaternion.LookRotation(_fireVelocity);
			}

			if (_trail != null)
			{
				_trail.gameObject.SetActive(true);
				_trail.Clear();
			}

			_isInitializedRender = true;
		}

		private Vector3 GetMovePosition(float currentTick)
		{
			float time = (currentTick - _fireTick) * Runner.DeltaTime;

			if (time <= 0f)
				return _firePosition;

			return _firePosition + _fireVelocity * time;
		}

		private void ShowDestroyEffect()
		{
			transform.position = _hitPosition;

			if (_hitEffect != null)
			{
				_hitEffect.SetActive(true);
			}

			// Hide projectile visual
			_visualsRoot.SetActive(false);
		}
	}
}
