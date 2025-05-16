// Assets/Scripts/TerrainGeneration/HexCell.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// A single hex-tile of the board.  
/// Holds its axial <see cref="coordinates"/> and run-time data such as
/// resource type & number token.  
/// **Does not** re-declare ResourceType / HexDirection – those now live in
/// ResourceType.cs to avoid duplicate-type errors.
/// </summary>
[RequireComponent(typeof(MeshRenderer))]
public class HexCell : MonoBehaviour
{
    [SerializeField] HexCell[] neighbors = new HexCell[6];
    [SerializeField] SO_HexMetrics cellHexMetrics;

    [Header("Gameplay")]
    [SerializeField] ResourceType resourceType;
    [SerializeField] int numberToken = 0;

    // ───────────────────────────── stored references
    MeshRenderer _renderer;

    // ───────────────────────────── public accessors
    public HexCoordinates coordinates;
    public HexCell[] Neighbors => neighbors;
    public SO_HexMetrics CellHexMetrics
    {
        get => cellHexMetrics;
        set => cellHexMetrics = value;
    }

    // ───────────────────────────── life-cycle
    void Awake()
    {
        if (!_renderer) _renderer = GetComponent<MeshRenderer>();
        ApplyColor();                     // show colour at play-time as well
    }

    void OnValidate()                     // editor updates
    {
        if (!_renderer) _renderer = GetComponent<MeshRenderer>();
        ApplyColor();
    }

    // ───────────────────────────── API used by map-gen
    public void Initialize(ResourceType type, int number, Material mat = null)
    {
        resourceType = type;
        numberToken = number;
        if (!_renderer) _renderer = GetComponent<MeshRenderer>();

        if (mat != null)
            _renderer.sharedMaterial = mat;
            
        ApplyColor();
    }

    public HexCell GetNeighbor(HexDirection dir) => neighbors[(int)dir];

    public void SetNeighbor(HexDirection dir, HexCell cell)
    {
        neighbors[(int)dir] = cell;
        cell.neighbors[(int)dir.Opposite()] = this;
    }

    public void DisplayCellInfo() =>
        Debug.Log($"Cell {coordinates}: {resourceType}  #{numberToken}");

    // ───────────────────────────── helpers
    void ApplyColor()
    {

    }
}

/* ──────────────────────────────────────────────────────────────
   OPTIONAL gizmo helper – keep it or delete it; but it does NOT
   create duplicate types.
──────────────────────────────────────────────────────────────── */
#if UNITY_EDITOR
[CustomEditor(typeof(HexCell))]
public class HexCellEditor : Editor
{
    [SerializeField, Range(10, 200)]
    float drawDistance = 50f;

    HexCell _cell;

    void OnEnable() => _cell = (HexCell)target;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        drawDistance = EditorGUILayout.Slider("Draw Distance",
                                              drawDistance, 10f, 200f);
    }

    void OnSceneGUI()
    {
        if (!_cell || !_cell.CellHexMetrics) return;

        Camera cam = SceneView.lastActiveSceneView?.camera;
        if (!cam) return;

        if (Vector3.Distance(cam.transform.position, _cell.transform.position)
            > drawDistance) return;

        // Draw hex outline
        Handles.color = Color.green;
        var corners = _cell.CellHexMetrics.Corners;
        var world = new Vector3[corners.Length];
        for (int i = 0; i < corners.Length; i++)
            world[i] = _cell.transform.position + corners[i];
        Handles.DrawPolyLine(world);

        // Label with coords
        Handles.Label(_cell.transform.position + Vector3.up * 0.2f,
                      _cell.coordinates.ToString());
    }
}
#endif
