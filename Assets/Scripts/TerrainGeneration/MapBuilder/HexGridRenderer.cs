using System.Collections.Generic;
using UnityEngine;

public class HexGridRenderer : MonoBehaviour
{
    public GameObject lineRendererPrefab;
    public int width = 5;
    public int height = 5;
    public float radius = 1f;

    private List<LineRenderer> lines = new List<LineRenderer>();

    private void Start()
    {
        GenerateHexGrid();
    }

    void GenerateHexGrid()
    {
        float tileWidth = radius * 2f;
        float tileHeight = Mathf.Sqrt(3f) * radius;
        float xOffset = tileWidth * 0.75f;
        float zOffset = tileHeight;

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                float xPos = x * xOffset;
                float zPos = z * zOffset;

                // Offset every second column
                if (x % 2 == 1)
                    zPos += zOffset / 2f;

                Vector3 center = new Vector3(xPos, 0f, zPos);
                DrawHex(center);
            }
        }
    }

    void DrawHex(Vector3 center)
    {
        Vector3[] corners = new Vector3[7]; // 7 to close the loop

        for (int i = 0; i < 6; i++)
        {
            float angleDeg = 60 * i;
            float angleRad = Mathf.Deg2Rad * angleDeg;
            corners[i] = center + new Vector3(radius * Mathf.Cos(angleRad), 0, radius * Mathf.Sin(angleRad));
        }

        corners[6] = corners[0]; // Close the loop

        var lineObj = Instantiate(lineRendererPrefab, transform);
        var lr = lineObj.GetComponent<LineRenderer>();
        lr.positionCount = corners.Length;
        lr.SetPositions(corners);

        lines.Add(lr);
    }
}
