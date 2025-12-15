using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Monster))]
public class MonsterHealthBar : MonoBehaviour
{
    private Monster _monster;
    [SerializeField] private Image _gaugeImage;
    [SerializeField] private Image _gaugeBackImage;
    [SerializeField] private Transform _healthBarTransform;

    [Header("체력바 이펙트 설정")]
    [SerializeField] private float _gaugeBackDelay = 0.3f;      // 뒷 게이지 딜레이
    [SerializeField] private float _gaugeBackDuration = 0.4f;   // 뒷 게이지 줄어드는 시간
    [SerializeField] private float _flashDuration = 0.1f;       // 흰색 플래시 지속 시간
    [SerializeField] private float _shakeDuration = 0.2f;       // 흔들림 지속 시간
    [SerializeField] private float _shakeStrength = 5f;         // 흔들림 강도

    private float _lastHealth = -1f;
    private Color _gaugeOriginalColor;
    private Tweener _gaugeBackTweener;

    private void Awake()
    {
        _monster = GetComponent<Monster>();
    }

    private void Start()
    {
        _gaugeOriginalColor = _gaugeImage.color;
        _lastHealth = _monster.Health.Value;
    }

    private void LateUpdate()
    {
        if (_lastHealth != _monster.Health.Value)
        {
            float newFillAmount = _monster.Health.Value / _monster.Health.MaxValue;
            
            // 체력이 감소했을 때만 이펙트 재생
            if (_monster.Health.Value < _lastHealth)
            {
                PlayDamageEffect(newFillAmount);
            }
            else
            {
                // 체력 회복 시에는 바로 적용
                _gaugeImage.fillAmount = newFillAmount;
                _gaugeBackImage.fillAmount = newFillAmount;
            }

            _lastHealth = _monster.Health.Value;
        }

        // 빌보드 기법
        _healthBarTransform.forward = Camera.main.transform.forward;
    }

    private void PlayDamageEffect(float targetFillAmount)
    {
        // 1. 흰색 플래시 효과
        _gaugeImage.DOKill();
        _gaugeImage.color = Color.white;
        _gaugeImage.DOColor(_gaugeOriginalColor, _flashDuration).SetEase(Ease.OutQuad);

        // 2. 앞 게이지 즉시 감소
        _gaugeImage.fillAmount = targetFillAmount;

        // 3. 뒷 게이지 딜레이 후 천천히 감소
        _gaugeBackTweener?.Kill();
        _gaugeBackTweener = _gaugeBackImage
            .DOFillAmount(targetFillAmount, _gaugeBackDuration)
            .SetDelay(_gaugeBackDelay)
            .SetEase(Ease.OutQuad);

        // 4. HP바 흔들림 효과
        _healthBarTransform.DOKill();
        _healthBarTransform.DOShakePosition(_shakeDuration, _shakeStrength, 20, 90, false, true);
    }

    private void OnDestroy()
    {
        // 트윈 정리
        _gaugeImage.DOKill();
        _gaugeBackImage.DOKill();
        _healthBarTransform.DOKill();
    }
}
