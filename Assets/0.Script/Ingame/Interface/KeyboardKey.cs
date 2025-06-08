using System;
using Jamcat.Ingame.Interface;
using UnityEngine;

namespace _0.Script.Ingame.Interface
{
    public class KeyboardKey : MonoBehaviour
    {
        private string text = "";
        private Monitor _monitor;

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
    }
}