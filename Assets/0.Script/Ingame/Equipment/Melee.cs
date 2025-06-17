using System;
using Jamcat.Ingame.Character;
using UnityEngine;

namespace Jamcat.Ingame.Equipment
{
    public class Melee : Weapon
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private AudioSource _shownSound;
        private BaseCharacter _owner;
        
        public void Init(BaseCharacter owner, HandController controller)
        {
            _owner = owner;
            controller.OnRightTriggerPressed += OnTriggerPressed;
            
            transform.SetParent(controller.transform);
            _shownSound = GetComponent<AudioSource>();
            
            Hide();
        }

        private void OnTriggerPressed(bool isPressed)
        {
            if (isPressed)
            {
                Show();
            }
            else
            {
                Hide();
            }
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
            target.TakeDamage(_owner, _damage, _knockBackPower);
        }

        private void OnEnable()
        {
            _shownSound.Play();
        }
    }
}