using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance => instance;

    [Header("Game Settings")]
    [SerializeField] private int totalLevels = 3;

    private Vector3 lastCheckpoint;
    private bool isGameOver = false;
    private TextMeshProUGUI gameOverText;

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
    }

    private void Start()
    {
        // تنظیم چک‌پوینت اولیه بر اساس صحنه
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName == "Level1")
            lastCheckpoint = new Vector3(-7f, -2.5f, 0f);
        else if (sceneName == "Level2")
            lastCheckpoint = new Vector3(-12f, -2.5f, 0f);
        else if (sceneName == "Level3")
            lastCheckpoint = new Vector3(-10f, -2.5f, 0f);
        else
            lastCheckpoint = new Vector3(-7f, -2.5f, 0f);

        gameOverText = GameObject.Find("GameOverText")?.GetComponent<TextMeshProUGUI>();
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(false);
        }

        Debug.Log($"Initial checkpoint set at: {lastCheckpoint} for {sceneName}");
    }

    // ===== بارگذاری سطح =====
    public void LoadLevel(int levelIndex)
    {
        Time.timeScale = 1f;
        isGameOver = false;

        string levelName = "Level" + (levelIndex + 1);

        // اگر سطح مورد نظر وجود داشته باشه، بارگذاری کن
        if (levelIndex < totalLevels)
        {
            SceneManager.LoadScene(levelName);
            Debug.Log($"Loading level: {levelName}");
        }
        else
        {
            // اگر همه سطح‌ها تموم شد، برو به Victory
            SceneManager.LoadScene("Victory");
            Debug.Log("All levels complete! Loading Victory...");
        }
    }

    // ===== بارگذاری صحنه Victory =====
    public void LoadVictory()
    {
        Time.timeScale = 1f;
        isGameOver = false;
        SceneManager.LoadScene("Victory");
        Debug.Log("Loading Victory scene...");
    }

    // ===== بارگذاری منوی اصلی =====
    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        isGameOver = false;
        SceneManager.LoadScene("MainMenu");
        Debug.Log("Loading Main Menu...");
    }

    // ===== GameOver =====
    public void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;

        // ریست کردن PressurePlate
        PressurePlate[] plates = FindObjectsOfType<PressurePlate>();
        foreach (var plate in plates)
        {
            plate.ForceReset();
        }

        // غیرفعال کردن Player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null) pc.SetActive(false);

            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
        }

        // غیرفعال کردن Shadow
        GameObject shadow = GameObject.FindGameObjectWithTag("Shadow");
        if (shadow != null)
        {
            ShadowController sc = shadow.GetComponent<ShadowController>();
            if (sc != null)
            {
                Rigidbody2D rb = shadow.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.velocity = Vector2.zero;
                    rb.angularVelocity = 0f;
                }
            }
        }

        Debug.Log("GAME OVER!");
        ShowGameOverMessage();

        Invoke(nameof(RestartFromCheckpoint), 2f);
    }

    // ===== نمایش پیغام GameOver =====
    private void ShowGameOverMessage()
    {
        Debug.Log("===== GAME OVER MESSAGE =====");

        if (gameOverText != null)
        {
            gameOverText.text = "Game Over! Restarting...";
            gameOverText.gameObject.SetActive(true);
            Invoke(nameof(HideGameOverMessage), 2f);
        }
        else
        {
            UIManager.Instance?.ShowMessage("Game Over! Restarting...", 2f);
        }
    }

    private void HideGameOverMessage()
    {
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(false);
        }
    }

    // ===== ریستارت از چک‌پوینت =====
    public void RestartFromCheckpoint()
    {
        Debug.Log($"Restarting from checkpoint: {lastCheckpoint}");

        isGameOver = false;

        // ریست کردن PressurePlate
        PressurePlate[] plates = FindObjectsOfType<PressurePlate>();
        foreach (var plate in plates)
        {
            plate.ForceReset();
        }

        // پیدا کردن Player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = lastCheckpoint;

            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }

            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.SetActive(true);
            }
        }
        else
        {
            Debug.LogError("Player not found!");
            RestartLevel();
            return;
        }

        // پیدا کردن Shadow
        GameObject shadow = GameObject.FindGameObjectWithTag("Shadow");
        if (shadow == null)
        {
            shadow = GameObject.Find("Shadow");
        }

        if (shadow != null)
        {
            ShadowController sc = shadow.GetComponent<ShadowController>();
            if (sc != null && !sc.IsAttached())
            {
                sc.AttachToPlayer();
            }

            Rigidbody2D rb = shadow.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }

            Debug.Log("Shadow found and reset!");
        }
        else
        {
            Debug.LogWarning("Shadow not found in scene!");
        }
    }

    // ===== ریستارت سطح =====
    public void RestartLevel()
    {
        isGameOver = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Debug.Log("Level restarted from beginning!");
    }

    // ===== تنظیم چک‌پوینت =====
    public void SetCheckpoint(Vector2 position)
    {
        lastCheckpoint = position;
        Debug.Log($"Checkpoint set at: {position}");
    }

    // ===== دریافت چک‌پوینت =====
    public Vector3 GetLastCheckpoint() => lastCheckpoint;
    public bool IsGameOver() => isGameOver;

    // ===== خروج از بازی =====
    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }

    // ===== برای تست - کلید G =====
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            Debug.Log("Test GameOver triggered by G key!");
            GameOver();
        }

        // کلید R برای ریستارت سریع
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Restart level triggered by R key!");
            RestartLevel();
        }
    }
}

