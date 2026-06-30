using UnityEngine;
using UnityEngine.AI;

public class FleeState : MonoBehaviour
{
    private SkyrimGuardAgent agent;
    private NavMeshAgent navMeshAgent;
    
    [Header("Flee Properties")]
    [Tooltip("Movement speed when running away.")]
    public float fleeSpeed = 5f;
    
    private float dialogueTimer = 0f;
    private readonly string[] fleeLines = new string[]
    {
        "Mercy! I yield! I yield!",
        "Guard! Help! A dragon is loose!",
        "I'm getting out of here! This is not my job!",
        "By the Nine Divines, save me!",
        "Too strong! Fall back!",
        "You haven't seen the last of me!"
    };

    void OnEnable()
    {
        agent = GetComponent<SkyrimGuardAgent>();
        navMeshAgent = GetComponent<NavMeshAgent>();

        if (navMeshAgent != null)
        {
            navMeshAgent.speed = fleeSpeed;
            if (navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.isStopped = false;
            }
        }

        Debug.Log("<color=magenta>[State: Flee]</color> Guard panicked and ran away in fear! 'Save yourselves!'");
        dialogueTimer = 0.1f; // Cry out immediately
    }

    void Update()
    {
        if (agent == null) return;

        // Determine player/threat position
        Vector3 playerPos = agent.playerTarget != null ? agent.playerTarget.transform.position : Vector3.zero;
        Vector3 fleeDir = (transform.position - playerPos).normalized;
        if (fleeDir == Vector3.zero) fleeDir = Vector3.back;

        // Flee target destination is 15 meters in the opposite direction
        Vector3 targetFleePos = transform.position + fleeDir * 15f;

        if (navMeshAgent != null && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.isStopped = false;
            
            // Sample closest point on NavMesh to flee to
            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetFleePos, out hit, 10f, NavMesh.AllAreas))
            {
                navMeshAgent.SetDestination(hit.position);
            }
            else
            {
                navMeshAgent.SetDestination(targetFleePos);
            }
        }
        else
        {
            // Fallback manual movement
            float travel = fleeSpeed * Time.deltaTime;
            agent.distanceToPlayer += travel;
            transform.position += fleeDir * fleeSpeed * Time.deltaTime;
        }

        Debug.Log($"<color=magenta>[Flee]</color> Running away from danger! Simulated distance: {agent.distanceToPlayer:F1}m");

        // Periodically scream in terror
        dialogueTimer -= Time.deltaTime;
        if (dialogueTimer <= 0)
        {
            string line = fleeLines[Random.Range(0, fleeLines.Length)];
            Debug.Log($"<color=magenta>[Flee Dialogue]</color> Guard yells: \"{line.ToUpper()}\"");
            dialogueTimer = Random.Range(3f, 6f);
        }

        // Passive health regeneration when far away. Once healthy, the guard can face the world again!
        if (agent.distanceToPlayer > 18f && agent.health < agent.maxHealth)
        {
            agent.health = Mathf.Min(agent.maxHealth, agent.health + 8f * Time.deltaTime);
            Debug.Log($"<color=magenta>[Flee Recovery]</color> Guard caught their breath. Healing: {agent.health:F0}/{agent.maxHealth} HP.");
            
            // Escape threshold reached: clear threat once healed above 50%
            if (agent.health >= agent.maxHealth * 0.5f)
            {
                agent.isThreatPresent = false;
                Debug.Log("<color=magenta>[Flee Success]</color> Guard successfully escaped the danger, healed up, and calmed down!");
            }
        }
    }

    void OnDisable()
    {
        if (navMeshAgent != null && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.isStopped = true;
        }
        Debug.Log("<color=magenta>[State: Flee]</color> Guard calmed down and stopped fleeing.");
    }
}
