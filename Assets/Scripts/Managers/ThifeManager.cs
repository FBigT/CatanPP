using Assets.Scripts.Dtos;
using Assets.Scripts.Utils;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class ThifeManager : Singleton<ThifeManager>
{
    private bool isPlacingThief = false;
    private bool isRobberMoveInProgress = false;
    private HexTile selectedRobberTile;
    private HexTile currentTile => BoardGen.Instance?.GetCurrentThiefTile();

    public Material dottedLineMaterial;
    public float yOffset = 0.2f;

    private LineRenderer lineRenderer;

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

        lineRenderer.textureMode = LineTextureMode.Tile;
        lineRenderer.numCapVertices = 0;
    }

    private void LateUpdate()
    {
        if (!isPlacingThief) return;

        UpdateThiefLine();

        if (Input.GetMouseButtonDown(1)) // Right-click cancel
        {
            CancelRobberPlacement();
        }

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                HexTile tile = hit.collider.GetComponent<HexTile>();
                if (tile != null && IsValidRobberTile(tile))
                {
                    selectedRobberTile = tile;
                }
            }
        }
    }

    public void EnableThiefPlacement()
    {
        if (!isRobberMoveInProgress)
        {
            _ = RobberMoveSequence();
        }
    }

    private async Task RobberMoveSequence()
    {
        isRobberMoveInProgress = true;
        isPlacingThief = true;
        lineRenderer.enabled = true;

        selectedRobberTile = null;
        HighlightValidRobberTiles();

        while (selectedRobberTile == null)
        {
            await Task.Yield();
        }

        var moveDto = new RobberMoveDto
        {
            originatingTileX = currentTile.xCoord,
            originatingTileY = currentTile.yCoord,
            destinationTileX = selectedRobberTile.xCoord,
            destinationTileY = selectedRobberTile.yCoord
        };

        try
        {
            await WebSocketService.SendRobberMove(moveDto);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Robber move failed: {ex.Message}");
        }

        MoveThief(selectedRobberTile);

        isPlacingThief = false;
        isRobberMoveInProgress = false;

        ClearHighlights();
    }

    private void CancelRobberPlacement()
    {
        isPlacingThief = false;
        isRobberMoveInProgress = false;
        selectedRobberTile = null;
        lineRenderer.enabled = false;
        ClearHighlights();
        DebugMenu.Instance.AppendLog("Thief movement cancelled.");
    }

    public void MoveThief(HexTile tile)
    {
        if (tile == null || tile == BoardGen.Instance.GetCurrentThiefTile()) return;

        BoardGen.Instance.MoveThiefTo(tile);
        lineRenderer.enabled = false;
        isPlacingThief = false;
        isRobberMoveInProgress = false;

        DebugMenu.Instance.AppendLog($"Thief moved to ({tile.Q}, {tile.R})");
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
        return Physics.Raycast(ray, out RaycastHit hit)
            ? hit.point + Vector3.up * yOffset
            : Vector3.zero;
    }

    private void HighlightValidRobberTiles()
    {
        foreach (var tile in BoardGen.Instance.TileList)
        {
            if (IsValidRobberTile(tile))
                tile.Highlight();
        }
    }

    private void ClearHighlights()
    {
        foreach (var tile in BoardGen.Instance.TileList)
            tile.ClearHighlight();
    }

    private bool IsValidRobberTile(HexTile tile)
    {
        return tile != BoardGen.Instance.GetCurrentThiefTile() &&
               tile.resourceType != "desert" &&
               !tile.isWater;
    }
}
