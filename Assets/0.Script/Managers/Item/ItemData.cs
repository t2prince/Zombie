using Jamcat.Managers.Data;
using UnityEngine;

namespace Jamcat.Managers.Item
{
    [CreateAssetMenu(fileName = "ItemData", menuName = "GameData/New ItemData", order = 0)]
    public class ItemData : BaseData
    {
        public enum ItemType
        {
            None,
            Gold,
            Material,
            Consumable,
        }
        
        public ItemType type;
        public int count;
        public GameObject itemPrefab;
    }
}