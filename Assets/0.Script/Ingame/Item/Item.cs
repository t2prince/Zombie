using System;
using Fusion;
using Fusion.XR.Shared.Grabbing;
using TMPro;
using UnityEngine;

namespace Jamcat.Ingame.Item
{
    public class Item : Grabbable
    {
        [SerializeField] private TMP_Text ownerName;
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

        public override void Grab(Grabber newGrabber, Transform grabPointTransform = null)
        {
            _rigidbody.isKinematic = true;
            _collider.enabled = false;
            base.Grab(newGrabber, grabPointTransform);
        }

        public override void Ungrab()
        {
            _rigidbody.isKinematic = false;
            _collider.enabled = true;
            base.Ungrab();
        }
    }
}
