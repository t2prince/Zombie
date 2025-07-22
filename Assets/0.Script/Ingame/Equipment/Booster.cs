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
        private bool isBoostingUp = false;
        private bool isBoostingForward = false;
        [SerializeField] private float boostForce = 10f;
        [SerializeField] private float boostEnergy = 5f;
        
        public Transform PlayerBody { get; set; } 
        

        public void Init(PlayerBody body, HandController leftHand, HandController rightHand)
        {
            _rigidbody = body.GetComponent<NetworkRigidbody3D>();
            _gamePlayer = body.GetComponent<GamePlayer>();
            leftHand.OnLeftSecondaryButtonPressed += BoostUpStart;
            leftHand.OnLeftSecondaryButtonReleased += BoostUpEnd;
            
            rightHand.OnLeftSecondaryButtonPressed += BoostForwardStart;
            rightHand.OnLeftSecondaryButtonReleased += BoostForwardEnd;
        }

        private void FixedUpdate()
        {
            if ((isBoostingUp || isBoostingForward) && _rigidbody != null)
            {
                // 위 방향으로 힘을 가함
                const float boostRate = 10f;
                var direction = Vector3.zero;
                if(isBoostingUp) direction += Vector3.up;
                if(isBoostingForward) direction += PlayerBody.transform.forward;
                _rigidbody.Rigidbody.AddForce(direction * (0.2f * boostForce), ForceMode.Force);
                if (_gamePlayer.UseBooster(Time.fixedDeltaTime * boostEnergy * boostRate)) return;
                isBoostingUp = false;
            }
        }

        public void BoostUpStart()
        {
            if (_gamePlayer.IsBoostable())
            {
                isBoostingUp = true;
            }
        }

        public void BoostUpEnd()
        {
            isBoostingUp = false;
        }
        
        public void BoostForwardStart()
        {
            if (_gamePlayer.IsBoostable())
            {
                isBoostingUp = true;
            }
        }

        public void BoostForwardEnd()
        {
            isBoostingUp = false;
        }
    }
}