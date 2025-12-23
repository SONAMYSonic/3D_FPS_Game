using System.Collections;
using UnityEngine;

/// <summary>
/// 엘리트 몬스터 - 차징 후 돌진 공격 패턴을 가진 강화된 몬스터입니다.
/// 비유: 황소처럼 힘을 모았다가 직선으로 돌진하는 패턴
/// </summary>
[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(CharacterController))]
public class EliteMonster : Monster
{
    [Header("엘리트 : 돌진 스킬")]
    [SerializeField] private float _chargeDistance = 10f;     // 돌진 트리거 거리
    [SerializeField] private float _chargeDuration = 2.5f;    // 차징 시간
    [SerializeField] private float _dashSpeed = 15f;          // 돌진 속도
    [SerializeField] private float _dashDamage = 20f;         // 돌진 데미지
    [SerializeField] private float _dashKnockbackForce = 15f; // 돌진 넉백 (증가)
    [SerializeField] private float _chargeCooldown = 10f;     // 스킬 쿨타임

    [Header("엘리트 : 돌진 감지 설정")]
    [SerializeField, Tooltip("돌진 중 플레이어 감지 반경입니다.")]
    private float _dashHitRadius = 2.0f;  // 반경 증가
    
    [SerializeField, Tooltip("돌진 목표 지점을 플레이어 위치보다 얼마나 더 멀리 잡을지 결정합니다.")]
    private float _dashExtraDistance = 2.0f;
    
    [SerializeField, Tooltip("돌진 목표 지점에 도달했다고 판단할 거리 임계값입니다.")]
    private float _dashArrivalThreshold = 0.5f;

    [SerializeField, Tooltip("플레이어 레이어 마스크")]
    private LayerMask _playerLayerMask;

    [Header("엘리트 : 근접 밀쳐내기")]
    [SerializeField] private float _pushDistance = 2.0f;

    private LineRenderer _lineRenderer;
    private float _lastChargeTime;
    private Vector3 _dashTargetPosition;
    private Vector3 _dashDirection;  // 돌진 방향 저장 (Y축 고정용)
    private CharacterController _eliteController;
    private float _originalStepOffset;

    // 이번 돌진에서 이미 타격을 했는지 여부를 저장하는 플래그
    private bool _hasHitDuringThisDash;

    protected override void Awake()
    {
        base.Awake();
        
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.enabled = false;

        _eliteController = GetComponent<CharacterController>();
        if (_eliteController != null)
        {
            _originalStepOffset = _eliteController.stepOffset;
        }

        // 탐지 거리 2배 증가
        DetectDistance *= 2f;

        // 플레이어 레이어 마스크 자동 설정 (Inspector에서 설정 안 했을 경우)
        if (_playerLayerMask == 0)
        {
            _playerLayerMask = LayerMask.GetMask("Player");
        }
    }

    /// <summary>
    /// 커스텀 상태(Charging, Dash) 처리를 담당합니다.
    /// 부모 클래스의 Update에서 호출되는 훅(Hook) 메서드입니다.
    /// </summary>
    protected override bool HandleCustomState()
    {
        switch (State)
        {
            case EMonsterState.Charging:
                UpdateCharging();
                return true;
            case EMonsterState.Dash:
                UpdateDash();
                return true;
        }
        return false;
    }

    /// <summary>
    /// 추적 상태 업데이트 - 돌진 스킬 트리거 조건 확인
    /// </summary>
    protected override void UpdateTrace()
    {
        if (_player.IsDead)
        {
            ChangeState(EMonsterState.Idle);
            return;
        }

        float distance = Vector3.Distance(transform.position, _player.Position);

        // 근접 밀쳐내기 거리 내에 들어오면 공격 상태로 전환
        if (distance <= _pushDistance)
        {
            ChangeState(EMonsterState.Attack);
            return;
        }

        // 돌진 스킬 조건: 거리 조건 + 쿨타임 완료
        bool canCharge = distance <= _chargeDistance && 
                         Time.time >= _lastChargeTime + _chargeCooldown;
        
        if (canCharge)
        {
            ChangeState(EMonsterState.Charging);
            return;
        }

        _agent.SetDestination(_player.Position);

        if (distance <= AttackDistance)
        {
            ChangeState(EMonsterState.Attack);
        }
    }

    /// <summary>
    /// 상태 진입 시 초기화 처리
    /// </summary>
    protected override void EnterState(EMonsterState enteringState)
    {
        base.EnterState(enteringState);

        switch (enteringState)
        {
            case EMonsterState.Charging:
                HandleEnterCharging();
                break;
            case EMonsterState.Dash:
                HandleEnterDash();
                break;
        }
    }

    /// <summary>
    /// 차징 상태 진입 처리 - 레이저 라인 표시 및 차징 코루틴 시작
    /// </summary>
    private void HandleEnterCharging()
    {
        _agent.ResetPath();
        _animator.CrossFadeInFixedTime("Idle", 0.1f);

        _lineRenderer.enabled = true;
        _lineRenderer.positionCount = 2;

        StartCoroutine(ChargingCoroutine());
    }

    /// <summary>
    /// 돌진 상태 진입 처리 - 이동 시스템 전환 및 방향 고정
    /// </summary>
    private void HandleEnterDash()
    {
        _hasHitDuringThisDash = false;
        _lineRenderer.enabled = false;
        
        // NavMeshAgent 비활성화 (CharacterController로 직접 이동)
        _agent.enabled = false;
        _animator.CrossFadeInFixedTime("Run", 0.1f);

        // ★ 핵심 수정 1: Step Offset을 0으로 설정하여 플레이어 위로 넘어가기 방지
        if (_eliteController != null)
        {
            _eliteController.stepOffset = 0f;
        }

        // ★ 핵심 수정 2: 돌진 방향을 XZ 평면에서만 계산 (Y축 이동 제거)
        Vector3 toPlayer = _player.Position - transform.position;
        _dashDirection = new Vector3(toPlayer.x, 0f, toPlayer.z).normalized;
        
        // 목표 지점 설정 (수평 방향으로만)
        _dashTargetPosition = transform.position + _dashDirection * (_chargeDistance + _dashExtraDistance);
        
        // Y축은 현재 위치 유지
        _dashTargetPosition.y = transform.position.y;
    }

    /// <summary>
    /// 차징 상태 업데이트 - 플레이어를 바라보며 레이저 라인 표시
    /// </summary>
    private void UpdateCharging()
    {
        // 플레이어를 향한 방향 (수평만)
        Vector3 direction = (_player.Position - transform.position);
        direction.y = 0f;
        direction.Normalize();
        
        // 몬스터가 플레이어를 바라봄
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        // 레이저 라인 업데이트 (돌진 경로 시각화)
        Vector3 lineStart = transform.position + Vector3.up * 0.5f;
        Vector3 lineEnd = transform.position + direction * _chargeDistance + Vector3.up * 0.5f;
        
        _lineRenderer.SetPosition(0, lineStart);
        _lineRenderer.SetPosition(1, lineEnd);
    }

    /// <summary>
    /// 돌진 상태 업데이트 - CharacterController로 직접 이동하며 충돌 체크
    /// </summary>
    private void UpdateDash()
    {
        if (_eliteController == null) return;

        // ★ 핵심 수정 3: Y축 속도 없이 수평으로만 이동
        Vector3 moveVector = _dashDirection * _dashSpeed * Time.deltaTime;
        
        // 중력은 적용하되, 위로 올라가지 않도록 함
        if (!_eliteController.isGrounded)
        {
            moveVector.y = -9.81f * Time.deltaTime;  // 간단한 중력 적용
        }
        
        _eliteController.Move(moveVector);

        // 타격 체크 (아직 맞추지 않았다면)
        if (!_hasHitDuringThisDash)
        {
            CheckDashCollision();
        }

        // 목표 지점 도달 확인 (XZ 평면에서만 계산)
        Vector3 currentPosFlat = new Vector3(transform.position.x, 0f, transform.position.z);
        Vector3 targetPosFlat = new Vector3(_dashTargetPosition.x, 0f, _dashTargetPosition.z);
        
        if (Vector3.Distance(currentPosFlat, targetPosFlat) < _dashArrivalThreshold)
        {
            EndDash();
        }
    }

    /// <summary>
    /// 상태 종료 시 정리 처리
    /// </summary>
    protected override void ExitState(EMonsterState exitingState)
    {
        base.ExitState(exitingState);

        switch (exitingState)
        {
            case EMonsterState.Dash:
                // NavMeshAgent 다시 활성화
                _agent.enabled = true;
                _lastChargeTime = Time.time;

                // Step Offset 복구
                if (_eliteController != null)
                {
                    _eliteController.stepOffset = _originalStepOffset;
                }
                break;
                
            case EMonsterState.Charging:
                _lineRenderer.enabled = false;
                break;
        }
    }

    /// <summary>
    /// 차징 시간 대기 후 돌진 상태로 전환
    /// </summary>
    private IEnumerator ChargingCoroutine()
    {
        float timer = 0f;
        
        while (timer < _chargeDuration)
        {
            timer += Time.deltaTime;
            yield return null;

            // 상태가 변경되었으면 코루틴 종료
            if (State != EMonsterState.Charging)
            {
                yield break;
            }
        }

        ChangeState(EMonsterState.Dash);
    }

    /// <summary>
    /// 돌진 중 플레이어와의 충돌 체크 및 넉백 적용
    /// OverlapSphere를 사용하여 주변의 플레이어를 감지합니다.
    /// </summary>
    private void CheckDashCollision()
    {
        // ★ 핵심 수정 4: LayerMask를 사용하여 플레이어만 감지
        Collider[] hits = Physics.OverlapSphere(
            transform.position + Vector3.up,  // 약간 위에서 감지 (바닥 무시)
            _dashHitRadius,
            _playerLayerMask
        );

        foreach (Collider hit in hits)
        {
            // PlayerHit 컴포넌트 확인
            if (!hit.TryGetComponent(out PlayerHit playerHit))
            {
                continue;
            }

            // 데미지 적용
            playerHit.TakeDamage(_dashDamage);

            // ★ 핵심 수정 5: 넉백 적용 로직 개선
            ApplyKnockbackToPlayer(playerHit, hit.gameObject);

            _hasHitDuringThisDash = true;
            Debug.Log($"[엘리트 몬스터] 돌진 타격 성공! 데미지: {_dashDamage}, 넉백: {_dashKnockbackForce}");
            
            break;  // 한 번만 타격
        }
    }

    /// <summary>
    /// 플레이어에게 넉백 효과를 적용합니다.
    /// PlayerMove 컴포넌트를 찾아서 AddImpact를 호출합니다.
    /// </summary>
    private void ApplyKnockbackToPlayer(PlayerHit playerHit, GameObject playerObject)
    {
        // PlayerMove 컴포넌트 찾기 (같은 오브젝트 또는 부모에서)
        PlayerMove playerMove = playerObject.GetComponent<PlayerMove>();
        
        if (playerMove == null)
        {
            playerMove = playerObject.GetComponentInParent<PlayerMove>();
        }

        if (playerMove != null)
        {
            // 넉백 방향: 몬스터 → 플레이어 방향 (수평)
            Vector3 knockbackDirection = (playerHit.Position - transform.position);
            knockbackDirection.y = 0f;
            knockbackDirection.Normalize();

            playerMove.AddImpact(knockbackDirection, _dashKnockbackForce);
            Debug.Log($"[엘리트 몬스터] 넉백 적용 완료! 방향: {knockbackDirection}, 힘: {_dashKnockbackForce}");
        }
        else
        {
            Debug.LogWarning("[엘리트 몬스터] PlayerMove 컴포넌트를 찾을 수 없습니다!");
        }
    }

    /// <summary>
    /// 돌진 종료 - 추적 상태로 복귀
    /// </summary>
    private void EndDash()
    {
        ChangeState(EMonsterState.Trace);
    }

    /// <summary>
    /// 데미지 처리 - 차징/돌진 중에는 경직 없음
    /// </summary>
    public override bool TryTakeDamage(Damage damage)
    {
        _health.Decrease(damage.Value);

        if (_health.Value <= 0)
        {
            ChangeState(EMonsterState.Death);
            return true;
        }

        // 차징 또는 돌진 중에는 경직(Hit 상태) 없이 데미지만 받음
        // 비유: "슈퍼아머" 상태 - 공격을 받아도 행동이 끊기지 않음
        if (State == EMonsterState.Charging || State == EMonsterState.Dash)
        {
            return true;
        }

        return base.TryTakeDamage(damage);
    }

    /// <summary>
    /// 디버그용: 돌진 감지 범위를 Scene View에서 시각화
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // 돌진 감지 범위 표시 (녹색)
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position + Vector3.up, _dashHitRadius);
        
        // 돌진 트리거 거리 표시 (노란색)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _chargeDistance);
    }
}
