using System;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerStats))]
public class PlayerMove : MonoBehaviour
{
    [Serializable]
    public class MoveConfig
    {
        [Tooltip("중력 가속도 (음수 값)")]
        public float Gravity = -20f;
        
        [Tooltip("달리기 시 초당 소모 스태미나")]
        public float RunStamina = 10f;
        
        [Tooltip("점프 시 소모 스태미나")]
        public float JumpStamina = 10f;
    }

    [SerializeField] private MoveConfig _config;

    private CharacterController _controller;
    private PlayerStats _stats;
    private GrapplingController _grapplingController;
    
    [SerializeField] private Animator _soliderAnimator;

    private float _yVelocity = 0f;

    [Header("넉백 설정")]
    [SerializeField, Tooltip("플레이어의 질량 - 높을수록 넉백에 저항")]
    private float _impactMass = 3.0f;
    
    [SerializeField, Tooltip("넉백 회복 속도 - 높을수록 빨리 회복")]
    private float _impactRecoveryRate = 5.0f;

    [SerializeField, Tooltip("넉백 최소 임계값 - 이 값 이하면 넉백 종료")]
    private float _impactThreshold = 0.2f;

    private Vector3 _impact = Vector3.zero;

    public bool IsBeingKnockedBack => _impact.magnitude > _impactThreshold;

    // ═══════════════════════════════════════════════════════════
    // Unity 생명주기
    // ═══════════════════════════════════════════════════════════

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _stats = GetComponent<PlayerStats>();
        _grapplingController = GetComponentInChildren<GrapplingController>();
    }

    private void Update()
    {
        if (GameManager.Instance.State != EGameState.Playing)
        {
            return;
        }

        if (!_controller.enabled)
        {
            return;
        }

        if (_grapplingController != null && _grapplingController.IsGrappling)
        {
            HandleGrapplingInput();
            return;
        }

        HandleNormalMovement();
    }

    // ═══════════════════════════════════════════════════════════
    // 일반 이동
    // ═══════════════════════════════════════════════════════════

    private void HandleNormalMovement()
    {
        ApplyGravity();
        Vector3 inputDirection = GetInputDirection();
        TryJump();
        float moveSpeed = CalculateMoveSpeed();
        Vector3 moveVelocity = CalculateMoveVelocity(inputDirection, moveSpeed);
        moveVelocity = ApplyImpactToMovement(moveVelocity);

        _controller.Move(moveVelocity * Time.deltaTime);
        
        DecayImpact();
    }

    // ═══════════════════════════════════════════════════════════
    // 그래플링 중 입력
    // ═══════════════════════════════════════════════════════════

    private void HandleGrapplingInput()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        if (Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f)
        {
            // 플레이어 기준 방향으로 변환 (카메라 대신)
            Vector3 inputDirection = new Vector3(horizontal, 0f, vertical);
            Vector3 worldDirection = transform.TransformDirection(inputDirection);
            worldDirection.y = 0f;
            worldDirection.Normalize();

            _grapplingController.AddSwingInput(worldDirection);
        }

        _soliderAnimator.SetFloat("Speed", 0f);
    }

    // ═══════════════════════════════════════════════════════════
    // 이동 계산 메서드들
    // ═══════════════════════════════════════════════════════════

    private void ApplyGravity()
    {
        if (_controller.isGrounded && _yVelocity < 0)
        {
            _yVelocity = -2f;
        }
        else
        {
            _yVelocity += _config.Gravity * Time.deltaTime;
        }
    }

    private Vector3 GetInputDirection()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 direction = new Vector3(horizontal, 0f, vertical);
        _soliderAnimator.SetFloat("Speed", direction.magnitude * _stats.MoveSpeedValue);
        
        return direction.normalized;
    }

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

    private float CalculateMoveSpeed()
    {
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        bool isMoving = Mathf.Abs(horizontal) > 0.01f || Mathf.Abs(vertical) > 0.01f;

        if (isRunning && isMoving)
        {
            bool hasStamina = _stats.TryConsumeStamina(_config.RunStamina * Time.deltaTime);
            
            if (hasStamina)
            {
                return _stats.RunSpeedValue;
            }
        }

        return _stats.MoveSpeedValue;
    }

    /// <summary>
    /// 이동 방향을 계산합니다.
    /// 플레이어의 Transform을 기준으로 방향을 변환합니다.
    /// </summary>
    private Vector3 CalculateMoveVelocity(Vector3 inputDirection, float moveSpeed)
    {
        // 카메라 대신 플레이어의 Transform을 기준으로 방향 변환
        // 플레이어는 마우스 X 입력으로 Y축 회전하므로, 항상 올바른 방향을 가리킴
        Vector3 worldDirection = transform.TransformDirection(inputDirection);
        worldDirection.y = 0f;
        worldDirection.Normalize();

        Vector3 velocity = worldDirection * moveSpeed;
        velocity.y = _yVelocity;

        return velocity;
    }

    private Vector3 ApplyImpactToMovement(Vector3 moveVelocity)
    {
        if (_impact.magnitude > _impactThreshold)
        {
            moveVelocity += _impact;
        }

        return moveVelocity;
    }

    private void DecayImpact()
    {
        _impact = Vector3.Lerp(_impact, Vector3.zero, _impactRecoveryRate * Time.deltaTime);
        
        if (_impact.magnitude <= _impactThreshold)
        {
            _impact = Vector3.zero;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // 공개 메서드
    // ═══════════════════════════════════════════════════════════

    public void SetVelocity(float yVelocity)
    {
        _yVelocity = yVelocity;
    }

    public void AddImpact(Vector3 direction, float force)
    {
        direction.Normalize();
        direction.y = 0.3f;
        
        Vector3 impactForce = direction.normalized * (force / _impactMass);
        _impact += impactForce;
        
        Debug.Log($"[PlayerMove] 넉백 적용! 방향: {direction}, 힘: {force}");
    }

    public void ClearImpact()
    {
        _impact = Vector3.zero;
    }
}
