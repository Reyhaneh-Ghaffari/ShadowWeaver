using UnityEngine;

public class EnemyFacing : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform flashlight;  // چراغ قوه
    //[SerializeField] private float rotationSpeed = 5f;

    private SpriteRenderer spriteRenderer;
    private int facingDirection = 1; // 1 = راست, -1 = چپ

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (flashlight == null)
        {
            flashlight = transform.Find("Flashlight");
        }
    }

    private void Update()
    {
        // تشخیص جهت بر اساس Sprite Renderer
        if (spriteRenderer != null)
        {
            if (spriteRenderer.flipX)
            {
                facingDirection = -1; // چپ
            }
            else
            {
                facingDirection = 1; // راست
            }
        }

        // چرخش چراغ قوه به سمت حرکت
        if (flashlight != null)
        {
            if (facingDirection == 1)
            {
                // به راست
                flashlight.localRotation = Quaternion.Euler(0, 0, 0);
            }
            else
            {
                // به چپ
                flashlight.localRotation = Quaternion.Euler(0, 180, 0);
            }
        }
    }
}