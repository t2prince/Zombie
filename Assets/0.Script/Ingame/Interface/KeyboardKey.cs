using System;
using Jamcat.Ingame.Interface;
using UnityEngine;

namespace _0.Script.Ingame.Interface
{
    public class KeyboardKey : MonoBehaviour
    {
        private string text = "";
        private Monitor _monitor;
        private const float originalHeight = 0.03885181f; // Original height of the key
        private const float pressHeight = 0.01f; // Height to press the key

        private void Awake()
        {
            var textMesh = GetComponentInChildren<TextMesh>();
            text = textMesh.text;
            _monitor = GetComponentInParent<Monitor>();
        }
        
        private void OnKeyPress()
        {
            _monitor.AddText(text);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                OnKeyPress();
            }
        }
        
        private void OnTriggerExit(Collider other)
        {
            
        }
    }
}