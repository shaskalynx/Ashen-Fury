using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class playerMovement : MonoBehaviour
{
    public float speed = 0f;
    public float attackRange = 2f;
    public float attackDamage = 10f;
    public LayerMask enemyLayer;
    private Rigidbody rb;
    private Vector2 moveVector;
    private float moveX, moveY;
    public float health = 100f;
    private float currentHealth;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentHealth = health;
    }

    private void Update() {
        Vector3 movement = new Vector3(moveX, 0.0f, moveY); 
        rb.AddForce(movement * speed);

        // Left mouse button
        if(Input.GetMouseButtonDown(0))
        {
            Attack();
        }
    }

    // Update is called once per frame
    private void OnMove(InputValue movementValue)
    {
        moveVector = movementValue.Get<Vector2>();
        moveX = moveVector.x;
        moveY = moveVector.y;
    }

    private void Attack()
    {
        // Perform a sphere cast to detect enemies within range
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange, enemyLayer);

        foreach (var hitCollider in hitColliders)
        {
            enemyHealthSystem enemyScript = hitCollider.GetComponent<enemyHealthSystem>();
            if (enemyScript != null)
            {
                enemyScript.TakeDamage(attackDamage);
                Debug.Log($"Attacked enemy! Dealt {attackDamage} damage.");
            }
        }
    }

    // Visualize the attack range in the editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
