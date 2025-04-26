using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject startPanel;
    public GameObject pausePanel;
    public GameObject winPanel;
    public GameObject tutorialPanel; // <-- New for Tutorial

    [Header("Buttons")]
    public Button startButton;
    public Button pauseButton;
    public Button resumeButton;
    public Button restartButtonInPause;
    public Button exitButtonInPause;
    public Button restartButtonInWin;
    public Button exitButtonInWin;
    public Button infoButton; // <-- New Info Button
    public Button closeTutorialButton; // <-- Button to close tutorial

    [Header("Tutorial Text")]
    public TMP_Text tutorialText; // <-- Text field for animated tutorial

    [Header("Settings")]
    public float typingSpeed = 0.04f;

    private string tutorialMessage =
        "1. Move the tray by swiping left, right, up, or down.\n" +
        "2. Collide the tray with the wall of the same color to place it outside the grid.\n" +
        "3. Move all 4 trays out to WIN the game ! ";

    void Start()
    {
        // Assign button listeners
        startButton.onClick.AddListener(StartGame);
        pauseButton.onClick.AddListener(PauseGame);
        resumeButton.onClick.AddListener(ResumeGame);
        restartButtonInPause.onClick.AddListener(RestartGame);
        exitButtonInPause.onClick.AddListener(ExitGame);
        restartButtonInWin.onClick.AddListener(RestartGame);
        exitButtonInWin.onClick.AddListener(ExitGame);
        infoButton.onClick.AddListener(ShowTutorial);
        closeTutorialButton.onClick.AddListener(CloseTutorial);

        // Panels
        startPanel.SetActive(true);
        pausePanel.SetActive(false);
        winPanel.SetActive(false);
        tutorialPanel.SetActive(false);

        Time.timeScale = 0f; // Game paused at start

        // Check if it's first time user
        
    }

    void StartGame()
    {
        startPanel.SetActive(false);
        if (PlayerPrefs.GetInt("FirstPlay", 1) == 1)
        {
            ShowTutorial();
            PlayerPrefs.SetInt("FirstPlay", 0); // Next time don't show
        }
        Time.timeScale = 1f; // Start the game
    }

    void PauseGame()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    void ResumeGame()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void ExitGame()
    {
        Debug.Log("Exiting game...");
        Application.Quit();
    }

    public void ShowWinPanel()
    {
        Time.timeScale = 0f;
        winPanel.SetActive(true);
    }

    public void ShowTutorial()
    {
        tutorialPanel.SetActive(true);
        tutorialText.text = "";
        closeTutorialButton.gameObject.SetActive(false); // Hide Close Button initially
        StartCoroutine(TypeTutorialText());
    }

    public void CloseTutorial()
    {
        tutorialPanel.SetActive(false);
    }

    IEnumerator TypeTutorialText()
    {
        foreach (char letter in tutorialMessage.ToCharArray())
        {
            tutorialText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        // After typing finishes, show the Close Button
        closeTutorialButton.gameObject.SetActive(true);
    }

}
