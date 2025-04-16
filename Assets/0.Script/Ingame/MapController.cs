using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Jamcat.Core;
using Jamcat.Ingame.Controllers.Component;
using UnityEngine;
using Util;

namespace Jamcat.Ingame
{
    public class MapController : MonoBehaviour
    {
        private List<Transform> _avatarSpawnPoints;
        
        public Transform GetSpawnPosition(int index)
        {
            return _avatarSpawnPoints[index];
        }
        
        private void Awake()
        {
            var map = Loader.LoadResource<GameObject>(Loader.ResourceType.Maps, InGame.MapName);
            Instantiate(map).transform.SetParent(transform);
            
            var spawnPoints = GetComponentsInChildren<Attacher>();
            _avatarSpawnPoints = GetSpawnPoints(spawnPoints, Attacher.SpawnPointType.Character);
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