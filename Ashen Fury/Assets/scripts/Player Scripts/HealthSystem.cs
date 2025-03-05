using UnityEngine;
 
public class HealthSystem : MonoBehaviour
{
    [SerializeField] public float health;
    [SerializeField] GameObject hitVFX;
    [SerializeField] GameObject ragdoll;
 
    Animator animator;
    void Start()
    {
        animator = GetComponent<Animator>();
    }
 
    public void TakeDamage(float damageAmount)
    {
        health -= damageAmount;
        animator.SetTrigger("damage");
        //CameraShake.Instance.ShakeCamera(2f, 0.2f);
 
        if (health <= 0)
        {
            Die();
        }
    }
 
    void Die()
    {
        animator.SetTrigger("die");
        gameObject.tag = "Untagged";
        // Disable the Character Controller component
        var characterController = GetComponent<CharacterController>();
        if (characterController != null)
        {
            characterController.enabled = false;
        }
        // Disable the Character script
        var character = GetComponent<Character>();
        if (character != null)
        {
            character.OnDeath();
        }
        //Instantiate(ragdoll, transform.position, transform.rotation);
        //Destroy(this.gameObject);
    }
    public void HitVFX(Vector3 hitPosition)
    {
        GameObject hit = Instantiate(hitVFX, hitPosition, Quaternion.identity);
        Destroy(hit, 3f);
 
    }
}