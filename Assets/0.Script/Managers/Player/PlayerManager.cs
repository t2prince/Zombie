using System.Collections.Generic;
using System.Linq;
using Jamcat.Core;
using UnityEngine;

namespace Jamcat.Managers.Player
{
    public static class PlayerManager
    {
        private static PlayerData _playerData;
        private static List<Ingame.Item.Item> _pocket;
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            LoadPlayer();
        }

        public static PlayerData.Wallet GetWallet()
        {
            return _playerData.wallet;
        }

        public static PlayerData.Weapons GetWeapons()
        {
            return _playerData.weapons;
        }
        private static void LoadPlayer()
        {
            _playerData = new PlayerData();
        }
        
        public static void SaveAll()
        {
            SaveWallet();
            SaveInventory();
        }

        public static bool AddItem(Ingame.Item.Item item)
        {
            if (!(_pocket.Sum(i => i.space) + item.space < _playerData.space)) return false;
            
            _pocket.Add(item);
            return true;
        }
        
        public static void GetGold(int amount)
        {
            _playerData.wallet.gold += amount;
        }
        
        public static void GetMaterial(int amount)
        {
            _playerData.wallet.material += amount;
        }
        
        public static bool UseGold(int amount)
        {
            if (_playerData.wallet.gold >= amount)
            {
                _playerData.wallet.gold -= amount;
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
            if (_playerData.wallet.material >= amount)
            {
                _playerData.wallet.material -= amount;
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
            if (_playerData.wallet.gem >= amount)
            {
                _playerData.wallet.gem -= amount;
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
            _playerData.wallet.gem += amount;
        }

        public static void SaveWallet()
        {
            
        }

        public static void SaveInventory()
        {
            
        }
    }
}