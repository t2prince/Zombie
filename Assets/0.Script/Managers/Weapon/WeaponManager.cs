using System;
using System.Linq;
using Jamcat.Core;
using Jamcat.Managers.Player;
using UnityEngine;

namespace Jamcat.Managers.Weapon
{
    [CreateAssetMenu(fileName = "MasterData", menuName = "MasterData/New WeaponMasterData", order = 0)]
    public static class WeaponManager
    {
        private static WeaponMasterData _data;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Init()
        {
            _data = Loader.LoadMasterData<WeaponMasterData>();
        }

        public static WeaponData GetCurrentWeaponData(WeaponData.WeaponType type)
        {
            switch (type)
            {
                case WeaponData.WeaponType.Gun:
                    return GetWeaponData(type, PlayerManager.GetWeapons().gunId);
                case WeaponData.WeaponType.Melee:
                    return GetWeaponData(type, PlayerManager.GetWeapons().meleeId);
                case WeaponData.WeaponType.Barrier:
                    return GetWeaponData(type, PlayerManager.GetWeapons().barrierId);
                case WeaponData.WeaponType.Booster:
                    return GetWeaponData(type, PlayerManager.GetWeapons().boosterId);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public static WeaponData GetWeaponData(WeaponData.WeaponType type, int id)
        {
            return _data.DataList.First(d => d.type == type && d.id == id);
        }
    }
}