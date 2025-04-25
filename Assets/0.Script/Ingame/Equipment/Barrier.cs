using UnityEngine;

namespace Jamcat.Ingame.Equipment
{
    public class Barrier
    {
        [SerializeField] private float energy;

        public void TakeDamage(float damage)
        {
            energy -= damage;
            if(energy <= 0)
                Broken();
        }

        private void Broken()
        {
            
        }
    }
}