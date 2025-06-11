using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Enums;
using Catan.GameMode;

public class VertexPoint : MonoBehaviour
{
    public Vector3 Position => transform.position;

    public object owner;
    public List<HexTile> nearbyTiles = new();
    public List<EdgePoint> edgePoints = new();

    public StructureType type = StructureType.NONE;

    private void Update()
    {
        OrientStructureToTileHeights();
    }

    public void Build(StructureType structureType)
    {
        type = structureType;
        owner = "debug";

        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(child.name.ToUpper() == structureType.ToString());

            var a = child.gameObject.GetComponent<PlayerMarker>();
            if (a != null)
            {
                a.SetColorForThisStructure(Color.cyan);
            }
        }

        Debug.Log($"Built: {structureType} at {name}");
    }

    public void Build(StructureType structureType, object player)
    {
        type = structureType;
        owner = (string)player;

        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(child.name.ToUpper() == structureType.ToString());
        }

        Debug.Log($"Built: {structureType} at {name}");
    }

    public void OrientStructureToTileHeights()
    {
        if (nearbyTiles == null || nearbyTiles.Count == 0)
        {
            Debug.LogWarning($"[VertexPoint] No nearby tiles to orient {name}");
            return;
        }

        List<Vector3> positions = new();

        foreach (var tile in nearbyTiles)
        {
            if (tile != null)
                positions.Add(tile.transform.position);
        }

        if (positions.Count <= 1) return;

        Vector3 normal;

        if (positions.Count == 2)
        {
            Vector3 a = positions[0] - transform.position;
            Vector3 b = positions[1] - transform.position;
            normal = Vector3.Cross(a, b).normalized;
        }
        else
        {
            Vector3 a = positions[1] - positions[0];
            Vector3 b = positions[2] - positions[0];
            normal = Vector3.Cross(a, b).normalized;
        }

        if (normal.y < 0) normal = -normal;

        transform.up = normal;
    }

    public int GetNeighborVertexIndex(HexTile tile)
    {
        if (nearbyTiles == null || nearbyTiles.Count == 0)
            return -1;

        if (tile == null)
            return -1;

        Vector3 center = tile.transform.position;

        Vector3 toVertex = (transform.position - center).normalized;

        Vector3[] referenceVectors = GetTileVertexDirections();

        float marginDegrees = 10f;

        for (int i = 0; i < referenceVectors.Length; i++)
        {
            float angle = Vector3.Angle(toVertex, referenceVectors[i]);
            if (angle <= marginDegrees)
                return i + 1;

        }
        return -1;
    }

    private Vector3[] GetTileVertexDirections()
    {
        Vector3[] directions = new Vector3[6];

        float[] angles = new float[] { 30f, 90f, 150f, 210f, 270f, 330f };

        for (int i = 0; i < 6; i++)
        {
            float rad = angles[i] * Mathf.Deg2Rad;
            directions[i] = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)).normalized;
        }

        return directions;
    }

}
