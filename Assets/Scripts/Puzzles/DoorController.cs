using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private PressurePlate requiredPlate;
    [SerializeField] private float openSpeed = 2f;
    [SerializeField] private Vector2 openOffset = new Vector2(0, 3f);
    [SerializeField] private bool allowShadowPass = true;

    [Header("References")]
    [SerializeField] private Collider2D doorCollider;  // ← Collider درب

    private Vector2 closedPosition;
    private Vector2 openPosition;
    private bool isOpen = false;
    private bool isMoving = false;

    private void Start()
    {
        closedPosition = transform.position;
        openPosition = closedPosition + openOffset;
        transform.position = closedPosition;

        // پیدا کردن Collider درب
        if (doorCollider == null)
            doorCollider = GetComponent<Collider2D>();

        if (requiredPlate != null)
        {
            requiredPlate.OnActivated.AddListener(OpenDoor);
            requiredPlate.OnDeactivated.AddListener(CloseDoor);
        }
    }

    private void Update()
    {
        if (!isMoving) return;

        Vector2 target = isOpen ? openPosition : closedPosition;
        transform.position = Vector2.Lerp(transform.position, target, openSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, target) < 0.01f)
        {
            transform.position = target;
            isMoving = false;
        }
    }

    public void OpenDoor()
    {
        if (!isOpen)
        {
            isOpen = true;
            isMoving = true;

            // ===== غیرفعال کردن Collider درب =====
            if (doorCollider != null)
                doorCollider.enabled = false;

            Debug.Log(" Door Opened! Collider disabled.");
        }
    }

    public void CloseDoor()
    {
        if (isOpen)
        {
            isOpen = false;
            isMoving = true;

            // ===== فعال کردن Collider درب =====
            if (doorCollider != null)
                doorCollider.enabled = true;

            Debug.Log(" Door Closed! Collider enabled.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isOpen && other.CompareTag("Shadow") && allowShadowPass)
        {
            Debug.Log(" Shadow passed through the door!");
            // اینجا میتونی مرحله بعد رو صدا بزنی
        }
        else if (!isOpen && other.CompareTag("Player"))
        {
            Debug.Log(" Door is closed!");
            UIManager.Instance?.ShowMessage(" Stand on the pressure plate to open the door!", 2f);
        }
        else if (!isOpen && other.CompareTag("Shadow"))
        {
            Debug.Log("Shadow cannot pass! Door is closed.");
            UIManager.Instance?.ShowMessage(" Switch to Light mode and stand on the plate!", 2f);
        }
    }

    public bool IsOpen() => isOpen;
}