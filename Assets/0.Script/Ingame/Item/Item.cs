using System;
using Fusion;
using Fusion.Addons.Physics;
using Fusion.XR.Shared.Grabbing;
using TMPro;
using UnityEngine;

namespace Jamcat.Ingame.Item
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(NetworkRigidbody3D))]
    
    public class Item : NetworkGrabbable
    {
        [SerializeField] private int quantity;
        public enum ITemType
        {
            Material,
            Gold,
            Use
        };

        public int id;
        public ITemType type;
        public int space;
        public int quality;
        
        private Rigidbody _rigidbody;
        private Collider _collider;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
        }
        public void Init()
        {
            //do it!
        }

        public override void LocalGrab()
        {
            _rigidbody.isKinematic = true;
            _collider.enabled = false;
            base.LocalGrab();
        }

        public override void LocalUngrab()
        {
            _rigidbody.isKinematic = false;
            _collider.enabled = true;
            base.LocalUngrab();
        }
    }
}
