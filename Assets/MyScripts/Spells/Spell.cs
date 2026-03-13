using UnityEngine;

public abstract class Spell : ScriptableObject
{
    [Header("Spell Info")]
    public string spellName;

    [Header("Combat")]
    public float damage = 10f;
    public float cooldown = 1f;
    public float castTime = 0.5f;

    [Header("Drawing")]
    public Vector3[] shapePoints;
    public float drawDuration = 0.5f;
    public AnimationCurve drawCurve;

    [Header("Visual Effects")]
    public GameObject castVFX;
    public GameObject impactVFX;
    public SpellVFX vfx;

    [Header("Audio")]
    public AudioClip castSFX;

    public abstract void Cast(Transform caster);
}