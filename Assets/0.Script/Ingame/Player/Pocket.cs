using System;
using Fusion;
using Jamcat.Managers.Player;
using UnityEngine;

namespace Jamcat.Ingame.Player
{
    public class Pocket : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (!other.tag.Equals("Item")) return;
            
            var item = other.GetComponent<Item.Item>();
            if (PlayerManager.AddItem(item))
            {
                InGame.Despawn(item.GetComponent<NetworkObject>());
            }
        }
    }
}