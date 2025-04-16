using UnityEngine;

namespace Jamcat.Ingame.Player
{
    public class PlayerBody : MonoBehaviour
    {
        [SerializeField] private Transform _head;
        [SerializeField] private Transform _body;
        
        public Transform Head => _head;
        public Transform Body => _body;
    }
}