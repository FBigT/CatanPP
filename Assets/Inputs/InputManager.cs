using UnityEngine;

public class InputManager : MonoBehaviour
{
    public Vector2 MovementInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public float ZoomInputDelta { get; private set; }

    private static InputManager instance;
    private InputSystem_Actions inputActions;

    public static InputManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = new GameObject("InputManager");
                instance = obj.AddComponent<InputManager>();
                DontDestroyOnLoad(obj);
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void OnEnable()
    {
        if (inputActions == null)
        {
            inputActions = new InputSystem_Actions();
            inputActions.Enable();
            Subscribe();
        }
        else
        {
           inputActions.Enable();
           Subscribe();
        }
    }

    private void OnDisable()
    {
        if (inputActions != null)
        {
            Unsubscribe();
            inputActions.Disable();
        }
    }

    private void Subscribe()
    {
        inputActions.Player.Move.performed += ctx => MovementInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => MovementInput = Vector2.zero;

        inputActions.Player.Look.performed += ctx => LookInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += ctx => LookInput = Vector2.zero;

        inputActions.Player.Zoom.performed += ctx => ZoomInputDelta = ctx.ReadValue<Vector2>().y;
        inputActions.Player.Zoom.canceled += ctx => ZoomInputDelta = 0f;
    }

    private void Unsubscribe()
    {
        inputActions.Player.Move.performed -= ctx => MovementInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled -= ctx => MovementInput = Vector2.zero;

        inputActions.Player.Look.performed -= ctx => LookInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled -= ctx => LookInput = Vector2.zero;

        inputActions.Player.Zoom.performed -= ctx => ZoomInputDelta = ctx.ReadValue<Vector2>().y;
        inputActions.Player.Zoom.canceled -= ctx => ZoomInputDelta = 0f;
    }
}
