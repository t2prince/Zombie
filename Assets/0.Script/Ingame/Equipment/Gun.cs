using System;
using Fusion;
using Jamcat.Ingame.Character;
using Projectiles.NetworkObjectFireData;
using Projectiles.ProjectileDataBuffer_Hitscan;
using UnityEngine;
using UnityEngine.Serialization;

namespace Jamcat.Ingame.Equipment
{
    public class Gun : Weapon
    {
        [SerializeField] float interval = 1f;
        [SerializeField] float bulletSpeed = 20f;
        [SerializeField] private ProjectileDataBuffer _fireData;
        
        public Transform FirePoint { get; set; } 
        
        private BaseCharacter _owner;
        private float _lastFireTime;
        
        [SerializeField] private GameObject bulletPrefab;

        public void SetFirePoint(Transform firePoint)
        {
            _fireData.FireTransform = firePoint;
        }

        public void Init(BaseCharacter owner, HandController controller)
        {
            _owner = owner;
            controller.OnLeftTriggerPressed += OnTriggerPressed;
            
            transform.SetParent(controller.transform);
        }

        public void Attack(BaseCharacter target)
        {
            target.TakeDamage(_owner, _damage, _knockBackPower);
        }
        
        private void OnTriggerPressed(bool isPressed)
        {
            if (isPressed)
            {
                if (Object.HasInputAuthority)
                {
                    RPC_RequestFire();
                }
            }
        }
        
        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_RequestFire()
        {
            Fire();
        }
            
        private void Fire()
        {
            if (Time.time - _lastFireTime < interval) return;
            if (!Object.HasStateAuthority) return; // 호스트만 발사
            
            _lastFireTime = Time.time;
            
            // 호스트에서 총알 스폰
            SpawnBullet();
        }
        
        private void SpawnBullet()
        {
            if (bulletPrefab == null || _fireData.FireTransform == null) return;
            
            var bulletObj = Runner.Spawn(bulletPrefab, 
                _fireData.FireTransform.position, 
                _fireData.FireTransform.rotation, 
                Object.InputAuthority);
                
            var fireDataProjectile = bulletObj.GetComponent<FireDataProjectile>();
            if (fireDataProjectile != null)
            {
                fireDataProjectile.Fire(_fireData.FireTransform.position, _fireData.FireTransform.forward);
            }
        }
    }
}