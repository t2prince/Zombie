using Jamcat.Ingame.Character;
using Jamcat.Managers.Player;
using Jjamcat.Util;
using TMPro;
using UnityEngine;

namespace _0.Script.Ingame.Player
{
    public class Watch : SingletonGameObject<Watch>
    {
        public TMP_Text hp;
        public TMP_Text energy;
        public TMP_Text material;
        public TMP_Text gold;

        public void UpdateUI(GamePlayer player)
        {
            hp.text = player.Hp.ToString("F0");
            energy.text = player.Energy.ToString("F0");
            material.text = PlayerManager.GetWallet().material.ToString();
            gold.text = PlayerManager.GetWallet().gold.ToString();
        }
        
    }
}