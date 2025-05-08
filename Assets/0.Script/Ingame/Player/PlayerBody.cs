using Fusion.XR.Shared.Rig;
using Jamcat.Ingame.Character;
using Jamcat.Ingame.Controllers.Component;
using Jamcat.Ingame.Equipment;
using Jamcat.Managers.Weapon;
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

            var leftController = leftHand.GetComponentInChildren<HandController>();
            var rightController = rightHand.GetComponentInChildren<HandController>();
            
            //TODO: 플레이어 정보 보고 gun / melee / booster 연결해야함
            //왼손 / 오른손 바꿀 수 있게끔 해줄 필요 있음
            var character = GetComponent<BaseCharacter>();

            var gunData = WeaponManager.GetCurrentWeaponData(WeaponData.WeaponType.Gun);
            var gun = Instantiate(gunData.weaponPrefab, leftHand.transform).GetComponent<Gun>();
            gun.Init(character, leftController);
            
            var meleeData = WeaponManager.GetCurrentWeaponData(WeaponData.WeaponType.Melee);
            var meleeWeapon = Instantiate(meleeData.weaponPrefab, rightHand.transform).GetComponent<Melee>();
            meleeWeapon.Init(character, rightController);
            
            var boosterData = WeaponManager.GetCurrentWeaponData(WeaponData.WeaponType.Booster);
            var boosterAttacher = networkRig.GetComponentInChildren<Attacher>();
            var booster = Instantiate(boosterData.weaponPrefab, boosterAttacher.transform).GetComponent<Booster>();
            booster.Init(character, boosterAttacher);
        }
    }
}