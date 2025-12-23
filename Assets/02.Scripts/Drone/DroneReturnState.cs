// DroneReturnState.cs
using UnityEngine;

public class DroneReturnState : IDroneState
{
    private readonly StrikerDroneController _drone;
    private Vector3 _currentVelocity;

    public DroneReturnState(StrikerDroneController drone)
    {
        _drone = drone;
    }

    public void Enter()
    {
        // 모든 타겟 해제
        _drone.ClearTarget();
    }

    public void Execute()
    {
        if (_drone.PlayerTransform == null) return;

        // 플레이어 몸체로 직접 이동
        Vector3 targetPos = _drone.PlayerTransform.position + Vector3.up * 1.5f;
        float distanceSqr = (targetPos - _drone.transform.position).sqrMagnitude;

        // 이동
        _drone.transform.position = Vector3.SmoothDamp(
            _drone.transform.position,
            targetPos,
            ref _currentVelocity,
            0.5f,
            _drone.MoveSpeed * 1.5f // 복귀는 좀 더 빠르게
        );

        // 도착 확인 (아주 가까워지면)
        if (distanceSqr < 0.5f * 0.5f)
        {
            DeactivateDrone();
        }
    }

    public void Exit() { }

    private void DeactivateDrone()
    {
        // 풀링 시스템을 쓴다면 ReturnToPool(), 아니면 Destroy
        _drone.gameObject.SetActive(false);
        // Destroy(_drone.gameObject);
    }
}