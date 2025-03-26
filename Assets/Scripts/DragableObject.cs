using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class DragableObject : MonoBehaviour, IDragable
{
    [SerializeField, Range(0, 100)] private float hoverMultiplyer = 1.2f;
    [SerializeField] private float radius = 1f;

    [SerializeField, Min(0)] private float displaceHeight = 2f;
    [SerializeField] private float displaceSmoothing = 1f;

    [SerializeField, Range(0, 90)] private float tiltAngle = 30f;
    [SerializeField] private float tiltSmoothing = 1f;

    [SerializeField] private LayerMask placementLayer;
    [SerializeField] private LayerMask pickUpLayer;

    private bool isDragging = false;
    private Vector3 hoverScale = Vector3.one;
    private SphereCollider hoverCollider;
    private Camera mainCamera;

    private void Awake()
    {
        SetCollider();
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    private void OnValidate() => SetCollider();

    private void OnMouseOver() => transform.localScale = hoverScale * hoverMultiplyer;

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

    public void OnDragStart() => isDragging = true;

    public void OnDrag(Vector3 position)
    {
        if (!isDragging) return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, pickUpLayer, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.gameObject == gameObject) return;

            transform.position = hit.point + new Vector3(0, displaceHeight, 0);
        }
    }

    public void OnDragEnd() => isDragging = false;
}
