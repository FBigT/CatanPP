using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class EdgeConnector : MonoBehaviour
{
    [SerializeField] private SO_EdgeMetrics edgeMetrics;
    private SphereCollider sphereCollider;
    private HexCell[] hexCells = new HexCell[2];

    public HexCell[] HexCells => hexCells;

    private void Awake()
    {
        if (sphereCollider == null)
            sphereCollider = GetComponent<SphereCollider>();
        if (edgeMetrics != null)
        {
            sphereCollider.radius = edgeMetrics.ColliderRadius;
        }
    }

    private void OnValidate() => Awake();
}
