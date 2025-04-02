using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class QuadGrid : MonoBehaviour
{
    [SerializeField, Range(0, 8)] private int subdivisions = 0;
    [SerializeField] private Vector3 size = new Vector3(1, 0, 1);
    [SerializeField] private ComputeShader computeShader;

    private Mesh mesh;
    private MeshFilter meshFilter;
    private Vector3[] vertices;
    private int[] triangles;
    private int lastSubdivisions = -1;
    private ComputeBuffer vertexBuffer, updatedVertexBuffer;

    public Vector3[] Vertices => vertices;
    public Vector3 Size { get { return size; } set { size = value; } }

    private void Awake()
    {
        if (meshFilter == null)
            meshFilter = GetComponent<MeshFilter>();

        GenerateGrid();
    }

    private void OnValidate()
    {
        if (meshFilter == null)
            meshFilter = GetComponent<MeshFilter>();

        if (lastSubdivisions != subdivisions)
        {
            lastSubdivisions = subdivisions;
            GenerateGrid();
        }

        Resize();
    }

    public void GenerateGrid()
    {
        mesh = new Mesh();
        mesh.MarkDynamic();
        meshFilter.mesh = mesh;

        int trueSubdivision = (int)Mathf.Pow(2, subdivisions);

        int vertsPerRow = trueSubdivision + 1;
        int totalVerts = vertsPerRow * vertsPerRow;
        int totalQuads = trueSubdivision * trueSubdivision;
        int totalTriangles = totalQuads * 2 * 3;

        vertices = new Vector3[totalVerts];
        triangles = new int[totalTriangles];

        float stepX = size.x / trueSubdivision;
        float stepZ = size.z / trueSubdivision;

        for (int i = 0; i < vertsPerRow; i++)
        {
            for (int j = 0; j < vertsPerRow; j++)
            {
                vertices[i * vertsPerRow + j] = new Vector3(j * stepX, 0, i * stepZ);
            }
        }

        int triIndex = 0;
        for (int i = 0; i < trueSubdivision; i++)
        {
            for (int j = 0; j < trueSubdivision; j++)
            {
                int topLeft = i * vertsPerRow + j;
                int topRight = topLeft + 1;
                int bottomLeft = topLeft + vertsPerRow;
                int bottomRight = bottomLeft + 1;

                triangles[triIndex++] = topLeft;
                triangles[triIndex++] = bottomLeft;
                triangles[triIndex++] = topRight;

                triangles[triIndex++] = topRight;
                triangles[triIndex++] = bottomLeft;
                triangles[triIndex++] = bottomRight;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    public void Resize()
    {
        if (mesh == null || vertices == null || computeShader == null) return;

        int trueSubdivision = (int)Mathf.Pow(2, subdivisions);

        int vertsPerRow = trueSubdivision + 1;
        int totalVerts = vertsPerRow * vertsPerRow;

        vertexBuffer = new ComputeBuffer(totalVerts, sizeof(float) * 3);
        updatedVertexBuffer = new ComputeBuffer(totalVerts, sizeof(float) * 3);

        vertexBuffer.SetData(vertices);

        computeShader.SetInt("vertsPerRow", vertsPerRow);
        computeShader.SetFloat("sizeX", size.x / trueSubdivision);
        computeShader.SetFloat("sizeZ", size.z / trueSubdivision);
        computeShader.SetBuffer(0, "vertices", vertexBuffer);
        computeShader.SetBuffer(0, "updatedVertices", updatedVertexBuffer);

        int threadGroups = Mathf.CeilToInt(totalVerts / 64f);
        computeShader.Dispatch(0, threadGroups, 1, 1);

        updatedVertexBuffer.GetData(vertices);
        mesh.vertices = vertices;
        mesh.RecalculateBounds();

        vertexBuffer.Release();
        updatedVertexBuffer.Release();
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(QuadGrid))]
public class TriangleMakeEditor : Editor
{
    private QuadGrid grid;

    private void OnEnable()
    {
        grid = (QuadGrid)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Generate grid"))
        {
            grid.GenerateGrid();
        }
    }

    private void OnSceneGUI()
    {
        Handles.color = Color.red;

        Vector3 newSize = Handles.FreeMoveHandle(grid.Size + grid.transform.position, .5f, Vector3.zero, Handles.DotHandleCap);
        Handles.DrawDottedLine(newSize, new Vector3(newSize.x, grid.transform.position.y, newSize.z), 2f);

        newSize -= grid.transform.position;

        if (grid.Size != newSize)
        {
            Undo.RecordObject(grid, "Grid resize");
            grid.Size = newSize;
            grid.Resize();
        }
    }

    [DrawGizmo(GizmoType.Selected | GizmoType.Active)]
    private static void OnDrawGizmos(QuadGrid grid, GizmoType gizmo)
    {

    }
}
#endif