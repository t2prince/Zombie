using Jamcat.Ingame.Character;
using UnityEngine;

namespace Jamcat.Ingame.Item
{
    public class MaterialPile : BaseCharacter
    {
        [SerializeField] private float defence;
        [SerializeField] private GameObject material;
        public override void TakeDamage(BaseCharacter attacher, float damage, float knockBackPower = 0.0f)
        {
            var finalDamage = Mathf.Max(damage - defence, 0);
            hp -= finalDamage;
            if (hp <= 0)
            {
                Die();
            }
        }

        protected override void Die()
        {
            Destroy(gameObject);
            SpawnMaterial();
        }

        private void SpawnMaterial()
        {
            
        }
    }
}