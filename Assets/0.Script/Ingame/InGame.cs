using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using Fusion.XR.Shared.Rig;
using Jamcat.Core;
using Jamcat.Effect.ScreenEffect;
using Jamcat.Ingame.Player;
using Jamcat.Ingame.Interface;
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
        
        private Dictionary<PlayerRef, (NetworkObject networkRig, NetworkObject gamePlayer)> _players = new(32);
        private UserBoard _userBoard;
        
        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            LoadController();
            _userBoard = FindAnyObjectByType<UserBoard>();
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef playerRef)
        {
            if (!runner.IsServer) return;

            var (networkRigObj, gamePlayerObj) = SpawnPlayer(playerRef);

            if (runner.IsServer)
            {
                SpawnItems();
                SpawnMaterials();
            }

            _players.Add(playerRef, (networkRigObj, gamePlayerObj));
            _userBoard?.AddUser($"Player_{playerRef.PlayerId}");
            EffectController.Instance.fadeInOut.FadeIn();
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef playerRef)
        {
            if (!runner.IsServer) return;

            if (_players.TryGetValue(playerRef, out var playerObjects))
            {
                runner.Despawn(playerObjects.networkRig);
                runner.Despawn(playerObjects.gamePlayer);
                _players.Remove(playerRef);
                _userBoard?.RemoveUser($"Player_{playerRef.PlayerId}");
            }
        }
        
        private (NetworkObject networkRig, NetworkObject gamePlayer) SpawnPlayer(PlayerRef playerRef)
        {
            var spot = _mapController.GetSpawnPosition(playerRef.PlayerId - 1);

            var networkRigObj = Runner.Spawn(
                Loader.LoadPrefab<NetworkObject>(Loader.ResourceType.Avatars, "NetworkRig"),
                spot.position, spot.rotation, inputAuthority: playerRef);
            networkRigObj.GetComponent<NetworkRig>().PlayerId = playerRef.PlayerId;

            var gamePlayerObj = Runner.Spawn(
                Loader.LoadPrefab<NetworkObject>(Loader.ResourceType.Avatars, "GamePlayer"),
                spot.position, spot.rotation, inputAuthority: playerRef);

            return (networkRigObj, gamePlayerObj);
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
        
         #region INetworkRunnerCallbacks (debug log only)
        public void OnConnectedToServer(NetworkRunner runner) {

        }
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
        }
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) {
        }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) {
        }
        #endregion

        #region Unused INetworkRunnerCallbacks 

        public void OnInput(NetworkRunner runner, NetworkInput input) { }
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
        #endregion
    }
}