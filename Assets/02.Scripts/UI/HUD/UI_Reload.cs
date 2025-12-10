using TMPro;
using UnityEngine;
using DG.Tweening;

public class UI_Reload : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private TextMeshProUGUI _reloadText;

    [Header("페이드 시간 설정")]
    [SerializeField] private float _fadeInDuration = 0.2f;
    [SerializeField] private float _visibleDuration = 0.4f;
    [SerializeField] private float _fadeOutDuration = 0.2f;

    private void Awake()
    {
        if (_reloadText == null)
            _reloadText = GetComponent<TextMeshProUGUI>();
        // 시작 시 비활성화
        _reloadText.gameObject.SetActive(false);
    }

    // 재장전 시 텍스트가 연속적으로 페이드 인/아웃 되도록 설정
    public void ShowReloadText()
    {
        // 1. 기존에 실행 중이던 트윈이 있다면 즉시 종료 (연속 재장전 시 꼬임 방지)
        _reloadText.DOKill();
        // 2. 텍스트 활성화 및 초기 알파값 설정
        _reloadText.gameObject.SetActive(true);
        Color initialColor = _reloadText.color;
        initialColor.a = 0f;
        _reloadText.color = initialColor;
        // 3. 페이드 인/유지/페이드 아웃 시퀀스 생성
        Sequence reloadSequence = DOTween.Sequence();
        reloadSequence.Append(_reloadText.DOFade(1f, _fadeInDuration)); // 페이드 인
        reloadSequence.AppendInterval(_visibleDuration);                 // 유지
        reloadSequence.Append(_reloadText.DOFade(0f, _fadeOutDuration)); // 페이드 아웃
        reloadSequence.Append(_reloadText.DOFade(1f, _fadeInDuration)); // 페이드 인
        reloadSequence.AppendInterval(_visibleDuration);                 // 유지
        reloadSequence.Append(_reloadText.DOFade(0f, _fadeOutDuration)); // 페이드 아웃
        // 4. 시퀀스 완료 후 텍스트 비활성화
        reloadSequence.OnComplete(() =>
        {
            _reloadText.gameObject.SetActive(false);
        });
    }
}
