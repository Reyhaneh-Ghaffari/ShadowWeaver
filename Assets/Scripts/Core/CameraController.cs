using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private float smoothSpeed = 0.125f;
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10);
    [SerializeField] private bool useBounds = true;
    [SerializeField] private Bounds levelBounds;

    private void FixedUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;

        if (useBounds)
        {
            desiredPosition.x = Mathf.Clamp(
                desiredPosition.x,
                levelBounds.min.x,
                levelBounds.max.x
            );
            desiredPosition.y = Mathf.Clamp(
                desiredPosition.y,
                levelBounds.min.y,
                levelBounds.max.y
            );
        }

        Vector3 smoothedPosition = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed
        );
        transform.position = smoothedPosition;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void SetBounds(Bounds newBounds)
    {
        levelBounds = newBounds;
        useBounds = true;
    }

    private void OnDrawGizmos()
    {
        if (useBounds)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(
                levelBounds.center,
                levelBounds.size
            );
        }
    }
}