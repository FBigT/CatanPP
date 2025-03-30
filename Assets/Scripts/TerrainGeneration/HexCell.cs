using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static HexCell;

public class HexCell : MonoBehaviour
{
    [SerializeField] private HexCell[] neighbors = new HexCell[6];

    public HexCoordinates coordinates;
    public HexCell[] Neighbors => neighbors;

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

    private void OnEnable()
    {
        cell = (HexCell)target;
    }

    private void OnSceneGUI()
    {
        Handles.color = Color.blue;

        foreach (var neighbor in cell.Neighbors)
        {
            if (neighbor != null)
            {
                Handles.DrawLine(cell.transform.position, neighbor.transform.position);
            }
        }
    }
}
#endif