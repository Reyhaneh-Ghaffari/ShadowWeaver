using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private static InputManager instance;
    public static InputManager Instance => instance;

    public PlayerControls Controls { get; private set; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Controls = new PlayerControls();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        if (Controls != null)
            Controls.Enable();
    }

    private void OnDisable()
    {
        if (Controls != null)
            Controls.Disable();
    }

    public Vector2 GetMoveInput()
    {
        if (Controls == null) return Vector2.zero;
        return Controls.Gameplay.Move.ReadValue<Vector2>();
    }

    public bool GetJumpPressed()
    {
        if (Controls == null) return false;
        return Controls.Gameplay.Jump.triggered;
    }

    public bool GetSwitchPressed()
    {
        if (Controls == null) return false;
        return Controls.Gameplay.SwitchMode.triggered;
    }

    public bool GetTeleportPressed()
    {
        if (Controls == null) return false;
        return Controls.Gameplay.Teleport.triggered;
    }

    public Vector2 GetTeleportDirection()
    {
        if (Controls == null) return Vector2.zero;
        return Controls.Gameplay.TeleportDirection.ReadValue<Vector2>();
    }
}