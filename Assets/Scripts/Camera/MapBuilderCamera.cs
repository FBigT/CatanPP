using UnityEngine;

public class MapBuilderCamera : MonoBehaviour
{
    [Header("References")]
    public Transform cameraPivot;
    public Camera cam;

    [Header("Settings")]
    public float moveSpeed = 10f;
    public float zoomSpeed = 20f;
    public float minZoom = 10f;
    public float maxZoom = 60f;

    private float currentZoom;

    private void Start()
    {
        if (cam == null)
            cam = Camera.main;

        if (cameraPivot == null)
            cameraPivot = this.transform;

        currentZoom = cam.transform.localPosition.y;
    }

    private void Update()
    {
        HandleMovement();
        HandleZoom();
    }

    private void HandleMovement()
    {
        Vector2 input = InputManager.Instance.MovementInput;
        Vector3 forward = new Vector3(cameraPivot.forward.x, 0, cameraPivot.forward.z).normalized;
        Vector3 right = new Vector3(cameraPivot.right.x, 0, cameraPivot.right.z).normalized;

        Vector3 move = (forward * input.y + right * input.x) * moveSpeed * Time.deltaTime;
        cameraPivot.position += move;
    }

    private void HandleZoom()
    {
        float zoomDelta = InputManager.Instance.ZoomInputDelta;

        if (Mathf.Abs(zoomDelta) > 0.01f)
        {
            currentZoom -= zoomDelta * zoomSpeed * Time.deltaTime;
            currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);

            Vector3 camPos = cam.transform.localPosition;
            cam.transform.localPosition = new Vector3(camPos.x, currentZoom, camPos.z);
        }
    }
}
