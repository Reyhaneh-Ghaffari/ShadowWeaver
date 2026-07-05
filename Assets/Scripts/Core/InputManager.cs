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
        Controls.Enable();
    }

    private void OnDisable()
    {
        Controls.Disable();
    }

    public Vector2 GetMoveInput()
    {
        return Controls.Gameplay.Move.ReadValue<Vector2>();
    }

    public bool GetJumpPressed()
    {
        return Controls.Gameplay.Jump.triggered;
    }

    public bool GetSwitchPressed()
    {
        return Controls.Gameplay.SwitchMode.triggered;
    }

    public bool GetTeleportPressed()
    {
        return Controls.Gameplay.Teleport.triggered;
    }

    public Vector2 GetTeleportDirection()
    {
        return Controls.Gameplay.TeleportDirection.ReadValue<Vector2>();
    }
}