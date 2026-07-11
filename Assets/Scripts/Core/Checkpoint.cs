using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private bool isActive = false;
    [SerializeField] private Sprite activeSprite;
    [SerializeField] private Sprite inactiveSprite;

    private SpriteRenderer spriteRenderer;
    private bool hasBeenUsed = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateSprite();
    }

    private void Start()
    {
        // اگر چک‌پوینت ابتدای مرحله هست، فعالش کن
        if (transform.position.x < -6f && !hasBeenUsed)
        {
            ActivateCheckpoint();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasBeenUsed)
        {
            ActivateCheckpoint();
        }
    }

    public void ActivateCheckpoint()
    {
        hasBeenUsed = true;
        isActive = true;
        UpdateSprite();
        GameManager.Instance?.SetCheckpoint(transform.position);
        Debug.Log($"Checkpoint activated at: {transform.position}");

        // نمایش پیغام
        UIManager.Instance?.ShowMessage(" Checkpoint Saved!", 1.5f);
    }

    private void UpdateSprite()
    {
        if (spriteRenderer != null)
        {
            if (isActive && activeSprite != null)
                spriteRenderer.sprite = activeSprite;
            else if (!isActive && inactiveSprite != null)
                spriteRenderer.sprite = inactiveSprite;
        }
    }

    public bool IsActive() => isActive;
}