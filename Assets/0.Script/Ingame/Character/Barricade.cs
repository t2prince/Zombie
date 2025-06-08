using UnityEngine;
using UnityEngine.AI;

namespace Jamcat.Ingame.Character
{
    public class Barricade : Building
    {
        private NavMeshObstacle _navMeshObstacle;
        
        protected override void Init()
        {
            base.Init();
            
            // NavMeshObstacle 컴포넌트 추가 또는 가져오기
            _navMeshObstacle = GetComponent<NavMeshObstacle>();
            if (_navMeshObstacle == null)
            {
                _navMeshObstacle = gameObject.AddComponent<NavMeshObstacle>();
            }
            
            // NavMeshObstacle 설정
            _navMeshObstacle.carving = true;
            _navMeshObstacle.shape = NavMeshObstacleShape.Box;
            
            // Collider를 기반으로 크기 설정
            var collider = GetComponent<Collider>();
            if (collider != null)
            {
                _navMeshObstacle.size = collider.bounds.size;
                _navMeshObstacle.center = collider.bounds.center - transform.position;
            }
            else
            {
                // 기본 크기 설정
                _navMeshObstacle.size = new Vector3(2f, 2f, 0.5f);
            }
        }
        
        protected override void Die()
        {
            // 바리케이트가 파괴되면 NavMeshObstacle도 비활성화
            if (_navMeshObstacle != null)
            {
                _navMeshObstacle.enabled = false;
            }
            
            base.Die();
        }
    }
}