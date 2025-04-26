using Jamcat.Managers.Data;
using UnityEngine;

namespace Jamcat.Managers.Weapon
{
    [CreateAssetMenu(fileName = "WeaponData", menuName = "GameData/New WeaponData", order = 0)]
    public class WeaponData : BaseData
    {
        public enum WeaponType
        {
            Gun,
            Melee,
            Barrier,
            Booster
        }

        public enum WeaponGrade
        {
            C,
            B,
            A,
            S,
        }

        public WeaponType type;
        public Ingame.Equipment.Weapon weaponPrefab;
        public WeaponGrade grade;
    }
}