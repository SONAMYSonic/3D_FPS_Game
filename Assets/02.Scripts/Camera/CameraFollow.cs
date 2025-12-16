using UnityEngine;
using DG.Tweening;

public class CameraFollow : MonoBehaviour
{
    [Header("Targets")]
    public Transform[] CameraTargets;

    [Header("Settings")]
    public float CameraMoveDuration = 0.5f;

    // 현재 카메라 인덱스
    private int _currentTargetIndex = 0;

    // 현재 타겟 반환
    public Transform CurrentTarget => CameraTargets[_currentTargetIndex];

    // 이동 중인지 체크하는 플래그
    private bool _isTransitioning = false;

    private void Start()
    {
        // 초기 위치 설정
        if (CameraTargets != null && CameraTargets.Length > 0)
        {
            transform.position = CurrentTarget.position;
            transform.rotation = CurrentTarget.rotation;
        }
    }

    private void LateUpdate()
    {
        if (CameraTargets == null || CameraTargets.Length == 0) return;

        // 1. 입력 처리
        if (Input.GetKeyDown(KeyCode.T))
        {
            SwitchToNextCamera();
        }

        // 2. 이동 중이 아닐 때만 타겟을 '딱' 붙여서 따라다님
        if (!_isTransitioning)
        {
            transform.position = CurrentTarget.position;
        }
    }

    private void SwitchToNextCamera()
    {
        // 다음 인덱스로 순환 (마지막이면 0으로 돌아감)
        _currentTargetIndex = (_currentTargetIndex + 1) % CameraTargets.Length;

        _isTransitioning = true;

        // 기존 트윈 제거 (안전장치)
        transform.DOKill();

        // 위치 이동
        transform.DOMove(CurrentTarget.position, CameraMoveDuration)
            .SetEase(Ease.OutQuad)
            .SetId(this)
            .OnComplete(() =>
            {
                _isTransitioning = false;
            });

        // 회전도 부드럽게
        transform.DORotateQuaternion(CurrentTarget.rotation, CameraMoveDuration)
            .SetEase(Ease.OutQuad)
            .SetId(this);
    }

    /// <summary>
    /// 특정 인덱스의 카메라로 전환
    /// </summary>
    public void SwitchToCamera(int index)
    {
        if (index < 0 || index >= CameraTargets.Length) return;

        _currentTargetIndex = index;
        _isTransitioning = true;

        transform.DOKill();

        transform.DOMove(CurrentTarget.position, CameraMoveDuration)
            .SetEase(Ease.OutQuad)
            .SetId(this)
            .OnComplete(() =>
            {
                _isTransitioning = false;
            });

        transform.DORotateQuaternion(CurrentTarget.rotation, CameraMoveDuration)
            .SetEase(Ease.OutQuad)
            .SetId(this);
    }

    private void OnDestroy()
    {
        DOTween.Kill(this);
    }
}
