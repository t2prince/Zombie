using System;
using Jamcat.Ingame.Character;
using UnityEngine;

namespace Jamcat.Ingame.Equipment
{
    public class Bullet : MonoBehaviour
    {
        public float damage;
        public Action onHit;
        
        private BaseCharacter _owner;
        public void Init(BaseCharacter owner)
        {
            _owner = owner;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag.Equals(_owner.tag)) return;
            
            var target = other.GetComponentInParent<BaseCharacter>();
            if (target == null) return;
            
            
        }
    }
}