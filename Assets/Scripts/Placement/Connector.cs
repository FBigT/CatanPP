// Assets/Scripts/Placement/Connector.cs
using UnityEngine;

namespace Catan.Placement
{
    [RequireComponent(typeof(SphereCollider), typeof(MeshRenderer))]
    public class Connector : MonoBehaviour
    {
        //───────────────────────────────────────────────
        //  NEW  →  board‑generator fills these once
        //───────────────────────────────────────────────
        [SerializeField] private long tileId;   // id of the hex this connector belongs to
        [SerializeField] private int  index;    // 0‑5 corner or edge slot on that hex

        public long TileId => tileId;
        public int  Index  => index;
        // (If you need write access from the map‑builder, just add “set;” as well.)

        //───────────────────────────────────────────────
        //  Existing fields
        //───────────────────────────────────────────────
        [SerializeField] private ConnectionType connectionType;

        SphereCollider _collider;
        GameObject     _currentStructure;

        /// <summary>The material the prefab originally had (restored after highlighting).</summary>
        public Material OriginalMaterial { get; private set; }

        public float EdgeRotation { get; set; }

        public ConnectionType Connection
        {
            get => connectionType;
            set => connectionType = value;      // writable for ConnectionSpawner
        }

        public bool IsOccupied => _currentStructure != null;

        void Awake()
        {
            _collider        = GetComponent<SphereCollider>();
            OriginalMaterial = GetComponent<MeshRenderer>().material;
        }

        //───────────────────────────────────────────────
        //  Validation helpers
        //───────────────────────────────────────────────
        public bool CanPlaceStructure(GameObject prefab)
        {
            if (IsOccupied || prefab == null) return false;

            return (Connection == ConnectionType.Corner && prefab.CompareTag("Corner")) ||
                   (Connection == ConnectionType.Edge   && prefab.CompareTag("Edge"));
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

        //───────────────────────────────────────────────
        //  Types
        //───────────────────────────────────────────────
        public enum ConnectionType { Corner, Edge }
    }
}
