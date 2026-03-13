using UnityEngine;

[CreateAssetMenu(fileName = "NewAOESpell", menuName = "Spells/AOE Spell")]
public class AOESpell : Spell
{
    [Header("AOE Settings")]
    public float radius = 5f;
    public LayerMask targetLayer;

    public override void Cast(Transform caster)
    {
        Vector3 center = caster.position;

        Collider[] hits = Physics.OverlapSphere(center, radius, targetLayer);

        foreach (Collider hit in hits)
        {
            IDamageable damageable = hit.GetComponent<IDamageable>();

            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }
        }

        if (impactVFX != null)
            Instantiate(impactVFX, center, Quaternion.identity);

        if (castVFX != null)
            Instantiate(castVFX, caster.position, Quaternion.identity);
    }
}