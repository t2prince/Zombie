using System.Collections;
using Fusion;
using Jamcat.Ingame.Character;
using Jamcat.Ingame.Equipment;
using UnityEngine;

namespace Projectiles.ProjectileDataBuffer_Hitscan
{
	public class ProjectileDataBuffer : WeaponBase
	{
		[Header("Weapon Settings")]
		[SerializeField] private LayerMask hitMask;
		[SerializeField] private float hitImpulse = 50f;
		[SerializeField] private DummyFlyingProjectile dummyProjectilePrefab;
		[SerializeField] private float damage = 10f;
		
		[Header("Network Sync")]
		[SerializeField] private float pingInterval = 1f;

		[Networked] private int fireCount { get; set; }
		[Networked, Capacity(32)] private NetworkArray<ProjectileData> projectileData { get; }

		private int visibleFireCount;
		private float averageRpcDelay = 0.05f;
		private float lastPingTime;
		private int delayMeasurements = 0;
		private Gun gun;

		#region Unity/Fusion Lifecycle

		private void Awake()
		{
			gun = GetComponent<Gun>();
		}

		public override void Spawned()
		{
			visibleFireCount = fireCount;
			lastPingTime = Time.time;
		}

		public override void Render()
		{
			visibleFireCount = fireCount;
			
			// 주기적 핑 측정
			if (Time.time - lastPingTime >= pingInterval && Object.HasInputAuthority)
			{
				lastPingTime = Time.time;
				Rpc_PingMeasurement(Time.time);
			}
		}

		#endregion

		#region Weapon Interface

		public override void Fire()
		{
			if (!Object.HasInputAuthority) return;

			var origin = FireTransform.position;
			var direction = FireTransform.forward;			
			ProcessFireImmediate(origin, direction);
			Rpc_FireProjectile(origin, direction);
		}

		#endregion

		#region Fire Processing

		private void ProcessFireImmediate(Vector3 origin, Vector3 direction)
		{
			if (Physics.Raycast(origin, direction, out RaycastHit hit, 100f, hitMask))
			{
				var hitPosition = hit.point;
				
				// 데미지는 항상 즉시 적용
				var target = hit.collider.GetComponent<BaseCharacter>();
				if (target != null)
				{
					gun.Attack(target);
					Debug.Log("Attack");
				}

				// 타겟 소유권에 따른 처리
				var targetNetObj = hit.collider.GetComponent<NetworkObject>();
				bool isMyTarget = targetNetObj != null && targetNetObj.HasInputAuthority;
				
				if (isMyTarget)
				{
					// 내 타겟: 모든 처리를 지연 (다른 클라이언트와 동기화)
					Util.Coroutine.DelayedAction(() => 
						ApplyPhysicsAndEffects(origin, direction, hitPosition, hit.collider), 
						averageRpcDelay);
				}
				else
				{
					// 남의 타겟: RPC 지연만큼 지연
					StartCoroutine(DelayedEffectsCoroutine(origin, direction, hitPosition, hit.collider, averageRpcDelay));
				}

				SaveProjectileData(hitPosition);
			}
			else
			{
				ProcessFireEffect(origin, direction, Vector3.zero, null);
			}
			
			fireCount++;
		}

		private void ApplyPhysicsAndEffects(Vector3 origin, Vector3 direction, Vector3 hitPosition, Collider hitCollider)
		{
			ApplyPhysics(direction, hitCollider);
			ProcessFireEffect(origin, direction, hitPosition, hitCollider);
		}

		private void ApplyPhysics(Vector3 direction, Collider hitCollider)
		{
			if (hitCollider != null && hitCollider.attachedRigidbody != null)
			{
				hitCollider.attachedRigidbody.AddForce(direction * hitImpulse, ForceMode.Impulse);
			}
		}

		private void ProcessFireEffect(Vector3 origin, Vector3 direction, Vector3 hitPosition, Collider hitTarget = null)
		{
			PlayFireEffect();
			
			if (dummyProjectilePrefab != null)
			{
				var dummyProjectile = Instantiate(dummyProjectilePrefab, origin, Quaternion.LookRotation(direction));
				dummyProjectile.SetHitPosition(hitPosition);

				// 맞은 타겟이 있으면 자식으로 붙임
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

		private void SaveProjectileData(Vector3 hitPosition)
		{
			projectileData.Set(fireCount % projectileData.Length, new ProjectileData
			{
				HitPosition = hitPosition
			});
		}

		#endregion

		#region RPC Handlers

		[Rpc(RpcSources.InputAuthority, RpcTargets.All, InvokeLocal = false, Channel = RpcChannel.Unreliable)]
		public void Rpc_FireProjectile(Vector3 origin, Vector3 direction)
		{
			if (Object.HasInputAuthority) return;

			if (Physics.Raycast(origin, direction, out RaycastHit hit, 100f, hitMask))
			{
				var hitPosition = hit.point;
				var targetNetObj = hit.collider.GetComponent<NetworkObject>();
				bool isMyTarget = targetNetObj != null && targetNetObj.HasInputAuthority;
				
				if (isMyTarget)
				{
					// 내 타겟만 처리 (즉시 반응)
					ApplyPhysicsAndEffects(origin, direction, hitPosition, hit.collider);
				}
				else
				{
					// 남의 타겟: 이펙트만 지연 표시 (동기화를 위해)
					StartCoroutine(DelayedEffectsOnlyCoroutine(origin, direction, hitPosition, hit.collider, averageRpcDelay));
				}

				SaveProjectileData(hitPosition);
			}
			else
			{
				// 빗나간 경우: 이펙트만 표시
				ProcessFireEffect(origin, direction, Vector3.zero, null);
			}
			
			fireCount++;
		}

		[Rpc(RpcSources.InputAuthority, RpcTargets.All, InvokeLocal = false, Channel = RpcChannel.Unreliable)]
		public void Rpc_PingMeasurement(float sendTime)
		{
			float pingTime = Time.time - sendTime;
			UpdateAverageDelay(pingTime);
		}

		#endregion

		#region Delay Management

		private void UpdateAverageDelay(float delay)
		{
			delayMeasurements++;
			float weight = Mathf.Min(1f / delayMeasurements, 0.1f);
			averageRpcDelay = averageRpcDelay * (1f - weight) + delay * weight;
		}

		#endregion

		#region Coroutines

		private IEnumerator DelayedEffectsCoroutine(Vector3 origin, Vector3 direction, Vector3 hitPosition, Collider hitCollider, float delay)
		{
			yield return Util.Coroutine.WaitForSeconds(delay);
			ApplyPhysicsAndEffects(origin, direction, hitPosition, hitCollider);
		}

		private IEnumerator DelayedEffectsOnlyCoroutine(Vector3 origin, Vector3 direction, Vector3 hitPosition, Collider hitTarget, float delay)
		{
			yield return Util.Coroutine.WaitForSeconds(delay);
			ProcessFireEffect(origin, direction, hitPosition, hitTarget);
		}

		#endregion

		#region Data Structures

		private struct ProjectileData : INetworkStruct
		{
			public Vector3 HitPosition;
		}

		#endregion
	}
}