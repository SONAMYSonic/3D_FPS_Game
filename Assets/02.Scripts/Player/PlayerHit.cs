using UnityEngine;
using UnityEngine.InputSystem.XR;

[RequireComponent(typeof(PlayerStats))]
public class PlayerHit : MonoBehaviour
{
    private PlayerStats _stats;

    private void Awake()
    {
        _stats = GetComponent<PlayerStats>();
    }

    public void TakeDamage(float damage)
    {
        _stats.Health.Decrease(damage);
        Debug.Log("플레이어가 대미지를 입었다!");
    }
}
