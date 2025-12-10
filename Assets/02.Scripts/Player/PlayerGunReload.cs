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

    private void Awake()
    {
        // GunStat 컴포넌트 가져오기
        _gunStat = GetComponent<GunStat>();

        _initailPlayerGunRotation = _playerGunObject.transform.eulerAngles;
    }

    private void Update()
    {

        // R 키를 누르면 재장전
        if (Input.GetKeyDown(KeyCode.R))
        {
            // 재장전 시 총 오브젝트를 X축으로 마구 회전 후 원래대로 돌아오게 함
            _playerGunObject.transform.DOKill();
            _playerGunObject.transform.DORotate(new Vector3(_initailPlayerGunRotation.x + 3600f, _initailPlayerGunRotation.y, _initailPlayerGunRotation.z), 1.6f, RotateMode.FastBeyond360).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                    _playerGunObject.transform.rotation = Quaternion.Euler(_initailPlayerGunRotation);
                    Reload();
            });
        }
    }

    private void Reload()
    {
        // 현재 탄약이 최대 탄약보다 적을 때만 재장전
        if (_gunStat.Ammo.Value < _gunStat.Ammo.MaxValue)
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
}
