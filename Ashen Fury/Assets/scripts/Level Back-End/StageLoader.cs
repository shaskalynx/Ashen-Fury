using UnityEngine;
using UnityEngine.SceneManagement;

public class StageLoader : MonoBehaviour
{
    // Method to load the next stage
    public void LoadNextStage()
    {
        // Get the current scene name
        string currentSceneName = SceneManager.GetActiveScene().name;

        // Check if the scene name follows any of the supported formats
        if (currentSceneName.StartsWith("EASY_Stage") || 
            currentSceneName.StartsWith("MEDIUM_Stage") || 
            currentSceneName.StartsWith("HARD_Stage"))
        {
            // Extract the stage number from the scene name
            string stageNumberString = currentSceneName.Substring(currentSceneName.LastIndexOf('e') + 1); // Get the number after the last 'e'
            if (int.TryParse(stageNumberString, out int currentStage))
            {
                // Increment the stage number
                int nextStage = currentStage + 1;

                // Determine the maximum number of stages based on the difficulty
                int maxStages = 4; // Default to 4 stages

                // Check if the next stage exceeds the maximum number of stages
                if (nextStage > maxStages)
                {
                    Debug.Log("All stages completed! Returning to the first stage.");
                    nextStage = 1; // Reset to the first stage
                }

                // Load the next stage scene
                string nextSceneName = currentSceneName.Substring(0, currentSceneName.LastIndexOf('e') + 1) + nextStage;
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
            Debug.LogError("Current scene name does not follow the supported formats: " + currentSceneName);
        }
    }
}