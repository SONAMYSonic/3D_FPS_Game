using System;
using UnityEngine;
using DG.Tweening;

public class CameraFollow : MonoBehaviour
{
    // 카메라 뷰 변경 이벤트 (true: 탑뷰, false: FPS/TPS)
    public static event Action<bool> OnTopViewChanged;

    [Header("Targets")]
    public Transform[] CameraTargets;
    public Transform Player; // 플레이어 Transform (탑뷰용)

    [Header("Settings")]
    public float CameraMoveDuration = 0.5f;
    public int TopViewIndex = 2; // 탑뷰 카메라 인덱스

    [Header("TopView Settings")]
    public Vector3 TopViewOffset = new Vector3(6, 8, -4); // 플레이어로부터의 오프셋
    public Vector3 TopViewRotation = new Vector3(35, -40, 0); // 탑뷰 카메라 회전 (아래를 바라봄)

    // 현재 카메라 인덱스
    private int _currentTargetIndex = 0;

    // 현재 타겟 반환
    public Transform CurrentTarget => CameraTargets[_currentTargetIndex];

    // 현재 카메라 인덱스 반환
    public int CurrentTargetIndex => _currentTargetIndex;

    // 탑뷰 상태인지 확인
    public bool IsTopView => _currentTargetIndex == TopViewIndex;

    // 이동 중인지 체크하는 플래그
    private bool _isTransitioning = false;

    // 싱글톤 (다른 스크립트에서 쉽게 접근)
    public static CameraFollow Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // 초기 위치 설정
        if (CameraTargets != null && CameraTargets.Length > 0)
        {
            transform.position = CurrentTarget.position;
            transform.rotation = CurrentTarget.rotation;
        }

        // 초기 카메라 뷰 상태 이벤트 발생
        NotifyViewChanged();
    }

    private void LateUpdate()
    {
        if (CameraTargets == null || CameraTargets.Length == 0) return;

        // 1. 입력 처리
        if (Input.GetKeyDown(KeyCode.T))
        {
            SwitchToNextCamera();
        }

        // 2. 이동 중이 아닐 때만 따라다님
        if (!_isTransitioning)
        {
            if (IsTopView && Player != null)
            {
                // 탑뷰: 플레이어 위치 + 오프셋 (월드 공간 기준, 회전 영향 없음)
                transform.position = Player.position + TopViewOffset;
                transform.rotation = Quaternion.Euler(TopViewRotation);
            }
            // FPS/TPS: 위치 업데이트 제거 - CameraTargets가 플레이어 자식이면 자동으로 따라감
            // 만약 카메라가 플레이어 자식이 아니라면 아래 주석 해제
            else
            {
                transform.position = CurrentTarget.position;
                // Y축 회전만 타겟을 따라가고, X축은 CameraRotate가 설정한 값 유지
                Vector3 currentEuler = transform.eulerAngles;
                Vector3 targetEuler = CurrentTarget.eulerAngles;
                transform.eulerAngles = new Vector3(currentEuler.x, targetEuler.y, 0);
            }
        }
    }

private void SwitchToNextCamera()
    {
        // 다음 인덱스로 순환 (마지막이면 0으로 돌아감)
        _currentTargetIndex = (_currentTargetIndex + 1) % CameraTargets.Length;
        StartCameraTransition();
    }

    // 특정 인덱스의 카메라로 전환
    public void SwitchToCamera(int index)
    {
        if (index < 0 || index >= CameraTargets.Length) return;
        _currentTargetIndex = index;
        StartCameraTransition();
    }

    // 카메라 전환 로직 (위치/회전 보간 및 커서 상태 업데이트)
    private void StartCameraTransition()
    {
        _isTransitioning = true;

        // 기존 트윈 제거 (안전장치)
        transform.DOKill();

        // 목표 위치/회전 계산
        Vector3 targetPosition;
        Quaternion targetRotation;

        if (IsTopView && Player != null)
        {
            targetPosition = Player.position + TopViewOffset;
            targetRotation = Quaternion.Euler(TopViewRotation);
        }
        else
        {
            targetPosition = CurrentTarget.position;
            targetRotation = CurrentTarget.rotation;
        }

        // 위치 이동
        transform.DOMove(targetPosition, CameraMoveDuration)
            .SetEase(Ease.OutQuad)
            .SetId(this)
            .OnComplete(() =>
            {
                _isTransitioning = false;
            });

        // 회전도 부드럽게
        transform.DORotateQuaternion(targetRotation, CameraMoveDuration)
            .SetEase(Ease.OutQuad)
            .SetId(this);

        // 카메라 뷰 변경 이벤트 발생
        NotifyViewChanged();
    }

    // 카메라 뷰 변경 이벤트 발생
    private void NotifyViewChanged()
    {
        OnTopViewChanged?.Invoke(IsTopView);
    }

    private void OnDestroy()
    {
        DOTween.Kill(this);
    }
}
