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
		private float _preCalculatedDelay = 0.05f; // 사전 계산된 딜레이
		private float _lastDelayUpdateTime;
		private const float DelayUpdateInterval = 1f; // 1초마다 딜레이 업데이트

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
				var fireResult = ProcessFireImmediate(origin, direction);
				
				// 모든 클라이언트에 raycast 결과 전송
				Rpc_FireProjectile(origin, direction, fireResult.hitPosition, fireResult.hitObjectId, fireResult.hasHit);
			}
		}
		
		private FireResult ProcessFireImmediate(Vector3 origin, Vector3 direction)
		{
			// 로컬 클라이언트에서 즉시 레이캐스트 및 처리
			if (Physics.Raycast(origin, direction, out RaycastHit hit, 100f, _hitMask))
			{
				var hitPosition = hit.point;
				var hitObjectId = default(NetworkId);
				
				// NetworkObject가 있는 경우 NetworkId 가져오기
				var targetNetworkObject = hit.collider.GetComponent<NetworkObject>();
				if (targetNetworkObject != null)
				{
					hitObjectId = targetNetworkObject.Id;
				}

				// 즉시 공격 처리 (데미지는 지연 없이)
				var target = hit.collider.GetComponent<BaseCharacter>();
				if (target != null)
				{
					_gun.Attack(target);
				}

				// 타겟 오브젝트의 Authority 확인
				bool isMyTarget = targetNetworkObject != null && targetNetworkObject.HasStateAuthority;
				
				if (isMyTarget)
				{
					// 내 오브젝트면 사전 계산된 딜레이 사용
					StartCoroutine(DelayedEffectsCoroutine(origin, direction, hitPosition, hit.collider, GetPredictedDelay()));
				}
				else
				{
					// 남의 오브젝트면 RPC 지연만큼 지연 (다른 클라이언트와 동기화)
					ApplyPhysicsAndEffects(origin, direction, hitPosition, hit.collider);
				}
				
				// 데이터 버퍼에 저장
				_projectileData.Set(_fireCount % _projectileData.Length, new ProjectileData()
				{
					HitPosition = hitPosition,
				});
				
				// 발사자의 fireCount 증가
				_fireCount++;
				
				return new FireResult
				{
					hitPosition = hitPosition,
					hitObjectId = hitObjectId,
					hasHit = true,
					hitCollider = hit.collider
				};
			}
			else
			{
				// 빗나간 경우는 항상 즉시 처리
				ProcessFireEffect(origin, direction, Vector3.zero);
				
				// 발사자의 fireCount 증가
				_fireCount++;
				
				return new FireResult
				{
					hitPosition = Vector3.zero,
					hitObjectId = default(NetworkId),
					hasHit = false,
					hitCollider = null
				};
			}
		}
		
		private System.Collections.IEnumerator DelayedEffectsCoroutine(Vector3 origin, Vector3 direction, Vector3 hitPosition, Collider hitCollider, float delay)
		{
			yield return new WaitForSeconds(delay);
			ApplyPhysicsAndEffects(origin, direction, hitPosition, hitCollider);
		}
		
		private System.Collections.IEnumerator DelayedEffectsOnlyCoroutine(Vector3 origin, Vector3 direction, Vector3 hitPosition, float delay, Collider hitCollider = null)
		{
			yield return new WaitForSeconds(delay);
			ProcessFireEffect(origin, direction, hitPosition, hitCollider);
		}
		
		private void ApplyPhysicsAndEffects(Vector3 origin, Vector3 direction, Vector3 hitPosition, Collider hitCollider)
		{
			// 물리 처리
			if (hitCollider != null && hitCollider.attachedRigidbody != null)
			{
				hitCollider.attachedRigidbody.AddForce(direction * _hitImpulse, ForceMode.Impulse);
			}
			
			// 시각적 효과 처리
			ProcessFireEffect(origin, direction, hitPosition, hitCollider);
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
			
			// 일정 주기마다 사전 계산된 딜레이 업데이트
			if (Time.time - _lastDelayUpdateTime > DelayUpdateInterval)
			{
				_preCalculatedDelay = _averageRpcDelay;
				_lastDelayUpdateTime = Time.time;
			}
		}
		
		private float GetPredictedDelay()
		{
			// 사전 계산된 딜레이 값을 반환 (RPC 내에서 계산하지 않음)
			return _preCalculatedDelay;
		}
		
		[Rpc(RpcSources.InputAuthority, RpcTargets.All, InvokeLocal = false, Channel = RpcChannel.Unreliable)]
		public void Rpc_FireProjectile(Vector3 origin, Vector3 direction, Vector3 hitPosition, NetworkId hitObjectId, bool hasHit)
		{
			// 발사자가 아닌 클라이언트들만 처리 (발사자는 이미 처리함)
			if (!Object.HasInputAuthority)
			{
				if (hasHit)
				{
					// 로컬에서 계산된 hit 결과 사용 (raycast 중복 제거)
					Collider hitCollider = null;
					
					// NetworkId로 타겟 오브젝트 찾기
					if (hitObjectId != default(NetworkId) && Runner.TryFindObject(hitObjectId, out var hitNetworkObject))
					{
						hitCollider = hitNetworkObject.GetComponent<Collider>();
						
						// 타겟 오브젝트의 Authority 확인
						bool isMyTarget = hitNetworkObject.HasInputAuthority;
						
						if (isMyTarget)
						{
							// 내 타겟이면 즉시 처리 (발사자보다 빠름)
							ApplyPhysicsAndEffects(origin, direction, hitPosition, hitCollider);
						}
						else
						{
							// 남의 타겟이면 물리는 즉시, 이펙트는 사전 계산된 딜레이 사용
							ApplyPhysicsOnly(direction, hitCollider);
							StartCoroutine(DelayedEffectsOnlyCoroutine(origin, direction, hitPosition, GetPredictedDelay(), hitCollider));
						}
					}
					else
					{
						// NetworkObject가 없는 정적 오브젝트 처리
						ProcessFireEffect(origin, direction, hitPosition);
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
		
		private void ProcessFireEffect(Vector3 origin, Vector3 direction, Vector3 hitPosition, Collider hitTarget = null)
		{
			// 즉시 발사 효과 재생
			PlayFireEffect();
			
			// 즉시 더미 프로젝타일 생성
			if (_dummyProjectilePrefab != null)
			{
				var dummyProjectile = Instantiate(_dummyProjectilePrefab, origin, Quaternion.LookRotation(direction));
				dummyProjectile.SetHitPosition(hitPosition);
				
				if (hitTarget != null && hitPosition != Vector3.zero)
				{
					// 탄환을 맞은 오브젝트의 자식으로 설정
					dummyProjectile.transform.SetParent(hitTarget.transform);
					// 월드 좌표를 로컬 좌표로 변환하여 위치 유지
					dummyProjectile.transform.position = hitPosition;
				}


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

		private struct FireResult
		{
			public Vector3 hitPosition;
			public NetworkId hitObjectId;
			public bool hasHit;
			public Collider hitCollider;
		}

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
