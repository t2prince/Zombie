using Jamcat.Ingame.Character;

namespace Jamcat.Ingame.Item
{
    public class MaterialPile : BaseCharacter
    {
        public override void TakeDamage(BaseCharacter attacher, float damage, float knockBackPower = 0.0f)
        {
            hp -= damage;
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