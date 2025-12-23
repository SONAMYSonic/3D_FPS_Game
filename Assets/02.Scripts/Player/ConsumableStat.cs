using System;
using UnityEngine;

[Serializable]
public class ConsumableStat
{
    [SerializeField] private float _maxValue;
    [SerializeField] private float _value;
    [SerializeField] private float _regenValue;

    public float MaxValue => _maxValue;
    public float Value => _value;

    // 나를 구독하는 구독자 명단(콜백함수 들)
    private event Action _onDataChanged;

    public void Initialize(Action onDataChanged = null)
    {
        _onDataChanged = onDataChanged;

        SetValue(_maxValue);
    }

    public void Regenerate(float time)
    {
        _value += _regenValue * time;

        if (_value > _maxValue)
        {
            _value = _maxValue;
        }

        if (_onDataChanged == null)
        {
            Debug.Log("구독자 없음");
        }
        else
        {
            Debug.Log("구독자 있음");
        }

        _onDataChanged?.Invoke();
    }

    public bool TryConsume(float amount)
    {
        if (_value < amount) return false;

        Consume(amount);

        return true;
    }


    public void Consume(float amount)
    {
        _value -= amount;

        _onDataChanged?.Invoke();
    }

    public void IncreaseMax(float amount)
    {
        _maxValue += amount;

        _onDataChanged?.Invoke();
    }
    public void Increase(float amount)
    {
        SetValue(_value + amount);
    }

    public void DecreaseMax(float amount)
    {
        _maxValue -= amount;
    }
    public void Decrease(float amount)
    {
        _value -= amount;
    }


    public void SetMaxValue(float value)
    {
        _maxValue = value;

        _onDataChanged?.Invoke();
    }
    public void SetValue(float value)
    {
        _value = value;

        if (_value > _maxValue)
        {
            _value = _maxValue;
        }

        _onDataChanged?.Invoke();
    }

}