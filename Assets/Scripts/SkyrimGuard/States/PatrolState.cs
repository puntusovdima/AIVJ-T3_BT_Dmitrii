using UnityEngine;
using UnityEngine.AI;

public class PatrolState : MonoBehaviour
{
    private SkyrimGuardAgent agent;
    private NavMeshAgent navMeshAgent;
    
    [Header("Patrol Properties")]
    [Tooltip("Movement speed during patrol.")]
    public float patrolSpeed = 2f;
    
    private float dialogueTimer = 0f;
    private readonly string[] patrolDialogues = new string[]
    {
        "Let me guess... someone stole your sweetroll?",
        "Guard duty here is pretty quiet. Mostly.",
        "Keep your hands to yourself, sneak thief.",
        "Fear not, the guard is here.",
        "Disrespect the law, and you disrespect me.",
        "My cousin is out fighting dragons, and what do I get? Guard duty."
    };

    void OnEnable()
    {
        agent = GetComponent<SkyrimGuardAgent>();
        navMeshAgent = GetComponent<NavMeshAgent>();

        if (navMeshAgent != null)
        {
            navMeshAgent.speed = patrolSpeed;
            if (navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.isStopped = false;
            }
        }

        Debug.Log("<color=cyan>[State: Patrol]</color> Guard began patrolling. 'Watch the skies, traveler.'");
        dialogueTimer = Random.Range(1f, 3f); // Speak shortly after starting patrol
    }

    void Update()
    {
        if (agent == null || agent.patrolWaypoints == null || agent.patrolWaypoints.Length == 0) return;

        Vector3 targetPos = agent.patrolWaypoints[agent.currentWaypointIndex];

        if (navMeshAgent != null && navMeshAgent.isOnNavMesh)
        {
            // Use NavMeshAgent pathfinding!
            navMeshAgent.SetDestination(targetPos);

            // Check if we reached the waypoint
            if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < 0.8f)
            {
                agent.currentWaypointIndex = (agent.currentWaypointIndex + 1) % agent.patrolWaypoints.Length;
                Debug.Log($"<color=cyan>[Patrol]</color> Reached waypoint. Target is now waypoint {agent.currentWaypointIndex}.");
            }
        }
        else
        {
            // Fallback manual movement if no NavMeshAgent is present/active
            targetPos.y = transform.position.y; // Maintain same height
            transform.position = Vector3.MoveTowards(transform.position, targetPos, patrolSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPos) < 0.2f)
            {
                agent.currentWaypointIndex = (agent.currentWaypointIndex + 1) % agent.patrolWaypoints.Length;
                Debug.Log($"<color=cyan>[Patrol]</color> Reached waypoint (Manual fallback). Target is now waypoint {agent.currentWaypointIndex}.");
            }
        }

        // Handle periodic immersive guard dialogue
        dialogueTimer -= Time.deltaTime;
        if (dialogueTimer <= 0)
        {
            string dialogue = patrolDialogues[Random.Range(0, patrolDialogues.Length)];
            Debug.Log($"<color=cyan>[Patrol Dialogue]</color> Guard: \"{dialogue}\"");
            dialogueTimer = Random.Range(8f, 15f);
        }
    }

    void OnDisable()
    {
        if (navMeshAgent != null && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.isStopped = true;
        }
        Debug.Log("<color=cyan>[State: Patrol]</color> Guard ended patrolling.");
    }
}
