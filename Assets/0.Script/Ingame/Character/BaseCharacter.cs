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

        private void Die()
        {
            //사망 애니메이션
            //30초 후 부활?
            Util.Coroutine.DelayedAction(Respawn,30f);
        }

        private void Respawn()
        {
            //스타트 지점에서 부활
        }
    }
}