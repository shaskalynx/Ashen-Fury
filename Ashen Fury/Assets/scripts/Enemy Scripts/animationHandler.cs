using UnityEngine;
using UnityEngine.AI;

public class animationHandler : MonoBehaviour
{
    private Animator animator;
    private enemy enemystatus;
    private NavMeshAgent agent;

    private float atkCD = 3f;
    private float timePassed = 0f;
    private Vector3 currentPosition;

    void Start()
    {
        enemystatus = GetComponent<enemy>();
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (enemystatus.distanceToPlayer >= 1.3f)
        {
            currentPosition = transform.position;
        }

        switch (enemystatus.stateName)
        {
            case "Patrol":
                HandlePatrolState();
                break;

            case "ChasePlayer":
                HandleChaseState();
                break;

            case "AttackPlayer":
                HandleAttackState();
                break;
        }

        timePassed += Time.deltaTime;
    }

    private void HandlePatrolState()
    {
        agent.speed = 1.5f;
        animator.SetFloat("Speed", 0.5f, 0.1f, Time.deltaTime);
    }

    private void HandleChaseState()
    {
        agent.speed = 3.5f;
        animator.SetFloat("Speed", 1f, 0.1f, Time.deltaTime);
        //RotateTowardsPlayer();
    }

    private void HandleAttackState()
    {
        transform.position = currentPosition;
        if (timePassed >= atkCD)
        {
            attack();
            timePassed = 0;
            animator.SetFloat("Speed", 0f);
        }
        //RotateTowardsPlayer();
    }

    /*private void RotateTowardsPlayer()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
            directionToPlayer.y = 0;

            if (directionToPlayer != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
            }
        }
    }*/

    public void TakeDamage()
    {
        animator.SetTrigger("damage");
    }

    public void attack()
    {
        animator.SetTrigger("attackPlayer");
        Debug.Log("The animation should be playing");
    }
}