using UnityEngine;

public class PlayerGunFire : MonoBehaviour
{
    // 목표: 마우스의 왼쪽 버튼을 누르면 카메라(플레이어)가 바라보는 방향으로 총을 발사하고 싶다.
    [SerializeField] private Transform _fireTransform;
    [SerializeField] private ParticleSystem _hitEffectPrefab;

    private void Update()
    {
        // 1. 마우스 왼쪽 버튼이 눌리면
        if (Input.GetMouseButtonDown(0))
        {
            // 2. Ray를 생성하고 발사할 위치와 방향, 거리를 설정한다. (쏜다)
            Ray ray = new Ray(_fireTransform.position, Camera.main.transform.forward);

            // 3. RayCastHit(충돌한 대상의 정보)를 저장할 변수를 생성한다.
            RaycastHit hitInfo = new RaycastHit();

            // 4. 발사하고
            bool isHit = Physics.Raycast(ray, out hitInfo);
            if (isHit)
            {
                // 5. 충돌했다면... 피격 이펙트 표시
                Debug.Log(hitInfo.transform.name);
                ParticleSystem hitEffect = Instantiate(_hitEffectPrefab, hitInfo.point, Quaternion.identity);
                hitEffect.transform.rotation = Quaternion.LookRotation(hitInfo.normal);
                hitEffect.Play();
            }

        }

        // Ray: 레이저 (시작위치, 방향, 거리)
        // RayCast: 레이저를 발사
        // RayCastHit: 레이저가 물체에 충돌했다면 그 정보를 저장하는 구조체

    }


}
