using Unity.VisualScripting;
using UnityEngine;
using DG.Tweening;

public class PlayerGunReload : MonoBehaviour
{
    private GunStat _gunStat;
    [SerializeField] private GameObject _playerGunObject;
    [SerializeField] private Vector3 _initailPlayerGunRotation;

    [SerializeField] private float _reloadDuration = 1.6f;
    [SerializeField] private float _reloadTimer = 0f;

    public UI_Reload ui_Reload;

    private void Awake()
    {
        // GunStat 컴포넌트 가져오기
        _gunStat = GetComponent<GunStat>();

        // 초기 "로컬" 회전 캐싱
        _initailPlayerGunRotation = _playerGunObject.transform.localEulerAngles;
    }

    private void Update()
    {
        if (GameManager.Instance.State != EGameState.Playing)
        {
            return;
        }

        // R 키를 누르면 재장전
        if (Input.GetKeyDown(KeyCode.R) && _gunStat.Ammo.Value < _gunStat.Ammo.MaxValue)
        {
            // UI에 재장전 텍스트 표시
            ui_Reload.ShowReloadText();

            // 재장전 시 총 오브젝트를 X축으로 마구 회전 후 원래대로 돌아오게 함
            _playerGunObject.transform.DOKill();
            _playerGunObject.transform.DOLocalRotate(
                new Vector3(_initailPlayerGunRotation.x + 3600f, _initailPlayerGunRotation.y, _initailPlayerGunRotation.z),
                1.6f,
                RotateMode.FastBeyond360
            ).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                _playerGunObject.transform.localRotation = Quaternion.Euler(_initailPlayerGunRotation);
                Reload();
            });
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
    }
}
