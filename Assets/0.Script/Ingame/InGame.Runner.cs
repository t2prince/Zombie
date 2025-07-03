using Fusion;
using Meta.XR.MRUtilityKit;
using UnityEngine;

namespace Jamcat.Ingame
{
    public partial class InGame
    {
        public static void Spawn(NetworkObject networkObject, Transform parent = null)
        {
            Instance.Runner.Spawn(networkObject, parent.position, parent.rotation);
        }
        
        public static void Despawn(NetworkObject networkObject)
        {
            Instance.Runner.Despawn(networkObject);
        }
    }
}