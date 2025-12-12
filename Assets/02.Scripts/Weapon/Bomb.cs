using UnityEngine;

public class Bomb : MonoBehaviour
{
    public GameObject ExplosionEffectPrefab;
    private void OnCollisionEnter(Collision collision)
    {
        // Enemy에 닿으면 대미지 주고 폭발 이펙트 생성
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Monster monster = collision.gameObject.GetComponent<Monster>();
            if (monster != null)
            {
                monster.TryTakeDamage(50f, gameObject.transform.position); // 예시로 50 대미지
                Explode();
            }
        }

        Explode();
    }

    private void Explode()
    {
        GameObject effectObject = Instantiate(ExplosionEffectPrefab);
        effectObject.transform.position = transform.position;

        // 자기 자신 풀에 반납
        gameObject.SetActive(false);
    }
}
