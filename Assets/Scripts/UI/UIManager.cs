using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    private static UIManager instance;
    public static UIManager Instance => instance;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI tutorialText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject victoryPanel;

    [Header("Welcome Message")]
    [SerializeField] private string welcomeMessage = "Welcome to Shadow Weaver!";
    [SerializeField] private float welcomeDuration = 3f;
    [SerializeField] private bool showWelcomeOnStart = true;

    private Coroutine messageCoroutine;
    private bool isInitialized = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeUI();
    }

    private void InitializeUI()
    {
        // پیدا کردن TutorialText در صحنه
        if (tutorialText == null)
        {
            // روش 1: پیدا کردن با نام
            GameObject textObj = GameObject.Find("TutorialText");
            if (textObj != null)
            {
                tutorialText = textObj.GetComponent<TextMeshProUGUI>();
                if (tutorialText != null)
                {
                    Debug.Log("TutorialText found by name!");
                }
            }

            // روش 2: پیدا کردن با Tag (اگه Tag داشته باشه)
            if (tutorialText == null)
            {
                TextMeshProUGUI[] allTexts = FindObjectsOfType<TextMeshProUGUI>();
                foreach (var text in allTexts)
                {
                    if (text.gameObject.name == "TutorialText")
                    {
                        tutorialText = text;
                        Debug.Log("TutorialText found by FindObjects!");
                        break;
                    }
                }
            }
        }

        if (tutorialText != null)
        {
            tutorialText.gameObject.SetActive(false);
            isInitialized = true;
            Debug.Log("UIManager initialized successfully!");
        }
        else
        {
            Debug.LogWarning("TutorialText not found! UI messages will not work.");
        }
    }

    private void Start()
    {
        if (showWelcomeOnStart && isInitialized)
        {
            ShowMessage(welcomeMessage, welcomeDuration);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // بعد از بارگذاری صحنه، دوباره TutorialText رو پیدا کن
        isInitialized = false;
        InitializeUI();

        if (showWelcomeOnStart && scene.name == "Level1")
        {
            ShowMessage(welcomeMessage, welcomeDuration);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void ShowMessage(string message, float duration = 3f)
    {
        if (!isInitialized || tutorialText == null)
        {
            Debug.LogWarning($"Cannot show message '{message}': UIManager not initialized!");
            return;
        }

        if (messageCoroutine != null)
        {
            StopCoroutine(messageCoroutine);
            messageCoroutine = null;
        }

        messageCoroutine = StartCoroutine(DisplayMessageCoroutine(message, duration));
    }

    private System.Collections.IEnumerator DisplayMessageCoroutine(string message, float duration)
    {
        if (tutorialText != null)
        {
            tutorialText.text = message;
            tutorialText.gameObject.SetActive(true);
            Debug.Log($"Showing: {message}");

            yield return new WaitForSeconds(duration);

            tutorialText.gameObject.SetActive(false);
            messageCoroutine = null;
        }
    }

    public void UpdateTimer(float timeLeft)
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timeLeft / 60);
            int seconds = Mathf.FloorToInt(timeLeft % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    public void TogglePause()
    {
        if (pausePanel == null) return;
        bool isPaused = pausePanel.activeSelf;
        pausePanel.SetActive(!isPaused);
        Time.timeScale = isPaused ? 1f : 0f;
    }

    public void ShowGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    public void ShowVictory()
    {
        if (victoryPanel != null)
            victoryPanel.SetActive(true);
    }

    public void OnRestartButtonClicked()
    {
        Time.timeScale = 1f;
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        GameManager.Instance?.RestartLevel();
    }

    public void OnResumeButtonClicked()
    {
        TogglePause();
    }

    public void OnMainMenuButtonClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}

