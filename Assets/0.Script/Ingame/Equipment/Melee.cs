using Jamcat.Ingame.Character;
using Jamcat.Managers.Map;
using UnityEngine;

namespace Jamcat.Ingame.Equipment
{
    public class Melee : Weapon
    {
        [SerializeField] private Animator _animator;
        private BaseCharacter _owner;
        
        public void Init(BaseCharacter owner, HandController controller)
        {
            _owner = owner;
            controller.OnRightPrimaryButtonPressed += Show;
            controller.OnRightPrimaryButtonReleased += Hide;
            
            transform.SetParent(controller.transform);
            
            Hide();
        }

        public void Show()
        {
            //TODO: Animation으로 틀어서 장착해야한다
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            //TODO: Animation으로 틀어서 빼야 한다
            gameObject.SetActive(false);
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