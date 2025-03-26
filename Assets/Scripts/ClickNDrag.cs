using UnityEngine;

public class ClickNDrag : MonoBehaviour
{
    [SerializeField] private Transform origin;
    [SerializeField] private LayerMask validPlacementLayer;

    private bool isDragging = false;
    private Camera mainCamera = Camera.main;
    private bool isValidPlacement = false;

    private void Awake()
    {
        if (origin == null)
            origin = transform;
        
    }

    private void Update()
    {
        if (isDragging)
        {
            DragObject();
        }
    }

    private void OnMouseDown()
    {
        isDragging = true;
    }

    private void OnMouseUp()
    {
        isDragging = false;

        // Check if it's over a valid placement area
        if (!isValidPlacement)
        {
            ReturnToOrigin();
        }
    }

    private void DragObject()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            transform.position = hit.point;
            isValidPlacement = ((1 << hit.collider.gameObject.layer) & validPlacementLayer) != 0;
        }
    }

    private void ReturnToOrigin() => transform.position = origin.position;
}
