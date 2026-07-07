using UnityEngine;

public class PlayerState : MonoBehaviour
{
    public enum PlayerMode { Light, Shadow }

    [Header("References")]
    [SerializeField] private GameObject playerBody;
    [SerializeField] private GameObject shadowBody;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private ShadowController shadowController;

    private PlayerMode currentMode = PlayerMode.Light;
    private bool isSwitching = false;

    public System.Action<PlayerMode> OnModeChanged;

    private void Start()
    {
        SetMode(PlayerMode.Light);
        Debug.Log("PlayerState Started! Mode: Light");
    }

    private void Update()
    {
        // ===== سوئیچ حالت با Shift =====
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isSwitching)
        {
            Debug.Log("Shift pressed! Switching mode...");
            StartCoroutine(SwitchMode());
        }

        // ===== مدیریت ورودی‌ها =====
        HandleInputs();
    }

    private void SetMode(PlayerMode newMode)
    {
        currentMode = newMode;
        OnModeChanged?.Invoke(newMode);
    }

    private System.Collections.IEnumerator SwitchMode()
    {
        isSwitching = true;
        Debug.Log($"Switching from {currentMode}...");

        if (currentMode == PlayerMode.Light)
        {
            // ===== از نور به سایه =====
            if (shadowController != null)
            {
                shadowController.DetachFromPlayer();
                shadowController.gameObject.SetActive(true);
                Debug.Log("Shadow detached and activated!");
            }

            if (playerController != null)
            {
                playerController.SetActive(false);  // ← متد جدید
                Debug.Log("PlayerController disabled!");
            }

            currentMode = PlayerMode.Shadow;
            Debug.Log("===切换到 Shadow Mode ===");
        }
        else
        {
            // ===== از سایه به نور =====
            if (shadowController != null)
            {
                Vector3 shadowPos = shadowController.transform.position;

                if (playerController != null)
                {
                    playerController.transform.position = shadowPos;
                    playerController.SetActive(true);  // ← متد جدید
                    Debug.Log("PlayerController enabled!");
                }

                shadowController.AttachToPlayer();
                shadowController.gameObject.SetActive(false);
                Debug.Log("Shadow attached and deactivated!");
            }

            currentMode = PlayerMode.Light;
            Debug.Log("===切换到 Light Mode ===");
        }

        OnModeChanged?.Invoke(currentMode);

        yield return new WaitForSeconds(0.1f);
        isSwitching = false;
    }

    private void HandleInputs()
    {
        if (currentMode == PlayerMode.Light)
        {
            // ورودی‌های Light توسط PlayerController مدیریت میشه
        }
        else if (currentMode == PlayerMode.Shadow)
        {
            if (shadowController == null)
            {
                Debug.LogError("ShadowController is NULL!");
                return;
            }

            // ===== حرکت با WASD =====
            float horizontal = 0f;
            if (Input.GetKey(KeyCode.A)) horizontal = -1f;
            if (Input.GetKey(KeyCode.D)) horizontal = 1f;
            shadowController.SetMoveInput(new Vector2(horizontal, 0f));

            // ===== پرش با Space =====
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("Shadow Jump Pressed!");
                shadowController.Jump();
            }

            // ===== Teleport با E =====
            if (Input.GetKeyDown(KeyCode.E))
            {
                Debug.Log("Shadow Teleport Pressed!");
                shadowController.Teleport(Vector2.right);
            }
        }
    }

    public PlayerMode GetCurrentMode() => currentMode;
    public bool IsInShadowMode() => currentMode == PlayerMode.Shadow;
}