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

            Vector3 hitDirection = (colliders[i].transform.position - transform.position).normalized;
            Damage damage = new Damage
            {
                Value = this.Damage,
                HitDirection = hitDirection,
                HitPoint = colliders[i].ClosestPoint(transform.position),
                Who = gameObject,
                Critical = false
            };
            monster.TryTakeDamage(damage);
        }

        // 충돌하면 나 자신을 삭제(풀 반환)한다.
        gameObject.SetActive(false);
    }
}