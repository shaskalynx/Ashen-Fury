using UnityEngine;
using UnityEngine.SceneManagement;

public class StageLoader : MonoBehaviour
{
    // Method to load the next stage
    public void LoadNextStage()
    {
        // Get the current scene name
        string currentSceneName = SceneManager.GetActiveScene().name;

        // Check if the scene name follows the "StageX" format
        if (currentSceneName.StartsWith("Stage"))
        {
            // Extract the stage number from the scene name
            string stageNumberString = currentSceneName.Substring(5); // Remove "Stage" from the name
            if (int.TryParse(stageNumberString, out int currentStage))
            {
                // Increment the stage number
                int nextStage = currentStage + 1;

                // Determine the maximum number of stages
                int maxStages = 4; // Default to 4 stages

                // Check if the next stage exceeds the maximum number of stages
                if (nextStage > maxStages)
                {
                    Debug.Log("All stages completed! Returning to Stage 1.");
                    nextStage = 1; // Reset to Stage 1
                }

                // Load the next stage scene
                string nextSceneName = "Stage" + nextStage;
                Debug.Log("Loading next stage: " + nextSceneName);
                SceneManager.LoadScene(nextSceneName);

                // Load the selected difficulty enemies in the new scene
                LoadDifficultyEnemies();
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

    // Method to load the selected difficulty enemies
    private void LoadDifficultyEnemies()
    {
        DifficultyLoader difficultyLoader = DifficultyLoader.Instance;
        if (difficultyLoader != null)
        {
            difficultyLoader.LoadDifficulty(DifficultyLoader.SelectedDifficulty);
        }
        else
        {
            Debug.LogWarning("DifficultyLoader not found in the scene!");
        }
    }
}