using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Monster))]
public class MonsterHealthBar : MonoBehaviour
{
    private Monster _monster;
    [SerializeField] private Image _gaugeImage;
    [SerializeField] private Transform _healthBarTransform;

    private float _lastHealth = -1f;

    private void Awake()
    {
        _monster = GetComponent<Monster>();
    }

    // 화면 갱신만 할 경우에는 LateUpdate를 사용하는게 좋다
    private void LateUpdate()
    {
        // 0 ~ 1
        // UI가 알고 있는 몬스터 체력값과 다를 경우에만 fillAmount 갱신
        if (_lastHealth != _monster.Health.Value)
        {
            _lastHealth = _monster.Health.Value;
            _gaugeImage.fillAmount = _monster.Health.Value / _monster.Health.MaxValue;
        }

        // 빌보드 기법: 카메라의 위치와 회전에 상관없이 항상 정면을 바라보게 하는 기법
        _healthBarTransform.forward = Camera.main.transform.forward;
    }
}
