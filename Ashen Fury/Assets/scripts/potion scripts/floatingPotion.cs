using UnityEngine;

public class FloatingPotion : MonoBehaviour
{
    public float floatHeight = 0.05f;
    public float floatSpeed = 1.5f;  
    public float rotationSpeed = 50f; 
    
    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        // Floating motion using a sine wave
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }
}