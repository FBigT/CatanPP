using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class DragableObject : MonoBehaviour, IDragable
{
    [SerializeField, Range(0, 100)] private float hoverMultiplier = 1.2f;
    [SerializeField] private float radius = 1f;

    [SerializeField, Min(0)] private float displaceHeight = 2f;
    [SerializeField] private float displaceSmoothing = 5f;

    [SerializeField, Range(0, 90)] private float tiltAngle = 30f;
    [SerializeField] private float tiltSmoothing = 5f;

    [SerializeField] private LayerMask placementLayer;
    [SerializeField] private LayerMask pickUpLayer;

    private bool isDragging = false;
    private Vector3 hoverScale = Vector3.one;
    private SphereCollider hoverCollider;
    private Camera mainCamera;

    private Vector3 targetPosition;
    private Quaternion targetRotation;

    private void Awake()
    {
        SetCollider();
        mainCamera = Camera.main;
        targetPosition = transform.position;
        targetRotation = transform.rotation;
    }

    private void OnValidate() => SetCollider();

    private void Update()
    {
        // Smoothly interpolate position and rotation
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * displaceSmoothing);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * tiltSmoothing);
    }

    private void OnMouseOver() => transform.localScale = hoverScale * hoverMultiplier;

    private void OnMouseExit() => transform.localScale = hoverScale;

    private void OnMouseDown() => OnDragStart();

    private void OnMouseDrag() => OnDrag(InputManager.Instance.MousePosition);

    private void OnMouseUp() => OnDragEnd();

    private void SetCollider()
    {
        if (hoverCollider == null)
            hoverCollider = GetComponent<SphereCollider>();
        hoverCollider.isTrigger = true;
        hoverCollider.radius = radius;
    }

    public void OnDragStart()
    {
        isDragging = true;
        targetPosition = transform.position + new Vector3(0, displaceHeight, 0); // Smooth lift
    }

    public void OnDrag(Vector3 position)
    {
        if (!isDragging) return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, pickUpLayer, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.gameObject == gameObject) return;

            Vector3 newTargetPosition = hit.point + new Vector3(0, displaceHeight, 0);
            Vector3 movementDirection = newTargetPosition - targetPosition;

            // Apply tilt based on movement direction
            if (movementDirection.magnitude > 0.01f)
            {
                Vector3 tiltAxis = Vector3.Cross(movementDirection, Vector3.up).normalized;
                targetRotation = Quaternion.AngleAxis(tiltAngle, tiltAxis) * Quaternion.identity;
            }
            else
            {
                targetRotation = Quaternion.identity;
            }

            targetPosition = newTargetPosition;
        }
    }

    public void OnDragEnd()
    {
        isDragging = false;

        // Find the final placement position
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, placementLayer, QueryTriggerInteraction.Ignore))
        {
            targetPosition = hit.point; // Smoothly move to ground
        }

        targetRotation = Quaternion.identity; // Reset tilt smoothly
    }
}
