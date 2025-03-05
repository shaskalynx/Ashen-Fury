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
    public float detectionRadius = 10f;

    public float health = 100f;
    public float currentHealth;

    void Start()
    {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();

        particles = new List<Node>
        {
            new AttackPlayer(agent, 2f),
            new ChasePlayer(agent, detectionRadius),
            new Patrol(agent, target),
        };

        pso = new ParticleSwarmOptimizer(particles);
        currentHealth = health;
    }

    void Update()
    {
        Vector3 currentState = GetCurrentState();
        pso.Optimize(10, currentState);
        
        var weightedBehaviors = pso.GetWeightedBehaviors();
        
        bool behaviorExecuted = false;
        foreach (var behavior in weightedBehaviors)
        {
            if (behavior.weight >= 0.3f) // Only execute significant behaviors
            {
                NodeState result = behavior.node.Evaluate(currentState);
                if (result != NodeState.fail)
                {
                    behaviorExecuted = true;
                    //Debug.Log($"Enemy performing: {behavior.node.name} (Weight: {behavior.weight:F2}, Position: {behavior.node.position:F2}, Velocity: {behavior.velocity:F2})");
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

    // Add to enemyPSO.cs
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
}