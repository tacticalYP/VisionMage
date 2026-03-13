using UnityEngine;

public class T_waterball_script : MonoBehaviour
{
    public float speed = 20f;
    public float lifetime = 5f;
    public int damage = 20;
    public GameObject splashEffect;

    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.linearVelocity = transform.forward * speed;

        Destroy(gameObject, lifetime);
    }

    // void OnCollisionEnter(Collision collision)
    // {
    //     GameObject splash = Instantiate(splashEffect, transform.position, Quaternion.identity);

    //     Destroy(splash,2f);
    //     Destroy(gameObject);
    // }

    void OnTriggerEnter(Collider other)
    {
        IDamageable damageable = other.GetComponent<IDamageable>();

        if (damageable != null)
        {
            damageable.TakeDamage(damage);
        }

        GameObject splash = Instantiate(splashEffect, transform.position, Quaternion.identity);

        Destroy(splash,2f);

        Destroy(gameObject);
    }
}