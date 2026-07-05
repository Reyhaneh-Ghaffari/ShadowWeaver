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
        // در ابتدا حالت نور هست
        SetMode(PlayerMode.Light);
    }

    private void Update()
    {
        if (InputManager.Instance.GetSwitchPressed() && !isSwitching)
        {
            StartCoroutine(SwitchMode());
        }

        // مدیریت ورودی‌ها بر اساس حالت فعلی
        HandleInputs();
    }

    private System.Collections.IEnumerator SwitchMode()
    {
        isSwitching = true;
        
        // افکت سوئیچ
        AudioManager.Instance?.PlaySFX("SwitchMode");
        
        if (currentMode == PlayerMode.Light)
        {
            // از نور به سایه
            shadowController.DetachFromPlayer();
            currentMode = PlayerMode.Shadow;
            playerController.gameObject.SetActive(false);
            shadowController.gameObject.SetActive(true);
            
            // غیرفعال کردن برخورد جسم
            playerController.gameObject.layer = LayerMask.NameToLayer("Player");
            shadowController.gameObject.layer = LayerMask.NameToLayer("Shadow");
        }
        else
        {
            // از سایه به نور - جسم میاد به سمت سایه
            Vector3 shadowPos = shadowController.transform.position;
            playerController.transform.position = shadowPos;
            
            shadowController.AttachToPlayer();
            currentMode = PlayerMode.Light;
            playerController.gameObject.SetActive(true);
            shadowController.gameObject.SetActive(false);
        }

        OnModeChanged?.Invoke(currentMode);
        
        yield return new WaitForSeconds(0.1f);
        isSwitching = false;
    }

    private void HandleInputs()
    {
        Vector2 moveInput = InputManager.Instance.GetMoveInput();
        
        if (currentMode == PlayerMode.Light)
        {
            // ورودی‌های جسم
            // حرکت به عهده PlayerController هست
        }
        else
        {
            // ورودی‌های سایه
            shadowController.SetMoveInput(moveInput);
            
            if (InputManager.Instance.GetJumpPressed())
            {
                shadowController.Jump();
            }
            
            // پرتاب سایه با Shift
            if (InputManager.Instance.GetTeleportPressed())
            {
                Vector2 direction = InputManager.Instance.GetTeleportDirection();
                shadowController.Teleport(direction.normalized);
            }
        }
    }

    public PlayerMode GetCurrentMode() => currentMode;
    public bool IsInShadowMode() => currentMode == PlayerMode.Shadow;
}