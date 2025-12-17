using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Monster : MonoBehaviour
{
    #region Comment
    // 목표: 처음에는 가만히 있지만 플레이어가 다가가면 쫓아오는 좀비 몬스터를 만들고 싶다.
    //       ㄴ 쫓아 오다가 너무 멀어지면 제자리로 돌아간다.

    // Idle   : 가만히 있는다.
    //   I  (플레이어가 가까이 오면) (컨디션, 트랜지션)
    // Trace  : 플레이러를 쫒아간다.
    //   I  (플레이어와 너무 멀어지면)
    // Return : 제자리로 돌아가는 상태
    //   I  (제자리에 도착했다면)
    //  Idle
    // 공격
    // 피격
    // 죽음



    // 몬스터 인공지능(AI) : 사람처럼 행동하는 똑똑한 시스템/알고리즘
    // - 규칙 기반 인공지능 : 정해진 규칙에 따라 조건문/반복문등을 이용해서 코딩하는 것
    //                     -> ex) FSM(유한 상태머신), BT(행동 트리)
    // - 학습 기반 인공지능: 머신러닝(딥러닝, 강화학습 .. )

    // Finite State Machine(유한 상태 머신)
    // N개의 상태를 가지고 있고, 상태마다 행동이 다르다.


    #endregion

    public EMonsterState State { get; private set; } = EMonsterState.Idle;

    // GameObject 대신 PlayerHit 컴포넌트를 직접 참조 (디미터 법칙 준수)
    [SerializeField] private PlayerHit _player;
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private Vector3 _initialPosition;

    [Header("탐지 및 공격")]
    public float DetectDistance = 4f;
    public float AttackDistance = 1.2f;

    [Header("이동")]
    public float MoveSpeed = 5f;
    
    [Header("공격")]
    public float AttackSpeed = 2f;
    public float AttackTimer = 0f;

    [Header("체력 및 데미지")]
    [SerializeField] private ConsumableStat _health;
    public float MonsterDamage = 10f;

    // 디미터 법칙 준수: 체력 관련 프로퍼티 직접 노출
    public float CurrentHealth => _health.Value;
    public float MaxHealth => _health.MaxValue;
    public float HealthRatio => _health.Value / _health.MaxValue;

    [Header("넉백")]
    public float KnockbackForce = 5f;
    public float KnockbackDuration = 0.2f;

    [Header("순찰")]
    public float PatrolSquareRange = 3f;
    public float PatrolMinTime = 2f;
    public float PatrolMaxTime = 10f;

    [Header("점프")]
    public float JumpHeight = 2f;           // 점프 최대 높이
    public float JumpDuration = 0.6f;       // 점프 소요 시간

    [Header("대기")]
    public float IdleDurationMin = 2f;
    public float IdleDurationMax = 5f;

    [Header("기타")]
    [SerializeField] private float _arrivalThreshold = 0.5f;    // 목표 지점 도착 판정 거리
    [SerializeField] private float _deathDuration = 2.0f;       // 죽음 상태 지속 시간

    private Vector3 _jumpStartPosition;
    private Vector3 _jumpEndPosition;

    // 코루틴 중복 실행 방지용 핸들
    private Coroutine _idleCoroutine;
    private Coroutine _patrolCoroutine;
    private Coroutine _hitCoroutine;
    private Coroutine _jumpCoroutine;

    // 플레이어 위치 접근용 프로퍼티 (한 곳에서만 transform 접근)
    private Vector3 PlayerPosition => _player.Position;

    private void Awake()
    {
        // ConsumableStat 초기화
        _health.Initialize();
    }

    private void Start()
    {
        _initialPosition = transform.position;
        _agent.speed = MoveSpeed;
        _agent.stoppingDistance = AttackDistance;
        // 초기 상태 진입 처리
        EnterState(State);
    }

    private void Update()
    {
        if (GameManager.Instance.State != EGameState.Playing)
        {
            return;
        }

        // 몬스터의 상태에 따라 다른 행동을한다. (다른 메서드를 호출한다.)
        switch (State)
        {
            case EMonsterState.Idle:
                Idle();
                break;

            case EMonsterState.Trace:
                Trace();
                break;

            case EMonsterState.Comeback:
                Comeback();
                break;
            case EMonsterState.Attack:
                Attack();
                break;
            case EMonsterState.Patrol:
                Patrol();
                break;
        }
    }

    #region 상태 전환 중앙 관리

    /// <summary>
    /// 상태 전환을 중앙에서 관리하는 메서드.
    /// 이전 상태의 코루틴을 정리하고, 새 상태로 전환합니다.
    /// </summary>
    private void ChangeState(EMonsterState newState)
    {
        // 같은 상태로의 전환은 무시
        if (State == newState) return;

        // 이전 상태 정리 (Exit)
        ExitState(State);

        // 새 상태로 전환
        State = newState;

        // 새 상태 진입 (Enter)
        EnterState(newState);
    }

    /// <summary>
    /// 이전 상태를 빠져나올 때 정리 작업 수행
    /// </summary>
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
                // 넉백 종료 후 NavMeshAgent 재활성화
                _agent.enabled = true;
                break;

            case EMonsterState.Attack:
                AttackTimer = 0f;
                break;
            case EMonsterState.Jump:
                StopCoroutineSafe(ref _jumpCoroutine);
                _agent.enabled = true;
                _agent.isStopped = false;
                break;
        }
    }

    /// <summary>
    /// 새 상태에 진입할 때 초기화 작업 수행
    /// </summary>
    private void EnterState(EMonsterState enteringState)
    {
        switch (enteringState)
        {
            case EMonsterState.Idle:
                _agent.ResetPath(); // 이동 중지
                float idleTime = Random.Range(IdleDurationMin, IdleDurationMax);
                _idleCoroutine = StartCoroutine(Idle_Coroutine(idleTime));
                break;

            case EMonsterState.Patrol:
                float patrolTime = Random.Range(PatrolMinTime, PatrolMaxTime);
                Vector3 patrolTarget = _initialPosition + new Vector3(
                    Random.Range(-PatrolSquareRange, PatrolSquareRange),
                    0,
                    Random.Range(-PatrolSquareRange, PatrolSquareRange)
                );
                _patrolCoroutine = StartCoroutine(Patrol_Coroutine(patrolTarget, patrolTime));
                break;

            case EMonsterState.Hit:
                // 넉백 시작 시 NavMeshAgent 비활성화 (직접 이동을 위해)
                _agent.ResetPath();
                _agent.enabled = false;
                break;

            case EMonsterState.Jump:
                _agent.isStopped = true;
                _agent.enabled = false;
                _jumpCoroutine = StartCoroutine(Jump_Coroutine());
                break;

            case EMonsterState.Death:
                _agent.enabled = false;
                StopAllStateCoroutines();
                StartCoroutine(Death_Coroutine());
                break;
        }
    }

    /// <summary>
    /// 코루틴을 안전하게 중지하고 핸들을 null로 초기화
    /// </summary>
    private void StopCoroutineSafe(ref Coroutine coroutine)
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
    }

    /// <summary>
    /// 모든 상태 관련 코루틴을 중지
    /// </summary>
    private void StopAllStateCoroutines()
    {
        StopCoroutineSafe(ref _idleCoroutine);
        StopCoroutineSafe(ref _patrolCoroutine);
        StopCoroutineSafe(ref _hitCoroutine);
        StopCoroutineSafe(ref _jumpCoroutine);
    }

    #endregion

    #region 거리 계산 헬퍼

    /// <summary>
    /// 플레이어와의 거리를 반환
    /// </summary>
    private float GetDistanceToPlayer()
    {
        return Vector3.Distance(transform.position, PlayerPosition);
    }

    /// <summary>
    /// 플레이어를 향하는 방향을 반환
    /// </summary>
    private Vector3 GetDirectionToPlayer()
    {
        return (PlayerPosition - transform.position).normalized;
    }

    /// <summary>
    /// 지정한 위치까지의 거리가 도착 임계값 이하인지 확인
    /// </summary>
    private bool HasArrivedAt(Vector3 target)
    {
        return Vector3.Distance(transform.position, target) <= _arrivalThreshold;
    }

    #endregion

    #region 상태별 Update 로직 (전환 조건 검사에 집중)

    // 1. 함수는 한 가지 일만 잘해야 한다.
    // 2. 상태별 행동을 함수로 만든다.
    private void Idle()
    {
        // 대기하는 상태
        // Todo. Idle 애니메이션 실행

        // 플레이어가 탐지범위 안에 있다면...
        if (GetDistanceToPlayer() <= DetectDistance)
        {
            ChangeState(EMonsterState.Trace);
        }
        // 코루틴 시작은 EnterState에서 처리됨
    }

    private void Trace()
    {
        // 플레이어를 쫓아가는 상태
        // Todo. Run 애니메이션 실행

        float distance = GetDistanceToPlayer();

        // 방향 설정 필요 없이 도착지만 설정해주면 네비게이션 시스템에 의해 자동으로 이동한다.
        _agent.SetDestination(PlayerPosition);

        // 플레이어와의 거리가 공격범위내라면
        if (distance <= AttackDistance)
        {
            ChangeState(EMonsterState.Attack);
        }
        
        if (_agent.isOnOffMeshLink)
        {
            Debug.Log("링크를 만남");
            OffMeshLinkData linkData = _agent.currentOffMeshLinkData;
            _jumpStartPosition = linkData.startPos;
            _jumpEndPosition = linkData.endPos;

            if (_jumpEndPosition.y > _jumpStartPosition.y)
            {
                Debug.Log("상태 전환: Trace -> Jump");
                //ChangeState(EMonsterState.Jump);
                // 점프 애니메이션 재생
                // 점프 사운드 재생
            }
        }
    }

/// <summary>
    /// 포물선 점프 코루틴 - 자연스러운 점프 연출
    /// </summary>
    private IEnumerator Jump_Coroutine()
    {
        // OffMeshLink 완료 처리
        _agent.CompleteOffMeshLink();

        float elapsed = 0f;
        Vector3 startPos = _jumpStartPosition;
        Vector3 endPos = _jumpEndPosition;

        // 높이 차이에 따라 점프 높이 조절
        float heightDiff = endPos.y - startPos.y;
        float actualJumpHeight = JumpHeight + Mathf.Max(0, heightDiff * 0.5f);

        Debug.Log($"점프 시작: {startPos} -> {endPos}, 높이: {actualJumpHeight}");

        while (elapsed < JumpDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / JumpDuration;

            // 수평 이동 (선형 보간)
            Vector3 horizontalPos = Vector3.Lerp(startPos, endPos, t);

            // 수직 이동 (포물선: 4 * t * (1 - t)는 t=0.5에서 최대값 1)
            float parabola = 4f * t * (1f - t);
            float verticalOffset = parabola * actualJumpHeight;

            // 최종 위치 = 수평 위치 + 포물선 높이
            transform.position = horizontalPos + Vector3.up * verticalOffset;

            yield return null;
        }

        // 정확한 도착 위치로 보정
        transform.position = endPos;

        Debug.Log("점프 완료 -> Trace 상태로 전환");
        ChangeState(EMonsterState.Trace);
    }

    private void Comeback()
    {
        // 복귀 중에도 플레이어가 가까워지면 다시 추격
        if (GetDistanceToPlayer() <= DetectDistance)
        {
            ChangeState(EMonsterState.Trace);
            return;
        }

        _agent.SetDestination(_initialPosition);

        // NavMeshAgent의 도착 판정 사용 (stoppingDistance 고려)
        if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
        {
            ChangeState(EMonsterState.Idle);
            Debug.Log("도착!");
        }
    }

    private void Attack()
    {
        // 플레이어를 공격하는 상태
        _agent.ResetPath(); // 공격 중에는 이동 중지

        // 플레이어와의 거리가 멀다면 다시 쫒아오는 상태로 전환
        float distance = GetDistanceToPlayer();
        if (distance > AttackDistance)
        {
            ChangeState(EMonsterState.Trace);
            return;
        }

        AttackTimer += Time.deltaTime;
        if (AttackTimer >= AttackSpeed)
        {
            AttackTimer = 0f;
            Debug.Log("플레이어 공격!");

            // 과제 2번. 플레이어 공격하기
            // PlayerHit을 직접 참조하므로 GetComponent 불필요
            _player.TakeDamage(MonsterDamage);
        }
    }

    private void Patrol()
    {
        // 순찰 상태의 매 프레임 로직
        // 실제 이동은 코루틴에서 처리되고, 여기서는 전환 조건만 검사
        if (GetDistanceToPlayer() <= DetectDistance)
        {
            ChangeState(EMonsterState.Trace);
        }
        // 코루틴 시작은 EnterState에서 처리됨
    }

    #endregion

    #region 데미지 및 코루틴

    public bool TryTakeDamage(float damage, Vector3 hitDirection)
    {
        if (State == EMonsterState.Death)
        {
            return false;
        }

        // ConsumableStat의 Decrease 메서드 사용
        _health.Decrease(damage);

        // NavMeshAgent가 활성화된 상태에서만 정지 처리
        if (_agent.enabled && _agent.isOnNavMesh)
        {
            _agent.isStopped = true;    // 이동 일시정지
            _agent.ResetPath();         // 경로(=목적지) 삭제
        }

        // ConsumableStat의 Value 프로퍼티로 체력 확인
        if (_health.Value <= 0)
        {
            ChangeState(EMonsterState.Death);
            return true;
        }

        // Hit 상태로 전환 (ChangeState가 기존 Hit 코루틴도 정리함)
        ChangeState(EMonsterState.Hit);

        // 넉백 코루틴 시작
        _hitCoroutine = StartCoroutine(Hit_Coroutine(hitDirection));

        return true;
    }

    private IEnumerator Hit_Coroutine(Vector3 hitDirection)
    {
        // Todo. Hit 애니메이션 실행

        float timer = 0f;

        hitDirection.y = 0f;
        hitDirection.Normalize();

        // NavMeshAgent가 비활성화된 상태에서 transform으로 직접 이동
        while (timer < KnockbackDuration)
        {
            transform.position += hitDirection * KnockbackForce * Time.deltaTime;
            timer += Time.deltaTime;
            yield return null;
        }

        // 코루틴 핸들 정리는 ChangeState -> ExitState에서 처리됨
        ChangeState(EMonsterState.Trace);
    }

    private IEnumerator Death_Coroutine()
    {
        // Todo. Death 애니메이션 실행
        yield return new WaitForSeconds(_deathDuration);
        Destroy(gameObject);
    }

    private IEnumerator Patrol_Coroutine(Vector3 target, float duration)
    {
        // NavMeshAgent로 목적지 설정
        _agent.SetDestination(target);

        float timer = 0f;
        while (timer < duration && !HasArrivedAt(target))
        {
            // 플레이어 감지는 Patrol() Update 메서드에서 처리됨
            if (State != EMonsterState.Patrol)
            {
                yield break;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        // 코루틴 핸들 정리는 ChangeState -> ExitState에서 처리됨
        if (State == EMonsterState.Patrol)
        {
            ChangeState(EMonsterState.Idle);
        }
    }

    private IEnumerator Idle_Coroutine(float duration)
    {
        yield return new WaitForSeconds(duration);

        // 코루틴 핸들 정리는 ChangeState -> ExitState에서 처리됨
        // 여전히 Idle 상태라면 순찰로 전환
        if (State == EMonsterState.Idle)
        {
            ChangeState(EMonsterState.Patrol);
        }
    }

    #endregion
}