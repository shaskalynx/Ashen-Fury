using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using static Node;

public class enemyPSO : MonoBehaviour
{
    public ParticleSwarmOptimizer pso;
    private List<Node> particles;
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

        particles = new List<Node>
        {
            new AttackPlayer(agent, attackRange, attackCD),
            new ChasePlayer(agent, detectionRadius, newDestinationCD),
            new Patrol(agent, target),
        };

        pso = new ParticleSwarmOptimizer(particles);
        currentHealth = enemyHealth.health;
    }

    void Update()
    {
        currentState = GetCurrentState();
        pso.Optimize(10, currentState);
        
        var weightedBehaviors = pso.GetWeightedBehaviors();
        
        bool behaviorExecuted = false;
        foreach (var behavior in weightedBehaviors)
        {
            if (behavior.weight >= 0.3f) // Only execute significant behaviors
            {
                result = behavior.node.Evaluate(currentState);
                if (result != NodeState.fail)
                {
                    behaviorExecuted = true;
                    stateName = behavior.node.name;
                    Debug.Log($"Enemy performing: {behavior.node.name} " +
                             $"(Weight: {behavior.weight:F2}, Position: {behavior.node.position:F2}, " +
                             $"Velocity: {behavior.velocity:F2}, Health: {currentHealth/health:F2}, " +
                             $"Distance: {currentState.x:F2})");
                    break;
                }
            }
        }

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
        if (particles != null && particles.Count >= 3)
        {
            // Update AttackPlayer node
            if (particles[0] is AttackPlayer attackNode)
            {
                attackNode.UpdateParameters(attackRange, attackCD);
            }
            
            // Update ChasePlayer node
            if (particles[1] is ChasePlayer chaseNode)
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
        
        if (particles != null)
        {
            int count = particles.Count;
            memory += sizeof(float) * count;     // positions
            memory += sizeof(float) * count;     // velocities
            memory += sizeof(float) * count * 2; // personal best positions and fitness
        }
        
        // Algorithm parameters
        memory += sizeof(float) * 4; // baseInertia, cognitive, social, globalBestPosition
        memory += sizeof(float);     // globalBestFitness
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