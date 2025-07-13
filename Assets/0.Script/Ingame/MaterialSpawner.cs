using System.Collections.Generic;
using Fusion;
using UnityEngine;
using Jamcat.Core;

namespace Jamcat.Ingame
{
    public class MaterialSpawner : MonoBehaviour
    {
        [System.Serializable]
        public class MaterialSpawnData
        {
            public string prefabName;
            public int spawnCount;
            public float minDistance;
            public float maxDistance;
            public AnimationCurve probabilityCurve = AnimationCurve.Linear(0, 1, 1, 0);
        }

        [Header("Spawn Settings")]
        [SerializeField] private float spawnRadius = 250f;
        [SerializeField] private LayerMask buildingLayers = -1;
        [SerializeField] private float minBuildingHeight = 5f;
        [SerializeField] private float maxRaycastDistance = 500f;

        [Header("Material Settings")]
        [SerializeField] private MaterialSpawnData[] materialData;

        private NetworkRunner _runner;
        private List<Vector3> _spawnedPositions = new List<Vector3>();
        private readonly float _minSpawnDistance = 3f;

        private void Awake()
        {
            if (materialData == null || materialData.Length == 0)
            {
                materialData = new MaterialSpawnData[]
                {
                    new MaterialSpawnData 
                    { 
                        prefabName = "Material10", 
                        spawnCount = 50, 
                        minDistance = 0f, 
                        maxDistance = 100f,
                        probabilityCurve = AnimationCurve.Linear(0, 1, 1, 0.2f)
                    },
                    new MaterialSpawnData 
                    { 
                        prefabName = "Material30", 
                        spawnCount = 30, 
                        minDistance = 50f, 
                        maxDistance = 150f,
                        probabilityCurve = AnimationCurve.Linear(0, 0.5f, 1, 0.8f)
                    },
                    new MaterialSpawnData 
                    { 
                        prefabName = "Material100", 
                        spawnCount = 30, 
                        minDistance = 100f, 
                        maxDistance = 200f,
                        probabilityCurve = AnimationCurve.Linear(0, 0.3f, 1, 1f)
                    },
                    new MaterialSpawnData 
                    { 
                        prefabName = "Material300", 
                        spawnCount = 20, 
                        minDistance = 150f, 
                        maxDistance = 250f,
                        probabilityCurve = AnimationCurve.Linear(0, 0.1f, 1, 1f)
                    }
                };
            }
        }

        public void Initialize(NetworkRunner gameRunner)
        {
            _runner = gameRunner;
            SpawnAllMaterials();
        }

        private void SpawnAllMaterials()
        {
            if (_runner == null)
            {
                Debug.LogError("NetworkRunner가 설정되지 않았습니다.");
                return;
            }

            foreach (var data in materialData)
            {
                SpawnMaterialType(data);
            }
        }

        private void SpawnMaterialType(MaterialSpawnData data)
        {
            int spawned = 0;
            int attempts = 0;
            int maxAttempts = data.spawnCount * 10;

            while (spawned < data.spawnCount && attempts < maxAttempts)
            {
                attempts++;
                
                Vector3 spawnPosition = GetRandomSpawnPosition(data);
                if (spawnPosition != Vector3.zero && IsValidSpawnPosition(spawnPosition))
                {
                    if (SpawnMaterial(data.prefabName, spawnPosition))
                    {
                        _spawnedPositions.Add(spawnPosition);
                        spawned++;
                    }
                }
            }

            Debug.Log($"{data.prefabName}: {spawned}/{data.spawnCount} 스폰 완료 (시도: {attempts})");
        }

        private Vector3 GetRandomSpawnPosition(MaterialSpawnData data)
        {
            // 거리 기반 확률 계산
            float randomDistance = Random.Range(0f, spawnRadius);
            float distanceNormalized = randomDistance / spawnRadius;
            
            // 확률 곡선을 사용하여 거리별 스폰 확률 적용
            float probability = data.probabilityCurve.Evaluate(distanceNormalized);
            
            // 해당 material의 거리 범위 내에 있는지 확인
            if (randomDistance < data.minDistance || randomDistance > data.maxDistance)
            {
                // 확률을 낮춤
                probability *= 0.1f;
            }

            if (Random.value > probability)
            {
                return Vector3.zero; // 확률에 통과하지 못함
            }

            // 원형 범위 내에서 무작위 위치 생성
            Vector2 randomCircle = Random.insideUnitCircle * randomDistance;
            Vector3 basePosition = new Vector3(randomCircle.x, 0, randomCircle.y);

            // 건물 외벽이나 옥상에 스폰할 위치 찾기
            return FindBuildingSpawnPosition(basePosition);
        }

        private Vector3 FindBuildingSpawnPosition(Vector3 basePosition)
        {
            // 위에서 아래로 레이캐스트하여 건물 찾기
            Vector3 rayStart = basePosition + Vector3.up * maxRaycastDistance;
            
            RaycastHit[] hits = Physics.RaycastAll(rayStart, Vector3.down, maxRaycastDistance, buildingLayers);
            
            if (hits.Length == 0) return Vector3.zero;

            // 가장 높은 건물 찾기
            RaycastHit bestHit = hits[0];
            float maxHeight = 0f;

            foreach (var hit in hits)
            {
                if (hit.point.y > maxHeight)
                {
                    maxHeight = hit.point.y;
                    bestHit = hit;
                }
            }

            // 최소 건물 높이 체크
            if (maxHeight < minBuildingHeight)
            {
                return Vector3.zero;
            }

            // 옥상 또는 외벽에 스폰 위치 결정
            Vector3 spawnPos = bestHit.point;
            
            // 수직 면인지 확인 (외벽)
            if (Vector3.Dot(bestHit.normal, Vector3.up) < 0.7f)
            {
                // 외벽에 약간 떨어져서 스폰
                spawnPos += bestHit.normal * 0.5f;
            }
            else
            {
                // 옥상에 스폰
                spawnPos += Vector3.up * 0.5f;
            }

            return spawnPos;
        }

        private bool IsValidSpawnPosition(Vector3 position)
        {
            // 다른 스폰된 오브젝트와 최소 거리 유지
            foreach (var existingPos in _spawnedPositions)
            {
                if (Vector3.Distance(position, existingPos) < _minSpawnDistance)
                {
                    return false;
                }
            }

            // 플레이어 스폰 지점과 너무 가깝지 않은지 확인
            if (Vector3.Distance(position, Vector3.zero) < 10f)
            {
                return false;
            }

            return true;
        }

        private bool SpawnMaterial(string prefabName, Vector3 position)
        {
            try
            {
                var prefab = Loader.LoadPrefab<NetworkObject>(Loader.ResourceType.Items, prefabName);
                if (prefab == null)
                {
                    Debug.LogError($"프리팹을 찾을 수 없습니다: {prefabName}");
                    return false;
                }

                var spawnedObject = _runner.Spawn(prefab, position, Quaternion.identity);
                if (spawnedObject != null)
                {
                    var item = spawnedObject.GetComponent<Item.Item>();
                    item?.Init();
                    return true;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Material 스폰 실패 {prefabName}: {e.Message}");
            }

            return false;
        }

        private void OnDrawGizmosSelected()
        {
            // 스폰 범위 시각화
            Gizmos.color = Color.yellow;
            DrawWireCircle(transform.position, spawnRadius);

            // 각 material의 거리 범위 시각화
            if (materialData != null)
            {
                Color[] colors = { Color.green, Color.blue, Color.red, Color.magenta };
                for (int i = 0; i < materialData.Length && i < colors.Length; i++)
                {
                    var data = materialData[i];
                    Gizmos.color = colors[i];
                    DrawWireCircle(transform.position, data.minDistance);
                    DrawWireCircle(transform.position, data.maxDistance);
                }
            }

            // 스폰된 위치들 시각화
            Gizmos.color = Color.white;
            foreach (var pos in _spawnedPositions)
            {
                Gizmos.DrawWireSphere(pos, 0.5f);
            }
        }

        private void DrawWireCircle(Vector3 center, float radius)
        {
            int segments = 64;
            float angleStep = 360f / segments;
            Vector3 prevPoint = center + Vector3.forward * radius;
            
            for (int i = 1; i <= segments; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                Vector3 currentPoint = center + new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle)) * radius;
                Gizmos.DrawLine(prevPoint, currentPoint);
                prevPoint = currentPoint;
            }
        }
    }
}