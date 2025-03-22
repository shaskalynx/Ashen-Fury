using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class DifficultyStageLoader : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Dropdown difficultyDropdown; // Dropdown for difficulty selection
    [SerializeField] private Button[] stageButtons;           // Buttons for stage selection

    private void Start()
    {
        // Initialize difficulty dropdown listener
        if (difficultyDropdown != null)
        {
            difficultyDropdown.onValueChanged.AddListener(OnDifficultyChanged);
        }

        // Initialize stage button listeners
        for (int i = 0; i < stageButtons.Length; i++)
        {
            int stageNumber = i + 1; // Stage numbers start from 1
            stageButtons[i].onClick.AddListener(() => LoadStage(stageNumber));
        }
    }

    // Method to handle difficulty dropdown changes
    private void OnDifficultyChanged(int index)
    {
        // Update the selected difficulty based on the dropdown value
        DifficultyLoader.Difficulty selectedDifficulty;
        switch (index)
        {
            case 0:
                selectedDifficulty = DifficultyLoader.Difficulty.Easy;
                break;
            case 1:
                selectedDifficulty = DifficultyLoader.Difficulty.Medium;
                break;
            case 2:
                selectedDifficulty = DifficultyLoader.Difficulty.Hard;
                break;
            default:
                selectedDifficulty = DifficultyLoader.Difficulty.Easy; // Default to Easy if something goes wrong
                break;
        }

        // Set the selected difficulty using the DifficultyLoader
        DifficultyLoader.SetDifficulty(selectedDifficulty);
    }

    // Method to load a stage based on the selected stage number
    private void LoadStage(int stageNumber)
    {
        string stageName = $"Stage{stageNumber}"; // Constructs the stage name (e.g., "Stage1")
        if (Application.CanStreamedLevelBeLoaded(stageName)) // Checks if the scene exists
        {
            // Load the stage
            SceneManager.LoadScene(stageName);
        }
        else
        {
            Debug.LogError($"Scene {stageName} does not exist or is not added to Build Settings.");
        }
    }
}