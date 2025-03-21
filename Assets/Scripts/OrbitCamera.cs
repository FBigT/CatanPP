using UnityEditor;
using UnityEngine;

public class OrbitCamera : MonoBehaviour
{
    #region Camera_Setup
    [Header("Camera Setup")]
    [SerializeField] Transform cameraHolder;
    #endregion

    #region Movement_Settings
    [Header("Movement Settings")]
    [SerializeField, Min(0)] float moveSpeed = 10f;
    [SerializeField] private float dragSpeed = 1f;
    [SerializeField] AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    #endregion

    #region Zoom_Settings
    [Header("Zoom Settings")]
    [SerializeField, Min(0)] float minZoom = 7f;
    [SerializeField, Min(0)] float maxZoom = 20f;
    [SerializeField, Min(0)] float zoomSpeed = 20f;
    [SerializeField, Range(1, 20)] float zoomSmoothing = 10f;
    [SerializeField] AnimationCurve zoomCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    #endregion

    #region Zoom_Rotation_Settings
    [Header("Zoom Rotation Settings")]
    [SerializeField, Min(0)] float minZoomRotation = 45f;
    [SerializeField, Min(0)] float maxZoomRotation = 55f;
    [SerializeField, Range(1, 20)] float zoomRotationSmoothing = 10f;
    [SerializeField] AnimationCurve rotationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    #endregion

    #region Rotation_Settings
    [Header("Rotation Settings")]
    [SerializeField, Min(0)] private float rotationSpeed = 10f;
    [SerializeField, Min(0)] private float mouseRotationSpeed = 0.2f;
    #endregion

    private float zoomFactor = 1f;
    private float zoomRange = 0f;
    private float rotationRange = 0f;

    private float moveTime = 0f;

    private Vector3 velocity = Vector3.zero;

    private void Awake()
    {
        zoomRange = maxZoom - minZoom;
        rotationRange = maxZoomRotation - minZoomRotation;

        if (cameraHolder == null)
            cameraHolder = GetComponentInChildren<Transform>();
        
    }

    private void OnValidate()
    {
        zoomRange = maxZoom - minZoom;
        rotationRange = maxZoomRotation - minZoomRotation;

        if(cameraHolder != null)
            CalculateZoom();
    }

    private void Update()
    {
        MoveCamera();
        ZoomCamera();
        RotateCamera();
    }

    private void MoveCamera()
    {
        Vector2 movementInput = InputManager.Instance.MovementInput;
        bool isDragging = InputManager.Instance.MouseRight;
        Vector2 mouseDelta = InputManager.Instance.MouseDelta;

        if (movementInput.magnitude >= 0.1f)
            moveTime += Time.deltaTime;
        else
            moveTime = 0f;

        float speed = moveCurve.Evaluate(moveTime) * moveSpeed;

        Vector3 forward = cameraHolder.forward;
        Vector3 right = cameraHolder.right;
        Vector3 moveDirection = (forward * movementInput.y + right * movementInput.x);
        moveDirection = Vector3.ProjectOnPlane(moveDirection, Vector3.up).normalized;

        Vector3 targetOffset = moveDirection * speed;
        
        if (isDragging)
        {
            Vector3 dragOffset = (-right * mouseDelta.x - forward * mouseDelta.y);
            dragOffset = Vector3.ProjectOnPlane(dragOffset, Vector3.up).normalized * dragSpeed;
            targetOffset += dragOffset;
        }
        
        // komentiranije je bolji od gornjeg, ali je broken
        /*
        if (isDragging)
        {
            Camera cam = Camera.main;

            Vector3 screenMovement = new Vector3(mouseDelta.x, 0, mouseDelta.y);
            Vector3 worldMovement = cam.ScreenToWorldPoint(transform.position + screenMovement) - cam.ScreenToWorldPoint(transform.position);

            worldMovement.y = 0;
            targetOffset -= worldMovement;
        }
        */
        Vector3 targetPosition = transform.position + targetOffset;

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, 0.2f);
    }

    private void ZoomCamera()
    {
        float scrollInput = InputManager.Instance.ZoomInputDelta;
        if (scrollInput != 0)
        {
            zoomFactor -= scrollInput / zoomSpeed;
            zoomFactor = Mathf.Clamp01(zoomFactor);
        }

        CalculateZoom();
    }

    private void CalculateZoom()
    {
        float zoomValue = zoomCurve.Evaluate(zoomFactor);
        float zoomLevel = zoomRange * zoomValue + minZoom;
        cameraHolder.localPosition = Vector3.Lerp(cameraHolder.localPosition, new Vector3(0, zoomLevel, -zoomLevel), Time.deltaTime * zoomSmoothing);

        float rotationValue = rotationCurve.Evaluate(zoomFactor);
        float rotationLevel = rotationRange * rotationValue + minZoomRotation;
        cameraHolder.localRotation = Quaternion.Slerp(cameraHolder.localRotation, Quaternion.Euler(rotationLevel, cameraHolder.localRotation.y, 0), Time.deltaTime * zoomRotationSmoothing);
    }

    private void RotateCamera()
    {
        float rotateInput = -InputManager.Instance.CameraRotate;
        bool rotateBinding = InputManager.Instance.MiddleMouseButton;

        if (Mathf.Abs(rotateInput) > 0.01f)
        {
            float rotationAmount = rotateInput * rotationSpeed * Time.deltaTime;
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y + rotationAmount, 0);
        }
        if (rotateBinding)
        {
            Vector2 mouseDelta = InputManager.Instance.MouseDelta;
            float mouseRotationAmount = mouseDelta.x * mouseRotationSpeed;

            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y + mouseRotationAmount, 0);
        }
    }
}