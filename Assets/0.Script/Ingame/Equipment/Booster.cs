using Fusion.Addons.Physics;
using UnityEngine;

namespace Jamcat.Ingame.Equipment
{
    public class Booster : Weapon
    {
        private NetworkRigidbody3D _rigidbody;
        private bool isBoosting = false;
        private float boostForce = 10f;

        private void Awake()
        {
            _rigidbody = GetComponent<NetworkRigidbody3D>();
        }

        public void Init(HandController leftHand, HandController rightHand)
        {
            leftHand.OnLeftSecondaryButtonPressed += BoostStart;
            leftHand.OnLeftSecondaryButtonReleased += BoostEnd;
        }

        private void FixedUpdate()
        {
            if (isBoosting && _rigidbody != null)
            {
                // 위 방향으로 힘을 가함
                _rigidbody.Rigidbody.AddForce(Vector3.up * boostForce, ForceMode.Force);
            }
        }

        public void BoostStart()
        {
            isBoosting = true;
        }

        public void BoostEnd()
        {
            isBoosting = false;
        }
    }
}