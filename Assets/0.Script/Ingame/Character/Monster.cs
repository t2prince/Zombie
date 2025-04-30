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
        private Dictionary<BaseCharacter,float> _aggro = new Dictionary<BaseCharacter,float>();

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _rigidbody = GetComponent<Rigidbody>();
        }

        public void Spawn()
        {
            gameObject.SetActive(true);
        }

        public override void TakeDamage(BaseCharacter attacker, float damage, float knockBackPower = 0.1f)
        {
            base.TakeDamage(attacker, damage);
            _aggro[attacker] += damage;
            if(_target == null || _aggro[attacker] > _aggro[_target])
                SetTarget(attacker);
            
            KnockBack(attacker.transform.position - transform.position, knockBackPower);
        }

        public void SetTarget(BaseCharacter target)
        {
            if (target.IsFastNull()) return;
            _target = target;
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
    }
}