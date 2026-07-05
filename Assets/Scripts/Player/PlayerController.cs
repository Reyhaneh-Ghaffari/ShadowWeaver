using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private LayerMask groundLayer;

    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    // State
    private Vector2 moveInput;
    private bool isGrounded;
    private bool isShadowMode = false;
    private int jumpCount = 0;
    private const int MAX_JUMPS = 2;

    // Events
    public System.Action<bool> OnModeChanged;

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (groundCheckPoint == null) 
        {
            GameObject go = new GameObject("GroundCheck");
            go.transform.parent = transform;
            go.transform.localPosition = new Vector3(0, -0.5f, 0);
            groundCheckPoint = go.transform;
        }
    }

    private void Update()
    {
        CheckGrounded();
        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        Move();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && jumpCount < MAX_JUMPS)
        {
            Jump();
        }
    }

    public void OnSwitchMode(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            ToggleShadowMode();
        }
    }

    private void Move()
    {
        float moveX = moveInput.x * moveSpeed;
        rb.velocity = new Vector2(moveX, rb.velocity.y);

        // Flip sprite based on movement direction
        if (moveX > 0.1f)
            spriteRenderer.flipX = false;
        else if (moveX < -0.1f)
            spriteRenderer.flipX = true;
    }

    private void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        jumpCount++;
        AudioManager.Instance?.PlaySFX("Jump");
    }

    private void CheckGrounded()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(
            groundCheckPoint.position, 
            groundCheckRadius, 
            groundLayer
        );
        
        bool wasGrounded = isGrounded;
        isGrounded = colliders.Length > 0;

        if (isGrounded && !wasGrounded)
        {
            jumpCount = 0;
        }
    }

    private void UpdateAnimations()
    {
        float speed = Mathf.Abs(rb.velocity.x);
        animator.SetFloat("Speed", speed);
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("IsShadowMode", isShadowMode);
    }

    private void ToggleShadowMode()
    {
        isShadowMode = !isShadowMode;
        OnModeChanged?.Invoke(isShadowMode);
        
        //// Changing layers for obstacle collisions
        if (isShadowMode)
        {
            gameObject.layer = LayerMask.NameToLayer("Shadow");
          // Disable collision with light objects
        }
        else
        {
            gameObject.layer = LayerMask.NameToLayer("Player");
        }
        
        AudioManager.Instance?.PlaySFX("SwitchMode");
    }

    public bool IsShadowMode() => isShadowMode;
    public bool IsGrounded() => isGrounded;

    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }
    }
}