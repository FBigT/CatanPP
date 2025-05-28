using Assets.Scripts.Enums;
using Catan.Managers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Catan.UI
{
    public class PuchaseUIManager : MonoBehaviour
    {
        [Header("Purchase Entries")]
        [Tooltip("Assign PurchaseType with corresponding Button and Prefab")]
        public List<PurchaseEntry> purchaseEntries;

        [Header("Purchase Manager")]
        public PurchaseManagerLocal purchaseManager;

        [Header("Layer Masks")]
        public LayerMask buildingLayerMask = 1;
        public LayerMask roadLayerMask = 1;

        [Header("Cursor")]
        public GameObject cursor3DPrefab;           // Prefab to instantiate
        public float cursorYLevel = 0.05f;          // Y plane height for cursor placement

        private GameObject activeCursor;            // Instantiated cursor object
        private Dictionary<PurchaseType, (Button button, GameObject prefab)> purchaseDict;

        private PurchaseType currentPurchaseType;
        private bool isPlacingStructure = false;

        void Awake()
        {
            purchaseDict = new Dictionary<PurchaseType, (Button, GameObject)>();
            foreach (var entry in purchaseEntries)
            {
                if (entry.button == null || entry.prefab == null)
                {
                    Debug.LogWarning($"PurchaseEntry for {entry.type} is missing button or prefab.");
                    continue;
                }

                purchaseDict[entry.type] = (entry.button, entry.prefab);
                entry.button.onClick.AddListener(() => TryPurchase(entry.type));
            }
        }

        void TryPurchase(PurchaseType type)
        {
            if (isPlacingStructure) return;

            if (purchaseManager == null)
            {
                Debug.LogError("PurchaseManager not assigned!");
                return;
            }

            if (!purchaseManager.HasEnoughFor(type))
            {
                Debug.Log("Not enough resources.");
                return;
            }

            if (!purchaseDict.TryGetValue(type, out var data))
            {
                Debug.LogError($"No purchase data found for type {type}");
                return;
            }

            currentPurchaseType = type;
            isPlacingStructure = true;

            EdgePoint.ShowPlacementHighlights = (type == PurchaseType.Road);

            if (cursor3DPrefab != null)
            {
                if (activeCursor != null)
                    Destroy(activeCursor); // Cleanup previous instance

                activeCursor = Instantiate(cursor3DPrefab);
            }
        }

        void Update()
        {
            if (!isPlacingStructure) return;

            if (activeCursor != null)
                activeCursor.transform.position = GetMousePlanePosition();

            if (Input.GetMouseButtonDown(1))
                CancelPlacement();

            if (Input.GetMouseButtonDown(0))
            {
                if (TryPlaceStructure())
                {
                    purchaseManager.SpendResources(currentPurchaseType);
                    isPlacingStructure = false;

                    if (activeCursor != null)
                        Destroy(activeCursor);
                }
            }
        }

        Vector3 GetMousePlanePosition()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane plane = new Plane(Vector3.up, new Vector3(0, cursorYLevel, 0));
            if (plane.Raycast(ray, out float enter))
            {
                return ray.GetPoint(enter);
            }
            return Vector3.zero;
        }

        bool TryPlaceStructure()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            switch (currentPurchaseType)
            {
                case PurchaseType.Road:
                    if (!Physics.Raycast(ray, out RaycastHit hitRoad, roadLayerMask)) return false;

                    EdgePoint ep = hitRoad.collider.GetComponent<EdgePoint>();
                    if (ep == null)
                    {
                        Debug.Log("Invalid road target.");
                        return false;
                    }

                    if (StructureManager.Instance.TryPlaceRoad(ep, "debug"))
                    {
                        EdgePoint.ShowPlacementHighlights = false;
                        return true;
                    }
                    return false;

                case PurchaseType.Settlement:
                case PurchaseType.City:
                    if (!Physics.Raycast(ray, out RaycastHit hitBuilding, buildingLayerMask)) return false;

                    VertexPoint vp = hitBuilding.collider.GetComponent<VertexPoint>();
                    if (vp == null)
                    {
                        Debug.Log("Not a valid placement target.");
                        return false;
                    }

                    StructureType targetStructure = currentPurchaseType == PurchaseType.City ? StructureType.CITY : StructureType.SETTLEMENT;
                    if (StructureManager.Instance.TryPlaceStructure(vp, targetStructure))
                    {
                        return true;
                    }
                    return false;
            }

            return false;
        }

        void CancelPlacement()
        {
            isPlacingStructure = false;
            EdgePoint.ShowPlacementHighlights = false;

            if (activeCursor != null)
                Destroy(activeCursor);
        }
    }
}
