// ChaserAI.cs

using UnityEngine;
using UnityEngine.AI; // Important: Include the AI namespace

public class ChaserAI : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Transform[] patrolPoints; // NEW: An array to hold our waypoints.

    [Header("Behavior Settings")]
    public float detectionDistance = 15f;
    public float chaseSpeed = 3.5f;
    public float patrolSpeed = 2f; // NEW: A slower speed for when patrolling.

    // Private variables
    private NavMeshAgent agent;
    private float distanceToPlayer;
    private bool isChasing = false;
    private int currentPatrolIndex = 0; // NEW: To keep track of the current waypoint.

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Find the player by tag if not assigned manually
        if (player == null)
        {
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null)
            {
                player = playerGO.transform;
            }
            else
            {
                Debug.LogError("ERROR! Player object with tag 'Player' not found. Disabling AI.");
                this.enabled = false; // Disable script if no player is found
            }
        }

        // MODIFIED: Start by patrolling, not standing still.
        GoToNextPatrolPoint();
    }

    void Update()
    {
        if (player == null) return;

        // Calculate distance to the player
        distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // --- Main Logic: Chase or Patrol ---

        // 1. CHASE LOGIC
        if (distanceToPlayer <= detectionDistance)
        {
            // If we weren't chasing before, start now.
            if (!isChasing)
            {
                isChasing = true;
                Debug.Log("Player spotted! Initiating chase.");
                agent.speed = chaseSpeed; // Switch to faster chase speed
            }

            // Set the player as the destination
            agent.SetDestination(player.position);
        }
        // 2. PATROL LOGIC
        else
        {
            // If we were just chasing, switch back to patrol mode.
            if (isChasing)
            {
                isChasing = false;
                Debug.Log("Player lost... returning to patrol.");
                agent.speed = patrolSpeed; // Switch back to slower patrol speed
                // No need to call GoToNextPatrolPoint() here, the logic below handles it.
            }

            // NEW: While patrolling, check if we've reached our destination.
            // !agent.pathPending ensures the agent has calculated a path.
            // agent.remainingDistance < 0.5f checks if we are very close to the target.
            if (!agent.pathPending && agent.remainingDistance < 0.5f)
            {
                GoToNextPatrolPoint();
            }
        }
    }

    // NEW FUNCTION: Manages the patrol logic.
    void GoToNextPatrolPoint()
    {
        // If there are no patrol points, do nothing.
        if (patrolPoints.Length == 0)
        {
            Debug.LogWarning("No patrol points assigned to the enemy.");
            return;
        }

        // Set the agent's destination to the current waypoint in the array.
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);

        // Update the index for the next waypoint, cycling back to the start.
        // The '%' (modulo) operator is perfect for creating loops.
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    // Optional: Draws the detection sphere in the Scene view for easier debugging.
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionDistance);
    }
}