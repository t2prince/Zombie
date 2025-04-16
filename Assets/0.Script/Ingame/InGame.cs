using Fusion;
using Fusion.XR.Shared.Rig;
using Jamcat.Core;
using Jamcat.Effect.ScreenEffect;
using Jamcat.Ingame.Player;
using UnityEngine;


namespace Jamcat.Ingame
{
    public class InGame : SimulationBehaviour, IPlayerJoined
    {
        public static InGame Instance;

        public enum GameMode
        {
            Tag,
        }
        
        [SerializeField] private GameMode _currentMode = GameMode.Tag;
        [SerializeField] private string _mapName = "Env_GER02_KleineAlster";
        public static string MapName => Instance._mapName;
        private MapController _mapController;
        
        public static int playerID;

        private void Awake()
        {
            Instance = this;
        }

        public void PlayerJoined(PlayerRef player)
        {
            if (player != Runner.LocalPlayer) return;
            playerID = Runner.LocalPlayer.PlayerId - 1;
                
            LoadController();
            SpawnPlayer();
            
            EffectController.Instance.fadeInOut.FadeIn();
        }
        
        private void SpawnPlayer()
        {
            //카메라, 컨트롤러 연동을 담당하는 프리팹
            var rigPrefab = Loader.LoadPrefab<NetworkObject>(Loader.ResourceType.Avatars, "NetworkRig");
            //충돌, 물리, 로코모션을 담당하는 프리팹
            var bodyPrefab = Loader.LoadPrefab<NetworkObject>(Loader.ResourceType.Avatars, "NetworkBody");
            
            //맵에서 스폰 위치 가져오기
            var spot = _mapController.GetSpawnPosition(playerID);
            
            //rig 및 body 스폰
            var rig =  Runner.Spawn(rigPrefab,spot.position,spot.rotation);
            var body = Runner.Spawn(bodyPrefab,spot.position,spot.rotation);
            
            //플레이어 body를 따라오는 카메라
            var playerCamera = FindAnyObjectByType<PlayerFollowerCamera>();
            playerCamera.Init(body.GetComponent<PlayerBody>().Body);
            
            //body의 로코모션에 컨트롤러 정보 전달
            var locomotion = body.GetComponent<Locomotion.Locomotion>();
            locomotion.Init(rig.GetComponent<NetworkRig>());
            
            
        }

        private void LoadController()
        {
            _mapController = Util.SingletonUtil.GetSingletonComponent<MapController>();
        }
    }
}