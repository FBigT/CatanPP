using UnityEngine;

namespace Catan.Placement
{
    [RequireComponent(typeof(SphereCollider), typeof(MeshRenderer))]
    public class Connector : MonoBehaviour
    {
        [SerializeField] ConnectionType connectionType;

        SphereCollider _collider;
        GameObject _currentStructure;

        /// <summary>The material the prefab originally had (restored after highlighting).</summary>
        public Material OriginalMaterial { get; private set; }

        public float EdgeRotation { get; set; }
        public ConnectionType Connection
        {
            get => connectionType;
            set => connectionType = value;          // **now writable for ConnectionSpawner**
        }

        public bool IsOccupied => _currentStructure != null;

        void Awake()
        {
            _collider = GetComponent<SphereCollider>();
            OriginalMaterial = GetComponent<MeshRenderer>().material;
        }

        /* ------------------------------------------------ validation helpers */

        public bool CanPlaceStructure(GameObject prefab)
        {
            if (IsOccupied || prefab == null) return false;

            return (Connection == ConnectionType.Corner && prefab.CompareTag("Corner")) ||
                   (Connection == ConnectionType.Edge && prefab.CompareTag("Edge"));
        }

        public void PlaceStructure(GameObject prefab)
        {
            if (IsOccupied || prefab == null) return;

            _currentStructure = Instantiate(prefab, transform.position, Quaternion.identity);
        }

        public void RemoveStructure()
        {
            if (!IsOccupied) return;

            Destroy(_currentStructure);
            _currentStructure = null;
        }

        /* ------------------------------------------------ types */

        public enum ConnectionType { Corner, Edge }
    }
}
