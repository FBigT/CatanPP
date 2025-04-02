using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class CornerConnector : MonoBehaviour
{
    [SerializeField] private SO_CornerMetrics cornerMetrics;
    private SphereCollider sphereCollider;
    private GameObject currentStructure;

    public bool IsOccupied => currentStructure != null;

    private void Awake()
    {
        if (sphereCollider == null)
            sphereCollider = GetComponent<SphereCollider>();
        if (cornerMetrics != null)
        {
            sphereCollider.radius = cornerMetrics.ColliderRadius;
        }
    }

    private void OnValidate() => Awake();



    public bool CanPlaceStructure(GameObject structure)
    {
        if (IsOccupied) return false; // Prevent multiple buildings

        if (structure.CompareTag("Road") && gameObject.CompareTag("Edge"))
            return true;
        if (!structure.CompareTag("Road") && gameObject.CompareTag("Corner"))
            return true;

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
}
