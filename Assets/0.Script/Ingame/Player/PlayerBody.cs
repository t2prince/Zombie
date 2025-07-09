using Fusion.XR.Shared.Grabbing;
using Fusion.XR.Shared.Rig;
using Ingame.Player;
using Jamcat.Ingame.Character;
using Jamcat.Ingame.Controllers.Component;
using Jamcat.Ingame.Equipment;
using Jamcat.Managers.Weapon;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Jamcat.Ingame.Player
{
    public class PlayerBody : MonoBehaviour
    {
        [SerializeField] private Transform _head;
        [SerializeField] private Transform _body;
        private Pocket _pocket;
        
        public Transform Head => _head;
        public Transform Body => _body;

        private Gun currentGun;
        private Melee currentMelee;
        private Barrier currentBarrier;
        private Booster currentBooster;

        

#if UNITY_EDITOR
        private InputAction arrowKeyAction;
        [SerializeField] private float moveSpeed = 2f;
        private void Start()
        {
            // 방향 키 입력 액션 초기화
            arrowKeyAction = new InputAction(type: InputActionType.PassThrough);
            arrowKeyAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");
            arrowKeyAction.Enable();
        }
        
        private void Update()
        {
            // 방향 키 입력 값 읽기
            var input = arrowKeyAction.ReadValue<Vector2>();
            var move = Vector3.forward * input.y * moveSpeed * Time.deltaTime;

            // 앞뒤 이동 (로컬 좌표계 기준)
            transform.Translate(move, Space.Self);

            // 좌우 회전
            var rotation = input.x * moveSpeed * 100f * Time.deltaTime;
            transform.Rotate(Vector3.up, rotation);
        }
#endif
        
        public void Init(HardwareRig rig, NetworkRig networkRig)
        {
            var leftHand = networkRig.leftHand;
            var rightHand = networkRig.rightHand;
            _pocket = GetComponentInChildren<Pocket>();
            _pocket.gameObject.SetActive(false);
            
            var grabbers = rig.GetComponentsInChildren<PlayerGrabber>();
            foreach (var grabber in grabbers)
            {
                grabber.onGrabbed += (g) =>
                {
                    var item = grabber.GetComponent<Item.Item>();
                    if (item == null || item.type == Item.Item.ITemType.Use) return;
                    
                    _pocket.gameObject.SetActive(true);
                };    
                
                grabber.onUngrabbed += (g) =>
                {
                    _pocket.gameObject.SetActive(false);
                };   
            }

            var leftController = leftHand.GetComponentInChildren<HandController>();
            var rightController = rightHand.GetComponentInChildren<HandController>();
            
            //TODO: 플레이어 정보 보고 gun / melee / booster 연결해야함
            //왼손 <-> 오른손 바꿀 수 있게끔 해줄 필요 있음
            var character = GetComponent<BaseCharacter>();

            // 네트워크 권한이 있는 경우에만 무기 스폰
            if (!character.Object.HasStateAuthority) return;
            
            var gunData = WeaponManager.GetCurrentWeaponData(WeaponData.WeaponType.Gun);
#if UNITY_EDITOR
            var gun = InGame.Instance.Runner.Spawn(gunData.weaponPrefab, leftHand.transform.position + new Vector3(-0.3f,0,0), leftHand.transform.rotation).GetComponent<Gun>();
#else
            var gun = InGame.Instance.Runner.Spawn(gunData.weaponPrefab, leftHand.transform.position, leftHand.transform.rotation).GetComponent<Gun>();
#endif
            gun.transform.SetParent(leftHand.transform);
            gun.Init(character, leftController);
                
            var meleeData = WeaponManager.GetCurrentWeaponData(WeaponData.WeaponType.Melee);
            var meleeWeapon = InGame.Instance.Runner.Spawn(meleeData.weaponPrefab, rightHand.transform.position, rightHand.transform.rotation).GetComponent<Melee>();
            meleeWeapon.transform.SetParent(rightHand.transform);
            meleeWeapon.Init(character, rightController);
                
            var boosterData = WeaponManager.GetCurrentWeaponData(WeaponData.WeaponType.Booster);
            var boosterAttacher = networkRig.GetComponentInChildren<Attacher>();
            var booster = InGame.Instance.Runner.Spawn(boosterData.weaponPrefab, boosterAttacher.transform.position, boosterAttacher.transform.rotation).GetComponent<Booster>();
            booster.transform.SetParent(boosterAttacher.transform);
            booster.Init(this, leftController, rightController);
        }
    }
}