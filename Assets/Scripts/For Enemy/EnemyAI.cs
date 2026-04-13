using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("Navigation")]
    public NavMeshAgent agent;
    public Transform player;
    public LayerMask whatIsGround, whatIsPlayer;

    [Header("Patroling")]
    public Vector3 walkPoint;
    public float walkPointRange;
    bool walkPointSet;
    public float patrolRotationSpeed = 5f;
    public float patrolSpeed = 3f;
    public float lookAroundSpeed = 2f;
    public float patrolDelay = 2f;
    float patrolDelayTimer;
    bool isPatrolDelayActive;

    [Header("Attacking")]
    public float timeBetweenAttacks;
    float lastAttackTime;

    [Header("Detection")]
    public float sightRange;
    public float detectionAngleThreshold = 60f;
    public float detectionBuffer = 10f; // Extra buffer for detection
    public float chaseRange = 15f; // Range to continue chasing after detection
    Vector3 lastKnownPlayerPosition;
    float lastSeenTime;
    public float playerLostTime = 3f; // How long to remember player after losing sight

    [Header("States")]
    public bool playerInSightRange;
    public bool playerInAngleRange;
    public bool playerDetected; // Overall detection state

    private void Awake()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogError("Player GameObject not found! Tag it as 'Player'.");
        }

        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent component not found on this enemy!");
        }
        else
        {
            agent.speed = patrolSpeed;
            agent.angularSpeed = 180f;
            agent.acceleration = 1000f;
            agent.stoppingDistance = 0.5f;
            agent.autoRepath = true;
        }
    }

    private void Update()
    {
        // Check if player is within sight range
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange + detectionBuffer, whatIsPlayer);
        
        // Check if player is within the enemy's forward-facing angle
        playerInAngleRange = CheckAngleToPlayer();
        
        // Check if player is within chase range (for memory)
        bool playerInChaseRange = Vector3.Distance(transform.position, player.position) < chaseRange;
        
        // Overall detection state
        playerDetected = playerInSightRange && playerInAngleRange;
        
        // Update last known position
        if (playerDetected)
        {
            lastKnownPlayerPosition = player.position;
            lastSeenTime = Time.time;
        }
        
        // Check if player is still remembered
        bool playerStillRemembered = (Time.time - lastSeenTime) < playerLostTime;

        // State Machine
        if (!playerDetected && !playerStillRemembered)
        {
            Patroling();
        }
        else if (playerDetected || playerStillRemembered)
        {
            ChasePlayer();
        }
    }

    private bool CheckAngleToPlayer()
    {
        if (player == null) return false;

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);

        return angle < detectionAngleThreshold;
    }

    private void Patroling()
    {
        if (!walkPointSet)
        {
            SearchWalkPoint();
        }

        if (walkPointSet && agent != null)
        {
            agent.SetDestination(walkPoint);
            RotateTowardsWalkPoint();
            LookAround();

            if (agent.remainingDistance < 0.5f && !agent.pathPending)
            {
                if (!isPatrolDelayActive)
                {
                    isPatrolDelayActive = true;
                    patrolDelayTimer = patrolDelay;
                }

                if (patrolDelayTimer > 0)
                {
                    patrolDelayTimer -= Time.deltaTime;
                }
                else
                {
                    isPatrolDelayActive = false;
                    walkPointSet = false;
                }
            }
        }
    }

    private void RotateTowardsWalkPoint()
    {
        Vector3 directionToWalkPoint = (walkPoint - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(directionToWalkPoint);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, patrolRotationSpeed * Time.deltaTime);
    }

    private void LookAround()
    {
        float lookAroundAmount = lookAroundSpeed * Time.deltaTime;
        transform.Rotate(Vector3.up, lookAroundAmount, Space.World);
    }

    private void SearchWalkPoint()
    {
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        NavMeshHit hit;
        if (NavMesh.SamplePosition(walkPoint, out hit, 2f, NavMesh.AllAreas))
        {
            walkPoint = hit.position;
            walkPointSet = true;
        }
        else
        {
            walkPointSet = false;
            Debug.Log("Walk point not on NavMesh, trying again...");
        }
    }

    private void ChasePlayer()
    {
        if (agent != null && player != null)
        {
            // Chase towards player or last known position
            Vector3 chaseTarget = playerDetected ? player.position : lastKnownPlayerPosition;
            agent.SetDestination(chaseTarget);
            
            // Rotate towards player while chasing
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, patrolRotationSpeed * Time.deltaTime);

            // Attack if player is close enough
            if (Vector3.Distance(transform.position, player.position) < sightRange * 0.5f)
            {
                AttackPlayer();
            }
        }
    }

    private void AttackPlayer()
    {
        if (Time.time - lastAttackTime >= timeBetweenAttacks)
        {
            if (CheckAngleToPlayer())
            {
                Debug.Log("Enemy Attacked!");
                lastAttackTime = Time.time;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw Sight Range (Green Sphere)
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, sightRange + detectionBuffer);

        // Draw Chase Range (Blue Sphere)
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        if (sightRange > 0)
        {
            // Draw Detection Angle (Yellow Cone)
            Gizmos.color = Color.yellow;
            float coneRadius = sightRange * Mathf.Tan(detectionAngleThreshold * 0.5f * Mathf.Deg2Rad);
            Vector3 coneBase = transform.position + transform.forward * sightRange;
            Gizmos.DrawWireSphere(coneBase, coneRadius);
            
            Vector3 leftEdge = transform.forward * sightRange;
            Vector3 rightEdge = transform.forward * sightRange;
            Quaternion leftRot = Quaternion.Euler(0, detectionAngleThreshold * 0.5f, 0);
            Quaternion rightRot = Quaternion.Euler(0, -detectionAngleThreshold * 0.5f, 0);
            
            Gizmos.DrawRay(transform.position, leftRot * leftEdge);
            Gizmos.DrawRay(transform.position, rightRot * rightEdge);
        }
    }
}