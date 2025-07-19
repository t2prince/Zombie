using Fusion;
using Jamcat.Ingame.Character;
using UnityEngine;

namespace Jamcat.Ingame.Item
{
    public class MaterialPile : BaseCharacter
    {
        [SerializeField] private float defence;
        [SerializeField] private GameObject material;
        [SerializeField] private int quantity;
        
        public override void TakeDamage(BaseCharacter attacher, float damage, float knockBackPower = 0.0f)
        {
            var finalDamage = Mathf.Max(damage - defence, 0);
            base.TakeDamage(attacher, finalDamage, knockBackPower);
        }

        protected override void Die()
        {
            SpawnMaterial();
            base.Die();
        }

        private void SpawnMaterial()
        {
            for (var i = 0; i < quantity; i++)
            {
                var offset = new Vector3(
                    Random.Range(-0.5f, 0.5f),
                    0.5f,
                    Random.Range(-0.5f, 0.5f)
                );
                
                var spawnPos = transform.position + offset;
                var randomRotation = Quaternion.Euler(
                    Random.Range(0f, 360f),
                    Random.Range(0f, 360f),
                    Random.Range(0f, 360f)
                );
                InGame.Instance.Runner.Spawn(material, spawnPos, randomRotation, onBeforeSpawned: (runner, obj) =>
                {
                    // NetworkRigidbody가 있는 경우 master client에게 authority 할당
                    var networkRigidbody = obj.GetComponent<Fusion.Addons.Physics.NetworkRigidbody3D>();
                    if (networkRigidbody != null)
                    {
                        obj.RequestStateAuthority();
                    }
                });
            }
        }
    }
}