using UnityEngine;
using TMPro; // Required for TextMeshPro
using System.Collections;
using System.Collections.Generic;

public class ObjectiveSystem : MonoBehaviour
{
    private List<GameObject> enemies = new List<GameObject>();

    [SerializeField] private UIController uiController;

    [SerializeField] private TextMeshProUGUI objectiveText; // Reference to the TextMeshPro UI element for the objective message
    [SerializeField] private float objectiveDisplayTime = 3f; // How long the objective text is displayed

    void Start()
    {
        // Find all objects with the "Enemy" tag and add them to the list
        GameObject[] enemyObjects = GameObject.FindGameObjectsWithTag("Enemy");
        enemies.AddRange(enemyObjects);

        Debug.Log($"Total enemies in scene: {enemies.Count}");

        // Display the objective text when the stage starts
        if (objectiveText != null)
        {
            objectiveText.text = "Objective: Defeat all enemies in the area.";
            objectiveText.gameObject.SetActive(true); // Show the objective text
            StartCoroutine(HideObjectiveTextAfterDelay(objectiveDisplayTime)); // Hide the text after a delay
        }
        else
        {
            Debug.LogWarning("Objective Text reference is missing!");
        }
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
                uiController.ShowCongratulatoryMessage(); // Call the UI controller to show the congratulatory message
            }
            else
            {
                Debug.LogWarning("UIController reference is missing!");
            }

            // Hide the objective text when all enemies are defeated
            if (objectiveText != null)
            {
                objectiveText.gameObject.SetActive(false);
            }

            enabled = false; // Disable the script to stop checking
        }
    }

    // Coroutine to hide the objective text after a delay
    private IEnumerator HideObjectiveTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (objectiveText != null)
        {
            objectiveText.gameObject.SetActive(false); // Hide the objective text
        }
    }
}