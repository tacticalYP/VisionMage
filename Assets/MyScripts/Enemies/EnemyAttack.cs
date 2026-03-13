using UnityEngine;
using UnityEngine.AI;

public class EnemyAttack : MonoBehaviour
{
    public Transform player;

    private NavMeshAgent agent;
    private Animator animator;

    [Header("Attack Settings")]
    public float attackRange = 15f;
    public float attackCooldown = 2f;
    // public int damage = 10;
    private EnemyHealth enemyHealth;
    private float lastAttackTime;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        enemyHealth = GetComponent<EnemyHealth>();
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    void Update()
    {
        if (player == null || agent.enabled==false)
            return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= attackRange)
        {
            // agent.isStopped = true;
            Attack();
            agent.velocity = Vector3.zero; 
        }
        else
        {
            agent.isStopped = false;
        }
    }

    void Attack()
    {
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            animator.SetTrigger("Attack");
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                // playerHealth.TakeDamage(damage);
                playerHealth.TakeDamage(enemyHealth.stats.attackDamage);
            }

            lastAttackTime = Time.time;
        }
    }
}