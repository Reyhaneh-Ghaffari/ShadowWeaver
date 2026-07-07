using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance => instance;

    [Header("Game Settings")]
    [SerializeField] private int currentLevel = 0;
    [SerializeField] private int totalLevels = 3;

    private Vector3 lastCheckpoint;
    private bool isGameOver = false;

    // ===== این خط رو اضافه کن =====
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
        lastCheckpoint = new Vector3(-7f, -2.5f, 0f);

        // ===== پیدا کردن GameOverText =====
        gameOverText = GameObject.Find("GameOverText")?.GetComponent<TextMeshProUGUI>();
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(false);
        }

        Debug.Log($"📍 Initial checkpoint set at: {lastCheckpoint}");
    }

    public void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;

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

        Debug.Log("💀 GAME OVER!");
        ShowGameOverMessage();

        Invoke(nameof(RestartFromCheckpoint), 2f);
    }

    // ===== این متد رو اضافه کن =====
    private void ShowGameOverMessage()
    {
        Debug.Log("💀 ===== GAME OVER MESSAGE ===== 💀");

        if (gameOverText != null)
        {
            gameOverText.text = "Game Over! Restarting...";
            gameOverText.gameObject.SetActive(true);
            Invoke(nameof(HideGameOverMessage), 2f);
        }
        else
        {
            // اگه GameOverText پیدا نشد، از UIManager استفاده کن
            UIManager.Instance?.ShowMessage("💀 Game Over! Restarting...", 2f);
        }
    }

    // ===== این متد رو اضافه کن =====
    private void HideGameOverMessage()
    {
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(false);
        }
    }

    public void RestartFromCheckpoint()
    {
        Debug.Log($"🔄 Restarting from checkpoint: {lastCheckpoint}");

        isGameOver = false;

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
            Debug.LogError("❌ Player not found!");
            RestartLevel();
            return;
        }

        GameObject shadow = GameObject.FindGameObjectWithTag("Shadow");
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
        }
    }

    public void RestartLevel()
    {
        isGameOver = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Debug.Log("🔄 Level restarted from beginning!");
    }

    public void SetCheckpoint(Vector2 position)
    {
        lastCheckpoint = position;
        Debug.Log($"📍 Checkpoint set at: {position}");
    }

    public Vector3 GetLastCheckpoint() => lastCheckpoint;
    public bool IsGameOver() => isGameOver;

    // ===== برای تست - کلید G =====
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            Debug.Log("🔴 Test GameOver triggered by G key!");
            GameOver();
        }
    }
}