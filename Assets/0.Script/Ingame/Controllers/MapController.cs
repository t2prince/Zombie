using System.Collections.Generic;
using System.Linq;
using Jamcat.Ingame.Controllers.Component;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Jamcat.Ingame
{
    public class MapController : MonoBehaviour
    {
        private List<Transform> _avatarSpawnPoints;
        public List<Transform> ItemSpawnPoints { get; private set; }

        public List<MonsterAttacher> MonsterSpawnPoints { get; private set; }

        public Transform GetSpawnPosition(int index)
        {
            return _avatarSpawnPoints[index];
        }
        private void Awake()
        {
            _ = InitAsync(); // async 메서드 호출 (fire-and-forget)
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
                allAttachers.AddRange(root.GetComponentsInChildren<Attacher>());
            }

            _avatarSpawnPoints = GetSpawnPoints(allAttachers, Attacher.SpawnPointType.Character);
            ItemSpawnPoints = GetSpawnPoints(allAttachers, Attacher.SpawnPointType.Item);
            MonsterSpawnPoints = GetSpawnPoints(allAttachers, Attacher.SpawnPointType.Monster).Select(p => p.GetComponent<MonsterAttacher>()).ToList();
        }

        private List<Transform> GetSpawnPoints(IEnumerable<Attacher> spawnPoints, Attacher.SpawnPointType type)
        {
            return spawnPoints
                .Where(point => point.type == type)
                .Select(p => p.transform)
                .ToList();
        }
    }
}