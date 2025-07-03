using System.Collections.Generic;
using System.Linq;
using Jamcat.Core;
using UnityEngine;

namespace Jamcat.Managers.Player
{
    public static class PlayerManager
    {
        private static Player _player;
        private static List<Ingame.Item.Item> _pocket;
        
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

        public static bool AddItem(Ingame.Item.Item item)
        {
            if (!(_pocket.Sum(i => i.space) + item.space < _player.space)) return false;
            
            _pocket.Add(item);
            return true;
        }
        
        public static void GetGold(int amount)
        {
            _player.wallet.gold += amount;
        }
        
        public static void GetMaterial(int amount)
        {
            _player.wallet.material += amount;
        }
        
        public static bool UseGold(int amount)
        {
            if (_player.wallet.gold >= amount)
            {
                _player.wallet.gold -= amount;
                return true;
            }
            else
            {
                Debug.LogWarning("Not enough gold!");
                return false;
            }
        }
        
        public static bool UseMaterial(int amount)
        {
            if (_player.wallet.material >= amount)
            {
                _player.wallet.material -= amount;
                return true;
            }
            else
            {
                Debug.LogWarning("Not enough material!");
                return false;
            }
        }
        
        public static bool UseGem(int amount)
        {
            if (_player.wallet.gem >= amount)
            {
                _player.wallet.gem -= amount;
                return true;
            }
            else
            {
                Debug.LogWarning("Not enough gem!");
                return false;
            }
        }
        
        public static void AddGem(int amount)
        {
            _player.wallet.gem += amount;
        }

        public static void SaveWallet()
        {
            
        }

        public static void SaveInventory()
        {
            
        }
    }
}