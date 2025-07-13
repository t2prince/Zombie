using Fusion.XR.Shared.Grabbing;
using UnityEngine;

namespace Ingame.Player
{
    public class PlayerGrabber : Grabber
    {
        public System.Action<Grabbable> onGrabbed;
        public System.Action<Grabbable> onUngrabbed;

        private Vector3 _previousPosition;
        private Vector3 _currentVelocity;
        private Vector3 _previousRotation;
        private Vector3 _currentAngularVelocity;

        [SerializeField] private float _velocityMultiplier = 1.0f;
        [SerializeField] private float _angularVelocityMultiplier = 1.0f;
        [SerializeField] private float _maxDropVelocity = 10f;

        private void Update()
        {
            // Check if the local hand is still grabbing the object
            if (grabbedObject != null && IsGrabbing == false)
            {
                // Object released by this hand
                Ungrab(grabbedObject);
            }
            CheckHovered();
            
            if (grabbedObject != null)
            {
                Vector3 currentPosition = transform.position;
                Vector3 currentRotation = transform.eulerAngles;
                
                _currentVelocity = (currentPosition - _previousPosition) / Time.deltaTime;
                _currentAngularVelocity = (currentRotation - _previousRotation) / Time.deltaTime;
                
                _previousPosition = currentPosition;
                _previousRotation = currentRotation;
            }
        }

        public override void Grab(Grabbable grabbable)
        {
            base.Grab(grabbable);
            _previousPosition = transform.position;
            _previousRotation = transform.eulerAngles;
            _currentVelocity = Vector3.zero;
            _currentAngularVelocity = Vector3.zero;
            onGrabbed?.Invoke(grabbable);
        }

        public override void Ungrab(Grabbable grabbable)
        {
            base.Ungrab(grabbable);
            
            var item = grabbable.GetComponent<Jamcat.Ingame.Item.Item>();
            if (item != null)
            {
                var rigidbody = item.GetComponent<Rigidbody>();
                if (rigidbody != null)
                {
                    Vector3 finalVelocity = _currentVelocity * _velocityMultiplier;
                    Vector3 finalAngularVelocity = _currentAngularVelocity * _angularVelocityMultiplier;
                    
                    finalVelocity = Vector3.ClampMagnitude(finalVelocity, _maxDropVelocity);
                    
                    rigidbody.linearVelocity = finalVelocity;
                    rigidbody.angularVelocity = finalAngularVelocity;
                }
            }
            
            onUngrabbed?.Invoke(grabbable);
        }
    }
}