using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI.MessageBox;

public class ConnectionSpawner : MonoBehaviour
{
    [SerializeField] private SO_HexMetrics hexMetrics;
    [SerializeField] private int width = 6;
    [SerializeField] private int height = 6;
    [SerializeField] private int radius = 6;
    [SerializeField] private HexCell cellPrefab;

    private HexCell[] hexCells;

    public HexCell[] HexCells => hexCells;
    public SO_HexMetrics HexMetrics => hexMetrics;

    private void Awake()
    {
        CreateGrid();
    }

    public void CreateGrid()
    {
        DestoryKids();

        hexCells = new HexCell[width * height];

        for (int z = 0, i = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                CreateCell(x, z, i++);
            }
        }
    }

    private void CreateCell(int x, int z, int i)
    {
        Vector3 position;
        position.x = (x + z * 0.5f - z / 2) * (hexMetrics.InnerRadius * 2f);
        position.y = 0f;
        position.z = z * (hexMetrics.OuterRadius * 1.5f);


        HexCell cell = hexCells[i] = Instantiate<HexCell>(cellPrefab);
        cell.transform.SetParent(transform, false);
        cell.transform.localPosition = position;
        cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
        cell.CellHexMetrics = hexMetrics;
        cell.name = cell.coordinates.ToString();

        if (x > 0)
        {
            cell.SetNeighbor(HexDirection.W, hexCells[i - 1]);
        }
        if (z > 0)
        {
            if ((z & 1) == 0)
            {
                cell.SetNeighbor(HexDirection.SE, hexCells[i - width]);
                if (x > 0)
                {
                    cell.SetNeighbor(HexDirection.SW, hexCells[i - width - 1]);
                }
            }
            else
            {
                cell.SetNeighbor(HexDirection.SW, hexCells[i - width]);
                if (x < width - 1)
                {
                    cell.SetNeighbor(HexDirection.SE, hexCells[i - width + 1]);
                }
            }
        }
    }

    public void DestoryKids()
    {
        foreach (var item in hexCells)
        {
            if (item != null)
                DestroyImmediate(item.gameObject);
        }
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(ConnectionSpawner))]
public class ConnectionSpawnerEditor : Editor
{
    private ConnectionSpawner spawner;

    private void OnEnable()
    {
        spawner = (ConnectionSpawner)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Generate grid"))
        {
            spawner.CreateGrid();
        }

        if (GUILayout.Button("Destory children"))
        {
            spawner.DestoryKids();
        }
    }

    private void OnSceneGUI()
    {
        Handles.color = Color.green;
        List<Vector3> corners = new List<Vector3>();

        foreach (var item in spawner.HexMetrics.Corners)
        {
            corners.Add(item + spawner.transform.position);
        }

        Handles.DrawPolyLine(corners.ToArray());
    }
}
#endif