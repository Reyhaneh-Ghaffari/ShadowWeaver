using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    [Header("Tutorial Settings")]
    [SerializeField] private string message;
    [SerializeField] private float displayDuration = 4f;

    private bool hasTriggered = false;

    private void Start()
    {
        // هر بار که بازی شروع میشه، تریگر رو ریست کن
        hasTriggered = false;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (hasTriggered) return;

        ShowTutorial();
    }

    private void ShowTutorial()
    {
        hasTriggered = true;
        Debug.Log($"📢 Tutorial: {message}");
        UIManager.Instance?.ShowMessage(message, displayDuration);
    }

    // وقتی بازی ری‌استارت میشه، تریگر رو دوباره فعال کن
    private void OnEnable()
    {
        hasTriggered = false;
    }

    // برای دیباگ
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}