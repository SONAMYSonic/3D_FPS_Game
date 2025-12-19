using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Drum : MonoBehaviour, IDamageable
{
    private Rigidbody _rigidbody;

    [SerializeField] private LayerMask _damageLayer;


    [SerializeField] private ConsumableStat _health;
    [SerializeField] private ValueStat _damage;
    [SerializeField] private ValueStat _explosionRadius;
    [SerializeField] private ParticleSystem _explosionParticePrefab;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();

        _health.Initialize();
    }

    public bool TryTakeDamage(Damage damage)
    {
        if (_health.Value <= 0) return false;

        _health.Decrease(damage.Value);

        if (_health.Value <= 0)
        {
            StartCoroutine(ExplodeCoroutine());
        }

        return true;
    }

    private IEnumerator ExplodeCoroutine()
    {
        ParticleSystem explosionParticle = Instantiate(_explosionParticePrefab);
        explosionParticle.transform.position = this.transform.position;
        explosionParticle.Play();

        // 
        _rigidbody.AddForce(Vector3.up * 1200f);
        _rigidbody.AddTorque(UnityEngine.Random.insideUnitSphere * 90f);

        Damage damage = new Damage
        {
            Value = _damage.Value,
            HitDirection = Vector3.zero,
            HitPoint = transform.position,
            Who = gameObject,
            Critical = false
        };

        Collider[] colliders = Physics.OverlapSphere(transform.position, _explosionRadius.Value, _damageLayer);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].TryGetComponent<IDamageable>(out IDamageable damageable))
            {
                damageable.TryTakeDamage(damage);
            }

            // 인터페이스를 사용하면 아래 코드들은 불필요해진다.
            /*
            if (colliders[i].TryGetComponent<Monster>(out Monster monster))
            {
                Vector3 hitDirection = (colliders[i].transform.position - transform.position).normalized;
                monster.TryTakeDamage(damage);
            }

            if (colliders[i].TryGetComponent<Drum>(out Drum drum))
            {
                drum.TryTakeDamage(damage);
            }
            */
        }




        yield return new WaitForSeconds(7f);

        Destroy(this.gameObject);
    }
}