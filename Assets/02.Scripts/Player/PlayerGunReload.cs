using Unity.VisualScripting;
using UnityEngine;
using DG.Tweening;

public class PlayerGunReload : MonoBehaviour
{
    private GunStat _gunStat;

    [SerializeField] private float _reloadDuration = 1.6f;
    [SerializeField] private bool _isReloading = false;

    public UI_Reload ui_Reload;

    private void Awake()
    {
        // GunStat 컴포넌트 가져오기
        _gunStat = GetComponent<GunStat>();
    }

    private void Update()
    {
        if (GameManager.Instance.State != EGameState.Playing)
        {
            return;
        }

        // R 키를 누르면 재장전
        if ((Input.GetKeyDown(KeyCode.R) && _gunStat.Ammo.Value < _gunStat.Ammo.MaxValue) && _isReloading == false)
        {
            // UI에 재장전 텍스트 표시
            ui_Reload.ShowReloadText();

            // 재장전 중임을 표시
            _isReloading = true;

            Reload();
        }
    }

    private void Reload()
    {
        // 재장전할 탄약 계산
        float neededAmmo = _gunStat.Ammo.MaxValue - _gunStat.Ammo.Value;
        float availableAmmo = _gunStat.FullAmmo.Value;
        float ammoToReload = Mathf.Min(neededAmmo, availableAmmo);
        // 탄약 재장전
        _gunStat.Ammo.Increase(ammoToReload);
        _gunStat.FullAmmo.Decrease(ammoToReload);

        // 재장전 완료 후 재장전 중 상태 해제
        _isReloading = false;
    }
}
