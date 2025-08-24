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
#if UNITY_EDITOR
using UnityEngine.InputSystem;
#endif

namespace Jamcat.Ingame.Player
{
    public class PlayerBody : NetworkBehaviour
    {
        #region Serialized Fields
        [SerializeField] private Transform _head;
        [SerializeField] private Transform _body;
        #endregion
        
        #region Public Properties
        public Transform Head => _head;
        public Transform Body => _body;
        #endregion

        #region Networked Properties
        [Networked] public int GunId { get; set; } = -1;
        [Networked] public int MeleeId { get; set; } = -1;
        [Networked] public int BoosterId { get; set; } = -1;
        [Networked] public NetworkBool WeaponsSpawned { get; set; } = false;
        #endregion
        
        #region Private Fields
        private Pocket _pocket;
        private PlayerRef _playerRef;
        private BaseCharacter _character;
        private NetworkRig _networkRig;
        private HardwareRig _hardwareRig;
        
        private Gun _currentGun;
        private Melee _currentMelee;
        private Barrier _currentBarrier;
        private Booster _currentBooster;
        #endregion

#if UNITY_EDITOR
        #region Editor Only Fields
        private InputAction _arrowKeyAction;
        [SerializeField] private float moveSpeed = 2f;
        #endregion
        private void Start()
        {
            // 방향 키 입력 액션 초기화
            _arrowKeyAction = new InputAction(type: InputActionType.PassThrough);
            _arrowKeyAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");
            _arrowKeyAction.Enable();
            
            _character = GetComponent<BaseCharacter>();
        }
        
        public override void FixedUpdateNetwork()
        {
            // 네트워크 권한이 없으면 이동 로직을 실행하지 않음
            if (_character != null && !_character.Object.HasInputAuthority) return;
            
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
            var input = _arrowKeyAction.ReadValue<Vector2>();
            float deltaTime = Application.isEditor ? Time.deltaTime : (_character?.Object?.Runner?.DeltaTime ?? Time.deltaTime);
            var move = Vector3.forward * input.y * moveSpeed * deltaTime;

            // 앞뒤 이동 (로컬 좌표계 기준)
            transform.Translate(move, Space.Self);

            // 좌우 회전
            var rotation = input.x * 100f * moveSpeed * deltaTime;
            transform.Rotate(Vector3.up, rotation);
        }
#endif
        

        #region Initialization
        public void Init(HardwareRig hardwareRig, NetworkRig networkRig, PlayerRef playerRef)
        {
            InitializeReferences(hardwareRig, networkRig, playerRef);
            SetupPocket();
            SetupGrabberEvents(hardwareRig);
            InitializeCharacter();
            RequestWeaponSpawnIfLocalPlayer(playerRef);
        }

        private void InitializeReferences(HardwareRig hardwareRig, NetworkRig networkRig, PlayerRef playerRef)
        {
            _networkRig = networkRig;
            _hardwareRig = hardwareRig;
            _playerRef = playerRef;
        }

        private void SetupPocket()
        {
            _pocket = _networkRig.GetComponentInChildren<Pocket>();
            _pocket.gameObject.SetActive(false);
        }

        private void SetupGrabberEvents(HardwareRig hardwareRig)
        {
            if (hardwareRig == null) return;

            var grabbers = hardwareRig.GetComponentsInChildren<PlayerGrabber>();
            foreach (var grabber in grabbers)
            {
                grabber.onGrabbed += _ => OnItemGrabbed(grabber);
                grabber.onUngrabbed += _ => OnItemUngrabbed();
            }
        }

        private void OnItemGrabbed(PlayerGrabber grabber)
        {
            var item = grabber.GetComponent<Item.Item>();
            if (item == null || item.type == Item.Item.ITemType.Use) return;
            
            _pocket.gameObject.SetActive(true);
        }

        private void OnItemUngrabbed()
        {
            _pocket.gameObject.SetActive(false);
        }

        private void InitializeCharacter()
        {
            _character = GetComponent<BaseCharacter>();
        }

        private void RequestWeaponSpawnIfLocalPlayer(PlayerRef playerRef)
        {
            if (playerRef == InGame.Instance.Runner.LocalPlayer)
            {
                var weapons = PlayerManager.GetWeapons();
                Debug.Log($"[PlayerBody] Requesting weapon spawn - GunId:{weapons.gunId}, MeleeId:{weapons.meleeId}, BoosterId:{weapons.boosterId}");
                RPC_RequestWeaponSpawn(weapons.gunId, weapons.meleeId, weapons.boosterId);
            }
            else
            {
                Debug.Log("[PlayerBody] Remote player, waiting for weapon data sync");
            }
        }
        #endregion

        public override void Spawned()
        {
            base.Spawned();

            if (Object.HasInputAuthority)
            {
                SetupPlayerCamera();
            }
        }

        private void SetupPlayerCamera()
        {
            var playerCamera = FindAnyObjectByType<PlayerFollowerCamera>();
            if (playerCamera != null)
            {
                playerCamera.Init(_head);
            }
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
        public void RPC_SetNetworkRig(NetworkRig networkRig)
        {
            if (_networkRig != null) return;

            if (Object.HasInputAuthority)
            {
                var hardwareRig = GetHardwareRig();
                Init(hardwareRig, networkRig, Object.InputAuthority);
                InitializeLocomotion(networkRig, hardwareRig);
            }
        }

        private HardwareRig GetHardwareRig()
        {
            var playerCamera = FindAnyObjectByType<PlayerFollowerCamera>();
            return playerCamera?.GetComponentInChildren<HardwareRig>();
        }

        private void InitializeLocomotion(NetworkRig networkRig, HardwareRig hardwareRig)
        {
            var locomotion = GetComponent<Locomotion.Locomotion>();
            locomotion?.Init(networkRig, hardwareRig);
        }
        
        #region Network Callbacks
        public override void Render()
        {
            base.Render();
            CheckAndSpawnWeaponsForRemotePlayer();
        }
        
        private void CheckAndSpawnWeaponsForRemotePlayer()
        {
            // 호스트에서만 실행
            if (!Object.HasStateAuthority) return;
            
            if (!WeaponsSpawned && HasValidWeaponIds())
            {
                Debug.Log($"[PlayerBody] Host spawning weapons for player {_playerRef} - GunId:{GunId}, MeleeId:{MeleeId}, BoosterId:{BoosterId}");
                SpawnWeapons();
                WeaponsSpawned = true;
            }
        }

        private bool HasValidWeaponIds()
        {
            return GunId >= 0 && MeleeId >= 0 && BoosterId >= 0;
        }
        #endregion

        #region Weapon Spawning
        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void RPC_RequestWeaponSpawn(int gunId, int meleeId, int boosterId)
        {
            Debug.Log($"[PlayerBody] Weapon spawn requested - GunId:{gunId}, MeleeId:{meleeId}, BoosterId:{boosterId}");
            
            SetWeaponIds(gunId, meleeId, boosterId);
            SpawnWeapons();
            WeaponsSpawned = true;
        }

        private void SetWeaponIds(int gunId, int meleeId, int boosterId)
        {
            GunId = gunId;
            MeleeId = meleeId;
            BoosterId = boosterId;
        }
        
        private void SpawnWeapons()
        {
            if (!CanSpawnWeapons()) return;
            
            var (leftHand, rightHand, leftController, rightController) = GetHandComponents();
            
            SpawnGun(leftHand, leftController);
            SpawnMelee(rightHand, rightController);
            SpawnBooster(leftController, rightController);
        }

        private bool CanSpawnWeapons()
        {
            if (_networkRig == null) 
            {
                Debug.LogWarning("[PlayerBody] NetworkRig is null, cannot spawn weapons");
                return false;
            }
            
            return true;
        }

        private (NetworkHand leftHand, NetworkHand rightHand, HandController leftController, HandController rightController) GetHandComponents()
        {
            var leftHand = _networkRig.leftHand;
            var rightHand = _networkRig.rightHand;
            var leftController = leftHand.GetComponentInChildren<HandController>();
            var rightController = rightHand.GetComponentInChildren<HandController>();
            
            return (leftHand, rightHand, leftController, rightController);
        }

        private void SpawnGun(NetworkHand leftHand, HandController leftController)
        {
            var gunData = WeaponManager.GetWeaponData(WeaponData.WeaponType.Gun, GunId);
            var gun = Runner.Spawn(gunData.weaponPrefab, leftHand.transform.position, leftHand.transform.rotation, _playerRef).GetComponent<Gun>();
            
            gun.transform.SetParent(leftHand.transform);
            gun.Init(_character, leftController);
            
            SetGunFirePoint(gun, gunData);
            _currentGun = gun;
            
            Debug.Log($"[PlayerBody] Gun spawned for player {_playerRef} by host");
        }

        private void SetGunFirePoint(Gun gun, WeaponData gunData)
        {
            if (_hardwareRig != null)
            {
                var gunAttacher = _hardwareRig.leftHand.GetComponentInChildren<GunAttacher>();
                var firePoint = gunAttacher.GetPosition(gunData.id);
                gun.SetFirePoint(firePoint);
            }
        }

        private void SpawnMelee(NetworkHand rightHand, HandController rightController)
        {
            var meleeData = WeaponManager.GetWeaponData(WeaponData.WeaponType.Melee, MeleeId);
            var meleeWeapon = Runner.Spawn(meleeData.weaponPrefab, rightHand.transform.position, rightHand.transform.rotation, _playerRef).GetComponent<Melee>();
            
            meleeWeapon.transform.SetParent(rightHand.transform);
            meleeWeapon.Init(_character, rightController);
            _currentMelee = meleeWeapon;
            
            Debug.Log($"[PlayerBody] Melee spawned for player {_playerRef} by host");
        }

        private void SpawnBooster(HandController leftController, HandController rightController)
        {
            var boosterData = WeaponManager.GetWeaponData(WeaponData.WeaponType.Booster, BoosterId);
            var boosterAttacher = _networkRig.GetComponentInChildren<Attacher>();
            var booster = Runner.Spawn(boosterData.weaponPrefab, boosterAttacher.transform.position, boosterAttacher.transform.rotation, _playerRef).GetComponent<Booster>();
            
            booster.transform.SetParent(boosterAttacher.transform);
            booster.Init(this, leftController, rightController);
            booster.PlayerBody = _networkRig.GetComponentInChildren<NetworkHeadset>().transform;
            _currentBooster = booster;
            
            Debug.Log($"[PlayerBody] Booster spawned for player {_playerRef} by host");
        }
        #endregion
    }
}