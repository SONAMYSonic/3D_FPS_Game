using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_PlayerStats : MonoBehaviour
{
    // ═══════════════════════════════════════════════════════════
    // 필드
    // ═══════════════════════════════════════════════════════════
    
    [SerializeField, Tooltip("플레이어 스탯 참조")]
    private PlayerStats _stats;
    
    [SerializeField, Tooltip("총 스탯 참조 (탄창 표시용)")]
    private GunStat _gunStat;
    
    [SerializeField, Tooltip("체력 슬라이더")]
    private Slider _healthSlider;
    
    [SerializeField, Tooltip("스태미나 슬라이더")]
    private Slider _staminaSlider;

    [SerializeField, Tooltip("탄창 UI")]
    private TextMeshProUGUI _bulletText;

    [SerializeField, Tooltip("폭탄 UI")]
    private TextMeshProUGUI _bombText;

    // ═══════════════════════════════════════════════════════════
    // Unity 생명주기
    // ═══════════════════════════════════════════════════════════

    private void Start()
    {
        ValidateReferences();
        SubscribeToEvents();
        Refresh();
        
        Debug.Log("[UI_PlayerStats] 초기화 완료 - 이벤트 구독됨");
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
        Debug.Log("[UI_PlayerStats] 파괴됨 - 이벤트 구독 해제됨");
    }

    // ═══════════════════════════════════════════════════════════
    // 초기화 메서드
    // ═══════════════════════════════════════════════════════════

    private void ValidateReferences()
    {
        if (_stats == null)
        {
            Debug.LogError("[UI_PlayerStats] PlayerStats 참조가 없습니다! Inspector에서 설정해주세요.");
        }
        
        if (_gunStat == null)
        {
            Debug.LogWarning("[UI_PlayerStats] GunStat 참조가 없습니다! 탄창 UI가 표시되지 않습니다.");
        }
        
        if (_healthSlider == null)
        {
            Debug.LogError("[UI_PlayerStats] Health Slider 참조가 없습니다!");
        }
        
        if (_staminaSlider == null)
        {
            Debug.LogError("[UI_PlayerStats] Stamina Slider 참조가 없습니다!");
        }
        
        if (_bulletText == null)
        {
            Debug.LogWarning("[UI_PlayerStats] Bullet Text 참조가 없습니다!");
        }
        
        if (_bombText == null)
        {
            Debug.LogWarning("[UI_PlayerStats] Bomb Text 참조가 없습니다!");
        }
    }

    private void SubscribeToEvents()
    {
        PlayerStats.OnDataChanged += Refresh;
    }

    private void UnsubscribeFromEvents()
    {
        PlayerStats.OnDataChanged -= Refresh;
    }

    // ═══════════════════════════════════════════════════════════
    // UI 갱신
    // ═══════════════════════════════════════════════════════════

    public void Refresh()
    {
        if (_stats == null)
        {
            return;
        }

        RefreshHealthBar();
        RefreshStaminaBar();
        RefreshBulletText();
        RefreshBombText();
    }

    private void RefreshHealthBar()
    {
        if (_healthSlider == null)
        {
            return;
        }
        
        _healthSlider.value = CalculatePercent(_stats.Health.Value, _stats.Health.MaxValue);
    }

    private void RefreshStaminaBar()
    {
        if (_staminaSlider == null)
        {
            return;
        }
        
        _staminaSlider.value = CalculatePercent(_stats.Stamina.Value, _stats.Stamina.MaxValue);
    }

    /// <summary>
    /// 탄창 UI를 갱신합니다.
    /// 표시 형식: (현재 탄약)/(남은 탄약)
    /// </summary>
    private void RefreshBulletText()
    {
        if (_bulletText == null || _gunStat == null)
        {
            return;
        }
        
        int currentAmmo = Mathf.RoundToInt(_gunStat.Ammo.Value);
        int remainingAmmo = Mathf.RoundToInt(_gunStat.FullAmmo.Value);
        
        _bulletText.text = $"{currentAmmo}/{remainingAmmo}";
    }

    /// <summary>
    /// 폭탄 UI를 갱신합니다.
    /// 표시 형식: X (폭탄 갯수)
    /// </summary>
    private void RefreshBombText()
    {
        if (_bombText == null)
        {
            return;
        }
        
        int bombCount = Mathf.RoundToInt(_stats.Bomb.Value);
        
        _bombText.text = $"X {bombCount}";
    }

    private float CalculatePercent(float current, float max)
    {
        if (max <= 0f)
        {
            return 0f;
        }
        
        return Mathf.Clamp01(current / max);
    }
}
