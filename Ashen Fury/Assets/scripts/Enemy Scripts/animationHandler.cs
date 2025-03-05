using UnityEngine;
using UnityEngine.AI;

public class animationHandler : MonoBehaviour
{
    Animator animator;
    enemy enemystatus;
    NavMeshAgent agent;

    float atkCD = 3f;
    float timePassed = 5f;
    Vector3 currentPosition;

    void Start()
    {
        enemystatus = GetComponent<enemy>();
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (enemystatus.distanceToPlayer >= 1.3f){
            currentPosition = transform.position;
        }

        if (enemystatus.stateName != "AttackPlayer"){
            if (enemystatus.stateName == "Patrol"){
                agent.speed = 1.5f;
                animator.SetFloat("Turn", 0.5f, 0.1f, Time.deltaTime);
            }
            else if (enemystatus.stateName == "ChasePlayer"){
                agent.speed = 3.5f;
                animator.SetFloat("Turn", 1f, 0.1f, Time.deltaTime);
            }   
        } 
        else if (enemystatus.stateName == "AttackPlayer"){
            transform.position = currentPosition;
            if (timePassed >= atkCD){
                attack();
                animator.SetFloat("Turn", 0f);
            }
        } 
        timePassed += Time.deltaTime;
    }

    public void TakeDamage(){
        animator.SetTrigger("damage");
    }

    public void attack(){
        animator.SetTrigger("attackPlayer");
        Debug.Log("the animation should be playing");
        timePassed = 0;
    }
}