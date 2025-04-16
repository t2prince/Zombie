using UnityEngine;

namespace Jamcat.Ingame.Controllers.Component
{
    public class Attacher : MonoBehaviour
    {
        public enum SpawnPointType
        {
            Character,
        }

        public SpawnPointType type;
    }
}