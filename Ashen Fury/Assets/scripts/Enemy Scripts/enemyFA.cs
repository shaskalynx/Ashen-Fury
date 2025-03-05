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
    public float detectionRadius = 10f;

    public float health = 100f;
    public float currentHealth;

    void Start()
    {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();

        fireflies = new List<Node>
        {
            new AttackPlayer(agent, 2f),
            new ChasePlayer(agent, detectionRadius),
            new Patrol(agent, target),
        };

        fa = new FireflyOptimizer(fireflies);
        currentHealth = health;
    }

    void Update()
    {
        Vector3 currentState = GetCurrentState();
        fa.Optimize(10, currentState);
        
        var weightedBehaviors = fa.GetWeightedBehaviors();
        
        bool behaviorExecuted = false;
        foreach (var behavior in weightedBehaviors)
        {
            if (behavior.weight >= 0.3f) // Only execute significantly bright behaviors
            {
                NodeState result = behavior.node.Evaluate(currentState);
                if (result != NodeState.fail)
                {
                    behaviorExecuted = true;
                    //Debug.Log($"Enemy performing: {behavior.node.name} (Weight: {behavior.weight:F2}, Brightness: {behavior.brightness:F2}, Position: {behavior.node.position:F2})");
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
        GameObject player = GameObject.FindWithTag("Player");
        float distanceToPlayer = player ? Vector3.Distance(transform.position, player.transform.position) : float.MaxValue;
        float healthPercentage = currentHealth / health;
        float distanceToPatrolPoint = Vector3.Distance(transform.position, target.position);

        return new Vector3(distanceToPlayer, healthPercentage, distanceToPatrolPoint);
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

    // Add to enemyFA.cs
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
}