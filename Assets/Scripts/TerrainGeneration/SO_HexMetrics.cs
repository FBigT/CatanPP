using UnityEngine;

[CreateAssetMenu(fileName = "NewHexMetric", menuName = "Hex/Metric")]
public class SO_HexMetrics : ScriptableObject
{
    [SerializeField]
    private float outerRadius = 10f;

    public float innerRadius { get { return outerRadius * 0.866025404f; } }

    public Vector3[] Corners => GenerateCorners();
    public float OuterRadius => outerRadius;

    private Vector3[] GenerateCorners()
    {
        Vector3[] corners = new Vector3[]
        {
            new Vector3(0f, 0f, outerRadius),
            new Vector3(innerRadius, 0f, 0.5f * outerRadius),
            new Vector3(innerRadius, 0f, -0.5f * outerRadius),
            new Vector3(0f, 0f, -outerRadius),
            new Vector3(-innerRadius, 0f, -0.5f * outerRadius),
            new Vector3(-innerRadius, 0f, 0.5f * outerRadius),
            new Vector3(0f, 0f, outerRadius)
        };

        return corners;
    }
}
