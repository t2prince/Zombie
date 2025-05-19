using System;
using Fusion;
using Jamcat.Ingame.Equipment;
using UnityEngine;

namespace Jamcat.Ingame.Character
{
    public class BaseCharacter : NetworkBehaviour
    {
        private int level;
        private NetworkTransform _networkTransform;
        
        [SerializeField] protected float hp;
        private float CurrentHp { get; set; }
        [SerializeField] private float energy;
        private float CurrentEnergy { get; set; }
        private float _energyRecoveryTimer = 0f;

        public int Level { get { return level; } set { level = value; } }
        public NetworkObject NetworkObject { get; private set; }
        
        private bool overBoosted = false;

        private void Awake()
        {
            Init();   
        }

        private void Start()
        {
            CurrentHp = hp;
            CurrentEnergy = energy;
            NetworkObject = GetComponent<NetworkObject>();
        }

        protected virtual void Init()
        {
            _networkTransform = GetComponent<NetworkTransform>();
        }

        public virtual void TakeDamage(BaseCharacter attacker, float damage, float knockBackPower = 0.0f)
        {
            CurrentHp -= damage;
            if(CurrentHp <= 0)
                Die();
        }
        
        public void UseBooster(float deltaTime)
        {
            CurrentEnergy -= deltaTime;
            if (CurrentEnergy <= 0)
            {
                overBoosted = true;
                CurrentEnergy = 0;
            }
        }

        public void Spawn()
        {
            CurrentHp = hp + 10 * level;
        }

        public void Heal(float heal)
        {
            CurrentHp += heal;
        }

        public bool IsBoostable()
        {
            return CurrentEnergy > 0;
        }

        public void SetPosition(Vector3 pos)
        {
            _networkTransform.Teleport(pos);
        }

        protected virtual void Die()
        {
            Util.Coroutine.DelayedAction(() => 
            {
                gameObject.SetActive(false);
            }, 3.0f);
        }

        private void Update()
        {
            // 에너지 회복 로직
            _energyRecoveryTimer += Time.deltaTime;
            if (_energyRecoveryTimer >= 2f)
            {
                _energyRecoveryTimer = 0f;
                CurrentEnergy = Mathf.Min(CurrentEnergy + energy * 0.01f, energy);
                if(CurrentEnergy * 0.5f >= energy)
                {
                    overBoosted = false;
                }
            }
        }
    }
}