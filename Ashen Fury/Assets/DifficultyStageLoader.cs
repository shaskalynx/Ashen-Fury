using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class DifficultyStageLoader : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Dropdown difficultyDropdown; // Dropdown for difficulty selection
    [SerializeField] private Button[] stageButtons;           // Buttons for stage selection

    private string selectedDifficulty = "EASY"; // Default difficulty

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
            stageButtons[i].onClick.AddListener(() => LoadStage(selectedDifficulty, stageNumber));
        }
    }

    // Method to handle difficulty dropdown changes
    private void OnDifficultyChanged(int index)
    {
        // Update the selected difficulty based on the dropdown value
        switch (index)
        {
            case 0:
                selectedDifficulty = "EASY";
                break;
            case 1:
                selectedDifficulty = "MEDIUM";
                break;
            case 2:
                selectedDifficulty = "HARD";
                break;
            default:
                selectedDifficulty = "EASY"; // Default to Easy if something goes wrong
                break;
        }
    }

    // Method to load a stage based on the selected difficulty and stage number
    private void LoadStage(string difficulty, int stageNumber)
    {
        string stageName = $"{difficulty}_Stage{stageNumber}"; // Constructs the stage name (e.g., "EASY_Stage1")
        if (Application.CanStreamedLevelBeLoaded(stageName)) // Checks if the scene exists
        {
            SceneManager.LoadScene(stageName);
        }
        else
        {
            Debug.LogError($"Scene {stageName} does not exist or is not added to Build Settings.");
        }
    }
}