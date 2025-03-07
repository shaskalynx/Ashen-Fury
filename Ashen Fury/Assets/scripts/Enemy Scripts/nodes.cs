using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Linq;

public abstract class Node
{
    public enum NodeState {success, fail, running}
    protected NodeState state;
    public float fitness;
    public float position; // Single value representing the position of the node in the tree
    public string name;


    public virtual void CalculateFitness(Vector3 currentState)
    {
        // Base fitness calculation
        float baseFitness = CalculateBaseFitness(currentState);
        
        // Combine with position influence
        fitness = baseFitness * (0.5f + position * 0.5f);
    }

    public abstract NodeState Evaluate(Vector3 currentState);
    protected abstract float CalculateBaseFitness(Vector3 currentState);

    protected void RotateTowards(Transform enemyTransform, Vector3 targetPosition)
    {
        Vector3 directionToTarget = (targetPosition - enemyTransform.position).normalized;
        directionToTarget.y = 0; // Keep the rotation only on the horizontal plane

        if (directionToTarget != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(directionToTarget);
            enemyTransform.rotation = Quaternion.Slerp(enemyTransform.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }
}

public class AttackPlayer : Node
{
    private NavMeshAgent agent;
    private float atkRange;
    private GameObject player;
    
    public AttackPlayer(NavMeshAgent agent, float atkRange)
    {
        this.agent = agent;
        this.atkRange = atkRange;
        this.name = "AttackPlayer";
    }

    public override NodeState Evaluate(Vector3 currentState)
    {
        player = GameObject.FindWithTag("Player");
        if(player == null) return NodeState.fail;
        
        float distanceToPlayer = Vector3.Distance(agent.transform.position, player.transform.position);

        if(distanceToPlayer <= atkRange)
        {
            RotateTowards(agent.transform, player.transform.position);
            return NodeState.success;
        }
        
        return NodeState.fail;
    }

    protected override float CalculateBaseFitness(Vector3 currentState)
    {
        float distanceToPlayer = currentState.x;
        float healthPercentage = currentState.y;
        
        float baseFitness = 0f;
        
        // High fitness when close and healthy
        if(distanceToPlayer <= atkRange)
        {
            baseFitness = 1.0f;
        }
        else
        {
            baseFitness = 1.0f / (distanceToPlayer + 1);
        }
        
        return baseFitness * healthPercentage;
    }
}

public class ChasePlayer : Node
{
    private NavMeshAgent agent;
    private float detectionRadius;
    
    public ChasePlayer(NavMeshAgent agent, float detectionRadius)
    {
        this.agent = agent;
        this.detectionRadius = detectionRadius;
        this.name = "ChasePlayer";
    }

    public override NodeState Evaluate(Vector3 currentState)
    {
        GameObject player = GameObject.FindWithTag("Player");
        if(player == null) return NodeState.fail;

        float distanceToPlayer = Vector3.Distance(agent.transform.position, player.transform.position);

        if(distanceToPlayer <= detectionRadius)
        {
            agent.SetDestination(player.transform.position);
            RotateTowards(agent.transform, player.transform.position);
            return agent.remainingDistance > agent.stoppingDistance ? 
                   NodeState.running : NodeState.success;
        }
        return NodeState.fail;
    }

    protected override float CalculateBaseFitness(Vector3 currentState)
    {
        float distanceToPlayer = currentState.x;
        float healthPercentage = currentState.y;
        
        // Optimal chase distance calculation
        float optimalDistance = detectionRadius * 0.5f;
        float distanceFactor = 1.0f - Mathf.Abs(distanceToPlayer - optimalDistance) / detectionRadius;
        
        return distanceFactor * healthPercentage;
    }
}

public class Patrol : Node
{
    private NavMeshAgent agent;
    private float wanderRadius = 10f;
    private float minWanderDistance = 5f;
    
    // Variables for stuck detection
    private float stuckCheckInterval = 1f;  // Check every second
    private float stuckThreshold = 0.1f;    // Distance to consider as stuck
    private float stuckTimer = 0f;
    private Vector3 lastPosition;
    private int maxPathRetries = 5;         // Maximum attempts to find new path
    private int pathRetryCount = 0;

    public Patrol(NavMeshAgent agent, Transform target)
    {
        this.agent = agent;
        this.name = "Patrol";
        lastPosition = agent.transform.position;
    }

    public override NodeState Evaluate(Vector3 currentState)
    {
        // Check if stuck
        stuckTimer += Time.deltaTime;
        if (stuckTimer >= stuckCheckInterval)
        {
            float distanceMoved = Vector3.Distance(agent.transform.position, lastPosition);
            
            // If barely moved and has a path, might be stuck
            if (distanceMoved < stuckThreshold && agent.hasPath)
            {
                HandleStuckState();
            }

            lastPosition = agent.transform.position;
            stuckTimer = 0f;
        }

        // Normal patrol behavior
        if (!agent.hasPath || agent.remainingDistance <= agent.stoppingDistance)
        {
            Vector3 newPoint = GetRandomPoint();
            if (newPoint != agent.transform.position)
            {
                agent.SetDestination(newPoint);
                pathRetryCount = 0; // Reset retry count on successful new path
            }
        }

        if(agent.pathPending)
            return NodeState.running;

        if(agent.remainingDistance <= agent.stoppingDistance)
            return !agent.hasPath || agent.velocity.sqrMagnitude == 0f ? 
                   NodeState.success : NodeState.running;
        
        return NodeState.running;
    }

    private void HandleStuckState()
    {
        if (pathRetryCount < maxPathRetries)
        {
            // Try to find a path in a different direction
            Vector3 newPoint = GetRandomPoint();
            if (newPoint != agent.transform.position)
            {
                agent.ResetPath();
                agent.SetDestination(newPoint);
                pathRetryCount++;
            }
        }
        else
        {
            // If too many retries, force a reset
            agent.ResetPath();
            pathRetryCount = 0;
        }
    }

    private Vector3 GetRandomPoint()
    {
        for (int i = 0; i < 30; i++) // Try 30 times to find a valid point
        {
            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
            randomDirection += agent.transform.position;
            NavMeshHit hit;

            // Try to find a valid point on the NavMesh
            if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
            {
                // Check if we can create a path to this point
                NavMeshPath path = new NavMeshPath();
                if (NavMesh.CalculatePath(agent.transform.position, hit.position, NavMesh.AllAreas, path))
                {
                    if (path.status == NavMeshPathStatus.PathComplete &&
                        Vector3.Distance(hit.position, agent.transform.position) >= minWanderDistance)
                    {
                        return hit.position;
                    }
                }
            }
        }
        
        // If no valid point found, return current position
        return agent.transform.position;
    }

    protected override float CalculateBaseFitness(Vector3 currentState)
    {
        float distanceToPlayer = currentState.x;
        float healthPercentage = currentState.y;
        float distanceToPatrolPoint = currentState.z;
        
        // Higher fitness when:
        // - Far from player
        // - Low health
        // - Close to patrol point
        float dangerFactor = distanceToPlayer / 20f;
        float healthFactor = 1.0f - healthPercentage;
        float patrolFactor = 1.0f / (distanceToPatrolPoint + 1);
        
        return (dangerFactor + healthFactor) * patrolFactor;
    }
}