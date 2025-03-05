using UnityEngine;
using TMPro; // Required for TextMeshPro
using UnityEngine.UI; // Required for Button
using UnityEngine.SceneManagement; // Required for SceneManager
using System.Collections;

public class UIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI congratulatoryText; // Reference to the TextMeshPro UI element
    [SerializeField] private Button exitButton; // Reference to the Exit button
    [SerializeField] private Button restartButton; // Reference to the Restart button
    [SerializeField] private Button nextStageButton; // Reference to the Next Stage button
    [SerializeField] private StageLoader stageLoader; // Reference to the StageLoader script


    // Call this method to show the congratulatory message with a fade-in effect
    public void ShowCongratulatoryMessage()
    {
        if (congratulatoryText != null)
        {
            Cursor.visible = true; // Hide the cursor in gameplay
            congratulatoryText.gameObject.SetActive(true);
            StartCoroutine(FadeInText(congratulatoryText, 2f)); // Fade in over 2 seconds
            Debug.Log("Congratulations! All enemies have been defeated!");

            // Enable the buttons
            if (exitButton != null) exitButton.gameObject.SetActive(true);
            if (restartButton != null) restartButton.gameObject.SetActive(true);
            if (nextStageButton != null) nextStageButton.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Congratulatory TextMeshPro reference is missing!");
        }
    }

    // Coroutine to fade in the text
    private IEnumerator FadeInText(TextMeshProUGUI text, float duration)
    {
        float elapsedTime = 0;
        Color startColor = text.color;
        startColor.a = 0; // Start fully transparent
        text.color = startColor;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / duration);
            text.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null; // Wait for the next frame
        }
    }

    // Button click handlers
    public void OnExitButtonClicked()
    {
        Debug.Log("Exit button clicked");
        SceneManager.LoadScene("Menu"); // Replace "Menu" with the name of your menu scene
    }

    public void OnRestartButtonClicked()
    {
        Debug.Log("Restart button clicked");
        // Reload the current scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void OnNextStageButtonClicked()
    {
        Debug.Log("Next Stage button clicked");
        // Load the next scene (replace "NextStageSceneName" with the actual scene name)
        stageLoader.LoadNextStage(); // Call the StageLoader to load the next stage
    }
}