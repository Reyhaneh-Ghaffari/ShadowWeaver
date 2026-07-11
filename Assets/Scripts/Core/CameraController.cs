using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform playerTarget;      // هدف Player
    [SerializeField] private Transform shadowTarget;      // هدف Shadow
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10);

    [Header("Smooth Settings")]
    [SerializeField] private float smoothSpeed = 8f;      // سرعت نرمی حرکت دوربین

    [Header("Bounds Settings")]
    [SerializeField] private bool useBounds = true;
    [SerializeField] private Vector2 minBounds;
    [SerializeField] private Vector2 maxBounds;

    private Transform currentTarget;
    private Vector3 velocity = Vector3.zero;  // برای SmoothDamp

    private void Start()
    {
        // پیدا کردن Player و Shadow
        if (playerTarget == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTarget = player.transform;
        }

        if (shadowTarget == null)
        {
            GameObject shadow = GameObject.FindGameObjectWithTag("Shadow");
            if (shadow != null) shadowTarget = shadow.transform;
        }

        // هدف اولیه = Player
        currentTarget = playerTarget;
    }

    private void LateUpdate()
    {
        if (playerTarget == null || shadowTarget == null) return;

        // تشخیص حالت فعلی
        PlayerState ps = playerTarget.GetComponent<PlayerState>();
        if (ps != null && ps.IsInShadowMode())
        {
            // حالت سایه → دوربین به سایه نگاه کنه
            currentTarget = shadowTarget;
        }
        else
        {
            // حالت نور → دوربین به Player نگاه کنه
            currentTarget = playerTarget;
        }

        // موقعیت مطلوب دوربین
        Vector3 desiredPosition = currentTarget.position + offset;

        // اعمال محدودیت
        if (useBounds)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minBounds.x, maxBounds.x);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minBounds.y, maxBounds.y);
        }

        // حرکت نرم با SmoothDamp (بدون لرزش)
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref velocity,
            1f / smoothSpeed
        );
    }

    // برای دیدن محدودیت‌ها در Scene View
    private void OnDrawGizmosSelected()
    {
        if (!useBounds) return;

        Gizmos.color = Color.yellow;
        Vector3 center = new Vector3(
            (minBounds.x + maxBounds.x) / 2,
            (minBounds.y + maxBounds.y) / 2,
            0
        );
        Vector3 size = new Vector3(
            maxBounds.x - minBounds.x,
            maxBounds.y - minBounds.y,
            0
        );
        Gizmos.DrawWireCube(center, size);
    }
}