using UnityEngine;
using DG.Tweening; // DoTween 네임스페이스 필수

public class UI_Crosshair : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private RectTransform _crosshairRect;

    [Header("Settings")]
    [SerializeField] private float _restingSize = 50f;    // 평상시 크기
    [SerializeField] private float _expandedSize = 100f;  // 발사 시 커지는 크기
    [SerializeField] private float _recoverDuration = 0.2f; // 원래대로 돌아오는 시간 (초)

    private void Awake()
    {
        if (_crosshairRect == null)
            _crosshairRect = GetComponent<RectTransform>();

        // 시작 시 사이즈 초기화
        _crosshairRect.sizeDelta = new Vector2(_restingSize, _restingSize);
    }

    public void ReactToFire()
    {
        // 1. 기존에 실행 중이던 트윈이 있다면 즉시 종료 (연사 시 꼬임 방지)
        _crosshairRect.DOKill();

        // 2. 발사 순간 즉시 커지게 설정 (타격감)
        _crosshairRect.sizeDelta = new Vector2(_expandedSize, _expandedSize);

        // 3. 지정된 시간(_recoverDuration) 동안 원래 크기로 부드럽게 복귀
        // SetEase(Ease.OutQuad)는 끝부분에서 부드럽게 감속하여 자연스러운 느낌을 줍니다.
        _crosshairRect.DOSizeDelta(new Vector2(_restingSize, _restingSize), _recoverDuration)
                      .SetEase(Ease.OutQuad);
    }
}