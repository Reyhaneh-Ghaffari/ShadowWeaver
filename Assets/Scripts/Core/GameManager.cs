using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;  // ← این رو اضافه کن

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance => instance;

    [Header("Game Settings")]
    [SerializeField] private int currentLevel = 0;
    [SerializeField] private int totalLevels = 3;

    private Vector3 lastCheckpoint;
    private bool isGameOver = false;

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

    private void ShowGameOverMessage()
    {
        // پیدا کردن TutorialText
        TextMeshProUGUI text = FindObjectOfType<TextMeshProUGUI>();
        if (text != null)
        {
            text.text = "💀 Game Over! Restarting...";
            text.gameObject.SetActive(true);
            Invoke(nameof(HideMessage), 2f);
        }
    }

    private void HideMessage()
    {
        TextMeshProUGUI text = FindObjectOfType<TextMeshProUGUI>();
        if (text != null)
        {
            text.gameObject.SetActive(false);
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

    // برای تست - کلید G
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            GameOver();
        }
    }
}