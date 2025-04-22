using UnityEngine;

namespace Jamcat.Ingame.Character
{
    public class BaseCharacter : MonoBehaviour
    {
        private int level;
        
        [SerializeField] private float hp;
        [SerializeField] private float currentHp;
        [SerializeField] private float energy;
        [SerializeField] private float currentEnergy;
    }
}