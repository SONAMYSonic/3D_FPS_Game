using UnityEngine;

public class Bomb : MonoBehaviour
{
    public GameObject ExplosionEffectPrefab;

    public float ExplosionRadius = 2;
    public float Damage = 1000;

    private void OnCollisionEnter(Collision collision)
    {
        // 내 위치에 폭발 이펙트 생성
        GameObject effectObject = Instantiate(ExplosionEffectPrefab);
        effectObject.transform.position = transform.position;

        // 가상의 구를 만들어서 그 구 영역에 안에있는 모든 콜라이더를 찾아서 배열로 반환한다..
        Collider[] colliders = Physics.OverlapSphere(transform.position, ExplosionRadius, LayerMask.GetMask("Monster"));
        for (int i = 0; i < colliders.Length; i++)
        {
            Monster monster = colliders[i].GetComponent<Monster>();
            if (monster == null) continue;

            monster.TryTakeDamage(Damage, transform.position);
        }

        // 충돌하면 나 자신을 삭제한다.
        Destroy(gameObject);
    }
}