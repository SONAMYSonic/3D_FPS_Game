using UnityEngine;

/// <summary>
/// 애니메이션 이벤트에서 호출되는 공격 스크립트
/// Zombie1(애니메이터가 있는 오브젝트)에 붙어야 함
/// Z_Attack 애니메이션 클립에 이벤트로 PlayerAttack() 메서드 호출
/// </summary>
public class MonsterAttack : MonoBehaviour
{
    [SerializeField] private Monster _monster;
    [SerializeField] private float _attackRange = 2.0f; // 공격 판정 범위
    
    private PlayerHit _playerHit;

    private void Awake()
    {
        // Monster 컴포넌트는 부모 오브젝트에 있음
        if (_monster == null)
        {
            _monster = GetComponentInParent<Monster>();
        }
    }

    private void Start()
    {
        // 플레이어 참조 캐싱 (FindAnyObjectByType는 매우 비용이 큰 연산)
        _playerHit = FindAnyObjectByType<PlayerHit>();
    }

    /// <summary>
    /// 애니메이션 이벤트에서 호출되는 메서드
    /// Z_Attack 애니메이션의 타격 타이밍에 이벤트로 추가
    /// </summary>
public void PlayerAttack()
    {
        // 널 체크
        if (_monster == null || _playerHit == null)
        {
            Debug.LogWarning("MonsterAttack: Monster 또는 Player 참조가 없습니다.");
            return;
        }

        // 게임이 끝났거나 플레이어가 죽었으면 공격 안 함
        if (GameManager.Instance.State != EGameState.Playing || _playerHit.IsDead)
        {
            return;
        }

        // 공격 범위 체크 - 애니메이션 중에 플레이어가 멀어졌을 수 있음
        float distance = Vector3.Distance(_monster.Position, _playerHit.Position);
        if (distance > _attackRange)
        {
            Debug.Log($"MonsterAttack: 플레이어가 공격 범위 밖에 있음 (distance: {distance:F2})");
            return;
        }

        // 데미지 적용
        _playerHit.TakeDamage(_monster.MonsterDamage);
        Debug.Log($"MonsterAttack: 플레이어에게 {_monster.MonsterDamage} 데미지!");
    }
}