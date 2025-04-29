using System.Collections.Generic;
using Jamcat.Core;
using Jamcat.Managers.Weapon;
using UnityEngine;

namespace Jamcat.Managers.Monster
{
    public static class MonsterManager
    {
        private static MonsterMasterData _data;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Init()
        {
            _data = Loader.LoadMasterData<MonsterMasterData>();
        }

        public static List<MonsterData> GetMonsterData()
        {
            return _data.DataList;
        }
    }
}