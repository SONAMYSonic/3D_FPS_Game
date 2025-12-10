using UnityEngine;
using DG.Tweening;

public class PlayerGunFire : MonoBehaviour
{
    // 목표: 마우스의 왼쪽 버튼을 누르면 카메라(플레이어)가 바라보는 방향으로 총을 발사하고 싶다.
    [SerializeField] private Transform _fireTransform;
    [SerializeField] private ParticleSystem _hitEffectPrefab;
    [SerializeField] private float _fireDuration = 0.5f;
    [SerializeField] private float _fireTimer = 0f;

    [Header("총 반동 효과 설정")]
    public GameObject PlayerGunObject;
    [SerializeField] private Vector3 _initialPlayerGunRotation;
    [SerializeField] private float recoilAngle = 5f; // 반동 각도
    [SerializeField] private float recoilDuration = 0.1f; // 반동 지속 시간

    [SerializeField] private UI_Crosshair _uiCrosshair;

    private ParticleSystem _hitEffect;
    private GunStat _gunStat;

    private void Awake()
    {
        // 피격 이펙트 미리 생성해두기 (풀링)
        _hitEffect = Instantiate(_hitEffectPrefab);

        // GunStat 컴포넌트 가져오기
        _gunStat = GetComponent<GunStat>();

        _initialPlayerGunRotation = PlayerGunObject.transform.eulerAngles;
    }

    private void Update()
    {
        // 타이머 증가
        _fireTimer += Time.deltaTime;

        // 1. 마우스 왼쪽 버튼이 눌림과 탄약이 0개 이상인지 확인하고 true 면 1개 감소
        if (Input.GetMouseButton(0) && _fireTimer > _fireDuration && _gunStat.Ammo.TryConsume(1))
        {
            // 1. 타이머 초기화
            _fireTimer = 0f;

            if (_uiCrosshair != null)
            {  
                _uiCrosshair.ReactToFire();
            }


            // 2. Ray를 생성하고 발사할 위치와 방향, 거리를 설정한다. (쏜다)
            Ray ray = new Ray(_fireTransform.position, Camera.main.transform.forward);

            // 3. RayCastHit(충돌한 대상의 정보)를 저장할 변수를 생성한다.
            RaycastHit hitInfo = new RaycastHit();

            // 총 반동 효과 적용
            ApplyRecoil();

            // 4. 발사하고
            bool isHit = Physics.Raycast(ray, out hitInfo);
            if (isHit)
            {
                // 5. 충돌했다면... 피격 이펙트 표시
                Debug.Log(hitInfo.transform.name);

                // 파티클 생성과 플레이 방식
                // 1. Instantiate 방식 (+풀링) -> 한 화면에 여러가지 수정 후 여러 개 그릴경우
                // 2. 하나를 캐싱해두고 Play -> 인스펙터 설정 그대로 그릴 경우
                // 3. 하나를 캐싱해두고 Emit -> 인스펙터 설정을 수정 후 그릴 경우

                _hitEffect.transform.position = hitInfo.point;
                _hitEffect.transform.forward = hitInfo.normal;
                _hitEffect.Play();
            }

        }

        // Ray: 레이저 (시작위치, 방향, 거리)
        // RayCast: 레이저를 발사
        // RayCastHit: 레이저가 물체에 충돌했다면 그 정보를 저장하는 구조체

    }

    // 총 반동 효과 메서드
    private void ApplyRecoil()
    {
        // 1. 기존에 실행 중이던 트윈이 있다면 즉시 종료 (연사 시 꼬임 방지)
        PlayerGunObject.transform.DOKill();
        // 2. 반동 각도만큼 위로 회전
        Vector3 recoilRotation = new Vector3(_initialPlayerGunRotation.x - recoilAngle, _initialPlayerGunRotation.y, _initialPlayerGunRotation.z);
        PlayerGunObject.transform.DORotate(recoilRotation, recoilDuration / 2).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            // 3. 원래 각도로 부드럽게 복귀
            PlayerGunObject.transform.DORotate(_initialPlayerGunRotation, recoilDuration / 2).SetEase(Ease.InQuad);
        });
    }
}
