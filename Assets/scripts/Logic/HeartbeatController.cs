using System.Collections;
using UnityEngine;

public class DynamicHeartbeatController : MonoBehaviour
{
    [Header("Audio Components")]
    public AudioSource heartbeatAudioSource; // Assign your AudioSource
    public AudioClip heartbeatClip;          // Assign your single beat sound clip

    [Header("Heartbeat Settings")]
    [Tooltip("The distance at which the heartbeat starts.")]
    public float maxDistance = 20f;
    [Tooltip("The distance for the most intense heartbeat.")]
    public float minDistance = 5f;

    [Tooltip("The longest time between beats (when enemy is far).")]
    public float maxInterval = 2.0f; // 2 seconds between beats
    [Tooltip("The shortest time between beats (when enemy is close).")]
    public float minInterval = 0.4f; // Almost 3 beats per second

    [Tooltip("Volume of the heartbeat when far/close.")]
    [Range(0, 1)] public float minVolume = 0.2f;
    [Range(0, 1)] public float maxVolume = 1.0f;

    [Header("Enemy Detection")]
    public string enemyTag = "Enemy";
    private Transform closestEnemy;
    private bool isHeartbeatActive = false; // Flag to check if the coroutine is running

    void Update()
    {
        FindClosestEnemy();

        if (closestEnemy != null)
        {
            float distance = Vector3.Distance(transform.position, closestEnemy.position);

            // If the enemy is within range and the heartbeat isn't already going...
            if (distance < maxDistance && !isHeartbeatActive)
            {
                // Start the heartbeat!
                StartCoroutine(HeartbeatCoroutine());
            }
        }
        else if (isHeartbeatActive)
        {
            // If there are no enemies left, but the heartbeat was running, stop it.
            StopCoroutine(HeartbeatCoroutine());
            isHeartbeatActive = false;
        }
    }

    private IEnumerator HeartbeatCoroutine()
    {
        isHeartbeatActive = true;

        // This loop will run as long as an enemy is nearby
        while (closestEnemy != null)
        {
            float distance = Vector3.Distance(transform.position, closestEnemy.position);

            // If the enemy has gone out of range, break the loop
            if (distance > maxDistance)
            {
                break;
            }

            // Calculate proximity (0 for far, 1 for close)
            float proximity = Mathf.InverseLerp(maxDistance, minDistance, distance);

            // Calculate the current volume and the time to wait for the next beat
            float currentVolume = Mathf.Lerp(minVolume, maxVolume, proximity);
            float currentInterval = Mathf.Lerp(maxInterval, minInterval, proximity);

            // Play the single heartbeat sound at the calculated volume
            heartbeatAudioSource.PlayOneShot(heartbeatClip, currentVolume);

            // Wait for the calculated interval before the next beat
            yield return new WaitForSeconds(currentInterval);
        }

        // The loop has ended (enemy is out of range), so reset the flag
        isHeartbeatActive = false;
    }

    // Finds the closest enemy (simplified for clarity, can be optimized if you have many enemies)
    private void FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        float closestDistSqr = Mathf.Infinity;
        closestEnemy = null;

        foreach (GameObject enemyObject in enemies)
        {
            float distSqr = (transform.position - enemyObject.transform.position).sqrMagnitude;
            if (distSqr < closestDistSqr)
            {
                closestDistSqr = distSqr;
                closestEnemy = enemyObject.transform;
            }
        }
    }
}