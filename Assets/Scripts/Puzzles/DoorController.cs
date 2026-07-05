using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private PressurePlate requiredPlate;
    [SerializeField] private bool startOpen = false;
    [SerializeField] private float openSpeed = 2f;
    [SerializeField] private Vector2 openOffset = new Vector2(0, 3f);
    [SerializeField] private bool allowShadowPass = true;

    private Vector2 closedPosition;
    private Vector2 openPosition;
    private bool isOpen = false;
    private bool isMoving = false;

    private void Start()
    {
        closedPosition = transform.position;
        openPosition = closedPosition + openOffset;

        isOpen = startOpen;
        transform.position = startOpen ? openPosition : closedPosition;

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
            AudioManager.Instance?.PlaySFX("DoorOpen");
            Debug.Log("Door Opened!");
        }
    }

    public void CloseDoor()
    {
        if (isOpen)
        {
            isOpen = false;
            isMoving = true;
            AudioManager.Instance?.PlaySFX("DoorClose");
            Debug.Log("Door Closed!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isOpen && other.CompareTag("Shadow") && allowShadowPass)
        {
            // سایه می‌تونه از در رد بشه
            Debug.Log("Shadow passed through door!");
        }
        else if (!isOpen && other.CompareTag("Player"))
        {
            // جسم نمی‌تونه از در بسته رد بشه
            // می‌تونیم اینجا یه پیام نشون بدیم
            UIManager.Instance?.ShowMessage("You need to activate the pressure plate first!");
        }
    }

    public bool IsOpen() => isOpen;
}