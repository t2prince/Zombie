using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
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
        
        private Dictionary<PlayerRef, (NetworkObject networkRig, NetworkObject gamePlayer)> _players = new(32);
        
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
            {
                return;
            }

            var (networkRigObj, gamePlayerObj) = SpawnPlayer(playerRef);

            if (runner.IsServer)
            {
                SpawnItems();
                SpawnMaterials();
            }

            _players.Add(playerRef, (networkRigObj, gamePlayerObj));
            EffectController.Instance.fadeInOut.FadeIn();
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef playerRef)
        {
            if (runner.Topology == Topologies.ClientServer && runner.IsServer == false)
                return;

            if (_players.TryGetValue(playerRef, out var playerObjects))
            {
                Debug.Log($"[InGame] OnPlayerLeft - Despawning NetworkRig and GamePlayer for PlayerRef: {playerRef.PlayerId}");

                // NetworkRig 디스폰
                if (playerObjects.networkRig != null)
                {
                    Debug.Log($"[InGame] OnPlayerLeft - Despawning NetworkRig: {playerObjects.networkRig.name}");
                    runner.Despawn(playerObjects.networkRig);
                }

                // GamePlayer 디스폰
                if (playerObjects.gamePlayer != null)
                {
                    Debug.Log($"[InGame] OnPlayerLeft - Despawning GamePlayer: {playerObjects.gamePlayer.name}");
                    runner.Despawn(playerObjects.gamePlayer);
                }

                _players.Remove(playerRef);
                Debug.Log($"[InGame] OnPlayerLeft - Player cleanup completed for PlayerRef: {playerRef.PlayerId}");
            }
            else
            {
                Debug.LogWarning($"[InGame] OnPlayerLeft - Player not found in dictionary for PlayerRef: {playerRef.PlayerId}");
            }
        }
        
        private (NetworkObject networkRig, NetworkObject gamePlayer) SpawnPlayer(PlayerRef playerRef)
        {
            var player = Loader.LoadPrefab<NetworkObject>(Loader.ResourceType.Avatars, "GamePlayer");
            var rigPrefab = Loader.LoadPrefab<NetworkObject>(Loader.ResourceType.Avatars, "NetworkRig");
            var spot = _mapController.GetSpawnPosition(playerRef.PlayerId - 1);

            Debug.Log($"[InGame] SpawnPlayer - Spawning NetworkRig for PlayerRef: {playerRef.PlayerId} at position: {spot.position}");
            // NetworkRig를 GamePlayer와 같은 스폰 위치에 스폰
            var networkRigObject = Runner.Spawn(rigPrefab, spot.position, spot.rotation, inputAuthority: playerRef);
            var networkRig = networkRigObject.GetComponent<NetworkRig>();
            networkRig.name = $"NetworkRig_{playerRef.PlayerId}";

            var gamePlayerObject = Runner.Spawn(player, spot.position, spot.rotation, inputAuthority: playerRef);
            var body = gamePlayerObject.GetComponent<PlayerBody>();
            body.name = $"GamePlayer_{playerRef.PlayerId}";

            Debug.Log($"[InGame] SpawnPlayer - Calling RPC_SetNetworkRig for PlayerRef: {playerRef.PlayerId}");
            // 스폰 로직 완료 후 클라이언트에게 NetworkRig 설정 및 카메라 따라가기 호출
            // 클라이언트에서 자신의 씬 위치로 NetworkRig를 이동시킴
            body.RPC_SetNetworkRig(networkRig);

            return (networkRigObject, gamePlayerObject);
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