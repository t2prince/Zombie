using Jamcat.Core;
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
    }
}