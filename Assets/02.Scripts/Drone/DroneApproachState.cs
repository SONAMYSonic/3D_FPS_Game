// DroneApproachState.cs
using UnityEngine;

public class DroneApproachState : IDroneState
{
    private readonly StrikerDroneController _drone;
    private Vector3 _currentVelocity;

    // 이동 중 회전 속도 배율
    private const float APPROACH_ROTATION_SPEED_MULTIPLIER = 1.0f;

    public DroneApproachState(StrikerDroneController drone)
    {
        _drone = drone;
    }

    public void Enter()
    {
        _currentVelocity = Vector3.zero;
    }

    public void Execute()
    {
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
        // 전술적 위치: 타겟 위쪽 (Inspector에서 설정한 높이 사용)
        Vector3 targetPos = _drone.CurrentTarget.position + Vector3.up * _drone.CombatHoverHeight;

        // 부드럽게 이동
        _drone.SmoothMoveToPosition(targetPos, ref _currentVelocity);

        // 이동 중에도 적을 바라봄
        Vector3 dirToTarget = (_drone.CurrentTarget.position - _drone.transform.position).normalized;
        if (dirToTarget != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(dirToTarget);
            _drone.transform.rotation = Quaternion.Slerp(
                _drone.transform.rotation,
                lookRot,
                Time.deltaTime * _drone.BodyRotationSpeed * APPROACH_ROTATION_SPEED_MULTIPLIER
            );
        }
    }

    private void CheckAttackRange()
    {
        float distSqr = (_drone.CurrentTarget.position - _drone.transform.position).sqrMagnitude;

        if (distSqr <= _drone.AttackRange * _drone.AttackRange)
        {
            _drone.ChangeState(new DroneAttackState(_drone));
        }
    }
}