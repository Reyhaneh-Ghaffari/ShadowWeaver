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

    private Vector2 moveInput;
    private bool isGrounded;
    private int jumpCount = 0;
    private const int MAX_JUMPS = 2;

    // برای دیباگ
    private bool isPlayerActive = true;

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
    }

    private void Update()
    {
        if (!isPlayerActive) return;

        CheckGrounded();

        // ===== ورودی با کیبورد =====
        float horizontal = 0f;
        if (Input.GetKey(KeyCode.A)) horizontal = -1f;
        if (Input.GetKey(KeyCode.D)) horizontal = 1f;
        moveInput = new Vector2(horizontal, 0f);

        // پرش
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log($"Space pressed! JumpCount: {jumpCount}, isGrounded: {isGrounded}, MAX: {MAX_JUMPS}");
            TryJump();
        }

        // برای دیباگ - هر ثانیه یک بار وضعیت رو چاپ کن
        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"Status - isGrounded: {isGrounded}, jumpCount: {jumpCount}, isActive: {isPlayerActive}");
        }


    }

    private void FixedUpdate()
    {
        if (!isPlayerActive) return;
        Move();
    }

    private void Move()
    {
        if (rb == null) return;

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
        if (jumpCount < MAX_JUMPS)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpCount++;
            Debug.Log($"✅ Jump! Count: {jumpCount}");
        }
        else
        {
            Debug.Log($"❌ Can't jump! Max jumps reached: {MAX_JUMPS}");
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
            Debug.Log("🟢 Landed! Jump reset.");
        }
    }

    // ===== متدهای کنترل فعال/غیرفعال =====
    public void SetActive(bool active)
    {
        isPlayerActive = active;
        if (!active)
        {
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
            // غیرفعال کردن ورودی
            moveInput = Vector2.zero;
        }
        Debug.Log($"PlayerController set to: {active}");
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