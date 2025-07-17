using Fusion;
using Fusion.XR.Shared.Rig;
using Jamcat.Core;
using Jamcat.Effect.ScreenEffect;
using Jamcat.Ingame.Player;
using UnityEngine;


namespace Jamcat.Ingame
{
    public partial class InGame : SimulationBehaviour, IPlayerJoined
    {
        public static InGame Instance;

        public enum GameMode
        {
            Tag,
        }
        
        [SerializeField] private GameMode _currentMode = GameMode.Tag;
        [SerializeField] private string _mapName = "city";
        public static string MapName => Instance._mapName;
        private MapController _mapController;
        private MonsterController _monsterController;
        private MaterialSpawner _materialSpawner;
        
        public static MapController Map => Instance._mapController;
        public static MonsterController Monster => Instance._monsterController;
        
        public static int playerID;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            LoadController();
        }

        public void PlayerJoined(PlayerRef player)
        {
            if (player != Runner.LocalPlayer) return;
            playerID = Runner.LocalPlayer.PlayerId - 1;
            
            SpawnPlayer();
            SpawnItems();
            SpawnMaterials();
            
            EffectController.Instance.fadeInOut.FadeIn();
        }
        
        private void SpawnPlayer()
        {
            //충돌, 물리, 로코모션을 담당하는 프리팹
            var player = Loader.LoadPrefab<NetworkObject>(Loader.ResourceType.Avatars, "GamePlayer");
            
            //카메라, 컨트롤러 연동을 담당하는 프리팹
            var rigPrefab = Loader.LoadPrefab<NetworkObject>(Loader.ResourceType.Avatars, "NetworkRig");
            
            //맵에서 스폰 위치 가져오기
            var spot = _mapController.GetSpawnPosition(playerID);
            
            //rig 및 body 스폰
            var networkRig =  Runner.Spawn(rigPrefab,spot.position,spot.rotation).GetComponent<NetworkRig>();
            var body = Runner.Spawn(player,spot.position,spot.rotation).GetComponent<PlayerBody>();
            
            //플레이어 body를 따라오는 카메라 및 HardwareRig
            var playerCamera = FindAnyObjectByType<PlayerFollowerCamera>();
            var hardwareRig = playerCamera.GetComponentInChildren<HardwareRig>();
            
            body.Init(hardwareRig, networkRig);
            playerCamera.Init(body.Head);
            
            
            //body의 로코모션에 컨트롤러 정보 전달
            var locomotion = body.GetComponent<Locomotion.Locomotion>();
            locomotion.Init(networkRig, hardwareRig);
        }

        private void SpawnItems()
        {
            var prefab = Loader.LoadPrefab<NetworkObject>(Loader.ResourceType.Items, "Cube");
            
            foreach (var point in _mapController.ItemSpawnPoints)
            {
                var item = Runner.Spawn(prefab,point.position,point.rotation).GetComponent<Item.Item>();
                item.transform.position = point.position;
                item.Init();
            }
        }

        private void SpawnMaterials()
        {
            _materialSpawner = FindAnyObjectByType<MaterialSpawner>();
            if (_materialSpawner == null)
            {
                var spawnerObject = new GameObject("MaterialSpawner");
                _materialSpawner = spawnerObject.AddComponent<MaterialSpawner>();
            }
            
            _materialSpawner.Initialize(Runner);
        }

        private void LoadController()
        {
            _mapController = Util.SingletonUtil.GetSingletonComponent<MapController>();
            _monsterController = Util.SingletonUtil.GetSingletonComponent<MonsterController>();
        }
    }
}