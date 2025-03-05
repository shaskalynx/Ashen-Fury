using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using static Node;

public class enemy : MonoBehaviour
{
    public GreyWolfOptimizer gwo;
    public List<Node> wolves;
    public Transform target;
    public UnityEngine.AI.NavMeshAgent agent;
    public float detectionRadius = 10f;

    public float health = 100f;
    public float currentHealth;
    public Vector3 currentState;
    public NodeState result;

    public string stateName;
    public float distanceToPlayer;
    public float distanceToPatrolPoint;

    enemyHealthSystem enemyHealth;

    void Start()
    {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        enemyHealth = GetComponent<enemyHealthSystem>();

        wolves = new List<Node>
        {
            new AttackPlayer(agent, 2f),
            new ChasePlayer(agent, detectionRadius),
            new Patrol(agent, target),
        };

        gwo = new GreyWolfOptimizer(wolves);
        currentHealth = enemyHealth.health;
    }

    void Update()
    {
        currentState = GetCurrentState();
        gwo.Optimize(10, currentState);
        
        // Get behaviors weighted by both fitness and GWO position
        var weightedBehaviors = gwo.GetWeightedBehaviors();
        
        // Execute behaviors with significant influence
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
                             $"Health: {currentHealth/health:F2}, " +
                             $"Distance: {currentState.x:F2})"); 
                    break;
                }
            }
        }

        // Fallback to highest weight behavior if nothing executed
        if (!behaviorExecuted && weightedBehaviors.Count > 0)
        {
            weightedBehaviors[0].node.Evaluate(currentState);
        }
    }

    private Vector3 GetCurrentState()
    {
        GameObject player = GameObject.FindWithTag("Player");
        distanceToPlayer = player ? Vector3.Distance(transform.position, player.transform.position) : float.MaxValue;
        float healthPercentage = currentHealth / enemyHealth.health;
        distanceToPatrolPoint = Vector3.Distance(transform.position, target.position);

        return new Vector3(distanceToPlayer, healthPercentage, distanceToPatrolPoint);
    }

    /* public void TakeDamage(float damageAmount)
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
    } */

    public float GetMemoryUsage()
    {
        float memory = 0;
        
        // List of wolves
        if (wolves != null)
        {
            memory += sizeof(float) * wolves.Count; // positions
            memory += sizeof(float) * wolves.Count; // fitness values
        }
        
        // Hierarchy references
        memory += sizeof(float) * 3; // alpha, beta, delta positions
        
        // Algorithm parameters
        memory += sizeof(float);     // a parameter
        memory += sizeof(float);     // MIN_POSITION constant
        memory += sizeof(float) * 3; // currentState (Vector3 = 3 floats)
        
        return memory;
    }
}