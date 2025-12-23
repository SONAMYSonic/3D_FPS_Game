// DroneIdleState.cs
using UnityEngine;

public class DroneIdleState : IDroneState
{
    private readonly StrikerDroneController _drone;
    private Vector3 _currentVelocity; // SmoothDamp용 지역 변수

    public DroneIdleState(StrikerDroneController drone)
    {
        _drone = drone;
    }

    public void Enter()
    {
        // 대기 애니메이션 재생 등
    }

    public void Execute()
    {
        if (_drone.PlayerTransform == null) return;

        // 1. 이동 로직: 플레이어 추적
        FollowPlayer();

        // 2. 타겟 탐색 로직
        // 복귀 명령이 없고, 타겟이 없을 때만 탐색
        if (!_drone.IsRecalled() && _drone.CurrentTarget == null)
        {
            CheckForEnemies();
        }
    }

    public void Exit() { }

    private void FollowPlayer()
    {
        // 플레이어의 엄폐 여부에 따라 높이 조절
        Vector3 targetOffset = _drone.IsPlayerInCover()
            ? new Vector3(1.5f, 1.0f, -0.5f) // 낮게
            : new Vector3(1.5f, 2.5f, -1.0f); // 기본 높이

        // 플레이어 로컬 좌표계를 월드 좌표계로 변환
        Vector3 targetPos = _drone.PlayerTransform.position + _drone.PlayerTransform.TransformDirection(targetOffset);

        // 부드러운 이동 (SmoothDamp)
        _drone.transform.position = Vector3.SmoothDamp(
            _drone.transform.position,
            targetPos,
            ref _currentVelocity,
            0.5f // 반응 속도 (낮을수록 빠름)
        );

        // 회전: 플레이어가 보는 방향을 천천히 따라봄
        Quaternion targetRot = _drone.PlayerTransform.rotation;
        _drone.transform.rotation = Quaternion.Slerp(_drone.transform.rotation, targetRot, Time.deltaTime * 2.0f);
    }

    private void CheckForEnemies()
    {
        // 최적화: 매 프레임 OverlapSphere는 무거우므로 타이머를 두거나,
        // 여기서는 단순화하여 즉시 상태 전환
        _drone.ChangeState(new DroneAcquireTargetState(_drone));
    }
}