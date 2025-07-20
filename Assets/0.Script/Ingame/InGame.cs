using System.Collections.Generic;
using Fusion;
using Fusion.XR.Shared.Rig;
using Jamcat.Core;
using Jamcat.Effect.ScreenEffect;
using Jamcat.Ingame.Player;
using UnityEngine;


namespace Jamcat.Ingame
{
    public partial class InGame : SimulationBehaviour, INetworkRunnerCallbacks
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
        
        private Dictionary<PlayerRef, NetworkObject> _players = new(32);
        
        public static int playerID;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            LoadController();
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef playerRef)
        {
            if (runner.Topology == Topologies.ClientServer && runner.IsServer == false)
                return;

            var playerObj = SpawnPlayer(playerRef);

            if (runner.IsServer)
            {
                SpawnItems();
                SpawnMaterials();
            }

            _players.Add(playerRef, playerObj);
            EffectController.Instance.fadeInOut.FadeIn();
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef playerRef)
        {
            if (runner.Topology == Topologies.ClientServer && runner.IsServer == false)
                return;

            if (_players.TryGetValue(playerRef, out var player))
            {
                runner.Despawn(player);
                _players.Remove(playerRef);
            }
        }
        
        private NetworkObject SpawnPlayer(PlayerRef playerRef)
        {
            var player = Loader.LoadPrefab<NetworkObject>(Loader.ResourceType.Avatars, "GamePlayer");
            var rigPrefab = Loader.LoadPrefab<NetworkObject>(Loader.ResourceType.Avatars, "NetworkRig");
            var spot = _mapController.GetSpawnPosition(playerID);

            var networkRig = Runner.Spawn(rigPrefab, spot.position, spot.rotation, inputAuthority: playerRef)
                .GetComponent<NetworkRig>();

            var body = Runner.Spawn(player, spot.position, spot.rotation, inputAuthority: playerRef)
                .GetComponent<PlayerBody>();

            var playerCamera = FindAnyObjectByType<PlayerFollowerCamera>();
            var hardwareRig = playerCamera.GetComponentInChildren<HardwareRig>();

            body.Init(hardwareRig, networkRig);
            playerCamera.Init(body.Head);

            var locomotion = body.GetComponent<Locomotion.Locomotion>();
            locomotion.Init(networkRig, hardwareRig);

            return networkRig.GetComponent<NetworkObject>();
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