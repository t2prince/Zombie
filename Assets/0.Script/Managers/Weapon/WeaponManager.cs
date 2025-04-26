<<<<<<< HEAD
=======
using System.Linq;
>>>>>>> 27151ea (플레이어 데이터 저장 로직 추가, 데이터 암호화 추가)
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
<<<<<<< HEAD
=======
        }

        public static WeaponData GetWeaponData(WeaponData.WeaponType type, int id)
        {
            return _data.DataList.First(d => d.type == type && d.id == id);
>>>>>>> 27151ea (플레이어 데이터 저장 로직 추가, 데이터 암호화 추가)
        }
    }
}