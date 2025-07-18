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

		[Networked]
		private int _fireCount { get; set; }
		[Networked, Capacity(32)]
		private NetworkArray<ProjectileData> _projectileData { get; }

		private int _visibleFireCount;

		private Gun _gun;

		// WeaponBase INTERFACE

		public override void Fire()
		{
			var origin = FireTransform.position;
			var direction = FireTransform.forward;

			// 발사자 클라이언트에서만 처리
			if (Object.HasInputAuthority)
			{
				if (Physics.Raycast(origin, direction, out RaycastHit hit, 100f, _hitMask))
				{
					var hitPosition = hit.point;

					var targetNetObj = hit.collider.GetComponent<NetworkObject>();
					if (targetNetObj != null)
					{
						var force = direction * _hitImpulse;

						// 상대가 직접 AddForce 하도록 요청
						Rpc_ReceiveHit(targetNetObj.Id, force);

						// 공격 처리 등 기타 로직
						var target = hit.collider.GetComponent<BaseCharacter>();
						if (target != null)
						{
							_gun.Attack(target);
						}
					}

					// 🔹 탄착 지점 동기화용 데이터 저장
					_projectileData.Set(_fireCount % _projectileData.Length, new ProjectileData()
					{
						HitPosition = hitPosition,
					});

					_fireCount++;
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
			if (_visibleFireCount < _fireCount)
			{
				PlayFireEffect();
			}

			if (_dummyProjectilePrefab != null)
			{
				// As opposed to Example 03, all missing projectiles are instantiated here
				for (int i = _visibleFireCount; i < _fireCount; i++)
				{
					var data = _projectileData[i % _projectileData.Length];

					var dummyProjectile = Instantiate(_dummyProjectilePrefab, FireTransform.position, FireTransform.rotation);
					dummyProjectile.SetHitPosition(data.HitPosition);

					// When using multipeer, move to correct scene and disable renderers for other clients. Can be omitted otherwise.
					if (Runner.Config.PeerMode == NetworkProjectConfig.PeerModes.Multiple)
					{
						Runner.MoveToRunnerScene(dummyProjectile);
						Runner.AddVisibilityNodes(dummyProjectile.gameObject);
					}
				}
			}

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
			
		}
	}
}
