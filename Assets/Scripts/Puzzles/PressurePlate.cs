using UnityEngine;
using UnityEngine.Events;

public class PressurePlate : MonoBehaviour
{
    [Header("Plate Settings")]
    [SerializeField] private bool requireLightMode = true;
    [SerializeField] private Sprite pressedSprite;
    [SerializeField] private Sprite unpressedSprite;

    [Header("Events")]
    public UnityEvent OnActivated;
    public UnityEvent OnDeactivated;

    private SpriteRenderer spriteRenderer;
    private bool isActivated = false;
    private int objectsOnPlate = 0;
    private Collider2D plateCollider;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

        plateCollider = GetComponent<Collider2D>();
        if (plateCollider == null)
            plateCollider = gameObject.AddComponent<BoxCollider2D>();

        if (plateCollider != null)
            plateCollider.isTrigger = true;

        UpdateSprite(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Trigger entered by: {other.gameObject.name} with tag: {other.tag}");

        if (other.CompareTag("Player"))
        {
            PlayerState playerState = other.GetComponent<PlayerState>();

            if (requireLightMode && playerState != null && !playerState.IsInShadowMode())
            {
                objectsOnPlate++;
                Debug.Log($"Objects on plate: {objectsOnPlate}");
                if (objectsOnPlate >= 1 && !isActivated)
                {
                    ActivatePlate();
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            objectsOnPlate--;
            Debug.Log($"Objects on plate: {objectsOnPlate}");
            if (objectsOnPlate <= 0 && isActivated)
            {
                DeactivatePlate();
            }
        }
    }

    private void ActivatePlate()
    {
        isActivated = true;
        UpdateSprite(true);
        OnActivated?.Invoke();
        Debug.Log("Pressure Plate Activated!");
    }

    private void DeactivatePlate()
    {
        isActivated = false;
        UpdateSprite(false);
        OnDeactivated?.Invoke();
        Debug.Log("Pressure Plate Deactivated!");
    }

    private void UpdateSprite(bool pressed)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = pressed ? pressedSprite : unpressedSprite;
        }
    }

    public void ForceReset()
    {
        // ریست کامل
        isActivated = false;
        objectsOnPlate = 0;
        UpdateSprite(false);

        // Deactivate رو صدا بزن تا درها بسته بشن
        OnDeactivated?.Invoke();

        // اطمینان از اینکه Collider فعال هست
        if (plateCollider != null)
        {
            plateCollider.enabled = true;
            plateCollider.isTrigger = true;
        }

        Debug.Log("Pressure Plate Force Reset!");
    }

    public bool IsActivated() => isActivated;
}