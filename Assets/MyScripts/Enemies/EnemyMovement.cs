using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator animator;
    public Transform player;

    [Header("Detection Settings")]
    public float detectionRadius = 200f;
    private bool isChasing = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        EnemyHealth health = GetComponent<EnemyHealth>();

        if (health != null)
        {
            agent.speed = health.stats.moveSpeed;
        }
        
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    void Update()
    {
        if (player == null || agent.enabled == false)
            return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRadius)
        {
            isChasing = true;
        }
        else
        {
            isChasing = false;
        }

        if (isChasing)
        {
            agent.SetDestination(player.position);
        }
        else
        {
            agent.ResetPath();
        }

        animator.SetFloat("Speed", agent.velocity.magnitude);

        // if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        // {
        //     // This stops the agent immediately
        //     agent.velocity = Vector3.zero; 
        // }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}