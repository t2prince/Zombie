using System;
using UnityEngine;

namespace Jamcat.Managers.Player
{
    public partial class Player
    {
        [Serializable]
        public class Wallet
        {
            public int gold = 0;
            public int gem = 0;
        }

        [Serializable]
        public class Weapons
        {
            public int gunId = 0;
            public int meleeId = 0;
            public int barrierId = 0;
            public int boosterId = 0;
        }
        
        public Weapons weapons = new Weapons();
        public Wallet wallet = new Wallet();
    }
}