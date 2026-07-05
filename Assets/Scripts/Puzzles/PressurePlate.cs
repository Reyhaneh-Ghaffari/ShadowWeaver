using UnityEngine;
using UnityEngine.Events;

public class PressurePlate : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool requireLightMode = true;
    [SerializeField] private bool requireShadowMode = false;
    [SerializeField] private float activationDelay = 0.5f;
    [SerializeField] private Sprite pressedSprite;
    [SerializeField] private Sprite unpressedSprite;

    [Header("Events")]
    public UnityEvent OnActivated;
    public UnityEvent OnDeactivated;

    private SpriteRenderer spriteRenderer;
    private bool isActivated = false;
    private int objectsOnPlate = 0;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        UpdateSprite(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Shadow"))
        {
            // چک کردن حالت مورد نیاز
            PlayerState playerState = other.GetComponent<PlayerState>();
            if (playerState != null)
            {
                bool isShadow = playerState.IsInShadowMode();

                if (requireLightMode && isShadow) return;
                if (requireShadowMode && !isShadow) return;
            }

            objectsOnPlate++;
            if (objectsOnPlate >= 1 && !isActivated)
            {
                ActivatePlate();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Shadow"))
        {
            objectsOnPlate--;
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
        AudioManager.Instance?.PlaySFX("PlateActivate");
        Debug.Log("Pressure Plate Activated!");
    }

    private void DeactivatePlate()
    {
        isActivated = false;
        UpdateSprite(false);
        OnDeactivated?.Invoke();
        AudioManager.Instance?.PlaySFX("PlateDeactivate");
        Debug.Log("Pressure Plate Deactivated!");
    }

    private void UpdateSprite(bool pressed)
    {
        if (pressed && pressedSprite != null)
            spriteRenderer.sprite = pressedSprite;
        else if (!pressed && unpressedSprite != null)
            spriteRenderer.sprite = unpressedSprite;
    }

    public bool IsActivated() => isActivated;
}