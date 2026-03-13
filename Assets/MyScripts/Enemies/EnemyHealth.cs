using UnityEngine;
using UnityEngine.InputSystem;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    // [Header("Health Settings")]
    // public float maxHealth = 50f;
    public EnemyStats stats;
    private float currentHealth;
    private Animator animator;
    public GameObject deathEffect;
    public AudioClip deathSound;
    private AudioSource audioSource;

    void Start()
    {
        currentHealth = stats.maxHealth;
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {   
        // Temporarly press K to damage enemy
        if (Keyboard.current != null && Keyboard.current.kKey.wasPressedThisFrame)
        {
            TakeDamage(10);
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        Debug.Log(gameObject.name + " took damage: " + damage);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public Transform GetTransform()
    {
        return transform;
    }

    private void Die()
    {   
        animator.SetTrigger("Die");
        
        // GameObject tempDeathEffect;
        if (deathEffect != null)
        {
            // tempDeathEffect = Instantiate(deathEffect, transform.position, Quaternion.identity);
            deathEffect = Instantiate(deathEffect, transform.position, Quaternion.identity);
        }
        if (deathSound != null)
        {   
            audioSource.PlayOneShot(deathSound);
        }

        Debug.Log(gameObject.name + " died");

        // Disable the navmesh agent to stop the enemy (prevent sliding)
        if(GetComponent<UnityEngine.AI.NavMeshAgent>()) 
            GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = false;

        // Disable the Collider so the player doesn't bump into a dead enemy
        if(GetComponent<Collider>()) 
            GetComponent<Collider>().enabled = false;

        Destroy(deathEffect, 2.0f);
        // Destroy(tempDeathEffect, 2.0f);
        Destroy(gameObject, 2.0f);
    }
}