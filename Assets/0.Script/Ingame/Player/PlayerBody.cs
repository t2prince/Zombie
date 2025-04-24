using Fusion.XR.Shared.Rig;
using Jamcat.Ingame.Character;
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
        
        public void Init(HardwareRig rig, NetworkRig networkRig)
        {
            var leftHand = networkRig.leftHand;
            var rightHand = networkRig.rightHand;

            var leftController = rig.leftHand.GetComponentInChildren<HandController>();
            var rightController = rig.rightHand.GetComponentInChildren<HandController>();
            
            //TODO: 플레이어 정보 보고 gun / melee / booster 연결해야함
            //왼손 / 오른손 바꿀 수 있게끔 해줄 필요 있음
            var gun = leftHand.gameObject.AddComponent<Gun>();
            var melee = rightController.gameObject.AddComponent<Melee>();
            var character = GetComponent<BaseCharacter>();
            var booster = gameObject.AddComponent<Booster>();

            gun.Init(character, leftController);
            melee.Init(character, rightController);
        }
    }
}