

using Jamcat.Ingame.Equipment;

namespace Jamcat.Ingame.Character
{
    public class Player : BaseCharacter
    {
        private Barrier _barrier;
        public virtual void TakeDamage(BaseCharacter attacher, float damage)
        {
            hp -= damage;
        }
    }
}