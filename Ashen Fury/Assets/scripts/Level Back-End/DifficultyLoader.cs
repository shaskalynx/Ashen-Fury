using UnityEngine;
using UnityEngine.SceneManagement;

public class DifficultyLoader : MonoBehaviour
{
    private GameObject easyDifficulty; // GameObject containing Easy enemies
    private GameObject mediumDifficulty; // GameObject containing Medium enemies
    private GameObject hardDifficulty; // GameObject containing Hard enemies

    private static Difficulty selectedDifficulty; // No default value
    private static DifficultyLoader instance; // Singleton instance

    private void Awake()
    {
        // Check if another DifficultyLoader already exists
        if (instance != null && instance != this)
        {
            Debug.Log("Duplicate DifficultyLoader found. Destroying the new one.");
            Destroy(gameObject);
            return;
        }

        // Set this as the instance
        instance = this;

        // Ensure this GameObject persists across scene changes
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        // Subscribe to the sceneLoaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // Unsubscribe from the sceneLoaded event
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        // Ensure all difficulty GameObjects are disabled at the start
        DisableAllDifficulties();

        // Load the selected difficulty (if it has been set)
        if (selectedDifficulty != null)
        {
            LoadDifficulty(selectedDifficulty);
        }
        else
        {
            Debug.LogWarning("No difficulty selected. Please set a difficulty before loading a stage.");
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Skip loading difficulty if the current scene is the main menu
        if (IsMainMenuScene())
        {
            Debug.Log("Main menu scene detected. Skipping difficulty load.");
            return;
        }

        // Find the difficulty-specific GameObjects in the new scene
        easyDifficulty = GameObject.Find("EASY_Enemies");
        mediumDifficulty = GameObject.Find("MEDIUM_Enemies");
        hardDifficulty = GameObject.Find("HARD_Enemies");

        // Load the selected difficulty for the new scene
        if (selectedDifficulty != null)
        {
            LoadDifficulty(selectedDifficulty);
        }
        else
        {
            Debug.LogWarning("No difficulty selected. Please set a difficulty before loading a stage.");
        }
    }

    // Method to load the selected difficulty
    public void LoadDifficulty(Difficulty difficulty)
    {
        Debug.Log($"Loading difficulty: {difficulty}");
        selectedDifficulty = difficulty;

        // Disable all difficulty GameObjects first
        DisableAllDifficulties();

        // Enable the selected difficulty GameObject
        switch (difficulty)
        {
            case Difficulty.Easy:
                if (easyDifficulty != null)
                {
                    easyDifficulty.SetActive(true);
                    Debug.Log("Easy difficulty loaded.");
                }
                break;
            case Difficulty.Medium:
                if (mediumDifficulty != null)
                {
                    mediumDifficulty.SetActive(true);
                    Debug.Log("Medium difficulty loaded.");
                }
                break;
            case Difficulty.Hard:
                if (hardDifficulty != null)
                {
                    hardDifficulty.SetActive(true);
                    Debug.Log("Hard difficulty loaded.");
                }
                break;
            default:
                Debug.LogWarning("Invalid difficulty selected.");
                break;
        }
    }

    // Method to disable all difficulty GameObjects
    private void DisableAllDifficulties()
    {
        if (easyDifficulty != null) easyDifficulty.SetActive(false);
        if (mediumDifficulty != null) mediumDifficulty.SetActive(false);
        if (hardDifficulty != null) hardDifficulty.SetActive(false);
    }

    // Enum to represent difficulty levels
    public enum Difficulty
    {
        Easy,
        Medium,
        Hard
    }

    // Property to get the selected difficulty
    public static Difficulty SelectedDifficulty => selectedDifficulty;

    // Property to get the singleton instance
    public static DifficultyLoader Instance => instance;

    // Method to set the selected difficulty
    public static void SetDifficulty(Difficulty difficulty)
    {
        selectedDifficulty = difficulty;
        Debug.Log($"Difficulty set to: {difficulty}");
    }

    // Helper method to check if the current scene is the main menu
    private bool IsMainMenuScene()
    {
        return SceneManager.GetActiveScene().name == "TheMainMenu";
    }
}