using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Drum : MonoBehaviour
{
    private Rigidbody _rigidbody;

    public LayerMask DamageLayer;


    [SerializeField] private ConsumableStat _health;
    [SerializeField] private ValueStat _damage;
    [SerializeField] private ValueStat _explosionRadius;
    [SerializeField] private ParticleSystem _explosionParticePrefab;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();

        _health.Initialize();
    }

    public bool TryTakeDamage(float damage)
    {
        if (_health.Value <= 0) return false;

        _health.Decrease(damage);

        if (_health.Value <= 0)
        {
            StartCoroutine(Explode_Coroutine());
        }

        return true;
    }

    private IEnumerator Explode_Coroutine()
    {
        ParticleSystem explosionParticle = Instantiate(_explosionParticePrefab);
        explosionParticle.transform.position = this.transform.position;
        explosionParticle.Play();

        // 
        _rigidbody.AddForce(Vector3.up * 1200f);
        _rigidbody.AddTorque(UnityEngine.Random.insideUnitSphere * 90f);


        Collider[] colliders = Physics.OverlapSphere(transform.position, _explosionRadius.Value, DamageLayer);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].TryGetComponent<Monster>(out Monster monster))
            {
                monster.TryTakeDamage(_damage.Value, transform.position);
            }

            if (colliders[i].TryGetComponent<Drum>(out Drum drum))
            {
                drum.TryTakeDamage(_damage.Value);
            }
        }




        yield return new WaitForSeconds(7f);

        Destroy(this.gameObject);
    }
}