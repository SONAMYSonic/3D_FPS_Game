// DroneAttackState.cs
using UnityEngine;

public class DroneAttackState : IDroneState
{
    private readonly StrikerDroneController _drone;
    private float _lastFireTime;
    private Vector3 _currentVelocity; // SmoothDamp용

    // 조준 보간 속도 배율
    private const float TURRET_AIM_SPEED_MULTIPLIER = 10f;

    // 공격 범위 버퍼 (적이 살짝 벗어나도 바로 추적으로 전환하지 않도록)
    private const float ATTACK_RANGE_BUFFER = 1.2f;

    public DroneAttackState(StrikerDroneController drone)
    {
        _drone = drone;
        _lastFireTime = Time.time;
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

        // 1. 적을 따라가면서 전술적 위치 유지
        FollowTarget();

        // 2. 조준
        AimAtTarget();

        // 3. 사격
        if (Time.time >= _lastFireTime + _drone.FireRate)
        {
            Fire();
            _lastFireTime = Time.time;
        }

        // 4. 거리 체크 (적이 너무 멀어지면 추적 상태로 전환)
        float distSqr = (_drone.CurrentTarget.position - _drone.transform.position).sqrMagnitude;
        if (distSqr > _drone.AttackRange * _drone.AttackRange * ATTACK_RANGE_BUFFER)
        {
            _drone.ChangeState(new DroneApproachState(_drone));
        }
    }

    public void Exit() { }

    private bool IsTargetInvalid()
    {
        return _drone.CurrentTarget == null || !_drone.CurrentTarget.gameObject.activeInHierarchy;
    }

    /// <summary>
    /// 적을 따라가면서 전술적 위치를 유지합니다.
    /// </summary>
    private void FollowTarget()
    {
        // 타겟 위치 + 설정된 높이로 전술적 위치 계산
        Vector3 targetPos = _drone.CurrentTarget.position + Vector3.up * _drone.CombatHoverHeight;

        // 부드럽게 이동
        _drone.SmoothMoveToPosition(targetPos, ref _currentVelocity);
    }

    private void AimAtTarget()
    {
        // 드론 몸체 회전 (타겟을 향해)
        Vector3 dirToTarget = (_drone.CurrentTarget.position - _drone.transform.position).normalized;
        if (dirToTarget != Vector3.zero)
        {
            Quaternion bodyLookRot = Quaternion.LookRotation(dirToTarget);
            _drone.transform.rotation = Quaternion.Slerp(
                _drone.transform.rotation,
                bodyLookRot,
                Time.deltaTime * _drone.BodyRotationSpeed
            );
        }

        // 포탑(Head) 회전 (빠르고 정확하게)
        Vector3 dirFromGun = (_drone.CurrentTarget.position - _drone.TurretHead.position).normalized;
        if (dirFromGun != Vector3.zero)
        {
            Quaternion gunLookRot = Quaternion.LookRotation(dirFromGun);
            _drone.TurretHead.rotation = Quaternion.RotateTowards(
                _drone.TurretHead.rotation,
                gunLookRot,
                _drone.RotationSpeed * TURRET_AIM_SPEED_MULTIPLIER * Time.deltaTime
            );
        }
    }

    private void Fire()
    {
        _drone.PlayMuzzleFlash();

        Vector3 fireOrigin = _drone.FirePoint.position;
        Vector3 fireDirection = _drone.TurretHead.forward;
        Ray ray = new Ray(fireOrigin, fireDirection);

        Vector3 targetPosition;
        bool isHit = Physics.Raycast(ray, out RaycastHit hitInfo, _drone.MaxShootRange, _drone.EnemyLayer);

        if (isHit)
        {
            targetPosition = hitInfo.point;
            _drone.PlayHitEffect(hitInfo.point, hitInfo.normal);
            ApplyDamage(hitInfo);
            Debug.Log($"드론 사격 명중! 대상: {hitInfo.collider.name}");
        }
        else
        {
            targetPosition = fireOrigin + fireDirection * _drone.MaxShootRange;
        }

        _drone.StartBulletTrail(targetPosition);
    }

    private void ApplyDamage(RaycastHit hitInfo)
    {
        IDamageable damageable = hitInfo.collider.GetComponent<IDamageable>();
        if (damageable != null)
        {
            Damage damage = new Damage
            {
                Value = _drone.Damage,
                HitPoint = hitInfo.point,
                HitDirection = (hitInfo.point - _drone.FirePoint.position).normalized,
                Who = _drone.gameObject,
                Critical = false
            };

            damageable.TryTakeDamage(damage);
        }
    }
}