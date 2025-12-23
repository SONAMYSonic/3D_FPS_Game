// StrikerDroneController.cs
using UnityEngine;
using System.Collections;

public class StrikerDroneController : MonoBehaviour
{
    // --- 설정 변수 (Inspector) ---
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 8.0f;
    [SerializeField] private float _rotationSpeed = 5.0f;
    [SerializeField] private Vector3 _idleOffset = new Vector3(1.5f, 2.5f, -1.0f); // 어깨 위
    [SerializeField] private float _coverHeightOffset = 1.0f; // 엄폐 시 낮아질 높이

    [Header("Combat Settings")]
    [SerializeField] private float _attackRange = 15.0f;
    [SerializeField] private float _fireRate = 0.1f;
    [SerializeField] private float _scanRadius = 20.0f;
    [SerializeField] private LayerMask _enemyLayer;

    [Header("References")]
    [SerializeField] private Transform _turretHead;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private ParticleSystem _muzzleFlash;

    // --- 내부 상태 변수 ---
    private IDroneState _currentState;
    private Transform _playerTransform;
    private Transform _currentTarget;
    private bool _isRecalled = false; // 복귀 명령 플래그

    // 물리 이동을 위한 변수
    private Vector3 _currentVelocity; // SmoothDamp용

    // --- 프로퍼티 (State 접근용) ---
    public float MoveSpeed => _moveSpeed;
    public float RotationSpeed => _rotationSpeed;
    public float AttackRange => _attackRange;
    public float FireRate => _fireRate;
    public float ScanRadius => _scanRadius;
    public LayerMask EnemyLayer => _enemyLayer;

    public Transform PlayerTransform => _playerTransform;
    public Transform CurrentTarget => _currentTarget;
    public Transform TurretHead => _turretHead;
    public Transform FirePoint => _firePoint;

    // --- 생명주기 메서드 ---
    private void Awake()
    {
        // 최적화를 위해 플레이어 참조 캐싱 (실제론 GameManager 등을 통해 가져오는 것 권장)
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            _playerTransform = playerObj.transform;
        }
        else
        {
            Debug.LogError("Player not found!");
        }

        // 초기 상태: Idle
        ChangeState(new DroneIdleState(this));
    }

    private void Update()
    {
        if (_currentState != null)
        {
            _currentState.Execute();
        }
    }

    // --- 공개 메서드 (API) ---

    // 상태 변경
    public void ChangeState(IDroneState newState)
    {
        if (_currentState != null)
        {
            _currentState.Exit();
        }

        _currentState = newState;
        _currentState.Enter();
    }

    // 타겟 설정 (외부에서 적 지정 시 사용)
    public void SetTarget(Transform target)
    {
        _currentTarget = target;
        // 타겟이 지정되면 즉시 접근/공격 로직으로 전환 가능
        if (_currentTarget != null && !IsRecalled())
        {
            ChangeState(new DroneApproachState(this));
        }
    }

    // 복귀 명령
    public void Recall()
    {
        _isRecalled = true;
        ChangeState(new DroneReturnState(this));
    }

    // 타겟 초기화
    public void ClearTarget()
    {
        _currentTarget = null;
    }

    public bool IsRecalled()
    {
        return _isRecalled;
    }

    // 발사 이펙트 재생 (State에서 호출)
    public void PlayMuzzleFlash()
    {
        if (_muzzleFlash != null)
        {
            _muzzleFlash.Play();
        }
    }

    // 플레이어가 엄폐 중인지 확인 (가상 로직)
    public bool IsPlayerInCover()
    {
        // 실제 게임 로직: PlayerController의 상태를 확인해야 함
        // 여기서는 예시로 false 반환
        return false;
    }
}