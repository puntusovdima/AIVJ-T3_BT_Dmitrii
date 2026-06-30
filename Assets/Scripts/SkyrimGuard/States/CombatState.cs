using UnityEngine;
using UnityEngine.AI;

public class CombatState : MonoBehaviour
{
    private SkyrimGuardAgent agent;
    private NavMeshAgent navMeshAgent;
    
    [Header("Combat Properties")]
    [Tooltip("Movement speed during combat chase.")]
    public float chaseSpeed = 3.5f;
    
    [Tooltip("How close the guard needs to be to attack.")]
    public float attackRange = 2f;
    
    [Tooltip("Time between attacks in seconds.")]
    public float attackInterval = 1.5f;

    private float attackTimer = 0f;
    private float dialogueTimer = 0f;

    private readonly string[] battleCries = new string[]
    {
        "Never should have come here!",
        "Skyrim belongs to the Nords!",
        "I'll mount your head on my wall!",
        "You picked a bad time to get lost!",
        "Die, fetcher!",
        "Victory or Sovngarde!"
    };

    void OnEnable()
    {
        agent = GetComponent<SkyrimGuardAgent>();
        navMeshAgent = GetComponent<NavMeshAgent>();

        if (navMeshAgent != null)
        {
            navMeshAgent.speed = chaseSpeed;
            if (navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.isStopped = false;
            }
        }

        Debug.Log("<color=red>[State: Combat]</color> Guard entered COMBAT! Weapon drawn.");
        dialogueTimer = 0.2f; // Shout immediately
        attackTimer = 0.5f;   // Ready to swing
    }

    void Update()
    {
        if (agent == null) return;

        // Determine player target position
        Vector3 targetPos = agent.playerTarget != null ? agent.playerTarget.transform.position : transform.position;
        targetPos.y = transform.position.y; // Maintain same height

        // Pursue threat / player
        if (agent.distanceToPlayer > attackRange)
        {
            if (navMeshAgent != null && navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.isStopped = false;
                navMeshAgent.SetDestination(targetPos);
            }
            else
            {
                // Fallback manual movement
                float travelDistance = chaseSpeed * Time.deltaTime;
                agent.distanceToPlayer = Mathf.Max(attackRange - 0.1f, agent.distanceToPlayer - travelDistance);
                transform.position = Vector3.MoveTowards(transform.position, targetPos, chaseSpeed * Time.deltaTime);
            }

            Debug.Log($"<color=red>[Combat]</color> Chasing player! Current distance: {agent.distanceToPlayer:F1}m");
        }
        else
        {
            if (navMeshAgent != null && navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.isStopped = true; // Stop moving when close enough to attack
            }
        }

        // Perform attacks when close enough
        if (agent.distanceToPlayer <= attackRange)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0)
            {
                PerformAttack();
                attackTimer = attackInterval;
            }
        }

        // Periodically shout classic battle dialogues
        dialogueTimer -= Time.deltaTime;
        if (dialogueTimer <= 0)
        {
            string cry = battleCries[Random.Range(0, battleCries.Length)];
            Debug.Log($"<color=red>[Combat Dialogue]</color> Guard shouts: \"{cry.ToUpper()}\"");
            dialogueTimer = Random.Range(4f, 8f);
        }
    }

    private void PerformAttack()
    {
        Debug.Log("<color=red>[Combat]</color> Guard swings steel sword! *Swoosh* Deal 15 damage to target!");
        
        // Simulating receiving retaliation damage occasionally to demonstrate transition to Flee state!
        if (Random.value < 0.4f)
        {
            agent.TakeDamage(15f);
        }
    }

    void OnDisable()
    {
        if (navMeshAgent != null && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.isStopped = true;
        }
        Debug.Log("<color=red>[State: Combat]</color> Guard sheathed weapon and left combat state.");
    }
}
