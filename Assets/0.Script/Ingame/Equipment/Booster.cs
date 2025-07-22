using Fusion;
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
        private BaseCharacter character;
        
        public Transform PlayerBody { get; set; } 
        

        public void Init(PlayerBody body, HandController leftHand, HandController rightHand)
        {
            _rigidbody = body.GetComponent<NetworkRigidbody3D>();
            _gamePlayer = body.GetComponent<GamePlayer>();
            character = body.GetComponent<BaseCharacter>();
            
            leftHand.OnLeftSecondaryButtonPressed += BoostUpStart;
            leftHand.OnLeftSecondaryButtonReleased += BoostUpEnd;
            
            rightHand.OnRightSecondaryButtonPressed += BoostForwardStart;
            rightHand.OnRightSecondaryButtonReleased += BoostForwardEnd;
        }

        public override void FixedUpdateNetwork()
        {
#if !UNITY_EDITOR
            // 네트워크 권한이 없으면 부스터를 실행하지 않음
            if (character != null && !character.Object.HasInputAuthority) return;
#endif
            
            PerformBoost();
        }
        
        private void FixedUpdate()
        {
#if UNITY_EDITOR
            // 에디터에서만 FixedUpdate에서 실행
            PerformBoost();
#endif
        }
        
        private void PerformBoost()
        {
            if ((isBoostingUp || isBoostingForward) && _rigidbody != null)
            {
                // 위 방향으로 힘을 가함
                const float boostRate = 10f;
                var direction = Vector3.zero;
                if(isBoostingUp) direction += Vector3.up;
                if(isBoostingForward) direction += PlayerBody.transform.forward;
                _rigidbody.Rigidbody.AddForce(direction * (0.2f * boostForce), ForceMode.Force);
                
                float deltaTime = Application.isEditor ? Time.fixedDeltaTime : (character?.Object?.Runner?.DeltaTime ?? Time.fixedDeltaTime);
                if (_gamePlayer.UseBooster(deltaTime * boostEnergy * boostRate)) return;
                isBoostingUp = false;
                isBoostingForward = false;
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
                isBoostingForward = true;
            }
        }

        public void BoostForwardEnd()
        {
            isBoostingForward = false;
        }
    }
}