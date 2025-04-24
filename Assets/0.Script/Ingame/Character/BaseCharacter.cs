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
        
        private Barrier _barrier;

        private void Start()
        {
            currentHp = hp;
            currentEnergy = energy;
        }

        public virtual void TakeDamage(BaseCharacter attacher, float damage)
        {
            hp -= damage;
        }

        private void Die()
        {
            //사망 애니메이션
        }
    }
}