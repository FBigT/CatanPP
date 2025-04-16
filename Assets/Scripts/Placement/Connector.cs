using UnityEngine;

namespace Catan.Placement
{
    [RequireComponent(typeof(SphereCollider), typeof(MeshRenderer))]
    public class Connector : MonoBehaviour
    {
        [SerializeField] private ConnectionType connectionType;

        private SphereCollider sphereCollider;
        private GameObject currentStructure;
        private float edgeRotation;

        /// <summary>
        /// Store the original MeshRenderer material so we can restore it.
        /// </summary>
        public Material OriginalMaterial { get; private set; }

        public float EdgeRotation { get => edgeRotation; set => edgeRotation = value; }
        public ConnectionType Connection { get => connectionType; set => connectionType = value; }
        public bool IsOccupied => currentStructure != null;

        private void Awake()
        {
            sphereCollider = GetComponent<SphereCollider>();
            OriginalMaterial = GetComponent<MeshRenderer>().material;
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
            var placed = Instantiate(structurePrefab, transform.position, Quaternion.identity);
            currentStructure = placed;
        }

        public void RemoveStructure()
        {
            if (!IsOccupied) return;
            Destroy(currentStructure);
            currentStructure = null;
        }

        public enum ConnectionType
        {
            Corner,
            Edge
        }
    }
}
