using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;
using Util;

namespace Jamcat.Ingame.Character
{
    public class Monster: BaseCharacter
    {
        public int id;
        
        private NavMeshAgent _agent;
        private BaseCharacter _target;
        private Rigidbody _rigidbody;
        private Collider _collider;
        private Animator _animator;
        private Dictionary<BaseCharacter,float> _aggro = new Dictionary<BaseCharacter,float>();
        [SerializeField] private string walkAnimationName = "Attack";
        [SerializeField] private string AttackAnimationName = "CrawilingAttack";    
        
        public enum AnimationState
        {
            Idle,
            Walk,
            Attack,
            Die,
        }

        private AnimationState animationState
        {
            set
            {
                if (_animator == null) return;

                switch (value)
                {
                    case AnimationState.Idle:
                        _animator.SetTrigger("Idle");
                        break;
                    case AnimationState.Walk:
                        _animator.SetTrigger(walkAnimationName);
                        break;
                    case AnimationState.Attack:
                        _animator.SetTrigger(AttackAnimationName);
                        break;
                    case AnimationState.Die:
                        _animator.SetTrigger("Die");
                        break;
                }
            }
        }
        

        protected override void Init()
        {
            base.Init();
            _agent = GetComponent<NavMeshAgent>();
            _rigidbody = GetComponent<Rigidbody>();
            _animator = GetComponent<Animator>();
            _collider = GetComponent<Collider>();
            
            // Rigidbody가 있다면 isKinematic 설정
            if (_rigidbody != null)
            {
                _rigidbody.isKinematic = true;
            }

            // NavMeshAgent 기본 설정 확인
            if (_agent != null)
            {
                _agent.isStopped = false;
            }
        }

        public void Spawn()
        {
            gameObject.SetActive(true);
        }

        public override void TakeDamage(BaseCharacter attacker, float damage, float knockBackPower = 5.0f)
        {
            base.TakeDamage(attacker, damage);
            if(!_aggro.ContainsKey(attacker))
                _aggro.Add(attacker, 0);
            _aggro[attacker] += damage;
            if(_target == null || _aggro[attacker] > _aggro[_target])
                SetTarget(attacker);
            
            KnockBack(attacker.transform.position - transform.position, knockBackPower);
        }

        public void SetTarget(BaseCharacter target)
        {
            if (!Object.HasStateAuthority) return;
            if (target.IsFastNull()) return;
            
            _target = target;
            _agent.enabled = true;
            _agent.destination = target.transform.position;
        }

        private void KnockBack(Vector3 dir, float power)
        {
            if (_agent == null) return;

            // NavMeshAgent를 일시적으로 비활성화
            _agent.isStopped = true;
            _agent.updatePosition = false;

            // Rigidbody를 사용해 넉백 적용
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(dir.normalized * power, ForceMode.Impulse);
            }

            // 일정 시간 후 NavMeshAgent 재활성화
            StartCoroutine(ResetAgent());
        }
        
        private IEnumerator ResetAgent()
        {
            yield return new WaitForSeconds(0.5f); // 넉백 후 대기 시간
            if (_agent != null)
            {
                _agent.isStopped = false;
                _agent.updatePosition = true;
            }
        }
        
        private void Update()
        {
            if (!Object.HasStateAuthority) return;

            if (!_target.IsFastNull())
            {
                // NavMeshAgent의 목적지 갱신
                _agent.destination = _target.transform.position;
            }
        }

        protected override void Die()
        {
            _agent.enabled = false;
            _collider.enabled = false;
            //사망 애니메이션 재생
            _animator.SetTrigger("Die");
            base.Die();
        }
    }
}