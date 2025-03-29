using UnityEngine;
using TMPro; // Required for TextMeshPro
using System.Collections;
using System.Collections.Generic;

public class ObjectiveSystem : MonoBehaviour
{
    private List<GameObject> enemies = new List<GameObject>();
    [SerializeField] private GameObject bossPrefab; // Reference to the boss prefab
    private GameObject activeBoss; // Reference to the active boss instance
    private bool bossDefeated = false;

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

        if (bossPrefab != null)
        {
            bossPrefab.SetActive(false); // Ensure boss is initially disabled
        }
    }

    void Update()
    {
        // Check if any enemies have been destroyed
        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            if (enemies[i] == null || // Check for destroyed enemies
                !enemies[i].activeInHierarchy || // Check for disabled GameObjects
                !enemies[i].GetComponent<enemy>().enabled) // Check for disabled enemy script
            {
                enemies.RemoveAt(i);
            }
        }

        // Check if all regular enemies are defeated and boss needs to be spawned
        if (enemies.Count == 0 && bossPrefab != null && activeBoss == null && !bossDefeated)
        {
            bossPrefab.SetActive(true);
            activeBoss = bossPrefab;
            objectiveText.text = "Objective: Defeat the Boss!";
            objectiveText.gameObject.SetActive(true);
        }

        // Check if boss is defeated
        if (activeBoss != null && (!activeBoss.activeInHierarchy || !activeBoss.GetComponent<enemy>().enabled))
        {
            bossDefeated = true;
            activeBoss = null;
        }

        // Check for victory condition (all enemies and boss defeated)
        if (enemies.Count == 0 && bossDefeated)
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