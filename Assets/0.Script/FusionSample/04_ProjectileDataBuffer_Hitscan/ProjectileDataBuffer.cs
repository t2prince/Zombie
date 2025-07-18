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
		private float _averageRpcDelay = 0.05f; // 기본값 50ms
		private int _delayMeasurements = 0;

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

				// 즉시 공격 처리 (데미지는 지연 없이)
				var target = hit.collider.GetComponent<BaseCharacter>();
				if (target != null)
				{
					_gun.Attack(target);
				}

				// 타겟 오브젝트의 Authority 확인
				var targetNetworkObject = hit.collider.GetComponent<NetworkObject>();
				bool isMyTarget = targetNetworkObject != null && targetNetworkObject.HasInputAuthority;
				
				if (isMyTarget)
				{
					// 내 오브젝트면 즉시 처리
					ApplyPhysicsAndEffects(origin, direction, hitPosition, hit.collider);
				}
				else
				{
					// 남의 오브젝트면 RPC 지연만큼 지연 (다른 클라이언트와 동기화)
					StartCoroutine(DelayedEffectsCoroutine(origin, direction, hitPosition, hit.collider, _averageRpcDelay));
				}
				
				// 데이터 버퍼에 저장
				_projectileData.Set(_fireCount % _projectileData.Length, new ProjectileData()
				{
					HitPosition = hitPosition,
				});
			}
			else
			{
				// 빗나간 경우는 항상 즉시 처리
				ProcessFireEffect(origin, direction, Vector3.zero);
			}
			
			// 발사자의 fireCount 증가
			_fireCount++;
		}
		
		private System.Collections.IEnumerator DelayedEffectsCoroutine(Vector3 origin, Vector3 direction, Vector3 hitPosition, Collider hitCollider, float delay)
		{
			yield return new WaitForSeconds(delay);
			ApplyPhysicsAndEffects(origin, direction, hitPosition, hitCollider);
		}
		
		private System.Collections.IEnumerator DelayedEffectsOnlyCoroutine(Vector3 origin, Vector3 direction, Vector3 hitPosition, float delay)
		{
			yield return new WaitForSeconds(delay);
			ProcessFireEffect(origin, direction, hitPosition);
		}
		
		private void ApplyPhysicsAndEffects(Vector3 origin, Vector3 direction, Vector3 hitPosition, Collider hitCollider)
		{
			// 물리 처리
			if (hitCollider != null && hitCollider.attachedRigidbody != null)
			{
				hitCollider.attachedRigidbody.AddForce(direction * _hitImpulse, ForceMode.Impulse);
			}
			
			// 시각적 효과 처리
			ProcessFireEffect(origin, direction, hitPosition);
		}
		
		private void ApplyPhysicsOnly(Vector3 direction, Collider hitCollider)
		{
			// 물리 처리만 (이펙트 제외)
			if (hitCollider != null && hitCollider.attachedRigidbody != null)
			{
				hitCollider.attachedRigidbody.AddForce(direction * _hitImpulse, ForceMode.Impulse);
			}
		}
		
		private void UpdateAverageDelay(float delay)
		{
			_delayMeasurements++;
			// 이동 평균 계산 (최근 10개 측정값 기준)
			float weight = Mathf.Min(1f / _delayMeasurements, 0.1f);
			_averageRpcDelay = _averageRpcDelay * (1f - weight) + delay * weight;
		}
		
		[Rpc(RpcSources.InputAuthority, RpcTargets.All, InvokeLocal = false, Channel = RpcChannel.Unreliable)]
		public void Rpc_FireProjectile(Vector3 origin, Vector3 direction, float fireTime)
		{
			float delay = Time.time - fireTime;
			
			// RPC 지연 시간 측정 및 평균 계산
			UpdateAverageDelay(delay);
			
			Debug.Log($"RPC received on client: {Runner.LocalPlayer}, HasAuthority: {Object.HasInputAuthority}, Delay: {delay:F3}s, Avg: {_averageRpcDelay:F3}s");
			
			// 발사자가 아닌 클라이언트들만 처리 (발사자는 이미 처리함)
			if (!Object.HasInputAuthority)
			{
				if (Physics.Raycast(origin, direction, out RaycastHit hit, 100f, _hitMask))
				{
					var hitPosition = hit.point;
					
					// 타겟 오브젝트의 Authority 확인
					var targetNetworkObject = hit.collider.GetComponent<NetworkObject>();
					bool isMyTarget = targetNetworkObject != null && targetNetworkObject.HasInputAuthority;
					
					if (isMyTarget)
					{
						// 내 타겟이면 즉시 처리 (발사자보다 빠름)
						ApplyPhysicsAndEffects(origin, direction, hitPosition, hit.collider);
					}
					else
					{
						// 남의 타겟이면 물리는 즉시, 이펙트는 지연
						ApplyPhysicsOnly(direction, hit.collider);
						StartCoroutine(DelayedEffectsOnlyCoroutine(origin, direction, hitPosition, delay));
					}

					// 데이터 버퍼에 저장
					_projectileData.Set(_fireCount % _projectileData.Length, new ProjectileData()
					{
						HitPosition = hitPosition,
					});
				}
				else
				{
					// 빗나간 경우는 즉시 이펙트 처리
					ProcessFireEffect(origin, direction, Vector3.zero);
				}
				
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
