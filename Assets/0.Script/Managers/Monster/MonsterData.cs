using Jamcat.Managers.Data;
using UnityEngine;

namespace Jamcat.Managers.Monster
{
    [CreateAssetMenu(fileName = "MonsterData", menuName = "GameData/New MonsterData", order = 2)]
    public class MonsterData : BaseData
    {
        public Ingame.Character.Monster monster;
    }
}