using Fusion;
using Fusion.XR.Shared.Grabbing;
using Fusion.XR.Shared.Rig;
using Ingame.Player;
using Jamcat.Ingame.Character;
using Jamcat.Ingame.Controllers.Component;
using Jamcat.Ingame.Equipment;
using Jamcat.Managers.Weapon;
using Jamcat.Managers.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Jamcat.Ingame.Player
{
    public class PlayerBody : NetworkBehaviour
    {
        [SerializeField] private Transform _head;
        [SerializeField] private Transform _body;
        private Pocket _pocket;
        
        public Transform Head => _head;
        public Transform Body => _body;

        [Networked] public int GunId { get; set; } = -1;
        [Networked] public int MeleeId { get; set; } = -1;
        [Networked] public int BoosterId { get; set; } = -1;
        
        private bool _weaponsSpawned = false;
        private PlayerRef _playerRef;

        private Gun currentGun;
        private Melee currentMelee;
        private Barrier currentBarrier;
        private Booster currentBooster;
        private BaseCharacter character;
        private NetworkRig _networkRig;
        private HardwareRig _hardwareRig;

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
            
            character = GetComponent<BaseCharacter>();
        }
        
        public override void FixedUpdateNetwork()
        {
            // 네트워크 권한이 없으면 이동 로직을 실행하지 않음
            if (character != null && !character.Object.HasInputAuthority) return;
            
            PerformMovement();
        }
        
        private void Update()
        {
            // 에디터에서만 Update에서 실행
            PerformMovement();
        }
        
        private void PerformMovement()
        {
            // 방향 키 입력 값 읽기
            var input = arrowKeyAction.ReadValue<Vector2>();
            float deltaTime = Application.isEditor ? Time.deltaTime : (character?.Object?.Runner?.DeltaTime ?? Time.deltaTime);
            var move = Vector3.forward * input.y * moveSpeed * deltaTime;

            // 앞뒤 이동 (로컬 좌표계 기준)
            transform.Translate(move, Space.Self);

            // 좌우 회전
            var rotation = input.x * moveSpeed * 100f * deltaTime;
            transform.Rotate(Vector3.up, rotation);
        }
#endif
        

        public void Init(HardwareRig hardwareRig, NetworkRig networkRig, PlayerRef playerRef)
        {
            Debug.Log($"[PlayerBody] Init - Starting initialization for PlayerRef: {playerRef.PlayerId}, HasInputAuthority: {Object.HasInputAuthority}, HasStateAuthority: {Object.HasStateAuthority}");
            
            _networkRig = networkRig;
            _hardwareRig = hardwareRig;
            _playerRef = playerRef;
            _pocket = networkRig.GetComponentInChildren<Pocket>();
            _pocket.gameObject.SetActive(false);
            
            if (hardwareRig != null)
            {
                Debug.Log($"[PlayerBody] Init - Setting up grabber events for PlayerRef: {playerRef.PlayerId}");
                var grabbers = hardwareRig.GetComponentsInChildren<PlayerGrabber>();
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
            }
            else
            {
                Debug.Log($"[PlayerBody] Init - No HardwareRig provided for PlayerRef: {playerRef.PlayerId}");
            }
            
            character = GetComponent<BaseCharacter>();
            Debug.Log($"[PlayerBody] Init - BaseCharacter component found: {character != null} for PlayerRef: {playerRef.PlayerId}");
            
            // 로컬 플레이어인 경우에만 무기 정보 설정 후 호스트에 스폰 요청
            if (playerRef == InGame.Instance.Runner.LocalPlayer)
            {
                Debug.Log($"[PlayerBody] Init - Local player detected, requesting weapon spawn for PlayerRef: {playerRef.PlayerId}");
                var weapons = PlayerManager.GetWeapons();
                Debug.Log($"[PlayerBody] Init - Requesting weapon spawn for local player - GunId:{weapons.gunId}, MeleeId:{weapons.meleeId}, BoosterId:{weapons.boosterId}");
                RPC_RequestWeaponSpawn(weapons.gunId, weapons.meleeId, weapons.boosterId);
            }
            else
            {
                Debug.Log($"[PlayerBody] Init - Remote player, waiting for weapon data sync for PlayerRef: {playerRef.PlayerId}");
            }
            
            Debug.Log($"[PlayerBody] Init - Initialization completed for PlayerRef: {playerRef.PlayerId}");
        }

        public override void Spawned()
        {
            base.Spawned();
            Debug.Log($"[PlayerBody] Spawned - PlayerRef: {Object.InputAuthority.PlayerId}, HasInputAuthority: {Object.HasInputAuthority}, HasStateAuthority: {Object.HasStateAuthority}");

            // InputAuthority가 있는 플레이어(로컬 플레이어)인 경우 카메라 설정
            if (Object.HasInputAuthority)
            {
                Debug.Log($"[PlayerBody] Spawned - Local player, setting up camera for PlayerRef: {Object.InputAuthority.PlayerId}");
                var playerCamera = FindAnyObjectByType<PlayerFollowerCamera>();
                if (playerCamera != null)
                {
                    playerCamera.Init(_head);
                    Debug.Log($"[PlayerBody] Spawned - Camera initialized for PlayerRef: {Object.InputAuthority.PlayerId}");
                }
                else
                {
                    Debug.LogWarning($"[PlayerBody] Spawned - PlayerCamera not found for PlayerRef: {Object.InputAuthority.PlayerId}");
                }
            }
            else
            {
                Debug.Log($"[PlayerBody] Spawned - Remote player, skipping camera setup for PlayerRef: {Object.InputAuthority.PlayerId}");
            }
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
        public void RPC_SetNetworkRig(NetworkRig networkRig)
        {
            Debug.Log($"[PlayerBody] RPC_SetNetworkRig - Called for PlayerRef: {Object.InputAuthority.PlayerId}, HasInputAuthority: {Object.HasInputAuthority}");
            
            // 이미 초기화되었으면 스킵
            if (_networkRig != null) 
            {
                Debug.Log($"[PlayerBody] RPC_SetNetworkRig - Already initialized, skipping for PlayerRef: {Object.InputAuthority.PlayerId}");
                return;
            }

            // InputAuthority가 있는 클라이언트에서만 실행
            if (Object.HasInputAuthority)
            {
                Debug.Log($"[PlayerBody] RPC_SetNetworkRig - Processing for local player PlayerRef: {Object.InputAuthority.PlayerId}");
                var playerCamera = FindAnyObjectByType<PlayerFollowerCamera>();
                HardwareRig hardwareRig = null;
                
                if (playerCamera != null)
                {
                    Debug.Log($"[PlayerBody] RPC_SetNetworkRig - Found PlayerCamera for PlayerRef: {Object.InputAuthority.PlayerId}");
                    hardwareRig = playerCamera.GetComponentInChildren<HardwareRig>();
                    // NetworkRig가 지속적으로 플레이어 카메라를 따라가도록 설정
                    SetupNetworkRigCameraFollowing(networkRig, playerCamera);
                }
                else
                {
                    Debug.LogWarning($"[PlayerBody] RPC_SetNetworkRig - PlayerCamera not found for PlayerRef: {Object.InputAuthority.PlayerId}");
                }
                
                Debug.Log($"[PlayerBody] RPC_SetNetworkRig - Calling Init for PlayerRef: {Object.InputAuthority.PlayerId}");
                // 초기화 실행
                Init(hardwareRig, networkRig, Object.InputAuthority);
                
                // Locomotion도 같이 초기화
                var locomotion = GetComponent<Locomotion.Locomotion>();
                if (locomotion != null)
                {
                    Debug.Log($"[PlayerBody] RPC_SetNetworkRig - Initializing Locomotion for PlayerRef: {Object.InputAuthority.PlayerId}");
                    locomotion.Init(networkRig, hardwareRig);
                }
            }
            else
            {
                Debug.Log($"[PlayerBody] RPC_SetNetworkRig - No InputAuthority, skipping for PlayerRef: {Object.InputAuthority.PlayerId}");
            }
        }
        
        private void SetupNetworkRigCameraFollowing(NetworkRig networkRig, PlayerFollowerCamera playerCamera)
        {
            if (networkRig == null || playerCamera == null) return;
            
            // NetworkRig에 카메라 따라가기 컴포넌트 추가 또는 가져오기
            var cameraFollower = networkRig.GetComponent<NetworkRigCameraFollower>();
            if (cameraFollower == null)
            {
                cameraFollower = networkRig.gameObject.AddComponent<NetworkRigCameraFollower>();
            }
            
            // 카메라 따라가기 초기화
            cameraFollower.Init(playerCamera.transform);
            
            Debug.Log($"[PlayerBody] NetworkRig set to follow camera continuously");
        }
        
        public override void Render()
        {
            base.Render();
            
            // 원격 플레이어의 무기 정보가 동기화되면 스폰
            if (!_weaponsSpawned && GunId >= 0 && MeleeId >= 0 && BoosterId >= 0)
            {
                Debug.Log($"[PlayerBody] Render - Spawning weapons for remote player - GunId:{GunId}, MeleeId:{MeleeId}, BoosterId:{BoosterId}");
                SpawnWeapons();
                _weaponsSpawned = true;
            }
        }
        
        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void RPC_RequestWeaponSpawn(int gunId, int meleeId, int boosterId)
        {
            Debug.Log($"[PlayerBody] RPC_RequestWeaponSpawn - GunId:{gunId}, MeleeId:{meleeId}, BoosterId:{boosterId}");
            
            // 무기 정보 설정
            GunId = gunId;
            MeleeId = meleeId;
            BoosterId = boosterId;
            
            // 호스트에서 무기 스폰
            SpawnWeapons();
            _weaponsSpawned = true;
        }
        
        private void SpawnWeapons()
        {
            if (_networkRig == null) return;
            
            // 호스트 권한이 있을 때만 스폰
            if (!Object.HasStateAuthority)
            {
                Debug.LogWarning("[PlayerBody] SpawnWeapons - Only host can spawn weapons");
                return;
            }
            
            var leftHand = _networkRig.leftHand;
            var rightHand = _networkRig.rightHand;
            var leftController = leftHand.GetComponentInChildren<HandController>();
            var rightController = rightHand.GetComponentInChildren<HandController>();
            
            //TODO: 플레이어 정보 보고 gun / melee / booster 연결해야함
            //왼손 <-> 오른손 바꿀 수 있게끔 해줄 필요 있음
            
            // Gun 스폰
            var gunData = WeaponManager.GetWeaponData(WeaponData.WeaponType.Gun, GunId);
            var gun = Runner.Spawn(gunData.weaponPrefab, leftHand.transform.position, leftHand.transform.rotation, _playerRef).GetComponent<Gun>();
            gun.transform.SetParent(leftHand.transform);
            gun.Init(character, leftController);
            
            if (_hardwareRig != null)
            {
                var position = _hardwareRig.leftHand.GetComponentInChildren<GunAttacher>().GetPosition(gunData.id);
                gun.SetFirePoint(position);
            }
            
            // Melee 스폰
            var meleeData = WeaponManager.GetWeaponData(WeaponData.WeaponType.Melee, MeleeId);
            var meleeWeapon = Runner.Spawn(meleeData.weaponPrefab, rightHand.transform.position, rightHand.transform.rotation, _playerRef).GetComponent<Melee>();
            meleeWeapon.transform.SetParent(rightHand.transform);
            meleeWeapon.Init(character, rightController);
            
            // Booster 스폰
            var boosterData = WeaponManager.GetWeaponData(WeaponData.WeaponType.Booster, BoosterId);
            var boosterAttacher = _networkRig.GetComponentInChildren<Attacher>();
            var booster = Runner.Spawn(boosterData.weaponPrefab, boosterAttacher.transform.position, boosterAttacher.transform.rotation, _playerRef).GetComponent<Booster>();
            booster.transform.SetParent(boosterAttacher.transform);
            booster.Init(this, leftController, rightController);
            booster.PlayerBody = _networkRig.GetComponentInChildren<NetworkHeadset>().transform;
        }
    }
}