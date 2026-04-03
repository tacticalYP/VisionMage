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
    public float stoppingDistance = 0f;

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
            Debug.Log("got player");
            player = playerObject.transform;
        }

    }

    void Update()
    {
        if (player == null || agent.enabled == false)
            return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        NavMeshHit hit;
        // Check if the player's position is within 1.0 unit of the NavMesh
        bool isPlayerOnNavMesh = NavMesh.SamplePosition(player.position, out hit, 1.0f, NavMesh.AllAreas);

        if (distanceToPlayer <= detectionRadius || detectionRadius == -1)
        {
            isChasing = true;
        }
        else
        {
            isChasing = false;
        }

        if (isPlayerOnNavMesh && isChasing)
        {   
            agent.SetDestination(player.position);
        }
        // else
        // {
        //     agent.ResetPath();
        // }

        // animator.SetFloat("Speed", agent.velocity.magnitude);
        // Debug.Log(agent.velocity.magnitude);

        if (agent.remainingDistance <= (stoppingDistance==0? agent.stoppingDistance : stoppingDistance) )
        {
            // This stops the agent immediately
            // agent.isStopped = true;
            agent.velocity = Vector3.zero; 
            animator.SetFloat("Speed", 0f);
        }
        else
        {   
            animator.SetFloat("Speed", agent.velocity.magnitude);
            agent.isStopped = false;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}