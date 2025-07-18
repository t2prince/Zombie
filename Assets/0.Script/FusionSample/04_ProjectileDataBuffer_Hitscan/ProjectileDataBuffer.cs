using System;
using Fusion;
using Jamcat.Ingame.Character;
using Jamcat.Ingame.Equipment;
using UnityEngine;

namespace Projectiles.ProjectileDataBuffer_Hitscan
{
	// Using projectile data buffer is the most versatile solution that can scale very well with the project.
	// In this example we use hitscan projectiles and the added complexity over Example 03 is minimal.
	// Hitscan projectiles are very easy to implement and are the most efficient. You can trick the player
	// that the projectile is flying through the air by using dummy flying projectile.
	// However if kinematic projectiles are needed, the solution needs to be more complex, proceed to Example 05.
	public class ProjectileDataBuffer : WeaponBase
	{
		[SerializeField]
		private LayerMask _hitMask;
		[SerializeField]
		private float _hitImpulse = 50f;
		[SerializeField]
		private DummyFlyingProjectile _dummyProjectilePrefab;

		[SerializeField] private float _damage = 10f;
		[SerializeField] private float _maxRpcDelay = 0.1f; // 100ms 이상 지연시 fallback 사용 

		[Networked]
		private int _fireCount { get; set; }
		[Networked, Capacity(32)]
		private NetworkArray<ProjectileData> _projectileData { get; }

		private int _visibleFireCount;
		private float _lastFireTime;

		private Gun _gun;

		// WeaponBase INTERFACE

		public override void Fire()
		{
			var origin = FireTransform.position;
			var direction = FireTransform.forward;

			// 발사자 클라이언트에서만 처리
			if (Object.HasInputAuthority)
			{
				_lastFireTime = Time.time;
				Debug.Log($"Fire initiated by {Runner.LocalPlayer} at time: {_lastFireTime}");
				
				// 로컬에서 즉시 예측 처리 (지연 없음)
				ProcessFireImmediate(origin, direction);
				
				// 모든 클라이언트에 발사 정보 전송
				Rpc_FireProjectile(origin, direction, _lastFireTime);
			}
		}
		
		private void ProcessFireImmediate(Vector3 origin, Vector3 direction)
		{
			// 로컬 클라이언트에서 즉시 레이캐스트 및 처리
			if (Physics.Raycast(origin, direction, out RaycastHit hit, 100f, _hitMask))
			{
				var hitPosition = hit.point;

				// 즉시 로컬 물리 처리 (예측)
				if (hit.collider != null && hit.collider.attachedRigidbody != null)
				{
					hit.collider.attachedRigidbody.AddForce(direction * _hitImpulse, ForceMode.Impulse);
				}

				// 즉시 공격 처리
				var target = hit.collider.GetComponent<BaseCharacter>();
				if (target != null)
				{
					_gun.Attack(target);
				}

				// 즉시 시각적 효과 처리
				ProcessFireEffect(origin, direction, hitPosition);
				
				// 데이터 버퍼에 저장
				_projectileData.Set(_fireCount % _projectileData.Length, new ProjectileData()
				{
					HitPosition = hitPosition,
				});
			}
			else
			{
				// 빗나간 경우에도 즉시 시각적 효과
				ProcessFireEffect(origin, direction, Vector3.zero);
			}
			
			// 발사자의 fireCount 증가
			_fireCount++;
		}
		
		[Rpc(RpcSources.InputAuthority, RpcTargets.All, InvokeLocal = false, Channel = RpcChannel.Unreliable)]
		public void Rpc_FireProjectile(Vector3 origin, Vector3 direction, float fireTime)
		{
			float delay = Time.time - fireTime;
			Debug.Log($"RPC received on client: {Runner.LocalPlayer}, HasAuthority: {Object.HasInputAuthority}, Delay: {delay:F3}s");
			
			// 발사자가 아닌 클라이언트들만 처리 (발사자는 이미 즉시 처리함)
			if (!Object.HasInputAuthority && Physics.Raycast(origin, direction, out RaycastHit hit, 100f, _hitMask))
			{
				var hitPosition = hit.point;

				// 충돌한 오브젝트가 있다면 물리 처리
				if (hit.collider != null && hit.collider.attachedRigidbody != null)
				{
					hit.collider.attachedRigidbody.AddForce(direction * _hitImpulse, ForceMode.Impulse);
				}

				// 시각적 효과 처리
				ProcessFireEffect(origin, direction, hitPosition);

				// 데이터 버퍼에 저장
				_projectileData.Set(_fireCount % _projectileData.Length, new ProjectileData()
				{
					HitPosition = hitPosition,
				});
			}
			else if (!Object.HasInputAuthority)
			{
				// 빗나간 경우에도 시각적 효과 처리 (발사자가 아닌 경우만)
				ProcessFireEffect(origin, direction, Vector3.zero);
			}

			// 모든 클라이언트에서 fireCount 증가
			if (!Object.HasInputAuthority)
			{
				_fireCount++;
			}
		}
		
		private void ProcessFireEffect(Vector3 origin, Vector3 direction, Vector3 hitPosition)
		{
			// 즉시 발사 효과 재생
			PlayFireEffect();
			
			// 즉시 더미 프로젝타일 생성
			if (_dummyProjectilePrefab != null)
			{
				var dummyProjectile = Instantiate(_dummyProjectilePrefab, origin, Quaternion.LookRotation(direction));
				dummyProjectile.SetHitPosition(hitPosition);

				if (Runner.Config.PeerMode == NetworkProjectConfig.PeerModes.Multiple)
				{
					Runner.MoveToRunnerScene(dummyProjectile);
					Runner.AddVisibilityNodes(dummyProjectile.gameObject);
				}
			}
		}
		
		[Rpc(RpcSources.InputAuthority, RpcTargets.All)]
		public void Rpc_ReceiveHit(NetworkId targetId, Vector3 force)
		{
			if (Runner.TryFindObject(targetId, out var target))
			{
				if (target.TryGetComponent(out Rigidbody rb))
				{
					rb.linearVelocity = Vector3.zero; // 초기화 (중복 방지)
					rb.AddForce(force, ForceMode.Impulse);
				}
			}
		}

		public override void Spawned()
		{
			_visibleFireCount = _fireCount;
		}

		public override void Render()
		{
			// RPC로 즉시 처리되므로 Render에서는 동기화만 확인
			_visibleFireCount = _fireCount;
		}

		// DATA STRUCTURES

		private struct ProjectileData : INetworkStruct
		{
			public Vector3 HitPosition;

			// ProjectileData struct can be expanded with additional data
			// like ImpactNormal, ImpactType to better reconstruct projectile effects on all clients
			// See ProjectileManager in the Projectiles Advanced.
			// It is however best practice to keep the ProjectileData struct as small as possible.
		}

		private void Awake()
		{
			_gun = GetComponent<Gun>();
		}
	}
}
