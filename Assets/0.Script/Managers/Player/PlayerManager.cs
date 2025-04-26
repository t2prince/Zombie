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
            LoadPlayer();
        }

        public static Player.Wallet GetWallet()
        {
            return _player.wallet;
        }

        public static Player.Weapons GetWeapons()
        {
            return _player.weapons;
        }
        private static void LoadPlayer()
        {
            _player = new Player();
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
    }
}