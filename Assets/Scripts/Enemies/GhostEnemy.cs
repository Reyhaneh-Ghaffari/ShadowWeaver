using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GhostEnemy : EnemyStateMachine
{
    [Header("Ghost Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float patrolRange = 3f;
    [SerializeField] private float chaseSpeed = 3f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private Light2D flashlight;
    [SerializeField] private float lightAngle = 45f;
    [SerializeField] private float lightRange = 4f;

    private Vector2 startPosition;
    private int direction = 1;
    private float patrolTimer = 0f;

    protected override void Start()
    {
        base.Start();
        startPosition = transform.position;
        SetState(EnemyState.Patrol);

        // تنظیمات چراغ قوه
        if (flashlight != null)
        {
            flashlight.pointLightOuterAngle = lightAngle;
            flashlight.pointLightInnerAngle = lightAngle * 0.5f;
            flashlight.pointLightOuterRadius = lightRange;
        }
    }

    protected override void Patrol()
    {
        // حرکت چپ و راست
        float newX = transform.position.x + direction * moveSpeed * Time.deltaTime;
        transform.position = new Vector2(newX, transform.position.y);

        // چرخش چراغ قوه
        if (flashlight != null)
        {
            Vector2 directionToPlayer = (target.position - transform.position).normalized;
            float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
            flashlight.transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        // تشخیص بازیکن در محدوده
        if (IsPlayerInRange(detectionRadius) && IsPlayerInSight())
        {
            SetState(EnemyState.Chase);
            return;
        }

        // برگشت در محدوده گشت‌زنی
        if (Mathf.Abs(transform.position.x - startPosition.x) > patrolRange)
        {
            direction *= -1;
        }

        // چرخاندن جهت روح
        transform.localScale = new Vector3(
            direction > 0 ? 1 : -1,
            transform.localScale.y,
            transform.localScale.z
        );

        patrolTimer += Time.deltaTime;
    }

    protected override void Chase()
    {
        if (target == null)
        {
            SetState(EnemyState.Patrol);
            return;
        }

        // حرکت به سمت بازیکن
        Vector2 directionToPlayer = (target.position - transform.position).normalized;
        transform.position += (Vector3)directionToPlayer * chaseSpeed * Time.deltaTime;

        // چرخش چراغ قوه به سمت بازیکن
        if (flashlight != null)
        {
            Vector2 dir = (target.position - transform.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            flashlight.transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        // بررسی برخورد نور با سایه بازیکن
        CheckLightCollision();

        // اگر بازیکن دور شد، برگرد به گشت‌زنی
        if (!IsPlayerInRange(detectionRadius))
        {
            SetState(EnemyState.Patrol);
            return;
        }

        // اگر به اندازه کافی نزدیک شد، حمله کن
        if (IsPlayerInRange(attackRange))
        {
            SetState(EnemyState.Attack);
        }
    }

    protected override void Attack()
    {
        // حمله روح - اگر سایه باشه، Game Over
        if (IsPlayerInRange(attackRange))
        {
            // چک کردن اینکه بازیکن در حالت سایه هست یا نه
            PlayerState playerState = target.GetComponent<PlayerState>();
            if (playerState != null && playerState.IsInShadowMode())
            {
                // Game Over - بازیکن در حالت سایه به روح برخورد کرده
                GameManager.Instance?.GameOver();
            }
        }

        // بعد از حمله، برگرد به گشت‌زنی
        SetState(EnemyState.Patrol);
    }

    private void CheckLightCollision()
    {
        // چک کردن اینکه آیا نور چراغ قوه به سایه برخورد کرده
        if (flashlight == null || target == null) return;

        PlayerState playerState = target.GetComponent<PlayerState>();
        if (playerState == null || !playerState.IsInShadowMode()) return;

        // محاسبه فاصله و زاویه
        Vector2 directionToPlayer = (target.position - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, target.position);

        // زاویه بین جهت روح و بازیکن
        float angleToPlayer = Vector2.Angle(
            flashlight.transform.right,
            directionToPlayer
        );

        // اگر بازیکن در محدوده نور و فاصله مناسب باشه
        if (distance <= lightRange && angleToPlayer <= lightAngle / 2f)
        {
            // Raycast برای تشخیص موانع
            RaycastHit2D hit = Physics2D.Raycast(
                transform.position,
                directionToPlayer,
                distance,
                LayerMask.GetMask("Obstacle", "Player")
            );

            if (hit.collider != null && hit.collider.CompareTag("Player"))
            {
                // نور به سایه برخورد کرده - Game Over
                GameManager.Instance?.GameOver();
                Debug.Log("Light hit the shadow!");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // محدوده گشت‌زنی
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(
            new Vector2(startPosition.x - patrolRange, startPosition.y - 0.5f),
            new Vector2(startPosition.x + patrolRange, startPosition.y - 0.5f)
        );
    }
}