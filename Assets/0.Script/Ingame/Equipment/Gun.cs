using Jamcat.Ingame.Character;
using UnityEngine;

namespace Jamcat.Ingame.Equipment
{
    public class Gun : Weapon
    {
        [SerializeField] float bulletSpeed = 20f;
        [SerializeField] private Transform _firePoint;
        private Transform _leftController;
        private BaseCharacter _owner;
        
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
            _leftController = controller.transform;
            controller.OnLeftPrimaryButtonPressed += Fire;
            transform.SetParent(controller.transform);
        }
        
        private void Fire()
        {
            var bullet = _bulletPool.Pop();
            bullet.gameObject.SetActive(true);
            bullet.Init(_owner,bulletSpeed,_damage);
            bullet.transform.position = _firePoint.position;
            bullet.Fire(_leftController.forward);
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