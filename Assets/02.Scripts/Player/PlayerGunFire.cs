using UnityEngine;
using UnityEngine.EventSystems;
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
    [SerializeField] private Vector3 _initialPlayerGunRotation = new Vector3(0, 0, 0);
    [SerializeField] private float recoilAngle = 5f; // 반동 각도
    [SerializeField] private float recoilDuration = 0.1f; // 반동 지속 시간
    public Camera playerCamera;
    [SerializeField] private Vector3 _cameraInitialRotation;
    [SerializeField] private float _cameraRecoilAmount = 0.1f;
    [SerializeField] private float _cameraRecoilDuration = 0.1f;

    [SerializeField] private UI_Crosshair _uiCrosshair;

    private ParticleSystem _hitEffect;
    private GunStat _gunStat;

    private void Awake()
    {
        // 피격 이펙트 미리 생성해두기 (풀링)
        _hitEffect = Instantiate(_hitEffectPrefab);

        // GunStat 컴포넌트 가져오기
        _gunStat = GetComponent<GunStat>();

        playerCamera = Camera.main;

        // 실제 초기 "로컬" 회전을 캐싱 (부모 회전 영향 제거)
        _initialPlayerGunRotation = PlayerGunObject.transform.localEulerAngles;
        _cameraInitialRotation = playerCamera.transform.localEulerAngles;
    }

    private void Update()
    {
        if (GameManager.Instance.State != EGameState.Playing)
        {
            return;
        }

        // 타이머 증가
        _fireTimer += Time.deltaTime;

        // UI 위에서 클릭한 경우 발사하지 않음
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        // 1. 마우스 왼쪽 버튼이 눌림과 탄약이 0개 이상인지 확인하고 true 면 1개 감소, 레이어 UI 클릭 시에는 발사 안되게
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

                // 대미지 처리
                ApplyDamage(hitInfo.collider.gameObject, ray.direction);
            }

        }

        // Ray: 레이저 (시작위치, 방향, 거리)
        // RayCast: 레이저를 발사
        // RayCastHit: 레이저가 물체에 충돌했다면 그 정보를 저장하는 구조ㅇㄴ체

    }

    /// <summary>
    /// 충돌한 오브젝트에 대미지 적용
    /// </summary>
    private void ApplyDamage(GameObject target, Vector3 hitDirection)
    {
        // 몬스터
        Monster monster = target.GetComponent<Monster>();
        if (monster != null)
        {
            monster.TryTakeDamage(_gunStat.Damage, hitDirection);
            return;
        }

        // 드럼통
        Barrel barrel = target.GetComponent<Barrel>();
        if (barrel != null)
        {
            barrel.TakeDamage(_gunStat.Damage);
            return;
        }
    }

    // 총 반동 효과 메서드
    private void ApplyRecoil()
    {
        // 1. 기존에 실행 중이던 트윈이 있다면 즉시 종료 (연사 시 꼬임 방지)
        PlayerGunObject.transform.DOKill();
        // 2. 총 반동 회전 적용 (로컬 기준)
        Vector3 currentLocal = PlayerGunObject.transform.localEulerAngles;
        Vector3 recoilRotation = new Vector3(currentLocal.x - recoilAngle, currentLocal.y, currentLocal.z);
        PlayerGunObject.transform.DOLocalRotate(recoilRotation, recoilDuration / 2).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            PlayerGunObject.transform.DOLocalRotate(_initialPlayerGunRotation, recoilDuration / 2).SetEase(Ease.InQuad);
        });

        // 카메라 반동 효과 (로컬 기준)
        playerCamera.transform.DOKill();
        _cameraInitialRotation = playerCamera.transform.localEulerAngles;

        Vector3 camLocal = playerCamera.transform.localEulerAngles;
        Vector3 cameraRecoilRotation = new Vector3(camLocal.x - _cameraRecoilAmount, camLocal.y, camLocal.z);
        playerCamera.transform.DOLocalRotate(cameraRecoilRotation, _cameraRecoilDuration / 2).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            playerCamera.transform.DOLocalRotate(_cameraInitialRotation, _cameraRecoilDuration / 2).SetEase(Ease.InQuad);
        });
    }
}
