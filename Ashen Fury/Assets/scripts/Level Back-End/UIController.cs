using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class UIController : MonoBehaviour
{
    [Header("UI Canvases")]
    [SerializeField] private GameObject victoryCanvas;
    [SerializeField] private GameObject defeatCanvas;
    [SerializeField] private GameObject pauseCanvas;

    [Header("Stage Loading")]
    [SerializeField] private StageLoader stageLoader;

    [Header("Pickup Notifications")]
    [SerializeField] private TextMeshProUGUI pickupMessageText;
    [SerializeField] private float messageDuration = 2f;

    private PlayerInput playerInput;
    private CinemachineBrain cinemachineBrain;
    private bool isPaused = false;
    private bool isGameOver = false;

    void Awake()
    {
        // Find required components automatically
        playerInput = FindObjectOfType<PlayerInput>();
        cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();

        if (playerInput == null) Debug.LogWarning("PlayerInput component not found!");
        if (cinemachineBrain == null) Debug.LogWarning("CinemachineBrain component not found!");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !isGameOver)
        {
            TogglePause();
        }
    }

    #region Game State Control
    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        pauseCanvas.SetActive(true);
        SetCursorState(true);
        DisableAllControls();
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        pauseCanvas.SetActive(false);
        SetCursorState(false);
        EnableAllControls();
    }

    public void ShowCongratulatoryMessage()
    {
        if (victoryCanvas == null) return;
        
        isGameOver = true;
        victoryCanvas.SetActive(true);
        SetCursorState(true);
        DisableAllControls();

        if (isPaused) ResumeGame(); // Auto-exit pause if game over
        
        TextMeshProUGUI victoryText = victoryCanvas.GetComponentInChildren<TextMeshProUGUI>();
        if (victoryText != null) StartCoroutine(FadeInText(victoryText, 2f));
    }

    public void ShowDefeatMessage()
    {
        if (defeatCanvas == null) return;
        
        isGameOver = true;
        defeatCanvas.SetActive(true);
        SetCursorState(true);
        DisableAllControls();

        if (isPaused) ResumeGame(); // Auto-exit pause if game over
        
        TextMeshProUGUI defeatText = defeatCanvas.GetComponentInChildren<TextMeshProUGUI>();
        if (defeatText != null) StartCoroutine(FadeInText(defeatText, 2f));
    }
    #endregion

    #region Control Management
    private void DisableAllControls()
    {
        // Player input
        if (playerInput != null) playerInput.enabled = false;
        
        // Camera control
        if (cinemachineBrain != null) cinemachineBrain.enabled = false;
    }

    private void EnableAllControls()
    {
        // Player input
        if (playerInput != null) playerInput.enabled = true;
        
        // Camera control
        if (cinemachineBrain != null) cinemachineBrain.enabled = true;
    }

    private void SetCursorState(bool visible)
    {
        Cursor.visible = visible;
        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
    }
    #endregion

    #region UI Effects
    private IEnumerator FadeInText(TextMeshProUGUI text, float duration)
    {
        float elapsedTime = 0;
        Color startColor = text.color;
        startColor.a = 0;
        text.color = startColor;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            text.color = new Color(startColor.r, startColor.g, startColor.b, Mathf.Clamp01(elapsedTime / duration));
            yield return null;
        }
    }

    public void ShowPickupMessage(string message)
    {
        if (pickupMessageText == null) return;
        
        StopAllCoroutines();
        StartCoroutine(ShowTemporaryMessage(message));
    }

    private IEnumerator ShowTemporaryMessage(string message)
    {
        pickupMessageText.text = message;
        pickupMessageText.gameObject.SetActive(true);
        
        yield return StartCoroutine(FadeInText(pickupMessageText, 0.5f));
        yield return new WaitForSeconds(messageDuration);
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
            text.color = new Color(startColor.r, startColor.g, startColor.b, 1 - Mathf.Clamp01(elapsedTime / duration));
            yield return null;
        }
    }
    #endregion

    #region Button Handlers
    public void OnExitButtonClicked()
    {
        Time.timeScale = 1f;
        EnableAllControls();
        SceneManager.LoadScene("TheMainMenu");
    }

    public void OnRestartButtonClicked()
    {
        Time.timeScale = 1f;
        EnableAllControls();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnNextStageButtonClicked()
    {
        Time.timeScale = 1f;
        EnableAllControls();
        stageLoader.LoadNextStage();
    }

    public void TogglePause()
    {
        if (isPaused) ResumeGame();
        else PauseGame();
    }
    #endregion
}