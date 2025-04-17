using UnityEngine;

namespace Jamcat.Ingame.Controllers.Component
{
    public class Attacher : MonoBehaviour
    {
        public enum SpawnPointType
        {
            Character,
            Item,
        }

        public SpawnPointType type;
        public string id;
    }
}