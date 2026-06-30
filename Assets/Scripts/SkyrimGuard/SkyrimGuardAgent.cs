using UnityEngine;

public class SkyrimGuardAgent : MonoBehaviour
{
    [Header("Guard Health")]
    [Tooltip("Current health of the guard. Below 20% they will flee from threats.")]
    public float health = 100f;
    public float maxHealth = 100f;

    [Header("Environment & Threats")]
    [Tooltip("Is there an active hostile threat (e.g. dragon, bandit)?")]
    public bool isThreatPresent = false;
    
    [Tooltip("Is it the guard's break time or night time?")]
    public bool isBreakTime = false;

    [Header("Player Tracking")]
    [Tooltip("The actual player GameObject in the scene. If left empty, the script will automatically search for a GameObject tagged 'Player' or named 'Player'.")]
    public GameObject playerTarget;

    [Tooltip("The player's current criminal bounty in the hold.")]
    public int playerBounty = 0;
    
    [Tooltip("Current distance between the guard and the player (automatically calculated if Player Target exists).")]
    public float distanceToPlayer = 15f;


    [Header("Patrol Settings")]
    [Tooltip("Waypoints for patrolling. If empty, default random ones are generated.")]
    public Vector3[] patrolWaypoints;
    
    [HideInInspector]
    public int currentWaypointIndex = 0;

    void Awake()
    {
        // Fallback: If no waypoints are assigned in the inspector, generate some mock ones around the guard
        if (patrolWaypoints == null || patrolWaypoints.Length == 0)
        {
            Vector3 startPos = transform.position;
            patrolWaypoints = new Vector3[]
            {
                startPos,
                startPos + new Vector3(5f, 0f, 5f),
                startPos + new Vector3(-5f, 0f, 8f),
                startPos + new Vector3(0f, 0f, 12f)
            };
        }
    }

    void Start()
    {
        // Try to automatically find the player in the scene
        if (playerTarget == null)
        {
            playerTarget = GameObject.FindGameObjectWithTag("Player");
            if (playerTarget == null)
            {
                playerTarget = GameObject.Find("Player");
            }
        }

        if (playerTarget != null)
        {
            Debug.Log($"<color=green>[Guard Agent]</color> Linked successfully with Player: <color=yellow>{playerTarget.name}</color>");
        }
        else
        {
            Debug.LogWarning("[Guard Agent] No active Player GameObject found in the scene! Defaulting to virtual calculations.");
        }
    }

    void Update()
    {
        // Automatically calculate 3D distance to the actual player capsule
        if (playerTarget != null)
        {
            distanceToPlayer = Vector3.Distance(transform.position, playerTarget.transform.position);
        }
    }

    /// <summary>
    /// Helper property to check if the guard is healthy (above 20% health).
    /// </summary>
    public bool IsHealthy => health > (maxHealth * 0.2f);

    /// <summary>
    /// Helper property to check if the player is within crime confrontation range.
    /// </summary>
    public bool IsPlayerNearby => distanceToPlayer <= 6.0f;

    /// <summary>
    /// Helper property to check if the player has a bounty.
    /// </summary>
    public bool HasPlayerBounty => playerBounty > 0;

    /// <summary>
    /// Simulates taking damage (useful for testing flee threshold).
    /// </summary>
    public void TakeDamage(float amount)
    {
        health = Mathf.Max(0f, health - amount);
        isThreatPresent = true; // Automatically registers a hostile threat upon taking damage!
        Debug.Log($"<color=orange>[Guard Agent]</color> Took {amount} damage. Current HP: {health}/{maxHealth}");
    }


    /// <summary>
    /// Heals the guard back to full health.
    /// </summary>
    public void HealFull()
    {
        health = maxHealth;
        Debug.Log($"<color=green>[Guard Agent]</color> Healed to full health: {health}/{maxHealth}");
    }
}
