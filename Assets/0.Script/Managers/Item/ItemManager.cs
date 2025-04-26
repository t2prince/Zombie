using UnityEngine;

namespace Jamcat.Managers.Item
{
    public static class ItemManager
    {
        private static ItemMasterData _data;
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Init()
        {
            
        }
    }
}