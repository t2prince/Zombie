using Fusion;
using UnityEngine;
using UnityEngine.Serialization;

namespace Jamcat.Ingame.Equipment
{
    public class Weapon : NetworkBehaviour
    {
        [SerializeField] protected float _damage;
        [FormerlySerializedAs("knockBackPower")] [SerializeField] protected float _knockBackPower;
    }
}