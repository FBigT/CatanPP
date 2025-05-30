using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    #region input data
    public Vector2 MovementInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public float ZoomInputDelta { get; private set; }
    public float CameraRotate { get; private set; }
    public bool MiddleMouseButton { get; private set; }
    public Vector2 MouseDelta { get; private set; }
    public bool MouseRight { get; private set; }
    public Vector2 MousePosition { get; private set; }
    #endregion

    #region events
    public static event Action OnLeftMouseClick;
    public static event Action OnLeftMouseRelease;

    public static event Action OnRightMouseClick;
    public static event Action OnRightMouseRelease;
    #endregion

    #region raw input data
    public InputSystem_Actions InputActions => inputActions;
    #endregion


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

        inputActions.Player.OrbitRotate.performed += ctx => CameraRotate = ctx.ReadValue<float>();
        inputActions.Player.OrbitRotate.canceled += ctx => CameraRotate = 0f;

        inputActions.Player.MiddleMouse.performed += ctx => MiddleMouseButton = true;
        inputActions.Player.MiddleMouse.canceled += ctx => MiddleMouseButton = false;

        inputActions.Player.Look.performed += ctx => MouseDelta = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += ctx => MouseDelta = Vector2.zero;

        inputActions.Player.MouseRight.performed += ctx => MouseRight = true;
        inputActions.Player.MouseRight.canceled += ctx => MouseRight = false;

        inputActions.Player.MouseScreenSpacePosition.performed += ctx => MousePosition = ctx.ReadValue<Vector2>();
        inputActions.Player.MouseScreenSpacePosition.canceled += ctx => MousePosition = Vector2.zero;

        //delegates
        inputActions.Player.MouseLeft.performed += ctx => OnLeftMouseClick?.Invoke();
        inputActions.Player.MouseLeft.canceled += ctx => OnLeftMouseRelease?.Invoke();

        inputActions.Player.MouseRight.performed += ctx => OnRightMouseClick?.Invoke();
        inputActions.Player.MouseRight.canceled += ctx => OnRightMouseRelease?.Invoke();
    }

    private void Unsubscribe()
    {
        inputActions.Player.Move.performed -= ctx => MovementInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled -= ctx => MovementInput = Vector2.zero;

        inputActions.Player.Look.performed -= ctx => LookInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled -= ctx => LookInput = Vector2.zero;

        inputActions.Player.Zoom.performed -= ctx => ZoomInputDelta = ctx.ReadValue<Vector2>().y;
        inputActions.Player.Zoom.canceled -= ctx => ZoomInputDelta = 0f;

        inputActions.Player.OrbitRotate.performed -= ctx => CameraRotate = ctx.ReadValue<float>();
        inputActions.Player.OrbitRotate.canceled -= ctx => CameraRotate = 0f;

        inputActions.Player.MiddleMouse.performed -= ctx => MiddleMouseButton = true;
        inputActions.Player.MiddleMouse.canceled -= ctx => MiddleMouseButton = false;

        inputActions.Player.Look.performed -= ctx => MouseDelta = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled -= ctx => MouseDelta = Vector2.zero;

        inputActions.Player.MouseRight.performed -= ctx => MouseRight = true;
        inputActions.Player.MouseRight.canceled -= ctx => MouseRight = false;

        inputActions.Player.MouseScreenSpacePosition.performed -= ctx => MousePosition = ctx.ReadValue<Vector2>();
        inputActions.Player.MouseScreenSpacePosition.canceled -= ctx => MousePosition = Vector2.zero;

        //delegates
        inputActions.Player.MouseLeft.performed -= ctx => OnLeftMouseClick?.Invoke();
        inputActions.Player.MouseLeft.canceled -= ctx => OnLeftMouseRelease?.Invoke();

        inputActions.Player.MouseRight.performed -= ctx => OnRightMouseClick?.Invoke();
        inputActions.Player.MouseRight.canceled -= ctx => OnRightMouseRelease?.Invoke();

    }
}
