using UnityEngine;
using UnityEngine.AI; // Needed if your player uses a NavMeshAgent

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public Transform respawnPoint; // Assign an empty GameObject in the Inspector
    public int currentHealth;
    private Rigidbody rb;
    private CharacterController cc;
    public PlayerHealthUI healthUI;

    void Awake()
    {
        // Cache these so we can "disable" them during the teleport
        rb = GetComponent<Rigidbody>();
        cc = GetComponent<CharacterController>();
    }

    void Start()
    {
        currentHealth = maxHealth;

        if (healthUI != null)
        {
            healthUI.UpdateHealthUI();
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("Player Health: " + currentHealth);

        if (healthUI != null)
        {
            healthUI.UpdateHealthUI();
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Player Died! Returning to start...");
        Respawn();
    }

    void Respawn()
    {
        currentHealth = maxHealth;

        // Disable the Controller
        if (cc != null) 
        {
            cc.enabled = false;
        }

        // Clear Physics Momentum (rigid body)
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = respawnPoint.position;
        }

        // Move the Transform
        transform.position = respawnPoint.position;
        transform.rotation = respawnPoint.rotation;

        // Sync the internal physics move immediately
        Physics.SyncTransforms();

        // Re-enable the Controller
        if (cc != null) 
        {
            cc.enabled = true;
        }

        if (healthUI != null)
        {
            healthUI.UpdateHealthUI();
        }

        Debug.Log("Wizard Restored at " + respawnPoint.name);
    }
}