using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using static Node;

public class enemyFA : MonoBehaviour
{
    public FireflyOptimizer fa;
    private List<Node> fireflies;
    public Transform target;
    private UnityEngine.AI.NavMeshAgent agent;
    
    [Header("Detection & Combat")]
    [SerializeField] public float detectionRadius = 10f;
    [SerializeField] public float attackRange = 2f;
    [SerializeField] public float attackCD = 3f;
    [SerializeField] public float newDestinationCD = 0.5f;
    private float currentDestinationCD;

    [Header("Health")]
    [SerializeField] public float health = 100f;
    public float currentHealth;
    public Vector3 currentState;
    public NodeState result;

    [Header("Status")]
    [SerializeField] public string stateName;
    [SerializeField] public float distanceToPlayer;
    [SerializeField] public float distanceToPatrolPoint;

    enemyHealthSystem enemyHealth;
    GameObject player;

    void Start()
    {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        enemyHealth = GetComponent<enemyHealthSystem>();
        player = GameObject.FindWithTag("Player");
        currentDestinationCD = 0;

        fireflies = new List<Node>
        {
            new AttackPlayer(agent, attackRange, attackCD),
            new ChasePlayer(agent, detectionRadius, newDestinationCD),
            new Patrol(agent, target),
        };

        fa = new FireflyOptimizer(fireflies);
        currentHealth = enemyHealth.health;
    }

    void Update()
    {
        currentState = GetCurrentState();
        fa.Optimize(10, currentState);
        
        var weightedBehaviors = fa.GetWeightedBehaviors();
        
        bool behaviorExecuted = false;
        foreach (var behavior in weightedBehaviors)
        {
            if (behavior.weight >= 0.3f) // Only execute significantly bright behaviors
            {
                result = behavior.node.Evaluate(currentState);
                if (result != NodeState.fail)
                {
                    behaviorExecuted = true;
                    stateName = behavior.node.name;
                    Debug.Log($"Enemy performing: {behavior.node.name} " +
                             $"(Weight: {behavior.weight:F2}, Brightness: {behavior.brightness:F2}, " +
                             $"Health: {currentHealth/health:F2}, " +
                             $"Distance: {currentState.x:F2})");
                    break;
                }
            }
        }

        // Fallback to brightest behavior if nothing executed
        if (!behaviorExecuted && weightedBehaviors.Count > 0)
        {
            weightedBehaviors[0].node.Evaluate(currentState);
        }
    }

    private Vector3 GetCurrentState()
    {
        player = GameObject.FindWithTag("Player");
        distanceToPlayer = player ? Vector3.Distance(transform.position, player.transform.position) : float.MaxValue;
        float healthPercentage = currentHealth / enemyHealth.health;
        distanceToPatrolPoint = Vector3.Distance(transform.position, target.position);

        return new Vector3(distanceToPlayer, healthPercentage, distanceToPatrolPoint);
    }

    // Method to update node parameters if they change in the inspector
    public void UpdateNodeParameters()
    {
        if (fireflies != null && fireflies.Count >= 3)
        {
            // Update AttackPlayer node
            if (fireflies[0] is AttackPlayer attackNode)
            {
                attackNode.UpdateParameters(attackRange, attackCD);
            }
            
            // Update ChasePlayer node
            if (fireflies[1] is ChasePlayer chaseNode)
            {
                chaseNode.UpdateParameters(detectionRadius, newDestinationCD);
            }
        }
    }

    // Call this in editor to apply changes made in inspector
    public void OnValidate()
    {
        if (Application.isPlaying)
        {
            UpdateNodeParameters();
        }
    }

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        if(currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    public float GetMemoryUsage()
    {
        float memory = 0;
        
        if (fireflies != null)
        {
            int count = fireflies.Count;
            memory += sizeof(float) * count;     // positions
            memory += sizeof(float) * count;     // intensity array
            memory += sizeof(float) * count;     // attractiveness array
        }
        
        // Algorithm parameters
        memory += sizeof(float) * 4; // beta0, gamma, alpha, populationSize
        memory += sizeof(float) * 3; // currentState (Vector3 = 3 floats)
        
        return memory;
    }
    
    private void OnDrawGizmos()
    {
        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Draw detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}