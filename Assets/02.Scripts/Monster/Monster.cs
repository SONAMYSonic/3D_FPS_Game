using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Monster : MonoBehaviour
{
    public EMonsterState State { get; private set; } = EMonsterState.Idle;

    [SerializeField] private PlayerHit _player;
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private Vector3 _initialPosition;
    [SerializeField] private Animator _animator;

    [Header("탐지 및 공격")]
    public float DetectDistance = 4f;
    public float AttackDistance = 1.2f;
    [Tooltip("이 거리 이상 멀어지면 추적 포기하고 복귀")]
    public float ComebackDistance = 15f;

    [Header("이동")]
    [Tooltip("추적(Trace) 시 이동 속도 - Run 애니메이션")]
    public float MoveSpeed = 3f;
    
    [Header("공격")]
    public float AttackSpeed = 2f;
    public float AttackTimer = 0f;
    private bool _isAttacking = false;

    [Header("체력 및 데미지")]
    [SerializeField] private ConsumableStat _health;
    public float MonsterDamage = 10f;

    public float CurrentHealth => _health.Value;
    public float MaxHealth => _health.MaxValue;
    public float HealthRatio => _health.Value / _health.MaxValue;

    [Header("넉백")]
    public float KnockbackForce = 5f;
    public float KnockbackDuration = 0.2f;

    [Header("순찰")]
    [Tooltip("순찰 범위 - 초기 위치 기준")]
    public float PatrolSquareRange = 10f;
    [Tooltip("순찰(Patrol) 시 이동 속도 - Walk 애니메이션")]
    public float PatrolSpeed = 1.5f;

    [Header("대기")]
    public float IdleDurationMin = 2f;
    public float IdleDurationMax = 5f;

    [Header("기타")]
    [SerializeField] private float _arrivalThreshold = 0.5f;
    [SerializeField] private float _deathDuration = 2.0f;

    private Coroutine _idleCoroutine;
    private Coroutine _patrolCoroutine;
    private Coroutine _hitCoroutine;

    private EMonsterState _previousState = EMonsterState.Idle;

    private Vector3 PlayerPosition => _player.Position;

    private void Awake()
    {
        _health.Initialize();
        _animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        _initialPosition = transform.position;
        _agent.speed = MoveSpeed;
        _agent.stoppingDistance = AttackDistance;
        EnterState(State);
    }

    private void Update()
    {
        if (GameManager.Instance.State != EGameState.Playing) return;

        switch (State)
        {
            case EMonsterState.Idle:
                UpdateIdle();
                break;
            case EMonsterState.Trace:
                UpdateTrace();
                break;
            case EMonsterState.Comeback:
                UpdateComeback();
                break;
            case EMonsterState.Attack:
                UpdateAttack();
                break;
            case EMonsterState.Patrol:
                UpdatePatrol();
                break;
        }
    }

    #region State Machine

    private void ChangeState(EMonsterState newState)
    {
        if (State == newState) return;

        Debug.Log($"상태 전환: {State} → {newState}");

        PlayTransitionAnimation(State, newState);
        ExitState(State);

        _previousState = State;
        State = newState;

        EnterState(newState);
    }

private void PlayTransitionAnimation(EMonsterState from, EMonsterState to)
    {
        // CrossFadeInFixedTime으로 직접 State 전환 (Trigger 대신)
        string stateName = GetAnimationStateName(to);
        if (!string.IsNullOrEmpty(stateName))
        {
            _animator.CrossFadeInFixedTime(stateName, 0.1f);
            Debug.Log($"애니메이션 전환: {stateName}");
        }
    }

private string GetAnimationStateName(EMonsterState state)
    {
        switch (state)
        {
            case EMonsterState.Idle: return "Idle";
            case EMonsterState.Patrol: return "Walk";
            case EMonsterState.Comeback: return "Walk";
            case EMonsterState.Trace: return "Trace";
            case EMonsterState.Attack: return "Attack";
            case EMonsterState.Hit: return "Hit";
            case EMonsterState.Death: return "Hit"; // Death 애니메이션이 없으면 Hit 사용
            default: return null;
        }
    }

    private void ExitState(EMonsterState exitingState)
    {
        switch (exitingState)
        {
            case EMonsterState.Idle:
                StopCoroutineSafe(ref _idleCoroutine);
                break;
            case EMonsterState.Patrol:
                StopCoroutineSafe(ref _patrolCoroutine);
                break;
            case EMonsterState.Hit:
                StopCoroutineSafe(ref _hitCoroutine);
                _agent.enabled = true;
                break;
            case EMonsterState.Attack:
                AttackTimer = 0f;
                _isAttacking = false;
                break;
        }
    }

    private void EnterState(EMonsterState enteringState)
    {
        switch (enteringState)
        {
            case EMonsterState.Idle:
                _agent.ResetPath();
                float idleTime = Random.Range(IdleDurationMin, IdleDurationMax);
                _idleCoroutine = StartCoroutine(IdleCoroutine(idleTime));
                break;

            case EMonsterState.Trace:
                _agent.speed = MoveSpeed;
                _agent.stoppingDistance = AttackDistance;
                break;

            case EMonsterState.Patrol:
                _agent.speed = PatrolSpeed;
                _agent.stoppingDistance = 0.1f;
                Vector3 patrolTarget = GetValidPatrolTarget();
                if (patrolTarget != Vector3.zero)
                {
                    _patrolCoroutine = StartCoroutine(PatrolCoroutine(patrolTarget));
                }
                else
                {
                    Debug.LogWarning("순찰 목적지를 찾지 못함. Idle로 전환.");
                    _idleCoroutine = StartCoroutine(IdleCoroutine(1f));
                }
                break;

            case EMonsterState.Comeback:
                _agent.speed = PatrolSpeed;
                _agent.stoppingDistance = 0.1f;
                break;

            case EMonsterState.Attack:
                _agent.ResetPath();
                _isAttacking = false;
                break;

            case EMonsterState.Hit:
                _animator.CrossFadeInFixedTime("Hit", 0.1f);
                _agent.ResetPath();
                _agent.enabled = false;
                break;

            case EMonsterState.Death:
                _agent.enabled = false;
                StopAllCoroutines();
                StartCoroutine(DeathCoroutine());
                break;
        }
    }



    #endregion

    #region State Updates

    private void UpdateIdle()
    {
        if (GetDistanceToPlayer() <= DetectDistance)
        {
            Debug.Log("플레이어 발견! 추적 시작!");
            ChangeState(EMonsterState.Trace);
        }
    }

private void UpdateTrace()
    {
        float distance = GetDistanceToPlayer();
        
        // 플레이어가 너무 멀어지면 복귀
        if (distance > ComebackDistance)
        {
            Debug.Log("플레이어가 너무 멀어짐. 복귀 시작.");
            ChangeState(EMonsterState.Comeback);
            return;
        }
        
        _agent.SetDestination(PlayerPosition);

        if (distance <= AttackDistance)
        {
            ChangeState(EMonsterState.Attack);
        }
    }

    private void UpdateComeback()
    {
        if (GetDistanceToPlayer() <= DetectDistance)
        {
            ChangeState(EMonsterState.Trace);
            return;
        }

        _agent.SetDestination(_initialPosition);

        if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance + 0.1f)
        {
            ChangeState(EMonsterState.Idle);
        }
    }

    private void UpdateAttack()
    {
        _agent.ResetPath();

        Vector3 lookDir = GetDirectionToPlayer();
        if (lookDir != Vector3.zero)
        {
            lookDir.y = 0;
            transform.rotation = Quaternion.LookRotation(lookDir);
        }

        float distance = GetDistanceToPlayer();
        if (distance > AttackDistance * 1.5f)
        {
            ChangeState(EMonsterState.Trace);
            return;
        }

        AttackTimer += Time.deltaTime;
        if (AttackTimer >= AttackSpeed && !_isAttacking)
        {
            AttackTimer = 0f;
            _isAttacking = true;
            _animator.CrossFadeInFixedTime("Attack", 0.1f);
        }
    }

    private void UpdatePatrol()
    {
        if (GetDistanceToPlayer() <= DetectDistance)
        {
            ChangeState(EMonsterState.Trace);
        }
    }

    #endregion

    #region Animation Events

    public void OnAttackAnimationEnd()
    {
        _isAttacking = false;
    }

    #endregion

    #region Damage & Coroutines

    public bool TryTakeDamage(float damage, Vector3 hitDirection)
    {
        if (State == EMonsterState.Death) return false;

        _health.Decrease(damage);

        if (_agent.enabled && _agent.isOnNavMesh)
        {
            _agent.isStopped = true;
            _agent.ResetPath();
        }

        if (_health.Value <= 0)
        {
            ChangeState(EMonsterState.Death);
            return true;
        }

        ChangeState(EMonsterState.Hit);
        _hitCoroutine = StartCoroutine(HitCoroutine(hitDirection));
        return true;
    }

    private IEnumerator HitCoroutine(Vector3 hitDirection)
    {
        float timer = 0f;
        hitDirection.y = 0f;
        hitDirection.Normalize();

        while (timer < KnockbackDuration)
        {
            transform.position += hitDirection * KnockbackForce * Time.deltaTime;
            timer += Time.deltaTime;
            yield return null;
        }

        if (GetDistanceToPlayer() <= DetectDistance)
        {
            ChangeState(EMonsterState.Trace);
        }
        else
        {
            ChangeState(EMonsterState.Idle);
        }
    }

    private IEnumerator DeathCoroutine()
    {
        yield return new WaitForSeconds(_deathDuration);
        Destroy(gameObject);
    }

private IEnumerator PatrolCoroutine(Vector3 target)
    {
        _agent.SetDestination(target);

        float timeout = 4f; // 2초 타임아웃
        float timer = 0f;

        while (timer < timeout)
        {
            if (State != EMonsterState.Patrol) yield break;
            
            // 도착 판정: 속도가 거의 0이거나 목적지 근처
            bool hasArrived = !_agent.pathPending && 
                              (_agent.remainingDistance <= 0.3f || _agent.velocity.magnitude < 0.1f);
            
            if (hasArrived && timer > 0.3f) // 최소 0.3초 후 판정
            {
                break;
            }
            
            timer += Time.deltaTime;
            yield return null;
        }

        if (State == EMonsterState.Patrol)
        {
            ChangeState(EMonsterState.Idle);
        }
    }

    private IEnumerator IdleCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        if (State == EMonsterState.Idle)
        {
            ChangeState(EMonsterState.Patrol);
        }
    }

    #endregion

    #region Helpers

    private Vector3 GetValidPatrolTarget()
    {
        int maxAttempts = 10;
        float sampleRadius = 2f;

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3 randomOffset = new Vector3(
                Random.Range(-PatrolSquareRange, PatrolSquareRange),
                0,
                Random.Range(-PatrolSquareRange, PatrolSquareRange)
            );
            Vector3 candidatePosition = _initialPosition + randomOffset;

            if (NavMesh.SamplePosition(candidatePosition, out NavMeshHit hit, sampleRadius, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }

        return Vector3.zero;
    }

    private float GetDistanceToPlayer()
    {
        return Vector3.Distance(transform.position, PlayerPosition);
    }

    private Vector3 GetDirectionToPlayer()
    {
        return (PlayerPosition - transform.position).normalized;
    }

    private bool HasArrivedAt(Vector3 target)
    {
        return Vector3.Distance(transform.position, target) <= _arrivalThreshold;
    }

    private void StopCoroutineSafe(ref Coroutine coroutine)
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
    }

    #endregion
}