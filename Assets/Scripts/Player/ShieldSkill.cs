using UnityEngine;

public class ShieldSkill : MonoBehaviour
{
    [Header("Skill Settings")]
    [SerializeField] private float rotateSpeed = 100f;
    [SerializeField] private GameObject pickupEffect;
    [SerializeField] private AudioClip pickupSound;

    private bool isCollected = false;

    private void Update()
    {
        // چرخش آیتم برای جلوه بصری
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected) return;

        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                // فعال کردن Skill روی Player
                player.ActivateShield();

                // جلوه برداشتن
                if (pickupEffect != null)
                    Instantiate(pickupEffect, transform.position, Quaternion.identity);

                if (pickupSound != null)
                    AudioManager.Instance?.PlaySFX("Pickup");

                isCollected = true;
                Destroy(gameObject);

                Debug.Log("🛡️ Shield Skill Collected!");
            }
        }
    }
}