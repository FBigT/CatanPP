using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ConnectionSpawner : MonoBehaviour
{
    [SerializeField] private SO_HexMetrics hexMetrics;
    [SerializeField] private int width = 6;
    [SerializeField] private int height = 6;
    [SerializeField] private HexCell cellPrefab;
    [SerializeField] private Connector connectorPrefab;
    [SerializeField] private string cornerTag, edgeTag;

    private HexCell[] hexCells;
    private Dictionary<Vector3, Connector> connectors = new();

    public HexCell[] HexCells => hexCells;
    public SO_HexMetrics HexMetrics => hexMetrics;
    public int Width => width;
    public int Height => height;

    private void Awake()
    {
        if (string.IsNullOrEmpty(cornerTag))
            cornerTag = "Corner";
        if (string.IsNullOrEmpty(edgeTag))
            edgeTag = "Edge";

        CreateGrid();
    }

    public void CreateGrid()
    {
        DestoryKids();

        hexCells = new HexCell[width * height];
        connectors.Clear();

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
        Vector3 position = new Vector3(
            (x + z * 0.5f - z / 2) * (hexMetrics.InnerRadius * 2f),
            0f,
            z * (hexMetrics.OuterRadius * 1.5f)
        );

        HexCell cell = hexCells[i] = Instantiate(cellPrefab, transform);
        cell.transform.localPosition = position;
        cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
        cell.CellHexMetrics = hexMetrics;
        cell.name = cell.coordinates.ToString();

        if (x > 0) cell.SetNeighbor(HexDirection.W, hexCells[i - 1]);
        if (z > 0)
        {
            if ((z & 1) == 0)
            {
                cell.SetNeighbor(HexDirection.SE, hexCells[i - width]);
                if (x > 0) cell.SetNeighbor(HexDirection.SW, hexCells[i - width - 1]);
            }
            else
            {
                cell.SetNeighbor(HexDirection.SW, hexCells[i - width]);
                if (x < width - 1) cell.SetNeighbor(HexDirection.SE, hexCells[i - width + 1]);
            }
        }

        CreateConnectors(cell);
        AssignCellProperties(cell);
    }

    private void CreateConnectors(HexCell cell)
    {
        Vector3[] corners = hexMetrics.Corners;
        for (int d = 0; d < 6; d++)
        {
            Vector3 edgePosition = cell.transform.position + (corners[d] + corners[(d + 1) % 6]) / 2f;
            Vector3 cornerPosition = cell.transform.position + corners[d];

            if (!connectors.ContainsKey(edgePosition))
            {
                Connector edgeConnector = Instantiate(connectorPrefab, edgePosition, Quaternion.identity, transform);
                edgeConnector.tag = edgeTag;
                edgeConnector.Connection = Connector.ConnectionType.Edge;
                connectors[edgePosition] = edgeConnector;
            }

            if (!connectors.ContainsKey(cornerPosition))
            {
                Connector cornerConnector = Instantiate(connectorPrefab, cornerPosition, Quaternion.identity, transform);
                cornerConnector.tag = cornerTag;
                cornerConnector.Connection = Connector.ConnectionType.Corner;
                connectors[cornerPosition] = cornerConnector;
            }
        }
    }

    private void AssignCellProperties(HexCell cell)
    {
        ResourceType[] availableResources = { ResourceType.Wood, ResourceType.Stone, ResourceType.Wheat, ResourceType.Clay, ResourceType.Sheep };
        ResourceType randomResource = availableResources[Random.Range(0, availableResources.Length)];
        int numberToken = GetRandomNumberToken();

        cell.Initialize(randomResource, numberToken);
    }

    private int GetRandomNumberToken()
    {
        int[] possibleNumbers = { 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 8, 8, 9, 9, 10, 10, 11, 11, 12 };
        return possibleNumbers[Random.Range(0, possibleNumbers.Length)];
    }

    public void DestoryKids()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        hexCells = null;
        connectors.Clear();
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
}
#endif