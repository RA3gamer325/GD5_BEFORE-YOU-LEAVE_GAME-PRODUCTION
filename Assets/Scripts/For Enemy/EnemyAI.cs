using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    [Header("Player Detection")]
    public Transform player;
    public float sightRange = 10f;
    public float sightAngle = 120f;
    public LayerMask sightLayers = -1;
    public float viewDistance = 20f;
    
    [Header("Patrol")]
    public float patrolSpeed = 2f;
    public Transform[] patrolPoints;
    public float waitAtPatrolPoint = 2f;
    
    [Header("Chase")]
    public float chaseSpeed = 5f;
    public float attackRange = 2f;
    
    [Header("Cover Detection")]
    public float investigateCoverDelay = 1f;
    
    [Header("Priority Detection")]
    public float confirmedChasePriority = 100f;
    public float coverInvestigatePriority = 50f;
    public float patrolPriority = 10f;

    [Header("Game Over / Jumpscare")]
    public bool hasKilledPlayer = false;
    public float killDelay = 1.5f;
    public GameObject jumpscareUI;
    public Camera playerCamera;

    // State
    private enum AIState { Patrolling, Chasing, InvestigatingCover, SearchingCover }
    private AIState currentState = AIState.Patrolling;

    // Components
    private NavMeshAgent agent;
    private Animator animator;

    // Detection
    [HideInInspector] public bool playerDetected;
    private Vector3 lastKnownPlayerPosition;
    private float lastSeenTime;
    private float lastConfirmedSightTime = 0f;
    public float confirmedSightMemory = 2f;

    // Patrol
    private int currentPatrolIndex;
    private float waitTime;

    // Priority
    private float currentPriority = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (agent != null)
        {
            agent.speed = patrolSpeed;
            agent.stoppingDistance = 0.5f;
        }

        if (patrolPoints.Length > 0)
            agent.SetDestination(patrolPoints[0].position);
    }

    void Update()
    {
        if (hasKilledPlayer) return;

        HandleDetection();
        CalculatePriorities();
        UpdateStateMachine();
    }

    void HandleDetection()
    {
        playerDetected = false;

        if (player == null) return;

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= sightRange)
        {
            float angle = Vector3.Angle(transform.forward, directionToPlayer);

            if (angle < sightAngle * 0.5f &&
                !Physics.Raycast(transform.position, directionToPlayer, distanceToPlayer, sightLayers))
            {
                playerDetected = true;
                lastKnownPlayerPosition = player.position;
                lastSeenTime = Time.time;
                lastConfirmedSightTime = Time.time;
            }
        }
    }

    void CalculatePriorities()
    {
        float chasePriority = 0f;
        float coverPriority = 0f;
        float patrolPrio = patrolPriority;

        if (playerDetected)
        {
            chasePriority = confirmedChasePriority;
        }
        else if ((Time.time - lastConfirmedSightTime) < confirmedSightMemory)
        {
            chasePriority = confirmedChasePriority * 0.8f;
        }
        else if (!playerDetected && (Time.time - lastSeenTime) < investigateCoverDelay)
        {
            coverPriority = coverInvestigatePriority;
        }

        currentPriority = Mathf.Max(patrolPrio, chasePriority, coverPriority);

        if (chasePriority >= confirmedChasePriority * 0.7f)
        {
            currentState = AIState.Chasing;
        }
        else if (coverPriority > patrolPrio)
        {
            currentState = AIState.InvestigatingCover;
        }
        else
        {
            currentState = AIState.Patrolling;
        }
    }

    void UpdateStateMachine()
    {
        UpdateAnimations();

        switch (currentState)
        {
            case AIState.Patrolling:
                Patrolling();
                break;

            case AIState.Chasing:
                ChasePlayerConfirmed();
                break;

            case AIState.InvestigatingCover:
                InvestigateCover();
                break;

            case AIState.SearchingCover:
                SearchCover();
                break;
        }
    }

    void UpdateAnimations()
    {
        if (animator == null) return;

        bool isMoving = agent.velocity.magnitude > 0.1f;

        animator.SetBool("isWalking", currentState == AIState.Patrolling && isMoving);
        animator.SetBool("isChasing", currentState == AIState.Chasing && isMoving);
    }

    void Patrolling()
    {
        ResetPatrolSpeed();

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            if (waitTime <= 0f)
            {
                if (patrolPoints.Length == 0)
                {
                    Vector3 randomPoint = transform.position + Random.insideUnitSphere * 10f;
                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(randomPoint, out hit, 10f, 1))
                        agent.SetDestination(hit.position);
                }
                else
                {
                    currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                    agent.SetDestination(patrolPoints[currentPatrolIndex].position);
                }

                waitTime = waitAtPatrolPoint;
            }
            else
            {
                waitTime -= Time.deltaTime;
            }
        }
    }

    void ChasePlayerConfirmed()
    {
        IncreaseSpeedForChase();

        if (agent == null || player == null) return;

        Vector3 chaseTarget;

        if (playerDetected)
        {
            Rigidbody playerRb = player.GetComponent<Rigidbody>();
            Vector3 playerVelocity = playerRb != null ? playerRb.linearVelocity : Vector3.zero;

            chaseTarget = player.position + playerVelocity * 0.5f;
        }
        else
        {
            chaseTarget = lastKnownPlayerPosition;
        }

        agent.SetDestination(chaseTarget);
        RotateTowardsTarget(chaseTarget);

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer < attackRange)
        {
            AttackPlayer();
        }
    }

    void InvestigateCover()
    {
        ResetPatrolSpeed();
        agent.SetDestination(lastKnownPlayerPosition);

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            currentState = AIState.SearchingCover;
        }
    }

    void SearchCover()
    {
        ResetPatrolSpeed();
        RotateTowardsTarget(lastKnownPlayerPosition);

        if ((Time.time - lastSeenTime) > 5f)
        {
            currentState = AIState.Patrolling;
        }
    }

    void AttackPlayer()
    {
        if (hasKilledPlayer) return;

        hasKilledPlayer = true;

        agent.isStopped = true;

        RotateTowardsTarget(player.position);

        if (animator != null)
            animator.SetTrigger("Attack");

        StartCoroutine(HandleGameOver());
    }

    IEnumerator HandleGameOver()
    {
        yield return new WaitForSeconds(killDelay);

        if (playerCamera != null)
            playerCamera.enabled = false;

        if (jumpscareUI != null)
            jumpscareUI.SetActive(true);

        Time.timeScale = 0f;
    }

    void IncreaseSpeedForChase()
    {
        if (agent != null && agent.speed < chaseSpeed)
        {
            agent.speed = chaseSpeed;
        }
    }

    void ResetPatrolSpeed()
    {
        if (agent != null && agent.speed > patrolSpeed)
        {
            agent.speed = patrolSpeed;
        }
    }

    void RotateTowardsTarget(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = playerDetected ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}