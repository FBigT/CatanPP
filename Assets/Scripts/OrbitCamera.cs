using UnityEditor;
using UnityEngine;

public class OrbitCamera : MonoBehaviour
{
    #region Camera_Setup
    [Header("Camera Setup")]
    [SerializeField] Transform cameraHolder;
    [SerializeField] Transform playerCamera;
    #endregion

    #region Movement_Settings
    [Header("Movement Settings")]
    [SerializeField, Min(0)] float moveSpeed = 10f;
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

    private float zoomFactor = 1f;
    private float zoomRange = 0f;
    private float rotationRange = 0f;
    private float rotation = 0f;

    private float moveTime = 0f;

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

        if (movementInput.magnitude >= 0.1f)
            moveTime += Time.deltaTime;    
        else   
            moveTime = 0f;
        
        float speed = moveCurve.Evaluate(moveTime);

        Vector3 offset = new Vector3(movementInput.x, 0, movementInput.y) * moveSpeed;

        transform.position = Vector3.Lerp(transform.position, transform.position + offset, Time.deltaTime);
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
        cameraHolder.localRotation = Quaternion.Slerp(cameraHolder.rotation, Quaternion.Euler(rotationLevel, cameraHolder.rotation.y, 0), Time.deltaTime * zoomRotationSmoothing);
    }

    private void RotateCamera()
    {
        //broken
        if (Input.GetKey(KeyCode.E))
        {
            float rotationSpeed = 50f; // You can adjust this value to control the speed of rotation
            float rotationAmount = rotationSpeed * Time.deltaTime;

            //transform.rotation += Quaternion.Euler(0, rotationAmount, 0);

            // Rotate the camera holder around the Y-axis (vertical axis) for camera rotation
            cameraHolder.Rotate(Vector3.up, rotationAmount);
        }
    }

}