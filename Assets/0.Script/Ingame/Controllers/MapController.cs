using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Jamcat.Ingame.Character;
using Jamcat.Ingame.Controllers.Component;
using UnityEngine;
using UnityEngine.SceneManagement;
using MonoBehaviourHelper = Util.MonoBehaviourHelper;

namespace Jamcat.Ingame
{
    public class MapController : MonoBehaviour
    {
        private List<Transform> _avatarSpawnPoints;
        public List<Transform> ItemSpawnPoints { get; private set; }

        public List<MonsterAttacher> MonsterSpawnPoints { get; private set; }
        
        private const float dayTime = 10.0f;
        private const float nightTime = 10.0f;
        private int waveCount = 20;
        
        public BaseCamp Camp { get; private set; }

        public Transform GetSpawnPosition(int index)
        {
            return _avatarSpawnPoints[index];
        }
        private void Awake()
        {
            _ = InitAsync(); // async 메서드 호출 (fire-and-forget)
        }

        private void Start()
        {
            
        }

        private async System.Threading.Tasks.Task InitAsync()
        {
            var asyncOp = SceneManager.LoadSceneAsync(InGame.MapName, LoadSceneMode.Additive);
            await asyncOp;

            var loadedScene = SceneManager.GetSceneByName(InGame.MapName);
            var rootObjects = loadedScene.GetRootGameObjects();

            var allAttachers = new List<Attacher>();
            foreach (var root in rootObjects)
            {
                root.transform.SetParent(transform);
                allAttachers.AddRange(root.GetComponentsInChildren<Attacher>());
            }

            _avatarSpawnPoints = GetSpawnPoints(allAttachers, Attacher.SpawnPointType.Character);
            ItemSpawnPoints = GetSpawnPoints(allAttachers, Attacher.SpawnPointType.Item);
            MonsterSpawnPoints = GetSpawnPoints(allAttachers, Attacher.SpawnPointType.Monster).Select(p => p.GetComponent<MonsterAttacher>()).ToList();

            Camp = GetComponentInChildren<BaseCamp>();
            
            StartCoroutine(StartDays());
        }

        private List<Transform> GetSpawnPoints(IEnumerable<Attacher> spawnPoints, Attacher.SpawnPointType type)
        {
            return spawnPoints
                .Where(point => point.type == type)
                .Select(p => p.transform)
                .ToList();
        }

        private IEnumerator StartDays()
        {
            var count = 0;
            yield return new WaitUntil(() => !MonoBehaviourHelper.IsFastNull(InGame.Instance.Runner));
            while (++count < waveCount)
            {
                foreach (var point in MonsterSpawnPoints)
                {
                    point.StopWave();
                }
                yield return Util.Coroutine.WaitForSeconds(dayTime);

                foreach (var point in MonsterSpawnPoints)
                {
                    point.SpawnMonster();
                }
                
                yield return Util.Coroutine.WaitForSeconds(nightTime);
            }
        }
    }
}