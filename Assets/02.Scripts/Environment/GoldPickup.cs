using UnityEngine;

/// <summary>
/// 골드(엽전) 픽업 - 스폰 시 튀어오르고, 플레이어 접근 시 자석처럼 빨려가서 습득
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class GoldPickup : MonoBehaviour
{
    #region Constants
    private const float GroundCheckDelay = 0.5f;  // 스폰 후 바닥 체크 시작까지 대기
    private const float MinVelocityThreshold = 0.1f;  // 이 속도 이하면 정지 판정
    #endregion

    #region Serialized Fields
    [Header("골드 설정")]
    [SerializeField, Tooltip("습득 시 획득 골드량")]
    private int _goldAmount = 1;

    [Header("스폰 물리")]
    [SerializeField, Tooltip("스폰 시 위로 튀어오르는 힘")]
    private float _scatterForceUp = 5f;

    [SerializeField, Tooltip("스폰 시 옆으로 퍼지는 힘")]
    private float _scatterForceHorizontal = 3f;

    [Header("자석 효과")]
    [SerializeField, Tooltip("자석 효과 시작 거리")]
    private float _magnetRange = 5f;

    [SerializeField, Tooltip("자석으로 빨려가는 속도")]
    private float _magnetSpeed = 10f;

    [SerializeField, Tooltip("자석 가속도 (거리 가까울수록 빨라짐)")]
    private float _magnetAcceleration = 2f;

    [Header("습득")]
    [SerializeField, Tooltip("습득 판정 거리")]
    private float _collectRange = 1f;
    #endregion

    #region Private Fields
    private Rigidbody _rigidbody;
    private Collider _collider;
    private Transform _playerTransform;
    private PlayerStats _playerStats;

    private EGoldState _state = EGoldState.Idle;
    private float _spawnTimer = 0f;
    private float _currentMagnetSpeed = 0f;
    #endregion

    #region Properties
    public int GoldAmount => _goldAmount;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
    }

    private void Update()
    {
        // 플레이어 참조 없으면 찾기 시도
        if (_playerTransform == null)
        {
            TryFindPlayer();
            return;
        }

        switch (_state)
        {
            case EGoldState.Scatter:
                UpdateScatter();
                break;
            case EGoldState.Idle:
                UpdateIdle();
                break;
            case EGoldState.Magnet:
                UpdateMagnet();
                break;
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// 풀에서 꺼낼 때 호출 - 위치 설정 후 튀어오름 시작
    /// </summary>
    public void Spawn(Vector3 position)
    {
        transform.position = position;
        gameObject.SetActive(true);

        // 물리 초기화
        _rigidbody.isKinematic = false;
        _collider.enabled = true;
        _spawnTimer = 0f;
        _currentMagnetSpeed = 0f;

        // 랜덤 방향으로 튀어오름
        Vector2 randomCircle = Random.insideUnitCircle;
        Vector3 scatterDirection = new Vector3(
            randomCircle.x * _scatterForceHorizontal,
            _scatterForceUp,
            randomCircle.y * _scatterForceHorizontal
        );
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        _rigidbody.AddForce(scatterDirection, ForceMode.Impulse);

        _state = EGoldState.Scatter;
    }

    /// <summary>
    /// 풀에 반환될 때 호출
    /// </summary>
    public void Despawn()
    {
        _state = EGoldState.Idle;
        _rigidbody.isKinematic = true;
        gameObject.SetActive(false);
    }
    #endregion

    #region State Updates
    /// <summary>
    /// Scatter: 튀어오른 후 바닥에 안착할 때까지 대기
    /// </summary>
    private void UpdateScatter()
    {
        _spawnTimer += Time.deltaTime;

        // 일정 시간 후 속도가 충분히 느려지면 Idle로 전환
        if (_spawnTimer > GroundCheckDelay)
        {
            if (_rigidbody.linearVelocity.magnitude < MinVelocityThreshold)
            {
                _state = EGoldState.Idle;
            }
        }
    }

    /// <summary>
    /// Idle: 바닥에 있으면서 플레이어 접근 감지
    /// </summary>
    private void UpdateIdle()
    {
        float distanceToPlayer = GetDistanceToPlayer();

        if (distanceToPlayer <= _magnetRange)
        {
            // 자석 모드 진입: 물리 비활성화하고 직접 이동
            _rigidbody.isKinematic = true;
            _collider.enabled = false;
            _currentMagnetSpeed = _magnetSpeed * 0.5f;  // 초기 속도
            _state = EGoldState.Magnet;
        }
    }

    /// <summary>
    /// Magnet: 플레이어에게 빨려감
    /// </summary>
    private void UpdateMagnet()
    {
        float distanceToPlayer = GetDistanceToPlayer();

        // 습득 판정
        if (distanceToPlayer <= _collectRange)
        {
            Collect();
            return;
        }

        // 가속하며 플레이어에게 이동
        _currentMagnetSpeed += _magnetAcceleration * Time.deltaTime;
        Vector3 direction = (_playerTransform.position - transform.position).normalized;
        transform.position += direction * _currentMagnetSpeed * Time.deltaTime;
    }
    #endregion

    #region Private Methods
    private void TryFindPlayer()
    {
        // PlayerStats를 가진 오브젝트 찾기
        _playerStats = FindFirstObjectByType<PlayerStats>();
        if (_playerStats != null)
        {
            _playerTransform = _playerStats.transform;
        }
    }

    private float GetDistanceToPlayer()
    {
        return Vector3.Distance(transform.position, _playerTransform.position);
    }

    private void Collect()
    {
        // 플레이어에게 골드 추가
        _playerStats.AddGold(_goldAmount);
        Debug.Log($"[GoldPickup] 골드 획득: +{_goldAmount} (현재: {_playerStats.CurrentGold})");

        // 풀에 반환
        PoolManager.Instance.ReturnGoldToPool(gameObject);
    }
    #endregion
}

/// <summary>
/// 골드 상태
/// </summary>
public enum EGoldState
{
    Idle,       // 바닥에 대기
    Scatter,    // 스폰 직후 튀어오르는 중
    Magnet      // 플레이어에게 빨려가는 중
}
