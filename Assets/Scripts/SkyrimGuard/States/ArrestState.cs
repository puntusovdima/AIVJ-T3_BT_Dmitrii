using UnityEngine;
using UnityEngine.AI;

public class ArrestState : MonoBehaviour
{
    private SkyrimGuardAgent agent;
    private NavMeshAgent navMeshAgent;
    private bool dialogueTriggered = false;

    void OnEnable()
    {
        agent = GetComponent<SkyrimGuardAgent>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        dialogueTriggered = false;

        if (navMeshAgent != null)
        {
            navMeshAgent.speed = 3f;
            if (navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.isStopped = false;
            }
        }

        Debug.Log("<color=yellow>[State: Arrest]</color> Guard approaching player for arrest. 'Stop right there!'");
    }

    void Update()
    {
        if (agent == null) return;

        // Determine player target position
        Vector3 targetPos = agent.playerTarget != null ? agent.playerTarget.transform.position : transform.position;
        targetPos.y = transform.position.y; // Maintain same height

        // Walk directly into the player's face
        if (agent.distanceToPlayer > 1.8f)
        {
            if (navMeshAgent != null && navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.isStopped = false;
                navMeshAgent.SetDestination(targetPos);
            }
            else
            {
                // Fallback manual movement
                agent.distanceToPlayer = Mathf.Max(1.5f, agent.distanceToPlayer - 3f * Time.deltaTime);
                transform.position = Vector3.MoveTowards(transform.position, targetPos, 3f * Time.deltaTime);
            }
        }
        else
        {
            if (navMeshAgent != null && navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.isStopped = true;
            }

            if (!dialogueTriggered)
            {
                dialogueTriggered = true;
                Debug.Log("<color=yellow>[Arrest Dialogue]</color> Guard: \"Stop right there, criminal scum! You've committed crimes against Skyrim and her people. What say you in your defense?\"");
            }
        }
    }

    void OnGUI()
    {
        if (agent == null || !dialogueTriggered) return;

        // Draw a dialogue menu in the center of the screen
        Rect rect = new Rect(Screen.width / 2 - 200, Screen.height / 2 - 100, 400, 220);
        GUI.Box(rect, "ARREST CONFRONTATION");

        GUILayout.BeginArea(new Rect(rect.x + 20, rect.y + 30, rect.width - 40, rect.height - 40));
        
        GUILayout.Label("Guard: \"Stop right there, criminal scum! You committed crimes against Skyrim and her people. What say you in your defense?\"");
        GUILayout.Label($"Your current bounty: <color=yellow>{agent.playerBounty} Gold</color>");
        
        GUILayout.Space(10);
        
        if (GUILayout.Button($"Pay Fine ({agent.playerBounty} Gold)"))
        {
            Debug.Log("<color=yellow>[Arrest Choice]</color> Player: I pay the fine.");
            Debug.Log("<color=green>[Arrest Result]</color> Guard: \"Alright, smart guy. I'll take that. Now move along.\"");
            agent.playerBounty = 0; // Clear bounty
            agent.distanceToPlayer = 8f; // Reset distance
        }

        if (GUILayout.Button("Go to Jail (Serve Sentence)"))
        {
            Debug.Log("<color=yellow>[Arrest Choice]</color> Player: Take me to jail.");
            Debug.Log("<color=green>[Arrest Result]</color> Guard: \"Hope you like Cidhna Mine. Let's go.\"");
            agent.playerBounty = 0; // Clear bounty
            agent.distanceToPlayer = 8f; // Reset distance
            agent.HealFull(); // Restored health
        }

        if (GUILayout.Button("Resist Arrest (Fight!)"))
        {
            Debug.Log("<color=yellow>[Arrest Choice]</color> Player: I'd rather die than go to prison!");
            Debug.Log("<color=red>[Arrest Result]</color> Guard: \"Then pay with your blood!\"");
            agent.isThreatPresent = true; // Spawns threat
        }

        GUILayout.EndArea();
    }

    void OnDisable()
    {
        if (navMeshAgent != null && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.isStopped = true;
        }
        Debug.Log("<color=yellow>[State: Arrest]</color> Guard left arrest state.");
    }
}
