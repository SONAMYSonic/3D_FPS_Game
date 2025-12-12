using UnityEngine;

public class Bomb : MonoBehaviour
{
    public GameObject _explosionEffectPrefab;

    public float ExplosionRadius = 2;
    public float Damage = 1000;

    private void OnCollisionEnter(Collision collision)
    {
        // 내 위치에 폭발 이펙트 생성
        GameObject effectObject = Instantiate(_explosionEffectPrefab);
        effectObject.transform.position = transform.position;

        // 목표 : 폭발했을때 일정범위안에 몬스터가 있다면 대미지를 주고싶다.
        // 속성:
        //  - 폭발 반경
        //  - 대미지

        Vector3 position = transform.position;
        // 1. 씬을 모두 순회하면서 게임 오브젝트를 찾는다. 1000 번 순회
        // 2. 모든 몬스터를 순회하면서 거리를 측정한다..   500 번 순회
        /*Monster[] monsters = FindObjectsOfType<Monster>();
        for (int i = 0; i < monsters.Length; i++)
        {
            if(몬스터와의 거리 < ExplosionRadius)
            {
                대미지를 준다.
            }
        }*/

        // 가상의 구를 만들어서 그 구 영역에 안에있는 모든 콜라이더를 찾아서 배열로 반환한다..
        Collider[] colliders = Physics.OverlapSphere(transform.position, ExplosionRadius, LayerMask.NameToLayer("Monster"));
        for (int i = 0; i < colliders.Length; i++)
        {
            Monster monster = collision.gameObject.GetComponent<Monster>();

            float distance = Vector3.Distance(transform.position, monster.transform.position);
            distance = Mathf.Min(1f, distance);

            float finalDamage = Damage / distance; // 폭발 원점과 거리에 따라서 대미지를 다르게 준다.

            monster.TryTakeDamage(finalDamage, transform.position);
        }



        /*if (collision.gameObject.layer == LayerMask.NameToLayer("Monster"))
        {
            Monster monster = collision.gameObject.GetComponent<Monster>();
            monster.TryTakeDamage(Damage);
        }*/


        // 충돌하면 나 자신을 삭제한다.
        Destroy(gameObject);
    }
}