// DroneAttackState.cs
using UnityEngine;

public class DroneAttackState : IDroneState
{
    private readonly StrikerDroneController _drone;
    private float _lastFireTime;

    public DroneAttackState(StrikerDroneController drone)
    {
        _drone = drone;
        _lastFireTime = Time.time;
    }

    public void Enter()
    {
        // 공격 준비 사운드 등
    }

    public void Execute()
    {
        if (IsTargetInvalid())
        {
            _drone.ClearTarget();
            _drone.ChangeState(new DroneIdleState(_drone));
            return;
        }

        // 1. 호버링 (약간의 상하 움직임 추가 가능)
        HoverInPlace();

        // 2. 조준
        AimAtTarget();

        // 3. 사격
        if (Time.time >= _lastFireTime + _drone.FireRate)
        {
            Fire();
            _lastFireTime = Time.time;
        }

        // 4. 거리 체크 (적이 도망가면 다시 추적)
        float distSqr = (_drone.CurrentTarget.position - _drone.transform.position).sqrMagnitude;
        if (distSqr > _drone.AttackRange * _drone.AttackRange * 1.2f) // 약간의 여유(Buffer) 둠
        {
            _drone.ChangeState(new DroneApproachState(_drone));
        }
    }

    public void Exit() { }

    private bool IsTargetInvalid()
    {
        return _drone.CurrentTarget == null || !_drone.CurrentTarget.gameObject.activeInHierarchy;
    }

    private void HoverInPlace()
    {
        // 제자리에서 아주 살짝 둥실거리는 느낌
        // 실제로는 ApproachState의 위치를 유지하거나, 장애물 회피 로직이 들어감
    }

    private void AimAtTarget()
    {
        // 드론 몸체 회전 (천천히)
        Vector3 dirToTarget = (_drone.CurrentTarget.position - _drone.transform.position).normalized;
        Quaternion bodyLookRot = Quaternion.LookRotation(dirToTarget);
        _drone.transform.rotation = Quaternion.Slerp(_drone.transform.rotation, bodyLookRot, Time.deltaTime * 2.0f);

        // 포탑(Head) 회전 (빠르고 정확하게)
        Vector3 dirFromGun = (_drone.CurrentTarget.position - _drone.TurretHead.position).normalized;
        Quaternion gunLookRot = Quaternion.LookRotation(dirFromGun);
        _drone.TurretHead.rotation = Quaternion.RotateTowards(_drone.TurretHead.rotation, gunLookRot, _drone.RotationSpeed * 10f * Time.deltaTime);
    }

    private void Fire()
    {
        // 실제 레이캐스트 발사 로직
        // RaycastHit hit... ApplyDamage...

        // 이펙트 재생
        _drone.PlayMuzzleFlash();
        Debug.Log("Bang!"); // 테스트용
    }
}