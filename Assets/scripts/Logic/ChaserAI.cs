// ChaserAI.cs

using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;

public class ChaserAI : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Transform[] patrolPoints;

    [Header("Behavior Settings")]
    public float detectionRadius = 15f;
    public float eyeHeight = 1.5f; // Altura de los "ojos" del monstruo para el Raycast
    public float chaseSpeed = 3.5f;
    public float patrolSpeed = 2f;
    public float attackDistance = 1f; // How close to get before attacking

    [Header("Audio")]
    public AudioClip[] walkFootstepSounds;
    public AudioClip[] runFootstepSounds;
    private AudioSource audioSource;

    // Componentes y estado
    private NavMeshAgent agent;
    private Animator animator;
    private bool isChasing = false;
    private int currentPatrolIndex = 0;
    private PlayerStatus playerStatus;
    private bool hasAttacked = false; // Prevents attacking multiple times

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        if (player == null)
        {
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null) player = playerGO.transform;
            else { this.enabled = false; return; }
        }

        if (player != null)
        {
            playerStatus = player.GetComponent<PlayerStatus>();
        }
        else
        {
            Debug.LogError("Chaser AI could not find the Player!");
            this.enabled = false;
            return;
        }

        // Iniciar patrulla
        agent.speed = patrolSpeed;
        GoToNextPatrolPoint();
    }

    void Update()
    {
        if (player == null) return;

        if (CanSeePlayer())
        {
            if (!isChasing)
            {
                isChasing = true;
                agent.speed = chaseSpeed;
            }
            agent.SetDestination(player.position);
        }
        else
        {
            if (isChasing)
            {
                isChasing = false;
                agent.speed = patrolSpeed;
                GoToNextPatrolPoint();
            }

            if (!agent.pathPending && agent.remainingDistance < 0.5f)
            {
                GoToNextPatrolPoint();
            }
        }

        if (!hasAttacked && isChasing && Vector3.Distance(transform.position, player.position) <= attackDistance)
        {
            // --- TRIGGER THE ATTACK SEQUENCE ---
            hasAttacked = true;

            // 1. Stop the agent from moving
            agent.isStopped = true;

            // 2. Tell the animator to play the attack animation
            animator.SetBool("attack01", true);

            // 3. Tell the player to start the death sequence
            if (playerStatus != null)
            {
                // We create a new Transform point for the player to look at, using the enemy's eyeHeight
                Vector3 lookTargetPosition = transform.position + Vector3.up * eyeHeight;
                // We create a temporary empty GameObject to pass as the target
                GameObject lookTarget = new GameObject("EnemyLookTarget");
                lookTarget.transform.position = lookTargetPosition;

                playerStatus.InitiateDeathSequence(lookTarget.transform);
            }
        }

        UpdateAnimator();
    }

    // Comprueba si el jugador está en el radio Y si hay línea de visión directa
    private bool CanSeePlayer()
    {
        if (Vector3.Distance(transform.position, player.position) > detectionRadius)
        {
            return false;
        }

        Vector3 startPoint = transform.position + Vector3.up * eyeHeight;
        Vector3 directionToPlayer = player.position - startPoint;
        RaycastHit hit;

        if (Physics.Raycast(startPoint, directionToPlayer, out hit, detectionRadius))
        {
            if (hit.transform == player)
            {
                return true; // El rayo golpeó al jugador, hay visión directa.
            }
        }
        return false; // El rayo golpeó una pared o nada.
    }

    public void PlayFootstepSound()
    {
        // 1. Choose the correct array of sounds based on the AI's state
        AudioClip[] clips = isChasing ? runFootstepSounds : walkFootstepSounds;

        // 2. If the array is empty, do nothing (to prevent errors)
        if (clips.Length == 0)
        {
            return;
        }

        // 3. Randomly select one clip from the array
        AudioClip clipToPlay = clips[Random.Range(0, clips.Length)];

        // 4. Add slight random pitch variation for more natural sound
        audioSource.pitch = Random.Range(0.95f, 1.05f);

        // 5. Play the chosen clip
        if (clipToPlay != null)
        {
            audioSource.volume = isChasing ? 2.5f : 1.3f; // Adjust volume based on state
            audioSource.PlayOneShot(clipToPlay);
        }
    }

    // Actualiza los parámetros del Animator basados en el estado actual
    void UpdateAnimator()
    {
        // Comprueba si el agente se está moviendo
        bool isMoving = agent.velocity.sqrMagnitude > 0.1f;

        animator.SetBool("walk", isMoving);
        animator.SetBool("run", isChasing);
    }

    void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    private void OnDrawGizmosSelected()
    {
        // 1. Dibuja la esfera de radio de detección en amarillo
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Solo dibuja la línea de visión si el jugador existe
        if (player != null)
        {
            // 2. Calcula el punto de origen de la visión (los "ojos" del monstruo)
            Vector3 startPoint = transform.position + Vector3.up * eyeHeight;

            // 3. Cambia el color de la línea basado en si puede ver al jugador o no
            //    - Verde: Hay línea de visión directa.
            //    - Rojo: No hay línea de visión (un obstáculo bloquea el camino).
            if (CanSeePlayer())
            {
                Gizmos.color = Color.green;
            }
            else
            {
                Gizmos.color = Color.red;
            }

            // 4. Dibuja la línea recta desde los ojos del monstruo hasta la posición del jugador
            Gizmos.DrawLine(startPoint, player.position);
        }
    }
}