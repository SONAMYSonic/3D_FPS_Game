using UnityEngine;
using UnityEngine.UI;

public class UI_PlayerStats : MonoBehaviour
{
    // 플레이어의 스탯 UI(슬라이더)를 관리하는 스크립트
    // 체력, 스태미나

    public Slider HealthSlider;
    public Slider StaminaSlider;

    // 플레이어의 현재 체력, 최대 체력
    private float _currentHealth = 100f;
    private float _maxHealth = 100f;

    // 플레이어의 현재 스태미나, 최대 스태미나
    private float _currentStamina = 100f;
    private float _maxStamina = 100f;

    private void Start()
    {
        // 초기화
        UpdateHealthUI();
        UpdateStaminaUI();
    }

    public void SetHealth(float current, float max)
    {
        _currentHealth = current;
        _maxHealth = max;
        UpdateHealthUI();
    }

    public void SetStamina(float current, float max)
    {
        _currentStamina = current;
        _maxStamina = max;
        UpdateStaminaUI();
    }

    private void UpdateHealthUI()
    {
        if (HealthSlider != null)
        {
            HealthSlider.maxValue = _maxHealth;
            HealthSlider.value = _currentHealth;
        }
    }

    private void UpdateStaminaUI()
    {
        if (StaminaSlider != null)
        {
            StaminaSlider.maxValue = _maxStamina;
            StaminaSlider.value = _currentStamina;
        }
    }


}
