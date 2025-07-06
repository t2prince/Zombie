using System;
using _0.Script.Ingame.Player;
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
        [SerializeField] protected float CurrentHp;
        
        [SerializeField] private AudioSource _hitSource;
        [SerializeField] private AudioSource _deathSource;
        
        public float Hp => CurrentHp;
        

        public int Level { get { return level; } set { level = value; } }
        public NetworkObject NetworkObject { get; protected set; }
        public Action<BaseCharacter> OnKilled;

        private void Awake()
        {
            Init();   
        }

        private void Start()
        {
            CurrentHp = hp;
            NetworkObject = GetComponent<NetworkObject>();
        }

        protected virtual void Init()
        {
            _networkTransform = GetComponent<NetworkTransform>();
        }

        public virtual void TakeDamage(BaseCharacter attacker, float damage, float knockBackPower = 0.0f)
        {
            if (CurrentHp <= 0)
                return;
            
            _hitSource.Play();
            CurrentHp -= damage;
            if(CurrentHp <= 0)
                Die();
        }

        public void Spawn()
        {
            CurrentHp = hp + 10 * level;
        }

        public void Heal(float heal)
        {
            CurrentHp += heal;
        }

        public bool IsDead()
        {
            return CurrentHp <= 0;
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
                Destroy(gameObject);
                InGame.Instance.Runner.Despawn(NetworkObject);
                OnKilled = null;
            }, 1.0f);
        }
    }
}