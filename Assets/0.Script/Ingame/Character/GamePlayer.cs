

using Jamcat.Ingame.Equipment;

namespace Jamcat.Ingame.Character
{
    public class GamePlayer : BaseCharacter
    {
        private Barrier _barrier;
        public virtual void TakeDamage(BaseCharacter attacher, float damage, float knockBackPower = 0.0f)
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