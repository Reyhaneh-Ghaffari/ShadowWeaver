using UnityEngine;

public class MirrorWorldTrigger : MonoBehaviour
{
    [Header("Mirror World Settings")]
    [SerializeField] private float activeDuration = 15f;
    [SerializeField] private GameObject mirrorEffect;
    [SerializeField] private bool isActive = false;

    private float remainingTime = 0f;
    private bool isTriggered = false;

    public System.Action<bool> OnMirrorWorldActivated;

    private void Start()
    {
        if (mirrorEffect != null)
            mirrorEffect.SetActive(false);
    }

    private void Update()
    {
        if (!isTriggered) return;

        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0)
        {
            DeactivateMirrorWorld();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isTriggered)
        {
            ActivateMirrorWorld();
        }
    }

    public void ActivateMirrorWorld()
    {
        if (!isActive)
        {
            isActive = true;
            isTriggered = true;
            remainingTime = activeDuration;

            if (mirrorEffect != null)
                mirrorEffect.SetActive(true);

            // معکوس کردن چیدمان دنیا
            FlipWorld();

            OnMirrorWorldActivated?.Invoke(true);
            AudioManager.Instance?.PlaySFX("MirrorWorldActivate");

            Debug.Log($"Mirror World Activated for {activeDuration} seconds!");
        }
    }

    public void DeactivateMirrorWorld()
    {
        isActive = false;
        isTriggered = false;

        if (mirrorEffect != null)
            mirrorEffect.SetActive(false);

        // برگرداندن چیدمان به حالت اولیه
        FlipWorld();

        OnMirrorWorldActivated?.Invoke(false);
        AudioManager.Instance?.PlaySFX("MirrorWorldDeactivate");

        Debug.Log("Mirror World Deactivated!");
    }

    private void FlipWorld()
    {
        // اینجا می‌تونیم همه پلتفرم‌ها و اشیاء رو معکوس کنیم
        // مثلاً پلتفرم‌ها رو ۱۸۰ درجه بچرخونیم یا جابه‌جا کنیم

        GameObject[] platforms = GameObject.FindGameObjectsWithTag("Platform");
        foreach (GameObject platform in platforms)
        {
            // چرخش یا جابه‌جایی پلتفرم‌ها
            platform.transform.Rotate(0, 0, 180);
        }

        // معکوس کردن جاذبه
        Physics2D.gravity = -Physics2D.gravity;
    }

    public bool IsActive() => isActive;
    public float GetRemainingTime() => remainingTime;
}