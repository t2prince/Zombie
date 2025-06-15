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
        [SerializeField] private float CurrentHp;
        [SerializeField] private float energy;
        [SerializeField] private float CurrentEnergy;
        [SerializeField] private AudioSource _hitSource;
        [SerializeField] private AudioSource _deathSource;
        
        private float _energyRecoveryTimer = 0f;

        public int Level { get { return level; } set { level = value; } }
        public NetworkObject NetworkObject { get; private set; }
        public Action<BaseCharacter> OnKilled;
        
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
            _hitSource.Play();
            CurrentHp -= damage;
            if(CurrentHp <= 0)
                Die();
        }
        
        public bool UseBooster(float deltaTime)
        {
            CurrentEnergy -= deltaTime;
            if (!(CurrentEnergy <= 0)) return true;
            
            overBoosted = true;
            CurrentEnergy = 0;
            return false;

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

        public void Kill()
        {
            TakeDamage(this, 99999,0);
        }

        protected virtual void Die()
        {
            Util.Coroutine.DelayedAction(() => 
            {
                OnKilled?.Invoke(this);
                gameObject.SetActive(false);
                OnKilled = null;
            }, 3.0f);
        }

        private void Update()
        {
            // 에너지 회복 로직
            _energyRecoveryTimer += Time.deltaTime;
            if (!(_energyRecoveryTimer >= 0.5f)) return;
            
            _energyRecoveryTimer = 0f;
            CurrentEnergy = Mathf.Min(CurrentEnergy + energy * 0.1f, energy);
            if(CurrentEnergy * 0.2f >= energy)
            {
                overBoosted = false;
            }
        }
    }
}