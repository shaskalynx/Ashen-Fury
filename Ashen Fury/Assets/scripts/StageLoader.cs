using UnityEngine;
using UnityEngine.SceneManagement;

public class StageLoader : MonoBehaviour
{
    // Method to load the next stage
    public void LoadNextStage()
    {
        // Get the current scene name
        string currentSceneName = SceneManager.GetActiveScene().name;

        // Extract the stage number from the scene name
        if (currentSceneName.StartsWith("Stage"))
        {
            // Get the stage number (e.g., "Stage1" -> 1)
            string stageNumberString = currentSceneName.Substring(5); // Remove "Stage" from the name
            if (int.TryParse(stageNumberString, out int currentStage))
            {
                // Increment the stage number
                int nextStage = currentStage + 1;

                // Check if the next stage exceeds the maximum number of stages (e.g., 4)
                if (nextStage > 4)
                {
                    Debug.Log("All stages completed! Returning to Stage 1.");
                    nextStage = 1; // Reset to Stage 1
                }

                // Load the next stage scene
                string nextSceneName = "Stage" + nextStage;
                Debug.Log("Loading next stage: " + nextSceneName);
                SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                Debug.LogError("Failed to parse stage number from scene name: " + currentSceneName);
            }
        }
        else
        {
            Debug.LogError("Current scene name does not follow the 'StageX' format: " + currentSceneName);
        }
    }
}