using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewChainSpell", menuName = "Spells/Chain Spell")]
public class ChainSpell : Spell
{
    [Header("Chain Settings")]
    public float range = 15f;
    public int maxChains = 3;
    public float chainRadius = 5f;
    public LayerMask targetLayer;

    public override void Cast(Transform caster)
    {
        Collider[] initialHits = Physics.OverlapSphere(caster.position, range, targetLayer);

        if (initialHits.Length == 0)
            return;

        List<IDamageable> hitTargets = new List<IDamageable>();

        IDamageable firstTarget = initialHits[0].GetComponent<IDamageable>();

        if (firstTarget == null)
            return;

        ChainToTarget(firstTarget, hitTargets, 0);

        if (castVFX != null)
            Instantiate(castVFX, caster.position, Quaternion.identity);
    }

    private void ChainToTarget(IDamageable currentTarget, List<IDamageable> hitTargets, int chainCount)
    {
        if (chainCount >= maxChains)
            return;

        currentTarget.TakeDamage(damage);
        hitTargets.Add(currentTarget);

        if (impactVFX != null)
            Instantiate(impactVFX, currentTarget.GetTransform().position, Quaternion.identity);

        Collider[] nearby = Physics.OverlapSphere(
            currentTarget.GetTransform().position,
            chainRadius,
            targetLayer
        );

        foreach (Collider col in nearby)
        {
            IDamageable nextTarget = col.GetComponent<IDamageable>();

            if (nextTarget != null && !hitTargets.Contains(nextTarget))
            {
                ChainToTarget(nextTarget, hitTargets, chainCount + 1);
                break;
            }
        }
    }
}