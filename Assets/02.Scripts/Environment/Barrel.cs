using UnityEngine;

public class Barrel : MonoBehaviour, IDamageable
{
    [Header("체력")]
    [SerializeField] private ConsumableStat _health;

    [Header("폭발 설정")]
    [SerializeField] private GameObject _explosionEffectPrefab;
    [SerializeField] private float _explosionRadius = 8f;
    [SerializeField] private float _explosionDamage = 1000f;

    [Header("폭발 물리 효과")]
    [SerializeField] private float _explosionForce = 10f;           // 날아가는 힘
    [SerializeField] private float _explosionTorque = 20f;          // 회전하는 힘
    [SerializeField] private float _horizontalSpread = 0.5f;        // 수평 방향 랜덤 정도 (0~1)

    [Header("대미지 대상 레이어")]
    [SerializeField] private LayerMask _damageLayers; // Monster, Player, Barrel 등

    private Rigidbody _rigidbody;
    private bool _isExploded = false;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _health.Initialize();
    }

    /// <summary>
    /// IDamageable 인터페이스 구현
    /// </summary>
    public bool TryTakeDamage(Damage damage)
    {
        if (_isExploded) return false;

        _health.Decrease(damage.Value);

        if (_health.Value <= 0)
        {
            Explode();
        }

        return true;
    }

    /// <summary>
    /// 폭발 처리
    /// </summary>
    private void Explode()
    {
        if (_isExploded) return;
        _isExploded = true;

        // 1. 폭발 이펙트 생성
        if (_explosionEffectPrefab != null)
        {
            GameObject effect = Instantiate(_explosionEffectPrefab, transform.position, Quaternion.identity);
            // 이펙트 자동 제거 (파티클 시스템이라면 duration 후 삭제)
            Destroy(effect, 3f);
        }

        // 2. 주위에 대미지 (몬스터, 플레이어, 드럼통)
        DealDamageToNearby();

        // 3. 휘리릭~! 날아가는 효과
        ApplyExplosionPhysics();

        // 4. 일정 시간 후 드럼통 제거
        Destroy(gameObject, 3f);
    }

    /// <summary>
    /// 폭발 시 물리 효과 적용 (휘리릭 날아가기)
    /// </summary>
    private void ApplyExplosionPhysics()
    {
        if (_rigidbody == null) return;

        // 1. 랜덤한 방향으로 날아가기 (위쪽 + 랜덤 수평 방향)
        Vector3 randomHorizontal = new Vector3(
            Random.Range(-1f, 1f),
            0f,
            Random.Range(-1f, 1f)
        ).normalized * _horizontalSpread;

        Vector3 launchDirection = (Vector3.up + randomHorizontal).normalized;
        _rigidbody.AddForce(launchDirection * _explosionForce, ForceMode.Impulse);

        // 2. 랜덤한 방향으로 회전시키기 (휘리릭 효과)
        Vector3 randomTorque = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f)
        ).normalized * _explosionTorque;

        _rigidbody.AddTorque(randomTorque, ForceMode.Impulse);
    }

    /// <summary>
    /// 주변 오브젝트에 대미지 적용
    /// </summary>
    private void DealDamageToNearby()
    {
        // OverlapSphere로 범위 내 콜라이더 탐색
        Collider[] colliders = Physics.OverlapSphere(transform.position, _explosionRadius, _damageLayers);

        for (int i = 0; i < colliders.Length; i++)
        {
            // 자기 자신은 제외
            if (colliders[i].gameObject == gameObject) continue;

            // 피격 방향 계산 (넉백용)
            Vector3 hitDirection = (colliders[i].transform.position - transform.position).normalized;

            // IDamageable 인터페이스로 통합 처리
            IDamageable damageable = colliders[i].GetComponent<IDamageable>();
            if (damageable != null)
            {
                Damage damage = new Damage
                {
                    Value = _explosionDamage,
                    HitDirection = hitDirection,
                    HitPoint = colliders[i].ClosestPoint(transform.position),
                    Who = gameObject,
                    Critical = false
                };
                damageable.TryTakeDamage(damage);
                continue;
            }

            // 플레이어 (IDamageable 미구현 시)
            PlayerHit playerHit = colliders[i].GetComponent<PlayerHit>();
            if (playerHit != null)
            {
                playerHit.TakeDamage(_explosionDamage);
            }
        }
    }

    // 디버그용: 폭발 범위 시각화
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _explosionRadius);
    }
}
