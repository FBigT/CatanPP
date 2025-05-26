using UnityEngine;

public class DbgMoveThief : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public float heightOffset = 0.2f;
    public LayerMask tileLayer; // Only allow raycasts on hex tiles
    public KeyCode activateKey = KeyCode.T; // Optional key to activate

    private bool isMoving = false;
    private HexTile currentTile;
    private GameObject thief;

    void Start()
    {
        lineRenderer.positionCount = 2;
        lineRenderer.enabled = false;

        // You can get this however you store it
        thief = GameObject.FindWithTag("Respawn");
        if (thief != null)
        {
            currentTile = thief.transform.parent.GetComponent<HexTile>();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(activateKey))
        {
            StartThiefMove();
        }

        if (!isMoving)
            return;

        // Cancel on right click
        if (Input.GetMouseButtonDown(1))
        {
            CancelMove();
            return;
        }

        // Update line
        Vector3 start = currentTile.transform.position + Vector3.up * heightOffset;
        Vector3 mouseWorld = GetMouseWorldPosition();
        Vector3 end = new Vector3(mouseWorld.x, 0, mouseWorld.z) + Vector3.up * heightOffset;

        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);

        // Confirm move on left click
        if (Input.GetMouseButtonDown(0))
        {
            TryMoveThief(mouseWorld);
        }
    }

    void StartThiefMove()
    {
        if (thief == null || currentTile == null)
            return;

        isMoving = true;
        lineRenderer.enabled = true;
    }

    void CancelMove()
    {
        isMoving = false;
        lineRenderer.enabled = false;
    }

    void TryMoveThief(Vector3 worldPos)
    {
        // Raycast to find tile
        Ray ray = new Ray(worldPos + Vector3.up * 10, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 20f, tileLayer))
        {
            HexTile newTile = hit.collider.GetComponentInParent<HexTile>();
            if (newTile != null) // <-- Make sure this method exists
            {
                MoveThiefTo(newTile);
                CancelMove();
            }
        }
    }

    void MoveThiefTo(HexTile newTile)
    {
        if (thief == null || newTile == null || newTile == currentTile)
            return;

        thief.transform.SetParent(newTile.transform);
        thief.transform.localPosition = Vector3.up * 0.1f;
        currentTile = newTile;
    }

    Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (new Plane(Vector3.up, Vector3.zero).Raycast(ray, out float enter))
        {
            return ray.GetPoint(enter);
        }
        return Vector3.zero;
    }
}
