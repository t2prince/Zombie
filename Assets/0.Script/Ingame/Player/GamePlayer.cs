

using _0.Script.Ingame.Player;
using Fusion;
using Fusion.XR.Shared.Rig;
using Jamcat.Core;
using Jamcat.Ingame.Equipment;
using Jamcat.Ingame.Player;
using UnityEngine;

namespace Jamcat.Ingame.Character
{
    public class GamePlayer : BaseCharacter
    {
        private Barrier _barrier;
        [SerializeField] private float energy;
        [SerializeField] private float CurrentEnergy;
        public float Energy => CurrentEnergy;
        
        private float _energyRecoveryTimer = 0f;
        
        private bool overBoosted = false;
        
        private void Start()
        {
            CurrentHp = hp;
            CurrentEnergy = energy;
            NetworkObject = GetComponent<NetworkObject>();
        }
        
        public override void TakeDamage(BaseCharacter attacher, float damage, float knockBackPower = 0.0f)
        {
            base.TakeDamage(attacher, damage, knockBackPower);
            Watch.Instance.UpdateUI(this);
        }
        
        protected override void Die()
        {
            //30초 후 부활?
            const float respawnTime = 30f;
            Util.Coroutine.DelayedAction(Respawn,respawnTime);
        }
        
        private void Respawn()
        {
            //스타트 지점에서 부활
        }
        
        public bool UseBooster(float useEnergy)
        {
            CurrentEnergy -= useEnergy;
            if (!(CurrentEnergy <= 0)) return true;
            
            overBoosted = true;
            CurrentEnergy = 0;
            Watch.Instance.UpdateUI(this);
            return false;

        }
        
        public bool IsBoostable()
        {
            return CurrentEnergy > 0;
        }

        
        private void Update()
        {
            // 에너지 회복 로직
            _energyRecoveryTimer += Time.deltaTime;
            if (!(_energyRecoveryTimer >= 0.5f)) return;
            
            _energyRecoveryTimer = 0f;
            CurrentEnergy = Mathf.Min(CurrentEnergy + energy * 0.1f, energy);
            if(CurrentEnergy * 0.2f >= energy)
            {
                overBoosted = false;
            }
            
            Watch.Instance.UpdateUI(this);
        }
    }
}