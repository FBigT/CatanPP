// Assets/Scripts/TerrainGeneration/ConnectionSpawner.cs
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Catan.Placement;

public class ConnectionSpawner : MonoBehaviour
{
    [SerializeField] private SO_HexMetrics hexMetrics;
    [SerializeField] private int width = 6;
    [SerializeField] private int height = 6;
    [SerializeField] private HexCell cellPrefab;
    [SerializeField] private Connector connectorPrefab;
    [SerializeField] private string cornerTag = "Corner", edgeTag = "Edge";

    private HexCell[] hexCells;
    private Dictionary<Vector3, Connector> connectors = new();

    // possible Catan resources
    private static readonly ResourceType[] ResourceTypes = new[]
    {
        ResourceType.Lumber,
        ResourceType.Wool,
        ResourceType.Grain,
        ResourceType.Brick,
        ResourceType.Ore
    };
    // standard Catan token distribution (2–12, excluding 7)
    private static readonly int[] NumberTokens = new[]
    { 2,3,3,4,4,5,5,6,6,8,8,9,9,10,10,11,11,12 };

    public HexCell[] HexCells => hexCells;
    public SO_HexMetrics HexMetrics => hexMetrics;
    public int Width => width;
    public int Height => height;

    private void Awake()
    {
        CreateGrid();
    }

    public void CreateGrid()
    {
        DestroyChildren();
        hexCells = new HexCell[width * height];
        connectors.Clear();

        for (int z = 0, i = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
                CreateCell(x, z, i++);
        }
    }

    private void CreateCell(int x, int z, int i)
    {
        Vector3 pos = new Vector3(
            (x + z * 0.5f - z / 2) * (hexMetrics.InnerRadius * 2f),
            0f,
            z * (hexMetrics.OuterRadius * 1.5f)
        );

        var cell = hexCells[i] = Instantiate(cellPrefab, transform);
        cell.transform.localPosition = pos;
        cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
        cell.CellHexMetrics = hexMetrics;
        cell.name = $"Cell {cell.coordinates}";

        CreateConnectors(cell);
        AssignCellProperties(cell);
    }

    private void CreateConnectors(HexCell cell)
    {
        Vector3[] corners = hexMetrics.Corners;
        for (int d = 0; d < 6; d++)
        {
            Vector3 edgePos = cell.transform.position + (corners[d] + corners[(d + 1) % 6]) * 0.5f;
            Vector3 cornerPos = cell.transform.position + corners[d];

            if (!connectors.ContainsKey(edgePos))
            {
                var edgeC = Instantiate(connectorPrefab, edgePos, Quaternion.identity, transform);
                edgeC.tag = edgeTag;
                edgeC.Connection = Connector.ConnectionType.Edge;
                connectors[edgePos] = edgeC;
            }

            if (!connectors.ContainsKey(cornerPos))
            {
                var cornerC = Instantiate(connectorPrefab, cornerPos, Quaternion.identity, transform);
                cornerC.tag = cornerTag;
                cornerC.Connection = Connector.ConnectionType.Corner;
                connectors[cornerPos] = cornerC;
            }
        }
    }

    private void AssignCellProperties(HexCell cell)
    {
        // Randomly pick a resource type and number token for each hex
        var type = ResourceTypes[Random.Range(0, ResourceTypes.Length)];
        var token = NumberTokens[Random.Range(0, NumberTokens.Length)];
        cell.Initialize(type, token);
    }

    public void DestroyChildren()
    {
        // clear old children
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);
        hexCells = null;
        connectors.Clear();
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(ConnectionSpawner))]
    public class ConnectionSpawnerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Generate grid"))
                (target as ConnectionSpawner).CreateGrid();
            if (GUILayout.Button("Destroy children"))
                (target as ConnectionSpawner).DestroyChildren();
        }
    }
#endif
}
