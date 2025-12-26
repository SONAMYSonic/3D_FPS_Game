using System;
using UnityEngine;

/// <summary>
/// 소모성 스탯을 관리하는 클래스입니다.
/// 체력, 스태미나, 마나 등 시간에 따라 회복되고 사용 시 감소하는 스탯에 사용됩니다.
/// 
/// ───────────────────────────────────────────────────────────
/// 📚 학습 포인트: 더티 플래그 패턴 (Dirty Flag Pattern)
/// ───────────────────────────────────────────────────────────
/// 
/// 문제 상황:
/// - 매 프레임 Regenerate() 호출
/// - 값이 안 바뀌어도 매번 이벤트 발행
/// - UI가 초당 60~120회 갱신 → 성능 저하!
/// 
/// 해결책 (더티 플래그):
/// - 값이 "실제로 변경"되었을 때만 이벤트 발행
/// - 이미 최대치면 이벤트 발행하지 않음
/// 
/// 비유: "택배 도착 알림"
/// - 나쁜 예: 매 초마다 "택배 아직 안 왔어요" 알림 (스팸!)
/// - 좋은 예: 택배가 진짜 도착했을 때만 알림
/// 
/// ───────────────────────────────────────────────────────────
/// </summary>
[Serializable]
public class ConsumableStat
{
    // ═══════════════════════════════════════════════════════════
    // 필드
    // ═══════════════════════════════════════════════════════════
    
    [SerializeField, Tooltip("최대값")]
    private float _maxValue;
    
    [SerializeField, Tooltip("현재값")]
    private float _value;
    
    [SerializeField, Tooltip("초당 회복량")]
    private float _regenValue;

    // ═══════════════════════════════════════════════════════════
    // 상수
    // ═══════════════════════════════════════════════════════════
    
    /// <summary>
    /// 값 변경 감지를 위한 최소 차이값입니다.
    /// 부동소수점 오차를 고려하여 0.001 이상 변해야 "변경"으로 인식합니다.
    /// 
    /// ───────────────────────────────────────────────────────────
    /// 📚 학습 포인트: 부동소수점 비교
    /// ───────────────────────────────────────────────────────────
    /// 
    /// float 타입은 정확한 값을 저장하지 못합니다.
    /// 예: 0.1 + 0.2 = 0.30000000000000004 (0.3이 아님!)
    /// 
    /// 그래서 == 대신 "충분히 가까운지"를 비교합니다:
    /// Mathf.Abs(a - b) < Epsilon
    /// 
    /// ───────────────────────────────────────────────────────────
    /// </summary>
    private const float ValueChangeThreshold = 0.001f;

    // ═══════════════════════════════════════════════════════════
    // 프로퍼티
    // ═══════════════════════════════════════════════════════════
    
    public float MaxValue => _maxValue;
    public float Value => _value;

    // ═══════════════════════════════════════════════════════════
    // 이벤트
    // ═══════════════════════════════════════════════════════════
    
    private Action _onDataChanged;

    // ═══════════════════════════════════════════════════════════
    // 초기화
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// 스탯을 초기화합니다.
    /// </summary>
    /// <param name="onDataChanged">값 변경 시 호출될 콜백 (선택)</param>
    public void Initialize(Action onDataChanged = null)
    {
        _onDataChanged = onDataChanged;
        
        // 초기화 시에는 무조건 이벤트 발행 (UI 초기 설정용)
        _value = _maxValue;
        NotifyChanged();
    }

    // ═══════════════════════════════════════════════════════════
    // 값 변경 메서드
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// 시간에 따라 스탯을 회복합니다.
    /// 
    /// ───────────────────────────────────────────────────────────
    /// 📚 핵심 수정: 조건부 이벤트 발행
    /// ───────────────────────────────────────────────────────────
    /// 
    /// 변경 전:
    /// public void Regenerate(float deltaTime)
    /// {
    ///     _value += _regenValue * deltaTime;
    ///     NotifyChanged();  // 매번 호출! (문제)
    /// }
    /// 
    /// 변경 후:
    /// - 이미 최대치면 아무것도 안 함 (조기 반환)
    /// - 값이 실제로 변했을 때만 이벤트 발행
    /// 
    /// ───────────────────────────────────────────────────────────
    /// </summary>
    /// <param name="deltaTime">경과 시간 (초)</param>
    public void Regenerate(float deltaTime)
    {
        // 이미 최대치면 회복할 필요 없음 → 조기 반환
        if (_value >= _maxValue)
        {
            return;
        }

        // 이전 값 저장 (변경 감지용)
        float previousValue = _value;

        // 회복 적용
        _value += _regenValue * deltaTime;
        _value = Mathf.Min(_value, _maxValue);

        // 실제로 값이 변했을 때만 이벤트 발행
        NotifyIfValueChanged(previousValue);
    }

    /// <summary>
    /// 지정한 양만큼 소모를 시도합니다.
    /// </summary>
    /// <param name="amount">소모량</param>
    /// <returns>소모 성공 여부</returns>
    public bool TryConsume(float amount)
    {
        if (_value < amount)
        {
            return false;
        }

        Consume(amount);
        return true;
    }

    /// <summary>
    /// 지정한 양만큼 소모합니다.
    /// </summary>
    /// <param name="amount">소모량</param>
    public void Consume(float amount)
    {
        float previousValue = _value;
        
        _value -= amount;
        _value = Mathf.Max(_value, 0f);

        NotifyIfValueChanged(previousValue);
    }

    /// <summary>
    /// 지정한 양만큼 증가합니다.
    /// </summary>
    /// <param name="amount">증가량</param>
    public void Increase(float amount)
    {
        SetValue(_value + amount);
    }

    /// <summary>
    /// 지정한 양만큼 감소합니다.
    /// </summary>
    /// <param name="amount">감소량</param>
    public void Decrease(float amount)
    {
        float previousValue = _value;
        
        _value -= amount;
        _value = Mathf.Max(_value, 0f);

        NotifyIfValueChanged(previousValue);
    }

    // ═══════════════════════════════════════════════════════════
    // 최대값 변경 메서드
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// 최대값을 증가시킵니다.
    /// </summary>
    public void IncreaseMax(float amount)
    {
        _maxValue += amount;
        NotifyChanged();
    }

    /// <summary>
    /// 최대값을 감소시킵니다.
    /// </summary>
    public void DecreaseMax(float amount)
    {
        _maxValue -= amount;
        _maxValue = Mathf.Max(_maxValue, 0f);
        
        if (_value > _maxValue)
        {
            _value = _maxValue;
        }
        
        NotifyChanged();
    }

    /// <summary>
    /// 최대값을 설정합니다.
    /// </summary>
    public void SetMaxValue(float value)
    {
        _maxValue = Mathf.Max(value, 0f);
        
        if (_value > _maxValue)
        {
            _value = _maxValue;
        }
        
        NotifyChanged();
    }

    /// <summary>
    /// 현재값을 설정합니다.
    /// </summary>
    public void SetValue(float value)
    {
        float previousValue = _value;
        
        _value = Mathf.Clamp(value, 0f, _maxValue);
        
        NotifyIfValueChanged(previousValue);
    }

    // ═══════════════════════════════════════════════════════════
    // 내부 헬퍼
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// 값이 실제로 변경되었을 때만 이벤트를 발행합니다.
    /// 
    /// ───────────────────────────────────────────────────────────
    /// 📚 학습 포인트: 조건부 알림
    /// ───────────────────────────────────────────────────────────
    /// 
    /// Mathf.Abs(previousValue - _value) > ValueChangeThreshold
    /// 
    /// 이 조건은:
    /// - 이전 값과 현재 값의 차이가 임계값보다 클 때만 true
    /// - 부동소수점 오차로 인한 불필요한 이벤트 방지
    /// 
    /// 예시:
    /// - 이전: 100.0, 현재: 100.0 → 차이 0 → 이벤트 X
    /// - 이전: 100.0, 현재: 99.5  → 차이 0.5 → 이벤트 O
    /// - 이전: 100.0, 현재: 100.0001 → 차이 0.0001 → 이벤트 X (오차)
    /// 
    /// ───────────────────────────────────────────────────────────
    /// </summary>
    private void NotifyIfValueChanged(float previousValue)
    {
        // 값 변화가 임계값보다 클 때만 이벤트 발행
        if (Mathf.Abs(previousValue - _value) > ValueChangeThreshold)
        {
            NotifyChanged();
        }
    }

    /// <summary>
    /// 구독자에게 데이터 변경을 알립니다.
    /// </summary>
    private void NotifyChanged()
    {
        _onDataChanged?.Invoke();
    }
}
