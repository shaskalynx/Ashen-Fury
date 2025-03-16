using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class EUIController : MonoBehaviour
{
    [Header("UI Canvases")]
    [SerializeField] private GameObject victoryCanvas;    // Canvas containing victory UI
    [SerializeField] private GameObject defeatCanvas;     // Canvas containing defeat UI
    [SerializeField] private GameObject pauseCanvas;      // Canvas containing pause menu UI

    [Header("Stage Loading")]
    [SerializeField] private StageLoaderEasy stageLoaderE;

    [Header("Pickup Notifications")]
    [SerializeField] private TextMeshProUGUI pickupMessageText;    // Reference to notification text
    [SerializeField] private float messageDuration = 2f;           // How long the message stays visible

    private bool isPaused = false;
    private bool isGameOver = false; // New flag to track game over state

    void Update()
    {
        // Only allow pause menu if game is not over
        if (Input.GetKeyDown(KeyCode.Escape) && !isGameOver)
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f; // Freeze game time
        pauseCanvas.SetActive(true);
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; // Resume game time
        pauseCanvas.SetActive(false);
        Cursor.visible = false;
    }

    public void ShowCongratulatoryMessage()
    {
        if (victoryCanvas != null)
        {
            isGameOver = true; // Set game over state
            
            Cursor.visible = true;
            victoryCanvas.SetActive(true);
            
            // If pause menu is open, close it
            if (isPaused)
            {
                pauseCanvas.SetActive(false);
                isPaused = false;
            }
            
            // Get the text component from the victory canvas and fade it in
            TextMeshProUGUI victoryText = victoryCanvas.GetComponentInChildren<TextMeshProUGUI>();
            if (victoryText != null)
            {
                StartCoroutine(FadeInText(victoryText, 2f));
            }
        }
        else
        {
            Debug.LogWarning("Victory Canvas reference is missing!");
        }
    }

    public void ShowDefeatMessage()
    {
        if (defeatCanvas != null)
        {
            isGameOver = true; // Set game over state
            
            Cursor.visible = true;
            defeatCanvas.SetActive(true);
            
            // If pause menu is open, close it
            if (isPaused)
            {
                pauseCanvas.SetActive(false);
                isPaused = false;
            }
            
            // Get the text component from the defeat canvas and fade it in
            TextMeshProUGUI defeatText = defeatCanvas.GetComponentInChildren<TextMeshProUGUI>();
            if (defeatText != null)
            {
                StartCoroutine(FadeInText(defeatText, 2f));
            }
        }
        else
        {
            Debug.LogWarning("Defeat Canvas reference is missing!");
        }
    }

    private IEnumerator FadeInText(TextMeshProUGUI text, float duration)
    {
        float elapsedTime = 0;
        Color startColor = text.color;
        startColor.a = 0;
        text.color = startColor;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / duration);
            text.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }
    }

    public void ShowPickupMessage(string message)
    {
        if (pickupMessageText != null)
        {
            StopAllCoroutines(); // Stop any existing fade coroutines
            StartCoroutine(ShowTemporaryMessage(message));
        }
    }

    private IEnumerator ShowTemporaryMessage(string message)
    {
        pickupMessageText.text = message;
        pickupMessageText.gameObject.SetActive(true);
        
        // Fade in
        yield return StartCoroutine(FadeInText(pickupMessageText, 0.5f));
        
        // Wait
        yield return new WaitForSeconds(messageDuration);
        
        // Fade out
        yield return StartCoroutine(FadeOutText(pickupMessageText, 0.5f));
        
        pickupMessageText.gameObject.SetActive(false);
    }

    private IEnumerator FadeOutText(TextMeshProUGUI text, float duration)
    {
        float elapsedTime = 0;
        Color startColor = text.color;
        startColor.a = 1;
        text.color = startColor;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = 1 - Mathf.Clamp01(elapsedTime / duration);
            text.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }
    }

    public void OnExitButtonClicked()
    {
        Debug.Log("Exit button clicked");
        Time.timeScale = 1f; // Ensure game is unpaused before going to the main menu
        SceneManager.LoadScene("TheMainMenu");
    }

    public void OnRestartButtonClicked()
    {
        Debug.Log("Restart button clicked");
        Time.timeScale = 1f; // Ensure game is unpaused before reloading the scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnNextStageButtonClicked()
    {
        Debug.Log("Next Stage button clicked");
        Time.timeScale = 1f;
        stageLoaderE.LoadNextStage();
    }
}
