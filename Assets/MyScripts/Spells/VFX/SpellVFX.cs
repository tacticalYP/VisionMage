using UnityEngine;

[System.Serializable]
public class SpellVFX
{
    [Header("Cast Effect")]
    public GameObject castEffect;

    [Header("Travel Effect")]
    public GameObject travelEffect;

    [Header("Impact Effect")]
    public GameObject impactEffect;

    [Header("Effect Settings")]
    public float travelSpeed = 20f;
}