using UnityEngine;

public class ShadowController : MonoBehaviour
{
    [Header("Shadow Settings")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float teleportDistance = 3f;

    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private Vector2 moveInput;
    private int jumpCount = 0;
    private const int MAX_JUMPS = 3;
    private bool isGrounded;
    private bool isAttached = true;

    private Transform playerTransform;

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        Debug.Log("ShadowController Awake!");
    }

    private void Start()
    {
        AttachToPlayer();
    }

    private void Update()
    {
        CheckGrounded();

        // برای دیباگ: وضعیت سایه رو چاپ کن
        if (!isAttached && moveInput.x != 0)
        {
            Debug.Log($"Shadow Update - Moving: {moveInput.x}");
        }
    }

    private void FixedUpdate()
    {
        if (!isAttached)
        {
            Move();
        }
    }

    public void DetachFromPlayer()
    {
        if (playerTransform == null)
        {
            Debug.LogError("Player Transform is null!");
            return;
        }

        isAttached = false;
        transform.parent = null;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.bodyType = RigidbodyType2D.Dynamic;
        }

        transform.position = playerTransform.position;

        // سایه با Obstacle برخورد کنه (نتونه ازش رد بشه)
        gameObject.layer = LayerMask.NameToLayer("Shadow");

        Debug.Log("Shadow detached!");
    }

    public void AttachToPlayer()
    {
        if (playerTransform == null)
        {
            Debug.LogError("Player Transform is null!");
            return;
        }

        isAttached = true;
        transform.parent = playerTransform;
        transform.localPosition = Vector3.zero;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        Debug.Log("Shadow attached!");
    }

    public void SetMoveInput(Vector2 input)
    {
        moveInput = input;
        Debug.Log($"Shadow SetMoveInput: {input}");
    }

    public void Jump()
    {
        Debug.Log($"Shadow Jump called - isAttached: {isAttached}, rb: {rb != null}");

        if (rb == null || isAttached)
        {
            Debug.Log("Shadow Jump - blocked (attached or no rb)");
            return;
        }

        if (jumpCount < MAX_JUMPS)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpCount++;
            Debug.Log($"Shadow Jump! Count: {jumpCount}");
        }
        else
        {
            Debug.Log($"Shadow Jump - max jumps reached ({MAX_JUMPS})");
        }
    }

    public void Teleport(Vector2 direction)
    {
        Debug.Log($"Teleport called - isAttached: {isAttached}, direction: {direction}");

        if (rb == null || isAttached)
        {
            Debug.Log("Teleport blocked (attached or no rb)");
            return;
        }

        Vector2 newPos = (Vector2)transform.position + direction * teleportDistance;
        transform.position = newPos;
        Debug.Log($"Teleported to: {newPos}");
    }

    private void Move()
    {
        if (rb == null || isAttached) return;

        float moveX = moveInput.x * moveSpeed;
        rb.velocity = new Vector2(moveX, rb.velocity.y);

        if (spriteRenderer != null)
        {
            if (moveX > 0.1f) spriteRenderer.flipX = false;
            else if (moveX < -0.1f) spriteRenderer.flipX = true;
        }
    }

    private void CheckGrounded()
    {
        if (rb == null) return;

        RaycastHit2D hit = Physics2D.Raycast(
            transform.position + Vector3.down * 0.6f,
            Vector2.down,
            0.2f,
            LayerMask.GetMask("Ground")
        );

        bool wasGrounded = isGrounded;
        isGrounded = hit.collider != null;

        if (isGrounded && !wasGrounded)
        {
            jumpCount = 0;
        }
    }

    public bool IsAttached() => isAttached;
}