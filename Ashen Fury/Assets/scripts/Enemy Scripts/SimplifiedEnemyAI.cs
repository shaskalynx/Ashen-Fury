using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class SimplifiedEnemyAI : MonoBehaviour
{
    [Header("References")]
    public Transform target;
    private NavMeshAgent agent;
    private EnemySoundManager soundManager;
    
    [Header("Detection & Combat")]
    [SerializeField] private float aggroRange = 10f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackCD = 3f;
    [SerializeField] private float newDestinationCD = 0.5f;
    
    [Header("Debug")]
    [SerializeField] private string currentBehavior;
    
    private List<SimplifiedNode> behaviors;
    private enemyHealthSystem enemyHealth;
    private GameObject player;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyHealth = GetComponent<enemyHealthSystem>();
        player = GameObject.FindWithTag("Player");
        
        if (soundManager == null)
        {
            soundManager = GetComponent<EnemySoundManager>();
            if (soundManager == null)
            {
                soundManager = gameObject.AddComponent<EnemySoundManager>();
            }
        }

        // Create behaviors in priority order
        behaviors = new List<SimplifiedNode>
        {
            new SimpleAttackNode(agent, attackRange, attackCD, soundManager),
            new SimpleChaseNode(agent, aggroRange, newDestinationCD, soundManager),
            new SimplePatrolNode(agent, target, soundManager)
        };
    }

    void Update()
    {
        Vector3 state = GetCurrentState();
        
        // Try behaviors in priority order until one succeeds
        foreach (var behavior in behaviors)
        {
            if (behavior.Evaluate(state) != SimplifiedNode.NodeState.fail)
            {
                currentBehavior = behavior.name;
                break;
            }
        }
    }

    private Vector3 GetCurrentState()
    {
        player = GameObject.FindWithTag("Player");
        float distanceToPlayer = player ? Vector3.Distance(transform.position, player.transform.position) : float.MaxValue;
        float healthPercentage = enemyHealth.health / 100f;
        float distanceToPatrolPoint = Vector3.Distance(transform.position, target.position);

        return new Vector3(distanceToPlayer, healthPercentage, distanceToPatrolPoint);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroRange);
    }
}

public abstract class SimplifiedNode
{
    public enum NodeState { success, fail, running }
    public string name;
    protected EnemySoundManager soundManager;
    
    protected void RotateTowards(Transform enemyTransform, Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - enemyTransform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            enemyTransform.rotation = Quaternion.Slerp(
                enemyTransform.rotation, 
                lookRotation, 
                Time.deltaTime * 5f
            );
        }
    }

    public abstract NodeState Evaluate(Vector3 state);
}

public class SimpleAttackNode : SimplifiedNode
{
    private NavMeshAgent agent;
    private float attackRange;
    private float attackCooldown;
    private float currentCooldown = 0f;

    public SimpleAttackNode(NavMeshAgent agent, float attackRange, float attackCooldown, EnemySoundManager soundManager)
    {
        this.agent = agent;
        this.attackRange = attackRange;
        this.attackCooldown = attackCooldown;
        this.soundManager = soundManager;
        this.name = "Attack";
    }

    public override NodeState Evaluate(Vector3 state)
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return NodeState.fail;

        float distanceToPlayer = Vector3.Distance(agent.transform.position, player.transform.position);
        currentCooldown -= Time.deltaTime;

        if (distanceToPlayer <= attackRange)
        {
            var animHandler = agent.GetComponent<animationHandler>();
            if (animHandler != null && !animHandler.IsAttacking)
            {
                RotateTowards(agent.transform, player.transform.position);
            }

            if (currentCooldown <= 0)
            {
                currentCooldown = attackCooldown;
                if (animHandler != null)
                {
                    animHandler.attack();
                }
            }
            return NodeState.success;
        }
        return NodeState.fail;
    }
}

public class SimpleChaseNode : SimplifiedNode
{
    private NavMeshAgent agent;
    private float aggroRange;
    private float newDestinationCD;
    private float currentDestinationCD = 0f;
    private bool wasChasing = false;

    public SimpleChaseNode(NavMeshAgent agent, float aggroRange, float newDestinationCD, EnemySoundManager soundManager)
    {
        this.agent = agent;
        this.aggroRange = aggroRange;
        this.newDestinationCD = newDestinationCD;
        this.soundManager = soundManager;
        this.name = "Chase";
    }

    public override NodeState Evaluate(Vector3 state)
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return NodeState.fail;

        float distanceToPlayer = Vector3.Distance(agent.transform.position, player.transform.position);
        currentDestinationCD -= Time.deltaTime;

        if (distanceToPlayer <= aggroRange)
        {
            if (!wasChasing && soundManager != null)
            {
                soundManager.PlayAlerted();
                wasChasing = true;
            }

            if (currentDestinationCD <= 0)
            {
                agent.SetDestination(player.transform.position);
                currentDestinationCD = newDestinationCD;
            }

            RotateTowards(agent.transform, player.transform.position);
            return NodeState.running;
        }

        wasChasing = false;
        return NodeState.fail;
    }
}

public class SimplePatrolNode : SimplifiedNode
{
    private NavMeshAgent agent;
    private Transform target;
    private float wanderRadius = 10f;
    private float minWanderDistance = 5f;

    public SimplePatrolNode(NavMeshAgent agent, Transform target, EnemySoundManager soundManager)
    {
        this.agent = agent;
        this.target = target;
        this.soundManager = soundManager;
        this.name = "Patrol";
    }

    public override NodeState Evaluate(Vector3 state)
    {
        if (!agent.hasPath || agent.remainingDistance <= agent.stoppingDistance)
        {
            Vector3 randomPoint = GetRandomPoint();
            if (randomPoint != agent.transform.position)
            {
                agent.SetDestination(randomPoint);
            }
        }

        return NodeState.running;
    }

    private Vector3 GetRandomPoint()
    {
        for (int i = 0; i < 30; i++)
        {
            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
            randomDirection += agent.transform.position;
            
            if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
            {
                if (Vector3.Distance(hit.position, agent.transform.position) >= minWanderDistance)
                {
                    return hit.position;
                }
            }
        }
        return agent.transform.position;
    }
}
