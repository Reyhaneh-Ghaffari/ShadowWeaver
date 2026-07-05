using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    [Header("Enemy Settings")]
    [SerializeField] protected float moveSpeed = 2f;
    [SerializeField] protected int health = 2;
    [SerializeField] protected float detectionRange = 5f;
    [SerializeField] protected float attackRange = 1.5f;
    [SerializeField] protected LayerMask playerLayer;

    [Header("References")]
    [SerializeField] protected Rigidbody2D rb;
    [SerializeField] protected Animator animator;
    [SerializeField] protected SpriteRenderer spriteRenderer;

    public enum EnemyState { Idle, Patrol, Chase, Attack, Dead }
    protected EnemyState currentState = EnemyState.Patrol;
    protected Transform target;
    protected bool isDead = false;

    // Events
    public System.Action<EnemyState> OnStateChanged;
    public System.Action OnEnemyDeath;

    protected virtual void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        target = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    protected virtual void Start()
    {
        SetState(EnemyState.Patrol);
    }

    protected virtual void Update()
    {
        if (isDead) return;
        UpdateState();
    }

    protected virtual void UpdateState()
    {
        switch (currentState)
        {
            case EnemyState.Idle:
                Idle();
                break;
            case EnemyState.Patrol:
                Patrol();
                break;
            case EnemyState.Chase:
                Chase();
                break;
            case EnemyState.Attack:
                Attack();
                break;
        }
    }

    // ===== State Methods =====
    protected virtual void Idle()
    {
        // منتظر می‌مونه و بعد به گشت‌زنی برمی‌گرده
        if (IsPlayerInRange(detectionRange))
            SetState(EnemyState.Chase);
        else
            SetState(EnemyState.Patrol);
    }

    protected abstract void Patrol();
    protected abstract void Chase();
    protected abstract void Attack();

    // ===== Helper Methods =====
    protected void SetState(EnemyState newState)
    {
        if (currentState == newState) return;

        currentState = newState;
        OnStateChanged?.Invoke(newState);
        UpdateAnimationState(newState);
    }

    protected virtual void UpdateAnimationState(EnemyState state)
    {
        if (animator != null)
        {
            animator.SetBool("IsMoving", state == EnemyState.Patrol || state == EnemyState.Chase);
            animator.SetBool("IsAttacking", state == EnemyState.Attack);
        }
    }

    protected bool IsPlayerInRange(float range)
    {
        if (target == null) return false;
        float distance = Vector2.Distance(transform.position, target.position);
        return distance <= range;
    }

    protected bool IsPlayerInSight()
    {
        if (target == null) return false;

        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            (target.position - transform.position).normalized,
            detectionRange,
            LayerMask.GetMask("Player", "Obstacle")
        );

        return hit.collider != null && hit.collider.CompareTag("Player");
    }

    protected void MoveToward(Vector2 targetPosition, float speed)
    {
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        rb.velocity = direction * speed;

        // چرخش بر اساس حرکت
        if (direction.x > 0.1f)
            spriteRenderer.flipX = false;
        else if (direction.x < -0.1f)
            spriteRenderer.flipX = true;
    }

    protected void StopMoving()
    {
        rb.velocity = Vector2.zero;
    }

    protected void FaceDirection(Vector2 direction)
    {
        if (direction.x > 0.1f)
            spriteRenderer.flipX = false;
        else if (direction.x < -0.1f)
            spriteRenderer.flipX = true;
    }

    // ===== Damage System =====
    public virtual void TakeDamage(int damage)
    {
        if (isDead) return;

        health -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage. Health: {health}");

        if (health <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        isDead = true;
        SetState(EnemyState.Dead);
        OnEnemyDeath?.Invoke();

        if (animator != null)
            animator.SetTrigger("Die");

        // بعد از مرگ نابود کن
        Destroy(gameObject, 1f);
    }

    // ===== Gizmos =====
    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    // ===== Properties =====
    public EnemyState GetCurrentState() => currentState;
    public bool IsDead() => isDead;
    public Transform GetTarget() => target;
    public float GetMoveSpeed() => moveSpeed;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}