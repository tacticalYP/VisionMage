using UnityEngine;

public class SpellProjectile : MonoBehaviour
{
    private float speed;
    private float damage;

    private GameObject impactEffect;

    public void Initialize(float projectileSpeed, float projectileDamage, GameObject impact)
    {
        speed = projectileSpeed;
        damage = projectileDamage;
        impactEffect = impact;
    }

    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (impactEffect != null)
        {
            Instantiate(
                impactEffect,
                transform.position,
                Quaternion.identity
            );
        }

        Destroy(gameObject);
    }
}