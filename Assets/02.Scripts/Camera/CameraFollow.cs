using UnityEngine;
using DG.Tweening;

public class CameraFollow : MonoBehaviour
{
    [Header("Targets")]
    public Transform FPSTarget;
    public Transform TPSTarget;

    [Header("Settings")]
    public float CameraMoveDuration = 0.5f;

    // 상태 확인용 (public이지만 내부에서만 조작하므로 private set 권장)
    public bool IsFPS { get; private set; } = true;

    // 이동 중인지 체크하는 플래그
    private bool _isTransitioning = false;

    private void LateUpdate()
    {
        // 1. 입력 처리
        if (Input.GetKeyDown(KeyCode.T))
        {
            SwitchCamera();
        }

        // 2. 이동 중이 아닐 때만 타겟을 '딱' 붙여서 따라다님
        if (!_isTransitioning)
        {
            if (IsFPS)
            {
                transform.position = FPSTarget.position;
                transform.rotation = FPSTarget.rotation; // 회전도 같이 맞춰주면 더 자연스럽습니다
            }
            else
            {
                transform.position = TPSTarget.position;
                transform.rotation = TPSTarget.rotation;
            }
        }
    }

    private void SwitchCamera()
    {
        // 상태 반전
        IsFPS = !IsFPS;
        _isTransitioning = true; // "이제부터 기계(DOTween)가 운전한다, 건드리지 마라"

        // 목표 지점 설정
        Transform targetTransform = IsFPS ? FPSTarget : TPSTarget;

        // 기존 트윈 제거 (안전장치)
        transform.DOKill();

        // ★ 핵심: DOMove가 끝나는 순간(OnComplete) 다시 따라다니기 모드로 전환
        transform.DOMove(targetTransform.position, CameraMoveDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                // 이 괄호 안의 코드는 0.5초 뒤 이동이 끝났을 때 실행됩니다.
                _isTransitioning = false;
            });

        // (선택 사항) 회전도 부드럽게 하고 싶다면 같이 실행
        transform.DORotateQuaternion(targetTransform.rotation, CameraMoveDuration)
            .SetEase(Ease.OutQuad);
    }
}
