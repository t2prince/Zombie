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
            _networkRig = networkRig;
            _playerRef = playerRef;
            _pocket = networkRig.GetComponentInChildren<Pocket>();
            _pocket.gameObject.SetActive(false);

            // HardwareRig는 NetworkRig에 직접 설정되었으므로, NetworkRig에서 가져옴
            if (hardwareRig == null && networkRig != null)
            {
                hardwareRig = networkRig.hardwareRig;
                Debug.Log($"[PlayerBody] Init - Using HardwareRig from NetworkRig: {hardwareRig?.name}");
            }

            if (hardwareRig != null)
            {
                var grabbers = hardwareRig.GetComponentsInChildren<PlayerGrabber>();
                foreach (var grabber in grabbers)
                {
                    grabber.onGrabbed += (_) =>
                    {
                        var item = grabber.GetComponent<Item.Item>();
                        if (item == null || item.type == Item.Item.ITemType.Use) return;

                        _pocket.gameObject.SetActive(true);
                    };

                    grabber.onUngrabbed += (_) =>
                    {
                        _pocket.gameObject.SetActive(false);
                    };
                }
            }
            else
            {
                Debug.LogWarning($"[PlayerBody] Init - No HardwareRig available for player: {playerRef.PlayerId}");
            }

            character = GetComponent<BaseCharacter>();

            // 로컬 플레이어인 경우에만 무기 정보 설정 후 호스트에 스폰 요청
            if (playerRef == InGame.Instance.Runner.LocalPlayer)
            {
                var weapons = PlayerManager.GetWeapons();
                RPC_RequestWeaponSpawn(weapons.gunId, weapons.meleeId, weapons.boosterId);
            }
        }

        public override void Spawned()
        {
            base.Spawned();

            // 모든 클라이언트에서 자신의 로컬 플레이어인지 확인
            if (Object.InputAuthority == Runner.LocalPlayer)
            {
                var playerCamera = FindAnyObjectByType<PlayerFollowerCamera>();
                if (playerCamera != null)
                {
                    playerCamera.Init(_head);
                }

                // 자신의 NetworkRig를 찾아서 초기화
                StartCoroutine(FindAndInitializeMyNetworkRig());
            }
        }

        private System.Collections.IEnumerator FindAndInitializeMyNetworkRig()
        {
            // NetworkRig가 스폰될 때까지 대기
            NetworkRig myNetworkRig = null;
            int maxAttempts = 10;
            int attempts = 0;

            while (myNetworkRig == null && attempts < maxAttempts)
            {
                yield return new WaitForSeconds(0.1f);
                attempts++;

                // 씬에서 자신의 NetworkRig 찾기 (InputAuthority 기준)
                var allNetworkRigs = FindObjectsByType<NetworkRig>(FindObjectsSortMode.None);
                foreach (var rig in allNetworkRigs)
                {
                    if (rig.Object != null && rig.Object.InputAuthority == Runner.LocalPlayer)
                    {
                        myNetworkRig = rig;
                        Debug.Log($"[PlayerBody] Found my NetworkRig: {rig.name} for Player: {Runner.LocalPlayer.PlayerId}");
                        break;
                    }
                }
            }

            if (myNetworkRig != null)
            {
                // NetworkRig 초기화
                InitializeNetworkRig(myNetworkRig);
            }
            else
            {
                Debug.LogError($"[PlayerBody] Could not find NetworkRig for Player: {Runner.LocalPlayer.PlayerId}");
            }
        }

        private void InitializeNetworkRig(NetworkRig networkRig)
        {
            Debug.Log($"[PlayerBody] InitializeNetworkRig - Starting initialization for NetworkRig: {networkRig.name}");

            var playerCamera = FindAnyObjectByType<PlayerFollowerCamera>();

            if (playerCamera != null)
            {
                Debug.Log($"[PlayerBody] InitializeNetworkRig - Found PlayerCamera for NetworkRig: {networkRig.name}");
                var hardwareRig = playerCamera.GetComponentInChildren<HardwareRig>();

                // NetworkRig가 지속적으로 플레이어 카메라를 따라가도록 설정
                SetupNetworkRigCameraFollowing(networkRig, playerCamera);

                // 클라이언트가 자신의 NetworkRig에 HardwareRig 직접 설정
                if (hardwareRig != null)
                {
                    Debug.Log($"[PlayerBody] InitializeNetworkRig - Setting HardwareRig for NetworkRig: {networkRig.name}, HardwareRig: {hardwareRig.name}");

                    // CustomNetworkRig인 경우 전용 메서드 사용
                    var customNetworkRig = networkRig as CustomNetworkRig;
                    if (customNetworkRig != null)
                    {
                        customNetworkRig.SetHardwareRig(hardwareRig);
                        Debug.Log($"[PlayerBody] InitializeNetworkRig - CustomNetworkRig.SetHardwareRig called for: {networkRig.name}");
                    }
                    else
                    {
                        networkRig.hardwareRig = hardwareRig;
                        Debug.Log($"[PlayerBody] InitializeNetworkRig - Direct hardwareRig assignment for: {networkRig.name}");
                    }
                }
                else
                {
                    Debug.LogError($"[PlayerBody] InitializeNetworkRig - HardwareRig not found in PlayerCamera for: {networkRig.name}");
                }
            }
            else
            {
                Debug.LogWarning($"[PlayerBody] InitializeNetworkRig - PlayerCamera not found for NetworkRig: {networkRig.name}");
            }

            Debug.Log($"[PlayerBody] InitializeNetworkRig - Calling Init for NetworkRig: {networkRig.name}");
            // 초기화 실행 (hardwareRig는 이제 NetworkRig에 설정되었으므로 null을 전달)
            Init(null, networkRig, Object.InputAuthority);

            Debug.Log($"[PlayerBody] InitializeNetworkRig - NetworkRig initialization completed for: {networkRig.name}");
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
            
        }
        
        public override void Render()
        {
            base.Render();
            
            // 원격 플레이어의 무기 정보가 동기화되면 스폰
            if (!_weaponsSpawned && GunId >= 0 && MeleeId >= 0 && BoosterId >= 0)
            {
                SpawnWeapons();
                _weaponsSpawned = true;
            }
        }
        
        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void RPC_RequestWeaponSpawn(int gunId, int meleeId, int boosterId)
        {
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