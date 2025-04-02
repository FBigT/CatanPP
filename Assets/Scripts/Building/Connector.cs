using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class Connector : MonoBehaviour
{
    [SerializeField] private ConnectionType connectionType;

    private SphereCollider sphereCollider;
    private GameObject currentStructure;
    private float edgeRotation;

    public float EdgeRotation { get { return edgeRotation; } set { edgeRotation = value; } }
    public ConnectionType Connection { get { return connectionType; } set { connectionType = value; } }
    public bool IsOccupied => currentStructure != null;

    private void Awake()
    {
        if (sphereCollider == null)
            sphereCollider = GetComponent<SphereCollider>();
    }

    public bool CanPlaceStructure(GameObject structure)
    {
        if (IsOccupied) return false;

        if (connectionType == ConnectionType.Corner && structure.CompareTag("Corner")) return true;
        if (connectionType == ConnectionType.Edge && structure.CompareTag("Edge")) return true;

        return false;
    }

    public void PlaceStructure(GameObject structurePrefab)
    {
        if (IsOccupied) return;

        GameObject placedStructure = Instantiate(structurePrefab, transform.position, Quaternion.identity);
        currentStructure = placedStructure;
    }

    public void RemoveStructure()
    {
        if (!IsOccupied) return;

        Destroy(currentStructure);
        currentStructure = null;
    }

    [System.Serializable]
    public enum ConnectionType
    {
        Corner,
        Edge
    }
}
