using UnityEngine;

[CreateAssetMenu(fileName = "NewSingleTargetSpell", menuName = "Spells/Single Target Spell")]
public class SingleTargetSpell : Spell
{
    [Header("Single Target Settings")]
    public float range = 20f;
    public LayerMask targetLayer;

    // public override void Cast(Transform caster)
    // {
    //     Ray ray = new Ray(caster.position + Vector3.up * 1.5f, caster.forward);
    //     RaycastHit hit;

    //     if (Physics.Raycast(ray, out hit, range, targetLayer))
    //     {
    //         IDamageable damageable = hit.collider.GetComponent<IDamageable>();

    //         if (damageable != null)
    //         {
    //             damageable.TakeDamage(damage);

    //             if (impactVFX != null)
    //                 Instantiate(impactVFX, hit.point, Quaternion.identity);
    //         }
    //     }

    //     if (castVFX != null)
    //         Instantiate(castVFX, caster.position + Vector3.up * 1.5f, Quaternion.identity);
    // }

    public override void Cast(Transform wand)
    {
        // Transform wand = caster.GetComponent<SpellCaster>().wandTip;

        GameObject projectile = Instantiate(
            vfx.travelEffect,
            wand.position,
            wand.rotation
        );

        SpellProjectile proj = projectile.GetComponent<SpellProjectile>();

        proj.Initialize(
            vfx.travelSpeed,
            damage,
            vfx.impactEffect
        );
    }
}