using System;
using System.Collections;
using System.Collections.Generic;
using Script.Util;
using UnityEngine.AI;
using UnityEngine;
using UnityEngine.Serialization;
using Util;

namespace Jamcat.Ingame.Character
{
    public class Monster: BaseCharacter
    {
        private NavMeshAgent _agent;
        private BaseCharacter _target;
        private BaseCharacter _mainTarget; // 메인 타겟 (셔틀 등)
        private bool _hasMainTarget = false; // Attacher를 통해 소환되어 메인 타겟이 있는지 여부
        private Rigidbody _rigidbody;
        private Collider _collider;
        private Animator _animator;
        private AIState _aiState = AIState.Idle;
        

        private AIState State
        {
            get => _aiState;
            set
            {
                if(_aiState == value) return;
                _aiState = value;
                switch (_aiState)
                {
                    case AIState.Idle:
                        StopMove();
                        StopAttack();
                        _animationState = AnimationState.Idle;
                        break;
                    case AIState.Move:
                        StartMove();
                        StopAttack();
                        _animationState = AnimationState.Walk;
                        break;
                    case AIState.Attack:
                        StopMove();
                        StartAttack();
                        _animationState = AnimationState.Attack;
                        break;
                    case AIState.Die:
                        StopMove();
                        _animationState = AnimationState.Die; 
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        
        private Dictionary<BaseCharacter,float> _aggro = new Dictionary<BaseCharacter,float>();
        [SerializeField] private float attackRange = 2.0f;
        [SerializeField] private float attackPower = 10f;
        [SerializeField] private float attackDelay = 1f;
        [SerializeField] private float attackInterval = 5f;
        [SerializeField] private bool isAggressive = false;
        [SerializeField] private WalkType walkType;
        
        public enum WalkType
        {
            Walk,
            Crawling,
        }
        
        public enum AnimationState
        {
            Idle,
            Walk,
            Attack,
            KnockBack,
            Die,
        }
        
        public enum AIState
        {
            Idle,
            Move,
            Attack,
            Die
        }

        private AnimationState _animationState
        {
            set
            {
                if (_animator == null) return;

                switch (value)
                {
                    case AnimationState.Idle:
                        _animator.SetTrigger("Idle");
                        break;
                    case AnimationState.Walk:
                        _animator.SetTrigger(walkType == WalkType.Crawling ? "Crawling" : "Walk001");
                        break;
                    case AnimationState.Attack:
                        _animator.SetTrigger(walkType == WalkType.Crawling ? "CrawlingAttack" : "Attack");
                        break;
                    case AnimationState.KnockBack:
                        _animator.SetTrigger("Injured");
                        break;
                    case AnimationState.Die:
                        _animator.SetTrigger("Die");
                        break;
                }
            } 
        }
        

        protected override void Init()
        {
            base.Init();
            _agent = GetComponent<NavMeshAgent>();
            _rigidbody = GetComponent<Rigidbody>();
            _animator = GetComponent<Animator>();
            _collider = GetComponent<Collider>();
            
            // Rigidbody가 있다면 isKinematic 설정
            if (_rigidbody != null)
            {
                _rigidbody.isKinematic = true;
            }

            // NavMeshAgent 기본 설정 확인
            if (_agent != null)
            {
                _agent.isStopped = false;
            }
        }

        public void Spawn()
        {
            gameObject.SetActive(true);
        }

        public override void TakeDamage(BaseCharacter attacker, float damage, float knockBackPower = 5.0f)
        {
            base.TakeDamage(attacker, damage);
            if (!_aggro.ContainsKey(attacker))
            {
                _aggro.Add(attacker, 0);
            }
            
            _aggro[attacker] += damage;
            if(_target == null || _aggro[attacker] > _aggro[_target])
                SetTarget(attacker);
            
            KnockBack(attacker.transform.position - transform.position, knockBackPower);
        }

        public void SetMainTarget(BaseCharacter target)
        {
            if (!Object.HasStateAuthority) return;
            if (target.IsFastNull()) return;
            
            _mainTarget = target;
            _hasMainTarget = true;
            SetTarget(target);
        }
        
        public void SetTarget(BaseCharacter target)
        {
            if (!Object.HasStateAuthority) return;
            if (target.IsFastNull()) return;
            if(!_aggro.ContainsKey(target))
                _aggro.Add(target, 0);
            
            _target = target;
            _agent.enabled = true;
            _agent.destination = target.transform.position;
            _animationState = AnimationState.Walk;
            _aiState = AIState.Move;
        }

        private void KnockBack(Vector3 dir, float power)
        {
            if (_agent == null) return;

            // NavMeshAgent를 일시적으로 비활성화
            _agent.isStopped = true;
            _agent.updatePosition = false;

            // Rigidbody를 사용해 넉백 적용
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(dir.normalized * power, ForceMode.Impulse);
            }
            
            _animationState = AnimationState.KnockBack; // 넉백 애니메이션 상태로 변경

            // 일정 시간 후 NavMeshAgent 재활성화
            StartCoroutine(ResetAgent());
        }
        
        private IEnumerator ResetAgent()
        {
            yield return new WaitForSeconds(0.5f); // 넉백 후 대기 시간
            if (_agent != null)
            {
                _agent.isStopped = false;
                _agent.updatePosition = true;
            }
        }
        
        private void Update()
        {
            if (!Object.HasStateAuthority) return;

            switch (State)
            {
                case AIState.Idle:
                    Idle();
                    break;
                case AIState.Move:
                    Move();
                    break;
                case AIState.Attack:
                    break;
                case AIState.Die:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override void Die()
        {
            _agent.enabled = false;
            _collider.enabled = false;
            //사망 애니메이션 재생
            _animator.SetTrigger("Die");
            base.Die();
        }

        private void Idle()
        {
            if (isAggressive)
            {
                //주변에서 플레이어 찾아서 공격
            }
        }

        private void StartAttack()
        {
            StartCoroutine(Attack());
        }

        private IEnumerator Attack()
        {
            yield return Util.Coroutine.WaitForSeconds(attackDelay);
            while (transform.InRange(_target.transform, attackRange))
            {
                _target.TakeDamage(this, attackPower);
                yield return Util.Coroutine.WaitForSeconds(attackInterval); 
            }

            State = AIState.Move;
        }
        
        private void StopAttack()
        {
            StopAllCoroutines();
        }

        private void Move()
        {
            if (_target.IsFastNull())
            {
                // 메인 타겟이 있는 몬스터만 메인 타겟으로 돌아가기
                if (_hasMainTarget && _mainTarget != null && !_mainTarget.IsFastNull())
                {
                    SetTarget(_mainTarget);
                    return;
                }
                
                State = AIState.Idle;
                return;
            }

            // 메인 타겟이 있는 몬스터만 바리케이트 파괴 후 복귀 로직 사용
            if (_hasMainTarget)
            {
                // 현재 타겟이 바리케이트이고 파괴되었는지 확인
                var currentBuilding = _target.GetComponent<Building>();
                if (currentBuilding != null && !currentBuilding.gameObject.activeInHierarchy)
                {
                    // 바리케이트가 파괴되었으면 메인 타겟으로 돌아가기
                    if (_mainTarget != null && !_mainTarget.IsFastNull())
                    {
                        SetTarget(_mainTarget);
                        return;
                    }
                }
            }

            if (transform.InRange(_target.transform, attackRange))
            {
                State = AIState.Attack;
            }
            else
            {
                _agent.destination = _target.transform.position;
                
                // 메인 타겟이 있는 몬스터만 바리케이트 우회 로직 사용
                if (_hasMainTarget)
                {
                    CheckForBarricadeObstacle();
                }
            }
        }
        
        private void CheckForBarricadeObstacle()
        {
            // 현재 타겟이 이미 바리케이트인 경우는 체크하지 않음
            if (_target.GetComponent<Building>() != null) return;
            
            // NavMeshAgent가 목적지에 도달할 수 없는 경우 체크
            if (_agent.pathStatus == NavMeshPathStatus.PathPartial || 
                (_agent.hasPath && _agent.remainingDistance < 0.5f && !transform.InRange(_target.transform, attackRange * 2)))
            {
                // 주변의 바리케이트 찾기
                Collider[] obstacles = Physics.OverlapSphere(transform.position, attackRange * 2);
                foreach (var obstacle in obstacles)
                {
                    var building = obstacle.GetComponent<Building>();
                    if (building != null && building.gameObject.activeInHierarchy)
                    {
                        // 건물(바리케이트)을 새로운 타겟으로 설정
                        SetTarget(building);
                        break;
                    }
                }
            }
        }

        private void StopMove()
        {
            _agent.isStopped = true;
            _agent.updatePosition = false;
        }
        
        private void StartMove()
        {
            if (_target == null || _target.IsFastNull()) return;
            _agent.isStopped = false;
            _agent.updatePosition = true;
            _agent.destination = _target.transform.position;
            _animationState = AnimationState.Walk;
        }
    }
}