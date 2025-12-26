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
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            _playerTransform = playerObj.transform;
        }
        else
        {
            Debug.LogError("Player not found!");
        }

        InitializeBulletPool();

        if (_hitEffectPrefab != null)
        {
            _hitEffect = Instantiate(_hitEffectPrefab);
            _hitEffect.gameObject.SetActive(false);
        }

        ChangeState(new DroneIdleState(this));
    }

    private void Update()
    {
        // 게임이 Playing 상태가 아니면 드론 동작 중지
        if (GameManager.Instance == null || GameManager.Instance.State != EGameState.Playing)
        {
            return;
        }

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

            float headDist = Mathf.Min(currentDistance, totalDistance);
            Vector3 headPos = startOrigin + direction * headDist;

            float tailDist = Mathf.Max(0f, currentDistance - bulletLength);
            Vector3 tailPos = startOrigin + direction * Mathf.Min(tailDist, totalDistance);

            lineRenderer.SetPosition(0, tailPos);
            lineRenderer.SetPosition(1, headPos);

            if (tailDist >= totalDistance)
            {
                break;
            }

            yield return null;
        }

        lineRenderer.enabled = false;
    }

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

    public void ChangeState(IDroneState newState)
    {
        if (_currentState != null)
        {
            _currentState.Exit();
        }

        _currentState = newState;
        _currentState.Enter();
    }

    public void SetTarget(Transform target)
    {
        _currentTarget = target;
        if (_currentTarget != null && !IsRecalled())
        {
            ChangeState(new DroneApproachState(this));
        }
    }

    public void Recall()
    {
        _isRecalled = true;
        ChangeState(new DroneReturnState(this));
    }

    public void ClearTarget()
    {
        _currentTarget = null;
    }

    public bool IsRecalled()
    {
        return _isRecalled;
    }

    public void PlayMuzzleFlash()
    {
        if (_muzzleFlash != null)
        {
            _muzzleFlash.Play();
        }
    }

    public bool IsPlayerInCover()
    {
        return false;
    }
}