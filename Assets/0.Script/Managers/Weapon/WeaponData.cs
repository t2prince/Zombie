using Jamcat.Managers.Data;

namespace Jamcat.Managers.Weapon
{
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