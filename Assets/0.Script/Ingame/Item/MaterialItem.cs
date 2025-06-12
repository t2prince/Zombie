using System;
using _0.Script.Managers;
using UnityEngine;

namespace Jamcat.Ingame.Item
{
    public class MaterialItem : Item
    {
        [SerializeField] private int quantity;
        

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(TagManager.HOLDER))
            {
                    
            }
            else if (other.CompareTag(TagManager.INVENTORY))
            {
                
            }
        }
    }
}