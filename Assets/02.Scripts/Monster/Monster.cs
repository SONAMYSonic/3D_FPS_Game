using System.Collections;
using UnityEngine;

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

    public EMonsterState State = EMonsterState.Idle;

    [SerializeField] private GameObject _player;
    [SerializeField] private CharacterController _controller;
    [SerializeField] private Vector3 _initialPosition;

    public float DetectDistance = 4f;
    public float AttackDistance = 1.2f;

    public float MoveSpeed = 5f;
    public float AttackSpeed = 2f;
    public float AttackTimer = 0f;

    public float Health = 100f;
    public float MonsterDamage = 10f;

    public float KnockbackForce = 5f;
    public float KnockbackDuration = 0.2f;
    private Coroutine _hitCoroutine;

    public float PatrolRadius = 3f;
    public float PatrolMinTime = 2f;
    public float PatrolMaxTime = 10f;
    public float IdleDurationMin = 2f;
    public float IdleDurationMax = 5f;

    // 코루틴 중복 실행 방지용 핸들
    private Coroutine _idleCoroutine;
    private Coroutine _patrolCoroutine;

    private void Start()
    {
        _initialPosition = transform.position;
    }

    private void Update()
    {
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

    // 1. 함수는 한 가지 일만 잘해야 한다.
    // 2. 상태별 행동을 함수로 만든다.
    private void Idle()
    {
        // 대기하는 상태
        // Todo. Idle 애니메이션 실행

        // 플레이어가 탐지범위 안에 있다면...
        if (Vector3.Distance(transform.position, _player.transform.position) <= DetectDistance)
        {
            // 진행 중인 Idle/Patrol 코루틴이 있다면 정리
            if (_idleCoroutine != null) { StopCoroutine(_idleCoroutine); _idleCoroutine = null; }
            if (_patrolCoroutine != null) { StopCoroutine(_patrolCoroutine); _patrolCoroutine = null; }

            State = EMonsterState.Trace;
            Debug.Log("상태 전환: Idle -> Trace");
        }
        else
        {
            // 이미 Idle 대기 코루틴이 돌고 있지 않을 때만 시작
            if (_idleCoroutine == null)
            {
                float idleTime = Random.Range(IdleDurationMin, IdleDurationMax);
                _idleCoroutine = StartCoroutine(Idle_Coroutine(idleTime));
            }
        }
    }

    private void Trace()
    {
        // 플레이어를 쫓아가는 상태
        // Todo. Run 애니메이션 실행

        float distance = Vector3.Distance(transform.position, _player.transform.position);

        // 1. 플레이어를 향하는 방향을 구한다.
        Vector3 direction = (_player.transform.position - transform.position).normalized;
        // 2. 방향에 따라 이동한다.
        _controller.Move(direction * MoveSpeed * Time.deltaTime);

        // 플레이어와의 거리가 공격범위내라면
        if (distance <= AttackDistance)
        {
            // 상태 전환 시 코루틴 정리
            if (_idleCoroutine != null) { StopCoroutine(_idleCoroutine); _idleCoroutine = null; }
            if (_patrolCoroutine != null) { StopCoroutine(_patrolCoroutine); _patrolCoroutine = null; }

            State = EMonsterState.Attack;
        }

        // 플레이어와의 거리가 너무 멀다면
        if (distance > DetectDistance)
        {
            // 제자리로 돌아가는 상태로 전환
            if (_idleCoroutine != null) { StopCoroutine(_idleCoroutine); _idleCoroutine = null; }
            if (_patrolCoroutine != null) { StopCoroutine(_patrolCoroutine); _patrolCoroutine = null; }

            State = EMonsterState.Comeback;
        }
    }

    private void Comeback()
    {
        // 과제 1. 제자리로 복귀하는 상태
        float distance = Vector3.Distance(transform.position, _initialPosition);
        Vector3 direction = (_initialPosition - transform.position).normalized;
        _controller.Move(direction * MoveSpeed * Time.deltaTime);
        if (distance <= 0.1f)
        {
            // 상태 전환 시 코루틴 정리
            if (_idleCoroutine != null) { StopCoroutine(_idleCoroutine); _idleCoroutine = null; }
            if (_patrolCoroutine != null) { StopCoroutine(_patrolCoroutine); _patrolCoroutine = null; }

            State = EMonsterState.Idle;
        }
    }

    private void Attack()
    {
        // 플레이어를 공격하는 상태

        // 플레이어와의 거리가 멀다면 다시 쫒아오는 상태로 전환
        float distance = Vector3.Distance(transform.position, _player.transform.position);
        if (distance > AttackDistance)
        {
            State = EMonsterState.Trace;
            return;
        }

        AttackTimer += Time.deltaTime;
        if (AttackTimer >= AttackSpeed)
        {
            AttackTimer = 0f;
            Debug.Log("플레이어 공격!");

            // 과제 2번. 플레이어 공격하기
            PlayerHit playerHit = _player.GetComponent<PlayerHit>();
            if (playerHit != null)
            {
                playerHit.TakeDamage(MonsterDamage);
            }
        }

    }

    public bool TryTakeDamage(float damage, Vector3 hitDirection)
    {
        if (State == EMonsterState.Death)
        {
            return false;
        }

        Health -= damage;

        if (Health <= 0)
        {
            State = EMonsterState.Death;
            // 죽었으니 실행 중이던 넉백 코루틴이 있다면 멈춤
            if (_hitCoroutine != null) StopCoroutine(_hitCoroutine);
            // 다른 진행 중 코루틴도 모두 정리
            if (_idleCoroutine != null) { StopCoroutine(_idleCoroutine); _idleCoroutine = null; }
            if (_patrolCoroutine != null) { StopCoroutine(_patrolCoroutine); _patrolCoroutine = null; }

            StartCoroutine(Death_Coroutine());
            return true;
        }

        // 1. 이미 피격 중이라면(Hit 상태라면), 진행 중이던 넉백을 취소합니다.
        if (State == EMonsterState.Hit && _hitCoroutine != null)
        {
            StopCoroutine(_hitCoroutine);
        }

        // 2. 상태를 Hit로 설정 (이미 Hit여도 다시 설정)
        State = EMonsterState.Hit;

        // 다른 상태 코루틴 정리
        if (_idleCoroutine != null) { StopCoroutine(_idleCoroutine); _idleCoroutine = null; }
        if (_patrolCoroutine != null) { StopCoroutine(_patrolCoroutine); _patrolCoroutine = null; }

        // 3. 넉백 코루틴을 '새로' 시작하고 변수에 저장합니다.
        _hitCoroutine = StartCoroutine(Hit_Coroutine(hitDirection));

        return true;
    }

    private IEnumerator Hit_Coroutine(Vector3 hitDirection)
    {
        // Todo. Hit 애니메이션 실행

        float timer = 0f;

        hitDirection.y = 0f;
        hitDirection.Normalize();

        while (timer < KnockbackDuration)
        {
            _controller.Move(hitDirection * KnockbackForce * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }

        State = EMonsterState.Trace;
        _hitCoroutine = null;
    }

    private IEnumerator Death_Coroutine()
    {
        // Todo. Death 애니메이션 실행
        yield return new WaitForSeconds(2.0f);
        Destroy(gameObject);
    }

    private void Patrol()
    {
        // 이미 순찰 코루틴이 돌고 있지 않을 때만 시작
        if (_patrolCoroutine == null)
        {
            float patrolTime = Random.Range(PatrolMinTime, PatrolMaxTime);
            Vector3 patrolTarget = _initialPosition + new Vector3(Random.Range(-PatrolRadius, PatrolRadius), 0, Random.Range(-PatrolRadius, PatrolRadius));
            _patrolCoroutine = StartCoroutine(Patrol_Coroutine(patrolTarget, patrolTime));
        }
    }

    private IEnumerator Patrol_Coroutine(Vector3 target, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            Vector3 direction = (target - transform.position).normalized;
            _controller.Move(direction * MoveSpeed * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;

            // 순찰 중 플레이어가 탐지범위에 들어오면 즉시 추적 상태로
            if (Vector3.Distance(transform.position, _player.transform.position) <= DetectDistance)
            {
                State = EMonsterState.Trace;
                break;
            }
        }

        // 코루틴 종료 처리
        _patrolCoroutine = null;

        if (State == EMonsterState.Patrol)
        {
            State = EMonsterState.Idle;
        }
    }

    private IEnumerator Idle_Coroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        _idleCoroutine = null;

        // 여전히 Idle 상태라면 순찰로 전환
        if (State == EMonsterState.Idle)
        {
            State = EMonsterState.Patrol;
        }
    }
}