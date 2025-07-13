using Fusion.Addons.Physics;
using Jamcat.Ingame.Character;
using Jamcat.Ingame.Player;
using UnityEngine;

namespace Jamcat.Ingame.Equipment
{
    public class Booster : Weapon
    {
        private GamePlayer _gamePlayer;
        private NetworkRigidbody3D _rigidbody;
        private bool isBoosting = false;
        [SerializeField] private float boostForce = 10f;
        [SerializeField] private float boostEnergy = 10f;
        
        public Transform PlayerBody { get; set; } 
        

        public void Init(PlayerBody body, HandController leftHand, HandController rightHand)
        {
            _rigidbody = body.GetComponent<NetworkRigidbody3D>();
            _gamePlayer = body.GetComponent<GamePlayer>();
            leftHand.OnLeftSecondaryButtonPressed += BoostStart;
            leftHand.OnLeftSecondaryButtonReleased += BoostEnd;
        }

        private void FixedUpdate()
        {
            if (isBoosting && _rigidbody != null)
            {
                // 위 방향으로 힘을 가함
                const float boostRate = 10f;
                _rigidbody.Rigidbody.AddForce((Vector3.up + PlayerBody.transform.forward * 0.2f)* boostForce, ForceMode.Force);
                if (_gamePlayer.UseBooster(Time.fixedDeltaTime * boostEnergy * boostRate)) return;
                isBoosting = false;
            }
        }

        public void BoostStart()
        {
            if (_gamePlayer.IsBoostable())
            {
                isBoosting = true;
            }
        }

        public void BoostEnd()
        {
            isBoosting = false;
        }
    }
}