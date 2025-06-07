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
        private float _lastFireTime = 0f;
        
        [SerializeField] private GameObject bulletPrefab;
        private GameObjectPool<Bullet> _bulletPool;

        private void Awake()
        {
            _bulletPool = new GameObjectPool<Bullet>(8, () =>
            {
                var bulletObj = Instantiate(bulletPrefab, transform);
                bulletObj.SetActive(false);
                
                var bullet = bulletObj.GetComponent<Bullet>();
                return bullet;
            });
        }

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
            if (Time.time - _lastFireTime < interval) return;

            _lastFireTime = Time.time; // 마지막 발사
            
            var bullet = _bulletPool.Pop();
            bullet.gameObject.SetActive(true);
            bullet.Init(_owner,bulletSpeed,_damage);
            bullet.transform.position = _firePoint.position;
            bullet.Fire(_firePoint.forward);
            bullet.transform.SetParent(null);
            bullet.onHit = () =>
            {
                CollectBullet(bullet);
            };
            
            Util.Coroutine.DelayedAction(() => CollectBullet(bullet), 5.0f);
        }

        private void CollectBullet(Bullet bullet)
        {
            if(!bullet.isActiveAndEnabled) return;
            bullet.gameObject.SetActive(false);
            _bulletPool.Push(bullet);
        }
    }
}