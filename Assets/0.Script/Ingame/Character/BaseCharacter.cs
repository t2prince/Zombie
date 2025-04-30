using Fusion;
using Jamcat.Ingame.Equipment;
using UnityEngine;

namespace Jamcat.Ingame.Character
{
    public class BaseCharacter : NetworkBehaviour
    {
        private int level;
        
        [SerializeField] protected float hp;
        private float currentHp { get; set; }
        [SerializeField] private float energy;
        private float currentEnergy { get; set; }

        public int Level { get { return level; } set { level = value; } }
        public NetworkObject NetworkObject { get; private set; }
        

        private void Start()
        {
            currentHp = hp;
            currentEnergy = energy;
            NetworkObject = GetComponent<NetworkObject>();
        }

        public virtual void TakeDamage(BaseCharacter attacker, float damage, float knockBackPower = 0.0f)
        {
            currentHp -= damage;
            if(currentHp <= 0)
                Die();
        }

        public void Spawn()
        {
            currentHp = hp + 10 * level;
        }

        public void Heal(float heal)
        {
            currentHp += heal;
        }

        protected virtual void Die()
        {
        
        }
    }
}