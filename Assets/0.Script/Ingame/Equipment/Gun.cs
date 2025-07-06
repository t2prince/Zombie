using Fusion;
using Jamcat.Ingame.Character;
using UnityEngine;

namespace Jamcat.Ingame.Equipment
{
    public class Gun : Weapon
    {
        [SerializeField] float interval = 1f;
        [SerializeField] float bulletSpeed = 20f;
        [SerializeField] private Transform _firePoint;
        private BaseCharacter _owner;
        private float _lastFireTime;
        private NetworkRunner Runner => InGame.Instance.Runner;
        
        [SerializeField] private GameObject bulletPrefab;

        public void Init(BaseCharacter owner, HandController controller)
        {
            _owner = owner;
            controller.OnLeftTriggerPressed += OnTriggerPressed;
            transform.SetParent(controller.transform);
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
            //if (!Object.HasStateAuthority) return;
            if (Time.time - _lastFireTime < interval) return;
            _lastFireTime = Time.time;

            var spawnPos = _firePoint.position;
            var direction = transform.forward;

            var bullet = Runner.Spawn(bulletPrefab, spawnPos, Quaternion.LookRotation(direction)).GetComponent<Bullet>();
            bullet.GetComponent<NetworkTransform>().Teleport(spawnPos);
            bullet.Init(_owner, bulletSpeed, _damage);
            bullet.Fire(direction); 
        }
    }
}