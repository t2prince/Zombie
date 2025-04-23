using Jamcat.Ingame.Character;
using UnityEngine;

namespace Jamcat.Ingame.Equipment
{
    public class Gun : MonoBehaviour
    {
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
                bullet.Init(_owner);
                
                return bullet;
            });
        }

        public void Init(BaseCharacter owner, HandController controller)
        {
            
        }
        
        public void Fire()
        {
            var bullet = _bulletPool.Pop();
        }
    }
}