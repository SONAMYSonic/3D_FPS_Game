using System;
using UnityEngine;

[RequireComponent(typeof(PlayerStats))]
public class Player : MonoBehaviour
{
    private PlayerStats _stats;
    [SerializeField] private Animator _animator;


    private void Awake()
    {
        _stats = GetComponent<PlayerStats>();
    }

    public bool TryTakeDamage(float damage)
    {
        if (_stats.Health.Value <= 0) return false;

        _stats.Health.Consume(damage);

        // 플레이어만 레이어 가중치
        //_animator.SetLayerWeight(2, _stats.Health.Value / _stats.Health.MaxValue);

        return true;
    }
}