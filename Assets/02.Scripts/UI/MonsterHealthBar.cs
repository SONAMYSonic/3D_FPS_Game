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
    [SerializeField] private int _shakeVibrato = 20;            // 흔들림 세기 (진동)
    [SerializeField] private float _shakeRandomness = 90f;      // 흔들림 무작위성

    private float _lastHealth = -1f;
    private Color _gaugeOriginalColor;
    private void Awake()
    {
        _monster = GetComponent<Monster>();
    }

    private void Start()
    {
        _gaugeOriginalColor = _gaugeImage.color;
        _lastHealth = _monster.CurrentHealth;
    }

    private void LateUpdate()
    {
        // 디미터 법칙 준수: Monster의 프로퍼티를 통해 체력 정보에 접근
        if (_lastHealth != _monster.CurrentHealth)
        {
            float newFillAmount = _monster.HealthRatio;
            
            // 체력이 감소했을 때만 이펙트 재생
            if (_monster.CurrentHealth < _lastHealth)
            {
                PlayDamageEffect(newFillAmount);
            }
            else
            {
                // 체력 회복 시에는 바로 적용
                _gaugeImage.fillAmount = newFillAmount;
                _gaugeBackImage.fillAmount = newFillAmount;
            }

            _lastHealth = _monster.CurrentHealth;
        }

        // 빌보드 기법: 카메라를 바라보도록 설정
        // Camera.main.transform.forward는 자식이어도 월드 좌표 기준이므로 정상 작동해야 함
        // 하지만 카메라가 회전하지 않는 경우, 카메라를 직접 바라보게 설정
        Vector3 directionToCamera = Camera.main.transform.position - _healthBarTransform.position;
        _healthBarTransform.forward = -directionToCamera.normalized;
    }

    private void PlayDamageEffect(float targetFillAmount)
    {
        // 기존 트윈 정리
        DOTween.Kill(this);

        // 1. 흰색 플래시 효과
        _gaugeImage.color = Color.white;
        _gaugeImage.DOColor(_gaugeOriginalColor, _flashDuration)
            .SetEase(Ease.OutQuad)
            .SetId(this);

        // 2. 앞 게이지 즉시 감소
        _gaugeImage.fillAmount = targetFillAmount;

        // 3. 뒷 게이지 딜레이 후 천천히 감소
        _gaugeBackImage.DOFillAmount(targetFillAmount, _gaugeBackDuration)
            .SetDelay(_gaugeBackDelay)
            .SetEase(Ease.OutQuad)
            .SetId(this);

        // 4. HP바 흔들림 효과
        _healthBarTransform.DOShakePosition(_shakeDuration, _shakeStrength, _shakeVibrato, _shakeRandomness, false, true)
            .SetId(this);
    }

    private void OnDestroy()
    {
        // 이 컴포넌트에 할당된 모든 트윈을 한 번에 정리
        DOTween.Kill(this);
    }
}
