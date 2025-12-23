// DroneAcquireTargetState.cs
using UnityEngine;

public class DroneAcquireTargetState : IDroneState
{
    private readonly StrikerDroneController _drone;
    // OverlapSphere 결과를 담을 버퍼 (GC Alloc 방지)
    private readonly Collider[] _hitColliders = new Collider[10];

    public DroneAcquireTargetState(StrikerDroneController drone)
    {
        _drone = drone;
    }

    public void Enter() { }

    public void Execute()
    {
        if (_drone.IsRecalled()) return;

        Transform bestTarget = FindClosestEnemy();

        if (bestTarget != null)
        {
            _drone.SetTarget(bestTarget);
            // 타겟을 찾았으면 접근 상태로 전환
            _drone.ChangeState(new DroneApproachState(_drone));
        }
        else
        {
            // 적이 없으면 다시 Idle (대기)
            _drone.ChangeState(new DroneIdleState(_drone));
        }
    }

    public void Exit() { }

    private Transform FindClosestEnemy()
    {
        int hitCount = Physics.OverlapSphereNonAlloc(
            _drone.transform.position,
            _drone.ScanRadius,
            _hitColliders,
            _drone.EnemyLayer
        );

        Transform closest = null;
        float closestDistSqr = Mathf.Infinity;
        Vector3 currentPos = _drone.transform.position;

        for (int i = 0; i < hitCount; i++)
        {
            Collider col = _hitColliders[i];
            if (col == null) continue;

            float distSqr = (col.transform.position - currentPos).sqrMagnitude;
            if (distSqr < closestDistSqr)
            {
                closestDistSqr = distSqr;
                closest = col.transform;
            }
        }

        return closest;
    }
}