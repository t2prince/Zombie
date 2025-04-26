using Jamcat.Core;
using UnityEngine;

namespace Jamcat.Managers.Player
{
    public static class PlayerManager
    {
        private static Player _player;
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            
        }
    }
}