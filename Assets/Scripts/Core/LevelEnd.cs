using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelEnd : MonoBehaviour
{
    [SerializeField] private string nextLevelName = "Level2";
    [SerializeField] private float delayBeforeLoad = 1f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player reached the gate!");
            DataManager.Instance?.SetCurrentLevel(1);
            UIManager.Instance?.ShowMessage("Level Complete!", 2f);
            Invoke(nameof(LoadNextLevel), delayBeforeLoad);
        }
        else if (other.CompareTag("Shadow"))
        {
            Debug.Log("Shadow cannot pass through the gate!");
            UIManager.Instance?.ShowMessage("Switch to Light Mode to pass!", 2f);
        }
    }

    private void LoadNextLevel()
    {
        SceneManager.LoadScene(nextLevelName);
    }
}