using UnityEngine;

public class SpiderEnemy : EnemyStateMachine
{
    [Header("Spider Settings")]
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float jumpHeight = 3f;
    [SerializeField] private float jumpInterval = 2f;
    [SerializeField] private float detectionRange = 4f;
    [SerializeField] private GameObject webAttack;

    private float jumpTimer = 0f;
    private bool isJumping = false;

    protected override void Start()
    {
        base.Start();
        SetState(EnemyState.Patrol);
        jumpTimer = Random.Range(0f, jumpInterval);
    }

    protected override void Update()
    {
        base.Update();
        jumpTimer -= Time.deltaTime;
    }

    protected override void Patrol()
    {
        float moveX = Mathf.Sin(Time.time * 0.5f) * moveSpeed * Time.deltaTime;
        transform.position += new Vector3(moveX, 0, 0);

        if (moveX > 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if (moveX < 0)
            transform.localScale = new Vector3(-1, 1, 1);

        if (jumpTimer <= 0 && !isJumping)
        {
            Jump();
            jumpTimer = jumpInterval;
        }

        if (IsPlayerInRange(detectionRange) || IsShadowInRange(detectionRange))
        {
            SetState(EnemyState.Chase);
        }
    }

    protected override void Chase()
    {
        Transform chaseTarget = GetClosestTarget();

        if (chaseTarget == null)
        {
            SetState(EnemyState.Patrol);
            return;
        }

        if (!isJumping)
        {
            Vector2 direction = (chaseTarget.position - transform.position).normalized;
            JumpToward(direction);
        }

        if (IsTargetInRange(chaseTarget, 1.5f))
        {
            SetState(EnemyState.Attack);
        }
    }

    protected override void Attack()
    {
        Transform chaseTarget = GetClosestTarget();

        if (chaseTarget != null && webAttack != null)
        {
            GameObject web = Instantiate(webAttack, transform.position, Quaternion.identity);
            Vector2 direction = (chaseTarget.position - transform.position).normalized;
            web.GetComponent<Rigidbody2D>().velocity = direction * 5f;
            Destroy(web, 2f);
        }

        SetState(EnemyState.Patrol);
    }

    private void Jump()
    {
        isJumping = true;
        GetComponent<Rigidbody2D>().velocity = Vector2.up * jumpHeight;
        Invoke(nameof(ResetJump), 0.5f);
    }

    private void JumpToward(Vector2 direction)
    {
        isJumping = true;
        GetComponent<Rigidbody2D>().velocity = direction * 5f + Vector2.up * 3f;
        Invoke(nameof(ResetJump), 0.5f);
    }

    private void ResetJump()
    {
        isJumping = false;
    }

    private bool IsShadowInRange(float range)
    {
        GameObject shadow = GameObject.FindGameObjectWithTag("Shadow");
        if (shadow == null) return false;
        return Vector2.Distance(transform.position, shadow.transform.position) <= range;
    }

    private Transform GetClosestTarget()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        GameObject shadow = GameObject.FindGameObjectWithTag("Shadow");

        if (player == null && shadow == null) return null;
        if (player == null) return shadow.transform;
        if (shadow == null) return player.transform;

        float distToPlayer = Vector2.Distance(transform.position, player.transform.position);
        float distToShadow = Vector2.Distance(transform.position, shadow.transform.position);

        return distToPlayer < distToShadow ? player.transform : shadow.transform;
    }

    private bool IsTargetInRange(Transform target, float range)
    {
        if (target == null) return false;
        return Vector2.Distance(transform.position, target.position) <= range;
    }

    // ===== اصلاح برخورد با Player =====
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();

            // ===== بررسی Shield =====
            if (player != null && player.HasShield())
            {
                // با Shield، Player از عنکبوت رد میشه
                player.UseShield();
                Debug.Log("🛡️ Player used Shield to pass Spider!");

                // عنکبوت رو نابود کن (اختیاری)
                Die();
                return;
            }

            // بدون Shield → GameOver
            Debug.Log("Player touched Spider! Game Over!");
            GameManager.Instance?.GameOver();
        }
        else if (collision.gameObject.CompareTag("Shadow"))
        {
            // سایه همیشه GameOver
            Debug.Log("Shadow touched Spider! Game Over!");
            GameManager.Instance?.GameOver();
        }
    }

    private void Die()
    {
        SetState(EnemyState.Dead);
        Destroy(gameObject, 0.5f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}