using System;
using Fusion;
using UnityEngine;

namespace Jamcat.Ingame.Player
{
    public class PlayerFollowerCamera : MonoBehaviour
    {
        [SerializeField] private Transform _targetTransform;
        public void Init(Transform targetTransform)
        {
            _targetTransform = targetTransform;
        }

        public void Update()
        {
            if (_targetTransform == null) return;
            
            transform.position = _targetTransform.position;
        }
    }
}