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
    [SerializeField] private GameObject shadowVisual;
    [SerializeField] private GameObject lightVisual;

    private Vector2 moveInput;
    private int jumpCount = 0;
    private const int MAX_JUMPS = 3; // Shadow can dash three times.
    private bool isGrounded;
    private bool isAttached = true;

    private Transform playerTransform;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void Start()
    {
        // The shadow is initially on the character.
        AttachToPlayer();
    }

    private void Update()
    {
        CheckGrounded();
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
        isAttached = false;
        transform.parent = null;
        rb.isKinematic = false;

        // Position the shadow exactly where the character is.
        transform.position = playerTransform.position;

        // Enable shadow physics
        rb.bodyType = RigidbodyType2D.Dynamic;

        // Change shadow layer to pass through specific obstacles
        gameObject.layer = LayerMask.NameToLayer("Shadow");

        // Show shadow and hide object
        shadowVisual.SetActive(true);
        lightVisual.SetActive(false);
    }

    public void AttachToPlayer()
    {
        isAttached = true;
        transform.parent = playerTransform;
        transform.localPosition = Vector3.zero;
        rb.isKinematic = true;
        rb.bodyType = RigidbodyType2D.Kinematic;

        // Hide shadow when attached to the body
        shadowVisual.SetActive(false);
        lightVisual.SetActive(true);
    }

    public void Move()
    {
        float moveX = moveInput.x * moveSpeed;
        rb.velocity = new Vector2(moveX, rb.velocity.y);

        if (moveX > 0.1f) spriteRenderer.flipX = false;
        else if (moveX < -0.1f) spriteRenderer.flipX = true;
    }

    public void Jump()
    {
        if (jumpCount < MAX_JUMPS)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpCount++;
            AudioManager.Instance?.PlaySFX("ShadowJump");
        }
    }

    public void Teleport(Vector2 direction)
    {
        // Casting a shadow towards the platform
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            direction,
            teleportDistance,
            LayerMask.GetMask("Platform")
        );

        if (hit.collider != null)
        {
            transform.position = hit.point + Vector2.up * 0.5f;
            AudioManager.Instance?.PlaySFX("ShadowTeleport");
        }
    }

    private void CheckGrounded()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(
            transform.position + Vector3.down * 0.5f,
            0.2f,
            LayerMask.GetMask("Ground", "Platform")
        );
        isGrounded = colliders.Length > 0;

        if (isGrounded) jumpCount = 0;
    }

    public void SetMoveInput(Vector2 input) => moveInput = input;
    public bool IsAttached() => isAttached;
    public bool IsGrounded() => isGrounded;
}