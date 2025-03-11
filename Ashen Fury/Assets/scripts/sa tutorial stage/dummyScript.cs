using UnityEngine;

public class dummyScript : MonoBehaviour
{
    Animator animator;
    GameObject player;

 
    private void Start()
    {
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player");
    }

    private void Update()
    {
        
    }

    /* private void TakeDamage()
    {
        Debug.Log("Dummy hit!");
        if(animator != null){
            animator.SetTrigger("damage");
        }else{
            Debug.LogError("Animator is null, cannot set trigger 'hit'");
        }
    } */    
}