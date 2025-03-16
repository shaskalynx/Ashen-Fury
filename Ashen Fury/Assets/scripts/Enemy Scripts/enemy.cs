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
    
    [Header("Detection & Combat")]
    [SerializeField] public float aggroRange = 10f;
    [SerializeField] public float attackRange = 2f;
    [SerializeField] public float attackCD = 3f;
    [SerializeField] public float newDestinationCD = 0.5f;
    private float currentDestinationCD;

    [Header("Status")]
    [SerializeField] public string stateName;
    [SerializeField] public float distanceToPlayer;
    [SerializeField] public float distanceToPatrolPoint;

    [Header("Sound System")]
    [SerializeField] private EnemySoundManager soundManager;

    enemyHealthSystem enemyHealth;
    GameObject player;

    // Declare currentState and result as class-level variables
    private Vector3 currentState;
    private NodeState result;

    void Start()
    {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        enemyHealth = GetComponent<enemyHealthSystem>();
        player = GameObject.FindWithTag("Player");
        currentDestinationCD = 0;
        
        // Get sound manager if not assigned
        if (soundManager == null)
        {
            soundManager = GetComponent<EnemySoundManager>();
            if (soundManager == null)
            {
                soundManager = gameObject.AddComponent<EnemySoundManager>();
            }
        }

        wolves = new List<Node>
        {
            new AttackPlayer(agent, attackRange, attackCD, soundManager),
            new ChasePlayer(agent, aggroRange, newDestinationCD, soundManager),
            new Patrol(agent, target, soundManager),
        };

        gwo = new GreyWolfOptimizer(wolves);
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
                             $"Health: {enemyHealth.health:F2}, " +
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
        player = GameObject.FindWithTag("Player");
        distanceToPlayer = player ? Vector3.Distance(transform.position, player.transform.position) : float.MaxValue;
        float healthPercentage = enemyHealth.health / 100f; // Assuming max health is 100
        distanceToPatrolPoint = Vector3.Distance(transform.position, target.position);

        return new Vector3(distanceToPlayer, healthPercentage, distanceToPatrolPoint);
    }

    // Method to update node parameters if they change in the inspector
    public void UpdateNodeParameters()
    {
        if (wolves != null && wolves.Count >= 3)
        {
            // Update AttackPlayer node
            if (wolves[0] is AttackPlayer attackNode)
            {
                attackNode.UpdateParameters(attackRange, attackCD);
            }
            
            // Update ChasePlayer node
            if (wolves[1] is ChasePlayer chaseNode)
            {
                chaseNode.UpdateParameters(aggroRange, newDestinationCD);
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
    
    private void OnDrawGizmos()
    {
        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Draw aggro/detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroRange);
    }
}