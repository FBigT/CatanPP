using System.Collections.Generic;
using UnityEngine;

public class EdgePoint : MonoBehaviour
{
    public VertexPoint pointA;
    public VertexPoint pointB;
    public Material roadMaterial;

    public object owner;
    public GameObject roadVisual;

    public List<EdgePoint> connectedEdges = new(); // Used for chaining

    public bool IsConnectedToPlayer(string player)
    {
        // Check if any adjacent structure is owned by the player
        if (pointA?.owner?.ToString() == player || pointB?.owner?.ToString() == player)
            return true;

        // Check if any adjacent road is owned
        foreach (var edge in connectedEdges)
        {
            if (edge != null && edge.owner?.ToString() == player)
                return true;
        }

        return false;
    }

    public void Build(string player)
    {
        if (owner != null) return;

        owner = player;

        RoadGenerator.AlignRoad(this.gameObject, pointA.transform.position, pointB.transform.position, roadMaterial);
    }
}
