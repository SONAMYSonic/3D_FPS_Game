using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Drum : MonoBehaviour
{
    private Rigidbody _rigidbody;
    [SerializeField] private ValueStat _health;
    [SerializeField] private ValueStat _damage;
    [SerializeField] private ParticleSystem _explosionParticle;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();

        _health.Initialize();
    }

    private bool TryTakeDamage(float damage)
    {
        if (_health.Value <= 0f) return false;

        _health.Decrease(damage);

        if (_health.Value <= 0f)
        {
            Explode();
        }

        return true;
    }

    private void Explode()
    {
        _explosionParticle.transform.position = this.transform.position;
        _explosionParticle.Play();

        Destroy(gameObject);
    }
}
