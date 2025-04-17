using UnityEngine;

[ExecuteAlways]
public class HexFridVisualizer : MonoBehaviour
{
    public int width = 5;
    public int height = 5;
    public float radius = 1f;
    public Color lineColor = Color.yellow;

    private void OnDrawGizmos()
    {
        Gizmos.color = lineColor;

        float tileWidth = radius * 2;
        float tileHeight = Mathf.Sqrt(3) * radius;

        float xOffset = tileWidth * 0.75f;
        float zOffset = tileHeight;

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                float xPos = x * xOffset;
                float zPos = z * zOffset;

                // Offset every other row
                if (x % 2 == 1)
                    zPos += zOffset / 2f;

                DrawHex(new Vector3(xPos, 0, zPos));
            }
        }
    }

    private void DrawHex(Vector3 center)
    {
        float angleDeg = 60;
        float angleRad = Mathf.Deg2Rad * angleDeg;

        Vector3[] corners = new Vector3[6];
        for (int i = 0; i < 6; i++)
        {
            float angle = angleRad * i;
            corners[i] = center + new Vector3(
                radius * Mathf.Cos(angle),
                0,
                radius * Mathf.Sin(angle)
            );
        }

        for (int i = 0; i < 6; i++)
        {
            Gizmos.DrawLine(corners[i], corners[(i + 1) % 6]);
        }
    }
}
