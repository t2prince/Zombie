using Fusion.XR.Shared.Rig;
using Jamcat.Ingame.Equipment;
using UnityEngine;

namespace Jamcat.Ingame.Player
{
    public class PlayerBody : MonoBehaviour
    {
        [SerializeField] private Transform _head;
        [SerializeField] private Transform _body;
        
        public Transform Head => _head;
        public Transform Body => _body;

        private Gun currentGun;
        private Melee currentMelee;
        private Barrier currentBarrier;
        private Booster currentBooster;
        public void Init(HardwareRig rig)
        {
            //TODO: Left, Right 컨트롤러 얻어서 각 무기의 실행 함수 연결
        }
    }
}