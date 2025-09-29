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

        // 네트워크 동기화를 위한 위치와 회전
        [Networked] public Vector3 NetworkPosition { get; set; }
        [Networked] public Quaternion NetworkRotation { get; set; }
        
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
            if (Object.InputAuthority != Runner.LocalPlayer) return;
            PerformMovement();
            NetworkPosition = transform.position;
            NetworkRotation = transform.rotation;
        }

        private void Update()
        {
            if (Object != null && Object.InputAuthority != Runner.LocalPlayer) return;
            PerformMovement();
        }

        private void PerformMovement()
        {
            if (arrowKeyAction == null) return;

            var input = arrowKeyAction.ReadValue<Vector2>();
            float deltaTime = Application.isEditor ? Time.deltaTime : (Object?.Runner?.DeltaTime ?? Time.deltaTime);

            transform.Translate(Vector3.forward * input.y * moveSpeed * deltaTime, Space.Self);
            transform.Rotate(Vector3.up, input.x * moveSpeed * 100f * deltaTime);
        }
#endif
        

        public void Init(HardwareRig hardwareRig, NetworkRig networkRig, PlayerRef playerRef)
        {
            _networkRig = networkRig;
            _playerRef = playerRef;
            _pocket = networkRig.GetComponentInChildren<Pocket>();
            _pocket.gameObject.SetActive(false);

            if (hardwareRig == null && networkRig != null && playerRef.PlayerId == networkRig.PlayerId)
            {
                hardwareRig = networkRig.hardwareRig;
            }

            if (hardwareRig != null)
            {
                SetupGrabberEvents(hardwareRig);
            }

            character = GetComponent<BaseCharacter>();

            if (playerRef == InGame.Instance.Runner.LocalPlayer)
            {
                var weapons = PlayerManager.GetWeapons();
                RPC_RequestWeaponSpawn(weapons.gunId, weapons.meleeId, weapons.boosterId);
            }
        }

        private void SetupGrabberEvents(HardwareRig hardwareRig)
        {
            var grabbers = hardwareRig.GetComponentsInChildren<PlayerGrabber>();
            foreach (var grabber in grabbers)
            {
                grabber.onGrabbed += (_) =>
                {
                    var item = grabber.GetComponent<Item.Item>();
                    if (item != null && item.type != Item.Item.ITemType.Use)
                        _pocket.gameObject.SetActive(true);
                };

                grabber.onUngrabbed += (_) => _pocket.gameObject.SetActive(false);
            }
        }

        public override void Spawned()
        {
            base.Spawned();
            NetworkPosition = transform.position;
            NetworkRotation = transform.rotation;

            if (Object.InputAuthority.PlayerId > 0)
            {
                name = $"GamePlayer_{Object.InputAuthority.PlayerId}";
            }

            if (Object.InputAuthority == Runner.LocalPlayer)
            {
                FindAnyObjectByType<PlayerFollowerCamera>()?.Init(_head);
                StartCoroutine(FindAndInitializeMyNetworkRig());
            }
        }

        private System.Collections.IEnumerator FindAndInitializeMyNetworkRig()
        {
            if (Object.InputAuthority != Runner.LocalPlayer) yield break;

            NetworkRig myNetworkRig = null;
            int myPlayerId = Runner.LocalPlayer.PlayerId;

            for (int attempts = 0; attempts < 10 && myNetworkRig == null; attempts++)
            {
                yield return new WaitForSeconds(0.1f);

                foreach (var rig in FindObjectsByType<NetworkRig>(FindObjectsSortMode.None))
                {
                    if (rig.Object?.InputAuthority == Runner.LocalPlayer &&
                        (rig.PlayerId == myPlayerId || rig.PlayerId == -1))
                    {
                        myNetworkRig = rig;
                        break;
                    }
                }
            }

            if (myNetworkRig != null) InitializeNetworkRig(myNetworkRig);
        }

        private void InitializeNetworkRig(NetworkRig networkRig)
        {
            var playerCamera = FindAnyObjectByType<PlayerFollowerCamera>();
            if (playerCamera != null)
            {
                var hardwareRig = playerCamera.GetComponentInChildren<HardwareRig>();
                SetupNetworkRigCameraFollowing(networkRig, playerCamera);

                if (hardwareRig != null)
                {
                    (networkRig as CustomNetworkRig)?.SetHardwareRig(hardwareRig);
                    if (!(networkRig is CustomNetworkRig))
                        networkRig.hardwareRig = hardwareRig;
                }
            }

            Init(null, networkRig, Object.InputAuthority);
        }
        


        
        private void SetupNetworkRigCameraFollowing(NetworkRig networkRig, PlayerFollowerCamera playerCamera)
        {
            if (networkRig == null || playerCamera == null) return;

            var cameraFollower = networkRig.GetComponent<NetworkRigCameraFollower>() ??
                                networkRig.gameObject.AddComponent<NetworkRigCameraFollower>();
            cameraFollower.Init(playerCamera.transform);
        }
        
        public override void Render()
        {
            base.Render();

            if (Object.InputAuthority != Runner.LocalPlayer)
            {
                transform.position = NetworkPosition;
                transform.rotation = NetworkRotation;
            }

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
            if (_networkRig == null || !Object.HasStateAuthority) return;

            var leftHand = _networkRig.leftHand;
            var rightHand = _networkRig.rightHand;
            var leftController = leftHand.GetComponentInChildren<HandController>();
            var rightController = rightHand.GetComponentInChildren<HandController>();

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