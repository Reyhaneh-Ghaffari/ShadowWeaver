using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelEnd : MonoBehaviour
{
    [Header("Level Settings")]
    [SerializeField] private string nextLevelName = "Level2";
    [SerializeField] private float delayBeforeLoad = 1.5f;
    [SerializeField] private string completeMessage = "🎉 Level Complete!";

    private bool isComplete = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isComplete) return;

        Debug.Log($"🚪 Gate triggered by: {other.gameObject.name} with tag: {other.tag}");

        // فقط Player میتونه از در عبور کنه
        if (other.CompareTag("Player"))
        {
            CompleteLevel();
        }
        else if (other.CompareTag("Shadow"))
        {
            Debug.Log("🚫 Shadow cannot pass through the gate!");
            UIManager.Instance?.ShowMessage("Switch to Light Mode to pass!", 2f);
        }
    }

    private void CompleteLevel()
    {
        isComplete = true;
        Debug.Log($"🎉 {completeMessage}");

        // نمایش پیغام
        UIManager.Instance?.ShowMessage(completeMessage, 2f);

        // ذخیره پیشرفت
        DataManager.Instance?.SetCurrentLevel(1);

        // بارگذاری مرحله بعد
        Invoke(nameof(LoadNextLevel), delayBeforeLoad);
    }

    private void LoadNextLevel()
    {
        Debug.Log($"🔄 Loading next level: {nextLevelName}");
        SceneManager.LoadScene(nextLevelName);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}