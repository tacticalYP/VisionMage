using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Diagnostics;

public class SpellCaster : MonoBehaviour
{
    [Header("Spell Slots (1-4)")]
    public Spell[] spellSlots = new Spell[4];

    [Header("References")]
    public Animator animator;
    public Transform castPoint;
    public Transform wandTip;

    private bool isCasting;
    private float[] cooldownTimers = new float[4];
    public SpellDrawer spellDrawer;

    public GameObject waterBallPrefab;
    [SerializeField] private LayerMask aimLayerMask;

    // private void Update()
    // {
    //     HandleCooldowns();
    // }

    // private void HandleCooldowns()
    // {
    //     for (int i = 0; i < cooldownTimers.Length; i++)
    //     {
    //         if (cooldownTimers[i] > 0)
    //             cooldownTimers[i] -= Time.deltaTime;
    //     }
    // }

    // // Input Events
    // public void OnSpell1(InputAction.CallbackContext context)
    // {
    //     if (context.started)
    //         TryCastSpell(0);
    // }

    public void OnSpell2()
    {
        // if (context.started)
        //     TryCastSpell(1);
        
        // Instantiate(waterBallPrefab, castPoint.position, castPoint.rotation);

        // if (!context.performed) return;
        
        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);

        // 2. Create a ray from camera through the screen center
        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
        Vector3 targetPoint=new Vector3(0,0,0);

        // 3. Raycast to find what we are looking at
        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimLayerMask))
        {
            targetPoint= raycastHit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(500f);
        }
        
        // Rotate the player to face the aim target
        // Vector3 aimDirection = new Vector3(targetPoint.x - transform.position.x, 0, targetPoint.z - transform.position.z);
        // transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 20f);

        // 4. Shoot when Left Mouse is clicked
        Vector3 shootDirection = (targetPoint - castPoint.position).normalized;

        // Instantiate and propel bullet
        GameObject bullet = Instantiate(waterBallPrefab, castPoint.position, Quaternion.LookRotation(shootDirection, Vector3.up));
        
        // Add velocity (assumes bullet has a Rigidbody)
        if(bullet.TryGetComponent(out Rigidbody rb))
        {
            rb.linearVelocity = shootDirection * 50f; 
        }
    }

//     public void OnSpell3(InputAction.CallbackContext context)
//     {
//         if (context.started)
//             TryCastSpell(2);
//     }

//     public void OnSpell4(InputAction.CallbackContext context)
//     {
//         if (context.started)
//             TryCastSpell(3);
//     }

//     private void TryCastSpell(int index)
//     {
//         if (isCasting)
//             return;

//         if (spellSlots[index] == null)
//             return;

//         if (cooldownTimers[index] > 0)
//             return;

//         StartCoroutine(CastSpellRoutine(index));
//     }

//     private IEnumerator CastSpellRoutine(int index)
//     {
//         isCasting = true;

//         Spell spell = spellSlots[index];

//         //  Optional: Trigger casting animation
//         animator.SetBool("IsCasting", true);
//         isCasting = true;

//         //Trigger draw animation trigger
//         animator.SetTrigger("CastTrigger");

//         UnityEngine.Debug.Log($"Drawing the {spell.spellName}");

//         GameObject effectInstance = PlayCastEffect(spell);

//         // Draw spell shape
//         if (spellDrawer != null){
//             UnityEngine.Debug.Log("1");
//             yield return StartCoroutine(
//                 spellDrawer.DrawSpellShape(spell)
//             );
//         }

//         yield return new WaitForSeconds(spell.castTime);

//         // Execute spell logic
//         // spell.Cast(castPoint != null ? castPoint : transform);
//         spell.Cast(wandTip);

//         UnityEngine.Debug.Log($"Casted {spell.spellName}");

//         cooldownTimers[index] = spell.cooldown;

//         if (effectInstance != null)
//         {
//             ParticleSystem ps = effectInstance.GetComponent<ParticleSystem>();
//             if (ps != null)
//             {
//                 ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
//             }

//             // Destroy the object after a delay to let trailing particles disappear
//             Destroy(effectInstance, 2f);
//         }
        

//         animator.SetBool("IsCasting", false);

//         isCasting = false;
//     }

//     public float GetCooldownRemaining(int index)
//     {
//         return cooldownTimers[index];
//     }

// private GameObject PlayCastEffect(Spell spell)
//     {
//         if (spell.vfx.castEffect == null) return null;

//         // Instantiate as a child of wandTip so it follows the wand movement
//         GameObject effect = Instantiate(spell.vfx.castEffect, wandTip.position, wandTip.rotation, wandTip);
//         return effect;
//     }
}