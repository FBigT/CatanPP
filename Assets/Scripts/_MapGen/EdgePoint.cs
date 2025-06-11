using System.Collections.Generic;
using UnityEngine;

public class EdgePoint : MonoBehaviour
{
    public VertexPoint pointA;
    public VertexPoint pointB;
    public Material roadMaterial;

    public object owner;
    public GameObject roadVisual;

    [Header("Visual Feedback")]
    public GameObject highlightVisual;

    public List<EdgePoint> connectedEdges = new();

    public HexTile[] adjacentTiles = new HexTile[2];

    // Static flag set from UI (when road is being placed)
    public static bool ShowPlacementHighlights = false;

    void Start()
    {
        SetHighlightVisible(false);
    }

    void Update()
    {
        if (highlightVisual == null) return;

        // Update highlight only if road is not yet built and we're placing a road
        if (owner == null && ShowPlacementHighlights && IsConnectedToPlayer("debug")) // Replace with real player ID
        {
            SetHighlightVisible(true);
        }
        else
        {
            SetHighlightVisible(false);
        }
    }

    public void SetHighlightVisible(bool isVisible)
    {
        if (highlightVisual != null)
            highlightVisual.SetActive(isVisible);
    }

    public bool IsPlaceable()
    {
        return owner == null && (pointA.owner != null || pointB.owner != null);
    }

    public bool IsConnectedToPlayer(string player)
    {
        if ((pointA.owner != null && pointA.owner.Equals(player)) ||
            (pointB.owner != null && pointB.owner.Equals(player)))
        {
            return true;
        }

        foreach (EdgePoint neighbor in pointA.edgePoints)
        {
            if (neighbor != this && neighbor.owner != null && neighbor.owner.Equals(player))
                return true;
        }

        foreach (EdgePoint neighbor in pointB.edgePoints)
        {
            if (neighbor != this && neighbor.owner != null && neighbor.owner.Equals(player))
                return true;
        }

        return false;
    }

    public void Build(string player)
    {
        if (owner != null) return;

        owner = player;
        SetHighlightVisible(false); // Hide visual once built
        RoadGenerator.AlignRoad(this.gameObject, pointA.transform.position, pointB.transform.position, roadMaterial);
    }

    public int GetEdgeIndexRelativeToTile(HexTile tile, EdgePoint edge)
    {
        Vector3 center = tile.transform.position;
        Vector3 toMidpoint = ((edge.pointA.Position + edge.pointB.Position) * 0.5f) - center;
        Vector3[] edgeDirections = GetHexEdgeDirections();

        float marginDegrees = 10f;

        for (int i = 0; i < edgeDirections.Length; i++)
        {
            float angle = Vector3.Angle(toMidpoint.normalized, edgeDirections[i]);
            if (angle <= marginDegrees)
                return i;
        }

        return -1;
    }

    private Vector3[] GetHexEdgeDirections()
    {
        Vector3[] directions = new Vector3[6];
        float[] angles = new float[] { 0f, 60f, 120f, 180f, 240f, 300f };

        for (int i = 0; i < 6; i++)
        {
            float rad = angles[i] * Mathf.Deg2Rad;
            directions[i] = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)).normalized;
        }

        return directions;
    }

}
