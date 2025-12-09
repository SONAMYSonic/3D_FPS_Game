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