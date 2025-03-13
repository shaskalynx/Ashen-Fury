using UnityEngine;
using TMPro; // Required for TextMeshPro
using System.Collections;
using System.Collections.Generic;

public class ObjectiveSystem : MonoBehaviour
{
    private List<GameObject> enemies = new List<GameObject>();

    [SerializeField] private UIController uiController;

    [SerializeField] private TextMeshProUGUI congratulatoryText; // Reference to the TextMeshPro UI element

    void Start()
    {
        // Find all objects with the "Enemy" tag and add them to the list
        GameObject[] enemyObjects = GameObject.FindGameObjectsWithTag("Enemy");
        enemies.AddRange(enemyObjects);

        Debug.Log($"Total enemies in scene: {enemies.Count}");
    }

    void Update()
    {
        // Check if any enemies have been destroyed
        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            if (enemies[i] == null) // If the enemy has been destroyed
            {
                enemies.RemoveAt(i); // Remove it from the list
            }
        }

        // Check if all enemies are defeated
        if (enemies.Count == 0)
        {
            if (uiController != null)
            {
                uiController.ShowCongratulatoryMessage(); // Call the UI controller to show the message
            }
            else
            {
                Debug.LogWarning("UIController reference is missing!");
            }

            enabled = false; // Disable the script to stop checking
        }
    }
}