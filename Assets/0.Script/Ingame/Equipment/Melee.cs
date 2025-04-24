using Jamcat.Ingame.Character;
using UnityEngine;

namespace Jamcat.Ingame.Equipment
{
    public class Melee : MonoBehaviour
    {
        [SerializeField] private float _damage;
        private BaseCharacter _owner;
        
        
        public void Init(BaseCharacter owner, HandController controller)
        {
               
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag.Equals(_owner.tag)) return;
            
            var target = other.GetComponentInParent<BaseCharacter>();
            
            if (target == null) return;
            target.TakeDamage(_owner, _damage);
        }
    }
}