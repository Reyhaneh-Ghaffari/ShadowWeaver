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
        // حرکت آرام چپ و راست
        float moveX = Mathf.Sin(Time.time * 0.5f) * moveSpeed * Time.deltaTime;
        transform.position += new Vector3(moveX, 0, 0);

        // چرخش عنکبوت
        if (moveX > 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if (moveX < 0)
            transform.localScale = new Vector3(-1, 1, 1);

        // پرش دوره‌ای
        if (jumpTimer <= 0 && !isJumping)
        {
            Jump();
            jumpTimer = jumpInterval;
        }

        // تشخیص بازیکن
        if (IsPlayerInRange(detectionRange))
        {
            SetState(EnemyState.Chase);
        }
    }

    protected override void Chase()
    {
        if (target == null)
        {
            SetState(EnemyState.Patrol);
            return;
        }

        // پرش به سمت بازیکن
        if (!isJumping)
        {
            Vector2 direction = (target.position - transform.position).normalized;
            JumpToward(direction);
        }

        // اگر نزدیک شد حمله کن
        if (IsPlayerInRange(1.5f))
        {
            SetState(EnemyState.Attack);
        }
    }

    protected override void Attack()
    {
        // پرتاب تار به سمت بازیکن
        if (target != null && webAttack != null)
        {
            GameObject web = Instantiate(webAttack, transform.position, Quaternion.identity);
            Vector2 direction = (target.position - transform.position).normalized;
            web.GetComponent<Rigidbody2D>().velocity = direction * 5f;

            // بعد از 2 ثانیه نابودش کن
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // اگر بازیکن در حالت نور باشه، می‌تونه عنکبوت رو بکشه
            PlayerState playerState = collision.gameObject.GetComponent<PlayerState>();
            if (playerState != null && !playerState.IsInShadowMode())
            {
                Die();
            }
            else if (playerState != null && playerState.IsInShadowMode())
            {
                // در حالت سایه، برخورد با عنکبوت = Game Over
                GameManager.Instance?.GameOver();
            }
        }
    }

    private void Die()
    {
        // انیمیشن مرگ
        SetState(EnemyState.Dead);
        Destroy(gameObject, 1f);
    }
}