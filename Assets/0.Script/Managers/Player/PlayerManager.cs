using Jamcat.Core;
using UnityEngine;

namespace Jamcat.Managers.Player
{
    public static class PlayerManager
    {
        private static Player _player;
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            
        }
<<<<<<< HEAD
=======

        private static void LoadPlayer()
        {
            
        }

        public static void SaveAll()
        {
            SaveWallet();
            SaveInventory();
        }

        public static void SaveWallet()
        {
            
        }

        public static void SaveInventory()
        {
            
        }
>>>>>>> 27151ea (플레이어 데이터 저장 로직 추가, 데이터 암호화 추가)
    }
}