using UnityEngine;
using TMPro;

public class TutorialMessage : MonoBehaviour
{
    [Header("Tutorial Settings")]
    [SerializeField] private string messageText;
    [SerializeField] private float displayDuration = 3f;
    [SerializeField] private bool showOnStart = true;
    [SerializeField] private bool autoHide = true;
    [SerializeField] private TextMeshProUGUI messageDisplay;

    [Header("Animation Settings")]
    //[SerializeField] private float fadeSpeed = 2f;
    [SerializeField] private bool useFadeAnimation = true;

    private CanvasGroup canvasGroup;
    private float currentDisplayTime = 0f;
    private bool isShowing = false;

    private void Awake()
    {
        if (messageDisplay == null)
            messageDisplay = GetComponent<TextMeshProUGUI>();

        if (messageDisplay == null)
        {
            Debug.LogWarning("TutorialMessage: No TextMeshProUGUI found!");
            return;
        }

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null && useFadeAnimation)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (showOnStart)
            ShowMessage(messageText);
    }

    private void Update()
    {
        if (!isShowing || !autoHide) return;

        currentDisplayTime -= Time.deltaTime;
        if (currentDisplayTime <= 0)
        {
            HideMessage();
        }

        if (useFadeAnimation && canvasGroup != null)
        {
            // Fade in at start, fade out when time is up
            float fadeProgress = 1f - (currentDisplayTime / displayDuration);
            if (fadeProgress < 0.3f)
            {
                canvasGroup.alpha = Mathf.Lerp(0, 1, fadeProgress * 3.33f);
            }
            else if (fadeProgress > 0.7f)
            {
                canvasGroup.alpha = Mathf.Lerp(1, 0, (fadeProgress - 0.7f) * 3.33f);
            }
        }
    }

    public void ShowMessage(string message, float duration = -1f)
    {
        if (messageDisplay == null) return;

        messageDisplay.text = message;
        messageDisplay.gameObject.SetActive(true);
        isShowing = true;

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        currentDisplayTime = duration > 0 ? duration : displayDuration;

        if (useFadeAnimation && canvasGroup != null)
        {
            // Fade in will happen in Update
        }
    }

    public void ShowMessageWithDelay(string message, float delay = 1f, float duration = -1f)
    {
        StartCoroutine(ShowMessageDelayed(message, delay, duration));
    }

    private System.Collections.IEnumerator ShowMessageDelayed(string message, float delay, float duration)
    {
        yield return new WaitForSeconds(delay);
        ShowMessage(message, duration);
    }

    public void HideMessage()
    {
        if (messageDisplay == null) return;

        isShowing = false;
        if (autoHide || !showOnStart)
        {
            messageDisplay.gameObject.SetActive(false);
        }

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }

    public void SetMessage(string message)
    {
        messageText = message;
    }

    public void SetDuration(float duration)
    {
        displayDuration = duration;
    }

    public bool IsShowing() => isShowing;
    public string GetCurrentMessage() => messageDisplay?.text ?? "";
}