using System;
using System.Collections;
using Fusion;
using Jamcat.Ingame.Character;
using UnityEngine;

namespace Jamcat.Ingame.Equipment
{
    public class Bullet : NetworkBehaviour
    {
        private float _damage;
        private float _knockBackPower;
        private float _speed;
        public Action onHit;
        private Vector3 moveDirection;
        private GameObject effect;
        private AudioSource _fireSound;
        
        [Networked] private Vector3 MoveDirection { get; set; }
        
        private BaseCharacter _owner;

        public void Init(BaseCharacter owner, float bulletSpeed, float bulletDamage)
        {
            _owner = owner;
            _speed = bulletSpeed;
            _damage = bulletDamage;
            _fireSound = GetComponent<AudioSource>();
            tag = owner.gameObject.tag;
        }

        public void Fire(Vector3 direction)
        {
            MoveDirection = direction.normalized;
            transform.forward = MoveDirection;
            _fireSound.Play();

            // 일정 시간 후 Despawn
            Util.Coroutine.DelayedAction(() =>
            {
                if (Object != null && Object.IsValid && Object.HasStateAuthority)
                    InGame.Instance.Runner.Despawn(Object);
            }, 5.0f);
        }

        public override void FixedUpdateNetwork()
        {
            if (HasStateAuthority || HasInputAuthority)
            {
                transform.position += MoveDirection * _speed * InGame.Instance.Runner.DeltaTime;
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
            InGame.Instance.Runner.Despawn(Object);
            onHit?.Invoke();
        }
    }
}