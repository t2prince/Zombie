using Fusion;
using Jamcat.Ingame.Character;
using Projectiles.NetworkObjectFireData;
using UnityEngine;

namespace Jamcat.Ingame.Equipment
{
    public class Gun : Weapon
    {
        [SerializeField] float interval = 1f;
        [SerializeField] float bulletSpeed = 20f;
        [SerializeField] private Transform _firePoint;
        [SerializeField] private Weapon_NetworkObjectFireData _fireData;
        
        private BaseCharacter _owner;
        private float _lastFireTime;
        private NetworkRunner Runner => InGame.Instance.Runner;
        
        [SerializeField] private GameObject bulletPrefab;

        public void Init(BaseCharacter owner, HandController controller)
        {
            _owner = owner;
            controller.OnLeftTriggerPressed += OnTriggerPressed;
            transform.SetParent(controller.transform);
            _fireData = GetComponent<Weapon_NetworkObjectFireData>();
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