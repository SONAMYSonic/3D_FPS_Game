using System;
using UnityEngine;

/// <summary>
/// 플레이어 이동을 담당하는 컴포넌트입니다.
/// CharacterController를 사용하여 물리 기반이 아닌 직접 이동 방식을 사용합니다.
/// </summary>
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerStats))]
public class PlayerMove : MonoBehaviour
{
    /// <summary>
    /// 이동 관련 설정값을 담는 직렬화 가능한 클래스
    /// </summary>
    [Serializable]
    public class MoveConfig
    {
        [Tooltip("중력 가속도 (음수 값)")]
        public float Gravity;
        
        [Tooltip("달리기 시 초당 소모 스태미나")]
        public float RunStamina;
        
        [Tooltip("점프 시 소모 스태미나")]
        public float JumpStamina;
    }

    [SerializeField] private MoveConfig _config;

    private CharacterController _controller;
    private PlayerStats _stats;
    
    [SerializeField] private Animator _soliderAnimator;

    private float _yVelocity = 0f;

    [Header("넉백 설정")]
    [SerializeField, Tooltip("플레이어의 질량 - 높을수록 넉백에 저항")]
    private float _impactMass = 3.0f;
    
    [SerializeField, Tooltip("넉백 회복 속도 - 높을수록 빨리 회복")]
    private float _impactRecoveryRate = 5.0f;

    [SerializeField, Tooltip("넉백 최소 임계값 - 이 값 이하면 넉백 종료")]
    private float _impactThreshold = 0.2f;

    // 현재 적용 중인 넉백(충격) 벡터
    private Vector3 _impact = Vector3.zero;

    /// <summary>
    /// 현재 넉백 상태인지 여부를 반환합니다.
    /// </summary>
    public bool IsBeingKnockedBack => _impact.magnitude > _impactThreshold;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _stats = GetComponent<PlayerStats>();
    }

    private void Update()
    {
        // 게임이 플레이 중이 아니거나 컨트롤러가 비활성화면 이동 불가
        if (!IsMovementAllowed())
        {
            return;
        }

        // 중력 누적
        ApplyGravity();

        // 플레이어 입력 처리
        Vector3 inputDirection = GetInputDirection();
        
        // 점프 처리
        TryJump();

        // 최종 이동 속도 계산
        float moveSpeed = CalculateMoveSpeed();
        
        // 이동 벡터 계산 (입력 + 중력)
        Vector3 moveVelocity = CalculateMoveVelocity(inputDirection, moveSpeed);
        
        // 넉백 적용
        moveVelocity = ApplyImpactToMovement(moveVelocity);

        // 실제 이동 실행
        _controller.Move(moveVelocity * Time.deltaTime);
        
        // 넉백 점진적 감소
        DecayImpact();
    }

    /// <summary>
    /// 이동이 허용된 상태인지 확인합니다.
    /// </summary>
    private bool IsMovementAllowed()
    {
        if (GameManager.Instance.State != EGameState.Playing)
        {
            return false;
        }

        if (!_controller.enabled)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// 중력을 누적합니다.
    /// 비유: 땅에서 떨어지면 점점 빨라지는 낙하 속도
    /// </summary>
    private void ApplyGravity()
    {
        // 땅에 있을 때는 약간의 음수 값만 유지 (바닥에 붙어있도록)
        if (_controller.isGrounded && _yVelocity < 0)
        {
            _yVelocity = -2f;
        }
        else
        {
            _yVelocity += _config.Gravity * Time.deltaTime;
        }
    }

    /// <summary>
    /// 키보드 입력을 받아 이동 방향을 반환합니다.
    /// </summary>
    private Vector3 GetInputDirection()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 direction = new Vector3(horizontal, 0f, vertical);
        
        // 애니메이션 블렌드 트리용 속도 파라미터 설정
        _soliderAnimator.SetFloat("Speed", direction.magnitude * _stats.MoveSpeedValue);
        
        return direction.normalized;
    }

    /// <summary>
    /// 점프 입력을 확인하고 처리합니다.
    /// </summary>
    private void TryJump()
    {
        bool jumpPressed = Input.GetButtonDown("Jump");
        bool isGrounded = _controller.isGrounded;

        if (jumpPressed && isGrounded)
        {
            _yVelocity = _stats.JumpPowerValue;
            _soliderAnimator.SetTrigger("Jump");
        }
    }

    /// <summary>
    /// 현재 이동 속도를 계산합니다.
    /// Shift 키를 누르면 달리기 속도를 반환합니다.
    /// </summary>
    private float CalculateMoveSpeed()
    {
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        bool hasStamina = _stats.TryConsumeStamina(_config.RunStamina * Time.deltaTime);

        if (isRunning && hasStamina)
        {
            return _stats.RunSpeedValue;
        }

        return _stats.MoveSpeedValue;
    }

    /// <summary>
    /// 입력 방향을 카메라 기준 월드 좌표로 변환하고 최종 이동 벡터를 계산합니다.
    /// </summary>
    private Vector3 CalculateMoveVelocity(Vector3 inputDirection, float moveSpeed)
    {
        // 카메라 방향 기준으로 입력 방향 변환 (로컬 → 월드)
        Vector3 worldDirection = Camera.main.transform.TransformDirection(inputDirection);
        worldDirection.y = 0f;  // 수평 이동만
        worldDirection.Normalize();

        Vector3 velocity = worldDirection * moveSpeed;
        velocity.y = _yVelocity;

        return velocity;
    }

    /// <summary>
    /// 넉백(충격)을 이동 벡터에 적용합니다.
    /// </summary>
    private Vector3 ApplyImpactToMovement(Vector3 moveVelocity)
    {
        if (_impact.magnitude > _impactThreshold)
        {
            moveVelocity += _impact;
        }

        return moveVelocity;
    }

    /// <summary>
    /// 넉백을 점진적으로 감소시킵니다.
    /// Lerp를 사용하여 부드럽게 0으로 수렴합니다.
    /// </summary>
    private void DecayImpact()
    {
        _impact = Vector3.Lerp(_impact, Vector3.zero, _impactRecoveryRate * Time.deltaTime);
        
        // 임계값 이하면 완전히 0으로
        if (_impact.magnitude <= _impactThreshold)
        {
            _impact = Vector3.zero;
        }
    }

    /// <summary>
    /// Y축 속도를 직접 설정합니다.
    /// 외부 시스템(그래플링 훅 등)에서 사용할 수 있습니다.
    /// </summary>
    public void SetVelocity(float yVelocity)
    {
        _yVelocity = yVelocity;
    }

    /// <summary>
    /// 외부에서 충격(넉백)을 가합니다.
    /// 비유: 폭발이나 적의 공격에 밀려나는 효과
    /// </summary>
    /// <param name="direction">충격 방향 (정규화되지 않아도 됨)</param>
    /// <param name="force">충격의 힘</param>
    public void AddImpact(Vector3 direction, float force)
    {
        // 방향 정규화
        direction.Normalize();
        
        // 약간의 상승 효과 추가 (타격감 향상)
        // 비유: 맞으면 살짝 붕 뜨는 느낌
        direction.y = 0.3f;
        
        // 질량에 반비례하여 충격 적용
        // 비유: 무거운 물체일수록 덜 밀림
        Vector3 impactForce = direction.normalized * (force / _impactMass);
        
        // 기존 충격에 누적 (연속 타격 시 효과 중첩)
        _impact += impactForce;
        
        Debug.Log($"[PlayerMove] 넉백 적용! 방향: {direction}, 힘: {force}, 결과 충격: {_impact}");
    }

    /// <summary>
    /// 현재 충격(넉백) 벡터를 초기화합니다.
    /// </summary>
    public void ClearImpact()
    {
        _impact = Vector3.zero;
    }
}
