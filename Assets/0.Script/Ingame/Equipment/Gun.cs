using System;
using Fusion;
using Jamcat.Ingame.Character;
using Projectiles.NetworkObjectFireData;
using UnityEngine;
using UnityEngine.Serialization;

namespace Jamcat.Ingame.Equipment
{
    public class Gun : Weapon
    {
        [SerializeField] float interval = 1f;
        [SerializeField] float bulletSpeed = 20f;
        [SerializeField] private Weapon_NetworkObjectFireData _fireData;
        
        public Transform FirePoint { get; set; } 
        
        private BaseCharacter _owner;
        private float _lastFireTime;
        
        [SerializeField] private GameObject bulletPrefab;

        public void Init(BaseCharacter owner, HandController controller)
        {
            _owner = owner;
            controller.OnLeftTriggerPressed += OnTriggerPressed;
            
            transform.SetParent(controller.transform);
            
            _fireData = GetComponent<Weapon_NetworkObjectFireData>();
            
        }

        public void SetFirePoint(Transform firePoint)
        {
            _fireData.FireTransform = firePoint;
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