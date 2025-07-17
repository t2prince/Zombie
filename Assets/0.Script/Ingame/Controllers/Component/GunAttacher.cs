using System.Collections.Generic;
using UnityEngine;

namespace Jamcat.Ingame.Controllers.Component
{
    public class GunAttacher : MonoBehaviour
    {
        [SerializeField] private List<Transform> positions;

        public Transform GetPosition(int index)
        {
            return positions[index];
        }
    }
}