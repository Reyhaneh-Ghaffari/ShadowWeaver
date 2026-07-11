using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private LayerMask groundLayer;

    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Shield Skill")]
    [SerializeField] private GameObject shieldVisual;  // المان گرافیکی Shield
    [SerializeField] private float shieldDuration = 5f; // مدت زمان محافظت (اختیاری)

    private Vector2 moveInput;
    private bool isGrounded;
    private int jumpCount = 0;
    private const int MAX_JUMPS = 2;
    private bool isPlayerActive = true;

    // ===== متغیرهای Shield =====
    private bool hasShield = false;
    private float shieldTimer = 0f;

    private float footstepTimer = 0f;
    private const float FOOTSTEP_INTERVAL = 0.3f;

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        if (groundCheckPoint == null)
        {
            GameObject go = new GameObject("GroundCheck");
            go.transform.parent = transform;
            go.transform.localPosition = new Vector3(0, -0.85f, 0);
            groundCheckPoint = go.transform;
        }

        // مخفی کردن Shield در ابتدا
        if (shieldVisual != null)
            shieldVisual.SetActive(false);
    }

    private void Update()
    {
        if (!isPlayerActive || rb == null) return;

        CheckGrounded();

        float horizontal = 0f;
        if (Input.GetKey(KeyCode.A)) horizontal = -1f;
        if (Input.GetKey(KeyCode.D)) horizontal = 1f;
        moveInput = new Vector2(horizontal, 0f);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryJump();
        }

        HandleFootstep();
        UpdateShieldTimer();
    }

    private void FixedUpdate()
    {
        if (!isPlayerActive || rb == null) return;
        Move();
    }

    private void Move()
    {
        float moveX = moveInput.x * moveSpeed;
        rb.velocity = new Vector2(moveX, rb.velocity.y);

        if (spriteRenderer != null)
        {
            if (moveX > 0.1f) spriteRenderer.flipX = false;
            else if (moveX < -0.1f) spriteRenderer.flipX = true;
        }
    }

    private void TryJump()
    {
        if (rb == null) return;

        if (jumpCount < MAX_JUMPS)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpCount++;
            AudioManager.Instance?.PlaySFX("Jump");
        }
    }

    private void CheckGrounded()
    {
        if (groundCheckPoint == null) return;

        RaycastHit2D hit = Physics2D.Raycast(
            groundCheckPoint.position,
            Vector2.down,
            0.2f,
            groundLayer
        );

        bool wasGrounded = isGrounded;
        isGrounded = hit.collider != null;

        if (isGrounded && !wasGrounded)
        {
            jumpCount = 0;
        }
    }

    private void HandleFootstep()
    {
        bool isMoving = Mathf.Abs(moveInput.x) > 0.1f;

        if (isMoving && isGrounded && isPlayerActive)
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0f)
            {
                AudioManager.Instance?.PlaySFX("Footstep");
                footstepTimer = FOOTSTEP_INTERVAL;
            }
        }
        else
        {
            footstepTimer = 0f;
        }
    }

    // ===== متدهای Shield =====
    public void ActivateShield()
    {
        hasShield = true;
        shieldTimer = shieldDuration;

        if (shieldVisual != null)
            shieldVisual.SetActive(true);

        Debug.Log("🛡️ Shield Activated!");
    }

    public bool HasShield()
    {
        return hasShield;
    }

    public void UseShield()
    {
        if (hasShield)
        {
            hasShield = false;
            if (shieldVisual != null)
                shieldVisual.SetActive(false);

            Debug.Log("🛡️ Shield Used!");
        }
    }

    private void UpdateShieldTimer()
    {
        if (hasShield)
        {
            shieldTimer -= Time.deltaTime;
            if (shieldTimer <= 0f)
            {
                hasShield = false;
                if (shieldVisual != null)
                    shieldVisual.SetActive(false);
                Debug.Log("🛡️ Shield Expired!");
            }
        }
    }

    public void SetActive(bool active)
    {
        isPlayerActive = active;
        if (!active && rb != null)
        {
            rb.velocity = Vector2.zero;
        }
    }

    public bool IsActive() => isPlayerActive;

    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheckPoint.position, 0.2f);
        }
    }
}