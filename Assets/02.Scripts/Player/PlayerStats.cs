using UnityEngine;

// 플레이어의 '스탯'을 관리하는 컴포넌트
public class PlayerStats : MonoBehaviour
{
    // 고민해볼 거리
    // 1. 옵저버 패턴은 어떻게 해야지?
    // 2. ConsumableStat의 Regenerate는 PlayerStats에서만 호출 가능하게 하고 싶다. 다른 속성/기능은 다른 클래스에서 사용할 수 있다.

    public ConsumableStat Health;
    public ConsumableStat Stamina;
    public ConsumableStat Bomb;
    public ValueStat Damage;
    public ValueStat MoveSpeed;
    public ValueStat RunSpeed;
    public ValueStat JumpPower;

    [Header("골드")]
    [SerializeField] private int _currentGold = 0;

    // 디미터 법칙 준수: 체력 관련 프로퍼티
    public bool IsDead => Health.Value <= 0f;
    public float CurrentHealth => Health.Value;
    public float MaxHealth => Health.MaxValue;
    public float HealthPercent => Health.Value / Health.MaxValue;
    public void DecreaseHealth(float amount) => Health.Decrease(amount);

    // 디미터 법칙 준수: 이동 관련 프로퍼티
    public float MoveSpeedValue => MoveSpeed.Value;
    public float RunSpeedValue => RunSpeed.Value;
    public float JumpPowerValue => JumpPower.Value;

    // 디미터 법칙 준수: 스태미나 관련 프로퍼티/메서드
    public bool TryConsumeStamina(float amount) => Stamina.TryConsume(amount);

    // 디미터 법칙 준수: 골드 관련 프로퍼티/메서드
    public int CurrentGold => _currentGold;
    public void AddGold(int amount)
    {
        _currentGold += amount;
        // TODO: UI 업데이트 이벤트 발행 (OnGoldChanged?.Invoke(_currentGold);)
    }
    public bool TrySpendGold(int amount)
    {
        if (_currentGold < amount) return false;
        _currentGold -= amount;
        return true;
    }

    private void Start()
    {
        Health.Initialize();
        Stamina.Initialize();
        Bomb.Initialize();
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;

        Health.Regenerate(deltaTime);
        Stamina.Regenerate(deltaTime);
    }
}