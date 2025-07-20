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
            public float minHeight;
            public float maxHeight;
            public AnimationCurve probabilityCurve = AnimationCurve.Linear(0, 1, 1, 0);
            public AnimationCurve heightProbabilityCurve = AnimationCurve.Linear(0, 1, 1, 1);
        }

        [Header("Spawn Settings")]
        [SerializeField] private float spawnRadius = 250f;
        [SerializeField] private LayerMask buildingLayers = -1;
        [SerializeField] private float minBuildingHeight = 5f;
        [SerializeField] private float maxRaycastDistance = 500f;
        [SerializeField] private float maxBuildingHeight = 100f;

        [Header("Material Settings")]
        [SerializeField] private MaterialSpawnData[] materialData;

        private NetworkRunner _runner;
        private List<Vector3> _spawnedPositions = new List<Vector3>();
        private readonly float _minSpawnDistance = 3f;

        private void Awake()
        {
            if (materialData == null || materialData.Length == 0)
            {
                materialData = new[]
                {
                    new MaterialSpawnData 
                    { 
                        prefabName = "Material10", 
                        spawnCount = 50, 
                        minDistance = 0f, 
                        maxDistance = 100f,
                        minHeight = 0f,
                        maxHeight = 20f,
                        probabilityCurve = AnimationCurve.Linear(0, 1, 1, 0.2f),
                        heightProbabilityCurve = AnimationCurve.Linear(0, 1, 1, 0.1f)
                    },
                    new MaterialSpawnData 
                    { 
                        prefabName = "Material30", 
                        spawnCount = 30, 
                        minDistance = 50f, 
                        maxDistance = 150f,
                        minHeight = 5f,
                        maxHeight = 40f,
                        probabilityCurve = AnimationCurve.Linear(0, 0.5f, 1, 0.8f),
                        heightProbabilityCurve = AnimationCurve.Linear(0, 0.7f, 1, 0.6f)
                    },
                    new MaterialSpawnData 
                    { 
                        prefabName = "Material100", 
                        spawnCount = 30, 
                        minDistance = 100f, 
                        maxDistance = 200f,
                        minHeight = 15f,
                        maxHeight = 70f,
                        probabilityCurve = AnimationCurve.Linear(0, 0.3f, 1, 1f),
                        heightProbabilityCurve = AnimationCurve.Linear(0, 0.4f, 1, 0.9f)
                    },
                    new MaterialSpawnData 
                    { 
                        prefabName = "Material300", 
                        spawnCount = 20, 
                        minDistance = 150f, 
                        maxDistance = 250f,
                        minHeight = 30f,
                        maxHeight = 100f,
                        probabilityCurve = AnimationCurve.Linear(0, 0.1f, 1, 1f),
                        heightProbabilityCurve = AnimationCurve.Linear(0, 0.2f, 1, 1f)
                    }
                };
            }
        }

        public void Initialize(NetworkRunner gameRunner)
        {
            _runner = gameRunner;
            
            // 서버에서만 머터리얼 스폰 실행
            if (_runner.IsServer)
            {
                SpawnAllMaterials();
            }
        }

        private void SpawnAllMaterials()
        {
            if (_runner == null)
            {
                Debug.LogError("NetworkRunner가 설정되지 않았습니다.");
                return;
            }

            // 서버에서만 실행
            if (!_runner.IsServer)
            {
                Debug.LogWarning("MaterialSpawner: 서버가 아닌 클라이언트에서 SpawnAllMaterials 호출됨");
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
            float distanceProbability = data.probabilityCurve.Evaluate(distanceNormalized);
            
            // 해당 material의 거리 범위 내에 있는지 확인
            if (randomDistance < data.minDistance || randomDistance > data.maxDistance)
            {
                // 확률을 낮춤
                distanceProbability *= 0.1f;
            }

            if (Random.value > distanceProbability)
            {
                return Vector3.zero; // 거리 확률에 통과하지 못함
            }

            // 원형 범위 내에서 무작위 위치 생성
            Vector2 randomCircle = Random.insideUnitCircle * randomDistance;
            Vector3 basePosition = new Vector3(randomCircle.x, 0, randomCircle.y);

            // 건물 외벽이나 옥상에 스폰할 위치 찾기
            Vector3 spawnPosition = FindBuildingSpawnPosition(basePosition, data);
            
            return spawnPosition;
        }

        private Vector3 FindBuildingSpawnPosition(Vector3 basePosition, MaterialSpawnData data)
        {
            // 위에서 아래로 레이캐스트하여 건물 찾기
            Vector3 rayStart = basePosition + Vector3.up * maxRaycastDistance;
            
            RaycastHit[] hits = Physics.RaycastAll(rayStart, Vector3.down, maxRaycastDistance, buildingLayers);
            
            if (hits.Length == 0) return Vector3.zero;

            // 적절한 높이의 건물들 필터링
            List<RaycastHit> validHits = new List<RaycastHit>();
            foreach (var hit in hits)
            {
                float height = hit.point.y;
                
                // 최소/최대 건물 높이 체크
                if (height < minBuildingHeight || height > maxBuildingHeight) continue;
                
                // material별 높이 범위 체크
                if (height >= data.minHeight && height <= data.maxHeight)
                {
                    validHits.Add(hit);
                }
                // 범위 밖이라도 낮은 확률로 포함
                else if (Random.value < 0.1f)
                {
                    validHits.Add(hit);
                }
            }

            if (validHits.Count == 0) return Vector3.zero;

            // 높이별 확률 적용하여 히트 선택
            RaycastHit selectedHit = SelectHitByHeightProbability(validHits, data);
            
            // 옥상 또는 외벽에 스폰 위치 결정
            Vector3 spawnPos = selectedHit.point;
            
            // 수직 면인지 확인 (외벽)
            if (Vector3.Dot(selectedHit.normal, Vector3.up) < 0.7f)
            {
                // 외벽에 약간 떨어져서 스폰
                spawnPos += selectedHit.normal * 0.5f;
            }
            else
            {
                // 옥상에 스폰
                spawnPos += Vector3.up * 0.5f;
            }

            return spawnPos;
        }

        private RaycastHit SelectHitByHeightProbability(List<RaycastHit> hits, MaterialSpawnData data)
        {
            if (hits.Count == 1) return hits[0];

            // 높이별 확률 가중치 계산
            float[] weights = new float[hits.Count];
            float totalWeight = 0f;

            for (int i = 0; i < hits.Count; i++)
            {
                float height = hits[i].point.y;
                float heightNormalized = Mathf.Clamp01((height - data.minHeight) / (data.maxHeight - data.minHeight));
                
                // 높이 확률 곡선 적용
                weights[i] = data.heightProbabilityCurve.Evaluate(heightNormalized);
                totalWeight += weights[i];
            }

            // 가중치 기반 랜덤 선택
            float randomValue = Random.value * totalWeight;
            float currentWeight = 0f;

            for (int i = 0; i < hits.Count; i++)
            {
                currentWeight += weights[i];
                if (randomValue <= currentWeight)
                {
                    return hits[i];
                }
            }

            return hits[hits.Count - 1]; // 폴백
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
            // 서버에서만 실행
            if (!_runner.IsServer)
            {
                Debug.LogError("MaterialSpawner: 서버가 아닌 클라이언트에서 SpawnMaterial 호출됨");
                return false;
            }

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
                    
                    // 높이 범위 시각화 (수직 원통 형태)
                    DrawHeightRange(transform.position, data.minHeight, data.maxHeight, colors[i]);
                }
            }

            // 스폰된 위치들 시각화
            Gizmos.color = Color.white;
            foreach (var pos in _spawnedPositions)
            {
                Gizmos.DrawWireSphere(pos, 0.5f);
            }
        }

        private void DrawHeightRange(Vector3 center, float minHeight, float maxHeight, Color color)
        {
            Gizmos.color = color;
            
            // 최소 높이 평면
            Vector3 minHeightPos = center + Vector3.up * minHeight;
            DrawWireCircle(minHeightPos, 10f);
            
            // 최대 높이 평면
            Vector3 maxHeightPos = center + Vector3.up * maxHeight;
            DrawWireCircle(maxHeightPos, 10f);
            
            // 수직선들
            for (int i = 0; i < 8; i++)
            {
                float angle = i * 45f * Mathf.Deg2Rad;
                Vector3 direction = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * 10f;
                Vector3 bottomPoint = minHeightPos + direction;
                Vector3 topPoint = maxHeightPos + direction;
                Gizmos.DrawLine(bottomPoint, topPoint);
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