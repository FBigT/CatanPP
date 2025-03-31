using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static HexCell;

public class HexCell : MonoBehaviour
{
    [SerializeField] private HexCell[] neighbors = new HexCell[6];
    [SerializeField] private SO_HexMetrics cellHexMetrics;

    public HexCoordinates coordinates;
    public HexCell[] Neighbors => neighbors;
    public SO_HexMetrics CellHexMetrics { get { return cellHexMetrics; } set { cellHexMetrics = value; } }

    public HexCell GetNeighbor(HexDirection direction)
    {
        return neighbors[(int)direction];
    }

    public void SetNeighbor(HexDirection direction, HexCell cell)
    {
        neighbors[(int)direction] = cell;
        cell.neighbors[(int)direction.Opposite()] = this;
    }
}

public enum HexDirection
{
    NE, E, SE, SW, W, NW
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