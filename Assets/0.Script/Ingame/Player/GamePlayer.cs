

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
        
        public override void TakeDamage(BaseCharacter attacher, float damage, float knockBackPower = 0.0f)
        {
            hp -= damage;
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
    }
}