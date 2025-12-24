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

    [Header("Combat Position Settings")]
    [SerializeField, Tooltip("전투 시 타겟 위로 떠있는 높이")]
    private float _combatHoverHeight = 1.5f;
    [SerializeField, Tooltip("이동 보간 시간 (낮을수록 빠름)")]
    private float _moveSmoothTime = 0.3f;
    [SerializeField, Tooltip("회전 보간 속도")]
    private float _bodyRotationSpeed = 5.0f;

    [Header("Combat Settings")]
    [SerializeField] private float _attackRange = 15.0f;
    [SerializeField] private float _fireRate = 0.1f;
    [SerializeField] private float _scanRadius = 20.0f;
    [SerializeField] private LayerMask _enemyLayer;

    [Header("Drone Damage Settings")]
    [SerializeField, Tooltip("드론 총알 한 발의 데미지")]
    private float _damage = 5f;
    [SerializeField, Tooltip("레이캐스트 최대 거리")]
    private float _maxShootRange = 50f;

    [Header("Bullet Trail Settings")]
    [SerializeField, Tooltip("총알 궤적용 LineRenderer 프리팹")]
    private LineRenderer _bulletLineRendererPrefab;
    [SerializeField, Tooltip("총알 LineRenderer 풀 사이즈")]
    private int _bulletPoolSize = 5;
    [SerializeField, Tooltip("총알 이동 속도")]
    private float _bulletSpeed = 80f;
    [SerializeField, Tooltip("총알 궤적 최대 길이")]
    private float _maxBulletLength = 1.0f;

    [Header("References")]
    [SerializeField] private Transform _turretHead;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private ParticleSystem _muzzleFlash;
    [SerializeField, Tooltip("피격 이펙트 프리팹")]
    private ParticleSystem _hitEffectPrefab;

    // --- 내부 상태 변수 ---
    private IDroneState _currentState;
    private Transform _playerTransform;
    private Transform _currentTarget;
    private bool _isRecalled = false;

    private Vector3 _currentVelocity;
    private LineRenderer[] _bulletPool;
    private int _currentBulletIndex = 0;
    private ParticleSystem _hitEffect;

    // --- 프로퍼티 (State 접근용) ---
    public float MoveSpeed => _moveSpeed;
    public float RotationSpeed => _rotationSpeed;
    public float AttackRange => _attackRange;
    public float FireRate => _fireRate;
    public float ScanRadius => _scanRadius;
    public LayerMask EnemyLayer => _enemyLayer;
    public float Damage => _damage;
    public float MaxShootRange => _maxShootRange;
    public float BulletSpeed => _bulletSpeed;
    public float MaxBulletLength => _maxBulletLength;
    public float CombatHoverHeight => _combatHoverHeight;
    public float MoveSmoothTime => _moveSmoothTime;
    public float BodyRotationSpeed => _bodyRotationSpeed;

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

        // 총알 풀 초기화
        InitializeBulletPool();

        // 피격 이펙트 캐싱
        if (_hitEffectPrefab != null)
        {
            _hitEffect = Instantiate(_hitEffectPrefab);
            _hitEffect.gameObject.SetActive(false);
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

    // --- 총알 풀링 메서드 ---
    private void InitializeBulletPool()
    {
        if (_bulletLineRendererPrefab == null)
        {
            Debug.LogWarning("Bullet LineRenderer Prefab이 할당되지 않았습니다.");
            return;
        }

        _bulletPool = new LineRenderer[_bulletPoolSize];
        for (int i = 0; i < _bulletPoolSize; i++)
        {
            LineRenderer lr = Instantiate(_bulletLineRendererPrefab, transform);
            lr.enabled = false;
            _bulletPool[i] = lr;
        }
    }

    /// <summary>
    /// 풀에서 다음 LineRenderer를 가져옵니다.
    /// </summary>
    public LineRenderer GetNextBulletLineRenderer()
    {
        if (_bulletPool == null || _bulletPool.Length == 0)
        {
            return null;
        }

        LineRenderer lr = _bulletPool[_currentBulletIndex];
        _currentBulletIndex = (_currentBulletIndex + 1) % _bulletPoolSize;
        return lr;
    }

    /// <summary>
    /// 피격 이펙트를 재생합니다.
    /// </summary>
    public void PlayHitEffect(Vector3 position, Vector3 normal)
    {
        if (_hitEffect != null)
        {
            _hitEffect.gameObject.SetActive(true);
            _hitEffect.transform.position = position;
            _hitEffect.transform.forward = normal;
            _hitEffect.Play();
        }
    }

    /// <summary>
    /// 총알 궤적 코루틴을 실행합니다.
    /// </summary>
    public Coroutine StartBulletTrail(Vector3 endPos)
    {
        return StartCoroutine(ShowBulletTrailCoroutine(endPos));
    }

    private IEnumerator ShowBulletTrailCoroutine(Vector3 endPos)
    {
        LineRenderer lineRenderer = GetNextBulletLineRenderer();
        if (lineRenderer == null)
        {
            yield break;
        }

        lineRenderer.enabled = true;

        Vector3 startOrigin = _firePoint.position;
        Vector3 direction = (endPos - startOrigin).normalized;
        float totalDistance = Vector3.Distance(startOrigin, endPos);
        float bulletLength = Mathf.Min(_maxBulletLength, totalDistance);
        float currentDistance = 0f;

        while (currentDistance < totalDistance + bulletLength)
        {
            currentDistance += _bulletSpeed * Time.deltaTime;

            // 총알 머리 위치 (목표 지점을 넘지 않도록)
            float headDist = Mathf.Min(currentDistance, totalDistance);
            Vector3 headPos = startOrigin + direction * headDist;

            // 총알 꼬리 위치
            float tailDist = Mathf.Max(0f, currentDistance - bulletLength);
            Vector3 tailPos = startOrigin + direction * Mathf.Min(tailDist, totalDistance);

            lineRenderer.SetPosition(0, tailPos);
            lineRenderer.SetPosition(1, headPos);

            // 꼬리가 목표 지점에 도달하면 종료
            if (tailDist >= totalDistance)
            {
                break;
            }

            yield return null;
        }

        lineRenderer.enabled = false;
    }

    /// <summary>
    /// 부드러운 위치 이동을 수행합니다. (State에서 호출)
    /// </summary>
    public void SmoothMoveToPosition(Vector3 targetPos, ref Vector3 velocity)
    {
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPos,
            ref velocity,
            _moveSmoothTime,
            _moveSpeed
        );
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