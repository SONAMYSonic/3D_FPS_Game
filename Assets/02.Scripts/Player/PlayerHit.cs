using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(PlayerStats))]
public class PlayerHit : MonoBehaviour
{
    private PlayerStats _stats;
    public Image DamageGlowScreen;
    public Image LowHealthScreen;

    [Header("대미지 이펙트 설정")]
    [SerializeField] private float _flashDuration = 0.2f;
    [SerializeField] private float _flashAlpha = 0.8f;
    [SerializeField] private float _lowHealthThreshold = 0.3f;

    [Header("플레이어 애니메이터")]
    [SerializeField] private Animator _soliderAnimator;

    private float _previousHealthPercent;

    /// <summary>
    /// 플레이어의 월드 위치를 반환합니다.
    /// </summary>
    public Vector3 Position => transform.position;
    public bool IsDead => _stats.IsDead;

    private void Awake()
    {
        _stats = GetComponent<PlayerStats>();
    }

    private void Start()
    {
        // 초기 상태: 투명하게 설정
        SetImageAlpha(DamageGlowScreen, 0f);
        SetImageAlpha(LowHealthScreen, 0f);
        DamageGlowScreen.gameObject.SetActive(false);
        LowHealthScreen.gameObject.SetActive(false);

        _previousHealthPercent = _stats.HealthPercent;
    }

    private void Update()
    {
        float healthPercent = _stats.HealthPercent;

        // 체력이 회복되었을 때 LowHealthScreen 업데이트
        if (healthPercent > _previousHealthPercent)
        {
            UpdateLowHealthScreen(healthPercent);
        }

        _previousHealthPercent = healthPercent;
    }

    private void UpdateLowHealthScreen(float healthPercent)
    {
        if (!LowHealthScreen.gameObject.activeSelf) return;

        // 체력이 임계값보다 높아지면 페이드 아웃
        if (healthPercent > _lowHealthThreshold)
        {
            LowHealthScreen.DOKill();
            LowHealthScreen.DOFade(0f, _flashDuration).SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    LowHealthScreen.gameObject.SetActive(false);
                });
        }
        // 체력이 임계값 이하일 때 알파값 업데이트
        else
        {
            float lowHealthAlpha = 1f - (healthPercent / _lowHealthThreshold);
            SetImageAlpha(LowHealthScreen, lowHealthAlpha);
        }
    }

    public void TakeDamage(float damage)
    {
        _stats.DecreaseHealth(damage);
        Debug.Log("플레이어가 대미지를 입었다!");

        // 대미지 피드백 UI 효과
        PlayDamageEffect();

        if (_stats.IsDead)
        {
            Debug.Log("플레이어가 사망했다!");
            GameManager.Instance.GameOver();
            _soliderAnimator.SetTrigger("Death");
        }
    }

    private void PlayDamageEffect()
    {
        float healthPercent = _stats.HealthPercent;
        float targetAlpha = 1f - healthPercent; // 체력이 낮을수록 진해짐

        // DamageGlowScreen 반짝임 효과
        DamageGlowScreen.gameObject.SetActive(true);
        DamageGlowScreen.DOKill();
        
        // 반짝인 후 체력에 맞는 알파값으로 돌아옴
        DOTween.Sequence()
            .Append(DamageGlowScreen.DOFade(_flashAlpha, _flashDuration / 2).SetEase(Ease.OutQuad))
            .Append(DamageGlowScreen.DOFade(targetAlpha, _flashDuration / 2).SetEase(Ease.InQuad))
            .OnComplete(() =>
            {
                DamageGlowScreen.gameObject.SetActive(false);
            });

        // LowHealthScreen: 대미지를 받으면 항상 반짝이고, 체력이 낮을 때만 유지
        LowHealthScreen.gameObject.SetActive(true);
        LowHealthScreen.DOKill();

        float lowHealthAlpha = 0f;
        if (healthPercent <= _lowHealthThreshold)
        {
            lowHealthAlpha = 1f - (healthPercent / _lowHealthThreshold); // 임계값 기준으로 알파 계산
        }

        DOTween.Sequence()
            .Append(LowHealthScreen.DOFade(_flashAlpha, _flashDuration / 2).SetEase(Ease.OutQuad))
            .Append(LowHealthScreen.DOFade(lowHealthAlpha, _flashDuration / 2).SetEase(Ease.InQuad))
            .OnComplete(() =>
            {
                // 체력이 임계값보다 높으면 비활성화
                if (healthPercent > _lowHealthThreshold)
                {
                    LowHealthScreen.gameObject.SetActive(false);
                }
            });
    }

    private void SetImageAlpha(Image image, float alpha)
    {
        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }
}
