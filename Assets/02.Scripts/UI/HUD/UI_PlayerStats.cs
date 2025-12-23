using UnityEngine;
using UnityEngine.UI;

public class UI_PlayerStats : MonoBehaviour
{
    [SerializeField] private PlayerStats _stats;
    [SerializeField] private Slider _healthSlider;
    [SerializeField] private Slider _staminaSlider;


    private void Start()
    {
        Debug.Log("UI_PlayerStats initialized");

        Refresh();

        // 플레이어 스탯의 데이터의 변화가 있을때 실행할 콜백 함수를 등록
        PlayerStats.OnDataChanged += Refresh;
    }


    public void Refresh()
    {
        // 유튜브에 영상이 올라왔는지 매번 새로고침 하는것과 똑같다.
        // 시각적인 변화가 없음에도 데이터를 참조하고 UI를 수정하므로 성능이 저하된다.

        _healthSlider.value = _stats.Health.Value / _stats.Health.MaxValue;
        _staminaSlider.value = _stats.Stamina.Value / _stats.Stamina.MaxValue;
    }
}