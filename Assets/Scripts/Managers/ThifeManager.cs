using UnityEngine;
using Assets.Scripts.Utils;

public class ThifeManager : Singleton<ThifeManager>
{
    public BoardGen BoardGen;

    private bool isPlacingThief = false;
    private HexTile currentTile => BoardGen?.GetCurrentThiefTile();
    private LineRenderer lineRenderer;

    public Material dottedLineMaterial;
    public float yOffset = 0.2f;

    protected override void Awake()
    {
        base.Awake();

        GameObject lrObj = new GameObject("ThiefLineRenderer");
        lrObj.transform.SetParent(transform);
        lineRenderer = lrObj.AddComponent<LineRenderer>();

        lineRenderer.material = dottedLineMaterial;
        lineRenderer.startWidth = 0.5f;
        lineRenderer.endWidth = 0.5f;
        lineRenderer.positionCount = 2;
        lineRenderer.enabled = false;

        // Set this if your material needs it
        lineRenderer.textureMode = LineTextureMode.Tile;
        lineRenderer.numCapVertices = 0;
    }

    private void Start()
    {
        if (BoardGen == null)
        {
            BoardGen = FindObjectOfType<BoardGen>();
            if (BoardGen == null)
            {
                Debug.LogError("BoardGen not found in the scene. Please ensure it is present.");
            }
        }
    }

    public void EnableThiefPlacement()
    {
        isPlacingThief = true;
        lineRenderer.enabled = true;
    }

    private void LateUpdate()
    {
        if (!isPlacingThief)
        {
            lineRenderer.enabled = false;
            return;
        }

        UpdateThiefLine();

        if (Input.GetMouseButtonDown(1)) // Right click to cancel
        {
            isPlacingThief = false;
            lineRenderer.enabled = false;
            DebugMenu.Instance.AppendLog("Thief movement cancelled.");
        }

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                HexTile tile = hit.collider.GetComponent<HexTile>();
                if (tile != null)
                {
                    MoveThief(tile);
                }
            }
        }
    }

    private void UpdateThiefLine()
    {
        if (currentTile == null)
        {
            lineRenderer.enabled = false;
            return;
        }

        Vector3 start = currentTile.transform.position + Vector3.up * yOffset;
        Vector3 end = GetMouseProjectedPosition();

        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
    }

    private Vector3 GetMouseProjectedPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
            return hit.point + Vector3.up * yOffset;
        return Vector3.zero;
    }

    public void MoveThief(HexTile tile)
    {
        BoardGen.MoveThiefTo(tile);
        isPlacingThief = false;
        lineRenderer.enabled = false;

        DebugMenu.Instance.AppendLog($"thief moved to ({tile.Q}, {tile.R})");
    }
}
