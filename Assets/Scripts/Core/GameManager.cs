using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance => instance;

    [Header("Game Settings")]
    [SerializeField] private int currentLevel = 0;
    [SerializeField] private int totalLevels = 3;
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private GameObject victoryUI;

    private Vector3 lastCheckpoint;
    private bool isGameOver = false;
    private bool isPaused = false;

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
        }

        // مخفی کردن UIهای اولیه
        if (gameOverUI != null) gameOverUI.SetActive(false);
        if (victoryUI != null) victoryUI.SetActive(false);
    }

    private void Start()
    {
        // شروع از چک‌پوینت اولیه (ابتدای مرحله)
        lastCheckpoint = Vector3.zero;
    }

    public void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;
        Time.timeScale = 0f;

        if (gameOverUI != null)
            gameOverUI.SetActive(true);

        AudioManager.Instance?.PlaySFX("GameOver");
        Debug.Log("Game Over!");
    }

    public void RestartFromCheckpoint()
    {
        isGameOver = false;
        Time.timeScale = 1f;

        if (gameOverUI != null)
            gameOverUI.SetActive(false);

        // پیدا کردن بازیکن و انتقال به چک‌پوینت
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = lastCheckpoint;
            // تنظیم مجدد وضعیت
            player.GetComponent<PlayerState>()?.gameObject.SetActive(true);
        }

        Debug.Log($"Restarted from checkpoint: {lastCheckpoint}");
    }

    public void RestartLevel()
    {
        isGameOver = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadLevel(int levelIndex)
    {
        currentLevel = levelIndex;
        SceneManager.LoadScene($"Level{levelIndex + 1}");
    }

    public void NextLevel()
    {
        currentLevel++;
        if (currentLevel >= totalLevels)
        {
            Victory();
        }
        else
        {
            LoadLevel(currentLevel);
        }
    }

    public void Victory()
    {
        Time.timeScale = 0f;
        if (victoryUI != null)
            victoryUI.SetActive(true);

        AudioManager.Instance?.PlaySFX("Victory");
        Debug.Log("Congratulations! You completed the game!");
    }

    public void SetCheckpoint(Vector2 position)
    {
        lastCheckpoint = position;
        Debug.Log($"Checkpoint set at: {position}");
    }

    public Vector3 GetLastCheckpoint() => lastCheckpoint;
    public bool IsGameOver() => isGameOver;
    public bool IsPaused() => isPaused;

    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
    }
}