using Jamcat.Ingame.Equipment;
using UnityEngine;

namespace Jamcat.Ingame.Character
{
    public class BaseCharacter : MonoBehaviour
    {
        private int level;
        
        [SerializeField] protected float hp;
        private float currentHp { get; set; }
        [SerializeField] private float energy;
        private float currentEnergy { get; set; }

        public int Level
        {
            get { return level; }
            set
            {
                level = value;
                currentHp += 10 * level;
            }
        }

        private void Start()
        {
            currentHp = hp;
            currentEnergy = energy;
        }

        public virtual void TakeDamage(BaseCharacter attacher, float damage)
        {
            currentHp -= damage;
            if(currentHp <= 0)
                Die();
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