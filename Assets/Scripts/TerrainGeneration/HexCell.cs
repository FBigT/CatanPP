using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class HexCell : MonoBehaviour
{
    [SerializeField] private HexCell[] neighbors = new HexCell[6];
    [SerializeField] private SO_HexMetrics cellHexMetrics;
    [SerializeField] private ResourceType resourceType;
    [SerializeField] private int numberToken;

    private MeshRenderer meshRenderer;

    public HexCoordinates coordinates;
    public HexCell[] Neighbors => neighbors;
    public SO_HexMetrics CellHexMetrics { get { return cellHexMetrics; } set { cellHexMetrics = value; } }

    private void Awake()
    {
        if (meshRenderer == null)
            meshRenderer = GetComponent<MeshRenderer>();
    }

    public void Initialize(ResourceType type, int number)
    {
        resourceType = type;
        numberToken = number;

        if (meshRenderer == null)
            meshRenderer = GetComponent<MeshRenderer>();
    }

    public void DisplayCellInfo()
    {
        Debug.Log($"Cell at {coordinates} has resource: {resourceType} and number: {numberToken}");
    }

    public HexCell GetNeighbor(HexDirection direction)
    {
        return neighbors[(int)direction];
    }

    public void SetNeighbor(HexDirection direction, HexCell cell)
    {
        neighbors[(int)direction] = cell;
        cell.neighbors[(int)direction.Opposite()] = this;
    }

    private void ApplyColor()
    {
        if (Application.isPlaying) return;
        
        if (meshRenderer == null || meshRenderer.sharedMaterial == null) return;

        Color resourceColor = GetResourceColor(resourceType);

        meshRenderer.material.color = resourceColor;
    }


    private Color GetResourceColor(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Wood: return new Color(0.2f, 0.6f, 0.2f);
            case ResourceType.Stone: return Color.gray;
            case ResourceType.Wheat: return new Color(0.9f, 0.8f, 0.2f);
            case ResourceType.Clay: return new Color(0.8f, 0.3f, 0.2f);
            case ResourceType.Sheep: return new Color(0.6f, 1.0f, 0.6f);
            case ResourceType.Desert: return new Color(1.0f, 1.0f, 0.5f);
            default: return Color.white;
        }
    }
}

public enum HexDirection
{
    NE, E, SE, SW, W, NW
}

public enum ResourceType
{
    Wood,
    Stone,
    Wheat,
    Clay,
    Sheep,
    Desert
}

public static class HexDirectionExtensions
{
    public static HexDirection Opposite(this HexDirection direction)
    {
        return (int)direction < 3 ? (direction + 3) : (direction - 3);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(HexCell))]
public class HexCellEditor : Editor
{
    private static HexCell cell;
    [SerializeField, Range(10, 200)] private float drawDistance = 50f;

    private void OnEnable()
    {
        cell = (HexCell)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        drawDistance = EditorGUILayout.Slider("Draw Distance", drawDistance, 10f, 200f);
    }

    private void OnSceneGUI()
    {
        if (cell == null) return;
        if (cell.Neighbors == null || cell.Neighbors.Length == 0) return;
        if (cell.CellHexMetrics == null) return;

        foreach (var neighbor in cell.Neighbors)
            if (neighbor != null)
                DrawHexagon(neighbor, Color.blue);

        DrawHexagon(cell, Color.green);
        DrawData();
    }

    private void DrawHexagon(HexCell targetCell, Color color)
    {
        Handles.color = color;
        List<Vector3> corners = new List<Vector3>();

        foreach (var corner in targetCell.CellHexMetrics.Corners)
        {
            corners.Add(corner + targetCell.transform.position);
        }

        Handles.DrawPolyLine(corners.ToArray());
    }

    private void DrawData()
    {
        Camera sceneCam = SceneView.lastActiveSceneView.camera;
        if (sceneCam == null) return;

        float distance = Vector3.Distance(sceneCam.transform.position, cell.transform.position);

        if (distance < drawDistance)
        {
            int validNeighborCount = 0;

            foreach (var neighbor in cell.Neighbors)
                if (neighbor != null)
                    validNeighborCount++;

            string data =
                "Coordinates: " + cell.coordinates.ToString() +
                "\n" +
                "Neigbours: " + validNeighborCount;

            Handles.Label(cell.transform.position, data);
        }
    }
}
#endif