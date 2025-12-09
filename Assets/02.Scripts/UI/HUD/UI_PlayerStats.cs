using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_PlayerStats : MonoBehaviour
{
    // 플레이어의 스탯 UI(슬라이더)를 관리하는 스크립트
    // 체력, 스태미나

    [SerializeField] private PlayerStats _stats;
    [SerializeField] private Slider _healthSlider;
    [SerializeField] private Slider _staminaSlider;
    [SerializeField] private TextMeshProUGUI _bombText;

    private void Update()
    {
        _healthSlider.value = _stats.Health.Value / _stats.Health.MaxValue;
        _staminaSlider.value = _stats.Stamina.Value / _stats.Stamina.MaxValue;
        _bombText.text = $"X {_stats.Bomb.Value}";
    }

}
