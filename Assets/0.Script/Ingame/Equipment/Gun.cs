using Jamcat.Ingame.Character;
using UnityEngine;

namespace Jamcat.Ingame.Equipment
{
    public class Gun : MonoBehaviour
    {
        [SerializeField] float bulletDamage;
        [SerializeField] float bulletSpeed = 20f;
        private Transform firePoint;
        private Transform leftController;
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
                bullet.Init(_owner,bulletSpeed,bulletDamage);
                
                return bullet;
            });
        }

        public void Init(BaseCharacter owner, HandController controller)
        {
            controller.OnLeftPrimaryButtonPressed += Fire;
        }
        
        private void Fire()
        {
            var bullet = _bulletPool.Pop();
            bullet.gameObject.SetActive(true);
            bullet.transform.position = firePoint.position;
            bullet.Fire(leftController.forward);
            bullet.onHit = () =>
            {
                bullet.gameObject.SetActive(false);
                _bulletPool.Push(bullet);
            };
        }
    }
}