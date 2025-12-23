// DroneApproachState.cs
using UnityEngine;

public class DroneApproachState : IDroneState
{
    private readonly StrikerDroneController _drone;
    private Vector3 _currentVelocity;

    public DroneApproachState(StrikerDroneController drone)
    {
        _drone = drone;
    }

    public void Enter() { }

    public void Execute()
    {
        // 타겟 유효성 검사
        if (IsTargetInvalid())
        {
            _drone.ClearTarget();
            _drone.ChangeState(new DroneIdleState(_drone));
            return;
        }

        MoveToTacticalPosition();
        CheckAttackRange();
    }

    public void Exit() { }

    private bool IsTargetInvalid()
    {
        return _drone.CurrentTarget == null || !_drone.CurrentTarget.gameObject.activeInHierarchy;
    }

    private void MoveToTacticalPosition()
    {
        // 전술적 위치: 타겟의 약간 위쪽 (공중 드론의 이점)
        Vector3 tacticalOffset = Vector3.up * 3.0f;
        Vector3 targetPos = _drone.CurrentTarget.position + tacticalOffset;

        // 적을 향해 이동
        _drone.transform.position = Vector3.SmoothDamp(
            _drone.transform.position,
            targetPos,
            ref _currentVelocity,
            0.5f,
            _drone.MoveSpeed
        );

        // 이동 중에도 적을 바라봄
        Vector3 dirToTarget = (_drone.CurrentTarget.position - _drone.transform.position).normalized;
        if (dirToTarget != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(dirToTarget);
            _drone.transform.rotation = Quaternion.Slerp(_drone.transform.rotation, lookRot, Time.deltaTime * 5.0f);
        }
    }

    private void CheckAttackRange()
    {
        float distSqr = (_drone.CurrentTarget.position - _drone.transform.position).sqrMagnitude;

        // 사거리 내에 들어오면 공격 시작
        if (distSqr <= _drone.AttackRange * _drone.AttackRange)
        {
            _drone.ChangeState(new DroneAttackState(_drone));
        }
    }
}