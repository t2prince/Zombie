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
        [SerializeField] private Weapon_ProjectileDataBuffer_Hitscan _fireData;
        
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
                Fire();
            }
        }
            
        private void Fire()
        {
            if (Time.time - _lastFireTime < interval) return;
            _lastFireTime = Time.time;

            _fireData.Fire();    
        }
    }
}