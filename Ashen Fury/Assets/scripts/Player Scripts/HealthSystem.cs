using UnityEngine;
using UnityEngine.UI; // Add this for UI elements

public class HealthSystem : MonoBehaviour
{
    [SerializeField] public float health;
    [SerializeField] public float maxHealth = 100f; // Add max health
    [SerializeField] GameObject hitVFX;
    [SerializeField] GameObject ragdoll;
    [SerializeField] private Slider healthBar; // Reference to UI slider
    [SerializeField] private UIController uiController; // Reference to UI controller
 
    Animator animator;
    void Start()
    {
        animator = GetComponent<Animator>();
        health = maxHealth; // Initialize health to max
        UpdateHealthBar(); // Initialize health bar
        
        // Find UI controller if not assigned
        if (uiController == null)
        {
            uiController = FindObjectOfType<UIController>();
        }
    }
 
    // Update health bar UI
    private void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.value = health / maxHealth;
        }
    }
 
    public void TakeDamage(float damageAmount)
    {
        health -= damageAmount;
        animator.SetTrigger("damage");
        //CameraShake.Instance.ShakeCamera(2f, 0.2f);
 
        UpdateHealthBar(); // Update health bar after taking damage
 
        if (health <= 0)
        {
            health = 0; // Prevent negative health
            Die();
        }
    }
 
    // Add method to heal player
    public void Heal(float healAmount)
    {
        health = Mathf.Min(health + healAmount, maxHealth); // Don't exceed max health
        UpdateHealthBar(); // Update health bar after healing
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
        
        // Show defeat message
        if (uiController != null)
        {
            uiController.ShowDefeatMessage();
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