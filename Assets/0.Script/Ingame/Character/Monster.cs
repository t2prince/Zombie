using System;
using System.Collections.Generic;
using UnityEngine.AI;
using Util;

namespace Jamcat.Ingame.Character
{
    public class Monster: BaseCharacter
    {
        public int id;
        
        private NavMeshAgent _agent;
        private BaseCharacter _target;
        private Dictionary<BaseCharacter,float> _aggro = new Dictionary<BaseCharacter,float>();

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
        }

        public void Spawn()
        {
            gameObject.SetActive(true);
        }

        public override void TakeDamage(BaseCharacter attacker, float damage)
        {
            base.TakeDamage(attacker, damage);
            _aggro[attacker] += damage;
            if(_target == null || _aggro[attacker] > _aggro[_target])
                SetTarget(attacker);
        }

        public void SetTarget(BaseCharacter target)
        {
            if (target.IsFastNull()) return;
            _target = target;
            _agent.destination = target.transform.position;
        }
    }
}