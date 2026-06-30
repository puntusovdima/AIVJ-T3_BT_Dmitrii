using UnityEngine;
using DecisionTree;

[RequireComponent(typeof(StateMachine))]
[RequireComponent(typeof(SkyrimGuardAgent))]
public class SkyrimGuardDecisionTreeController : MonoBehaviour
{
    private StateMachine stateMachine;
    private SkyrimGuardAgent agent;
    
    private DecisionNode<SkyrimGuardAgent> rootNode;
    private string lastDecision = "";

    void Start()
    {
        stateMachine = GetComponent<StateMachine>();
        agent = GetComponent<SkyrimGuardAgent>();

        // Build the Decision Tree Hierarchy
        BuildDecisionTree();
        
        Debug.Log("<color=green>[DecisionTreeController]</color> Skyrim Guard Decision Tree successfully compiled and initialized!");
    }

    /// <summary>
    /// Constructs the binary decision tree.
    /// </summary>
    private void BuildDecisionTree()
    {
        // 1. Create Leaf Action Nodes (representing State Machine state names)
        var combatAction = new ActionNode<SkyrimGuardAgent>("Combat");
        var fleeAction = new ActionNode<SkyrimGuardAgent>("Flee");
        var arrestAction = new ActionNode<SkyrimGuardAgent>("Arrest");
        var relaxAction = new ActionNode<SkyrimGuardAgent>("Relax");
        var patrolAction = new ActionNode<SkyrimGuardAgent>("Patrol");

        // 2. Sub-branch for Non-Threat conditions
        // Question: Is it guard break time / night shift?
        var queryRelaxTime = new DecisionQueryNode<SkyrimGuardAgent>(
            guard => guard.isBreakTime,
            relaxAction,   // YES -> Go relax by the fire
            patrolAction   // NO  -> Patrol hold waypoints
        );

        // Question: Does the player have an active crime bounty, AND is the player nearby?
        var queryConfrontation = new DecisionQueryNode<SkyrimGuardAgent>(
            guard => guard.HasPlayerBounty && guard.IsPlayerNearby,
            arrestAction,   // YES -> Confront player immediately
            queryRelaxTime  // NO  -> Evaluate shift/patrol
        );

        // 3. Sub-branch for Hostile Threat conditions (Dragon, monster, hostile player)
        // Question: Am I healthy enough to fight (HP > 20%)?
        var queryCombatStrength = new DecisionQueryNode<SkyrimGuardAgent>(
            guard => guard.IsHealthy,
            combatAction,   // YES -> Defend Skyrim!
            fleeAction      // NO  -> Run away yelling for help
        );

        // 4. Root Node
        // Question: Is there an active hostile threat present?
        rootNode = new DecisionQueryNode<SkyrimGuardAgent>(
            guard => guard.isThreatPresent,
            queryCombatStrength, // YES branch
            queryConfrontation   // NO branch
        );
    }

    void Update()
    {
        if (rootNode == null || agent == null || stateMachine == null) return;

        // Evaluate the decision tree
        string recommendedState = rootNode.Evaluate(agent);

        if (recommendedState != lastDecision)
        {
            lastDecision = recommendedState;
            Debug.Log($"<color=orange>[Decision Tree Decision]</color> Path evaluation chose: <color=yellow>\"{recommendedState}\"</color>");
        }

        // Apply state transition if required
        if (recommendedState != stateMachine.currentState)
        {
            stateMachine.ChangeState(recommendedState);
        }
    }

    private void OnGUI()
    {
        if (agent == null || stateMachine == null) return;

        // Display current AI state and world state info at the top-left of the screen
        Rect statsRect = new Rect(20, 20, 300, 185);
        GUI.Box(statsRect, "SKYRIM GUARD AI MONITOR");
        
        GUILayout.BeginArea(new Rect(statsRect.x + 10, statsRect.y + 20, statsRect.width - 20, statsRect.height - 30));
        GUILayout.Label($"<b>Active State (FSM):</b> <color=cyan>{stateMachine.currentState}</color>");
        GUILayout.Label($"<b>Decision Tree Choice:</b> <color=yellow>{lastDecision}</color>");
        GUILayout.Label($"<b>Guard HP:</b> {agent.health:F0}/{agent.maxHealth} ({(agent.IsHealthy ? "<color=green>Healthy</color>" : "<color=red>Critical</color>")})");
        GUILayout.Label($"<b>Active Threat:</b> {(agent.isThreatPresent ? "<color=red>YES (Dragon/Bandit)</color>" : "<color=green>None</color>")}");
        GUILayout.Label($"<b>Shift status:</b> {(agent.isBreakTime ? "Break Time / Night" : "Duty Hours")}");
        GUILayout.Label($"<b>Player Bounty:</b> <color=yellow>{agent.playerBounty} Gold</color> | <b>Distance:</b> {agent.distanceToPlayer:F1}m");
        GUILayout.EndArea();

        // Display interactive test control panel at the bottom-left of the screen
        Rect controlsRect = new Rect(20, 215, 300, 280);
        GUI.Box(controlsRect, "TEST CONTROL PANEL");

        GUILayout.BeginArea(new Rect(controlsRect.x + 10, controlsRect.y + 20, controlsRect.width - 20, controlsRect.height - 30));
        
        GUILayout.Label("<i>Manipulate variables to force FSM transitions:</i>");
        
        GUILayout.Space(5);

        if (GUILayout.Button(agent.isThreatPresent ? "Remove Threat (Calm)" : "Spawn Dragon (Threat Present)"))
        {
            agent.isThreatPresent = !agent.isThreatPresent;
            Debug.Log($"[Control Panel] Set Threat Present to: {agent.isThreatPresent}");
        }

        if (GUILayout.Button("Deal 30 Damage to Guard"))
        {
            agent.TakeDamage(30f);
        }

        if (GUILayout.Button("Heal Guard to Full"))
        {
            agent.HealFull();
        }

        GUILayout.Space(5);

        if (GUILayout.Button("Commit Sweetroll Theft (Bounty +100)"))
        {
            agent.playerBounty += 100;
            Debug.Log($"[Control Panel] Bounty increased to: {agent.playerBounty}");
        }

        if (GUILayout.Button("Clear Bounty"))
        {
            agent.playerBounty = 0;
            Debug.Log("[Control Panel] Bounty cleared.");
        }

        GUILayout.Space(5);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Approach Guard (Near)"))
        {
            if (agent.playerTarget != null)
            {
                agent.playerTarget.transform.position = agent.transform.position + agent.transform.forward * 3.0f;
            }
            agent.distanceToPlayer = 3.0f;
            Debug.Log("[Control Panel] Player stepped close to guard.");
        }
        if (GUILayout.Button("Back Away (Far)"))
        {
            if (agent.playerTarget != null)
            {
                agent.playerTarget.transform.position = agent.transform.position + agent.transform.forward * 15.0f;
            }
            agent.distanceToPlayer = 15.0f;
            Debug.Log("[Control Panel] Player backed away from guard.");
        }
        GUILayout.EndHorizontal();

        if (GUILayout.Button(agent.isBreakTime ? "Assign to Duty (Patrol)" : "Send on Break (Relax)"))
        {
            agent.isBreakTime = !agent.isBreakTime;
            Debug.Log($"[Control Panel] Shift Status toggled to Break Time = {agent.isBreakTime}");
        }

        GUILayout.EndArea();
    }
}
