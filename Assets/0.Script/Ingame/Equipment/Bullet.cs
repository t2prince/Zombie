using System;
using System.Collections;
using Jamcat.Ingame.Character;
using UnityEngine;

namespace Jamcat.Ingame.Equipment
{
    public class Bullet : MonoBehaviour
    {
        private float _damage;
        private float _knockBackPower;
        private float _speed;
        public Action onHit;
        private Vector3 moveDirection;
        private GameObject effect;
        
        private BaseCharacter _owner;
        public void Init(BaseCharacter owner, float bulletSpeed, float bulletDamage)
        {
            _owner = owner;
            _speed = bulletSpeed;
            _damage = bulletDamage;
        }

        public void Fire(Vector3 direction)
        {
            moveDirection = direction.normalized;
            transform.forward = moveDirection;
            StartCoroutine(MoveForward());
        }

        private IEnumerator MoveForward()
        {
            while (true)
            {
                transform.Translate(moveDirection * _speed * Time.deltaTime, Space.World);
                yield return null;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag.Equals(_owner.tag)) return;
            
            var target = other.GetComponentInParent<BaseCharacter>();
            Hit();
            
            if (target == null) return;
            target.TakeDamage(_owner, _damage,_knockBackPower);
        }

        private void Hit()
        {
            //사운드 재생
            //피격 이팩트
            onHit?.Invoke();
        }
    }
}