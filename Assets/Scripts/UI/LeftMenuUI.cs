using UnityEngine;
using UnityEngine.UIElements;
using Assets.Scripts.Enums;

namespace Catan.UI
{
    public class LeftMenuUI : MonoBehaviour
    {
        [Header("Prefabs")]
        public GameObject roadPrefab, settlementPrefab, cityPrefab;

        [Header("Preview Shader")]
        public Material hologramMaterial;

        [Header("Local Resource Inventory")]
        public PlayerInventory localInventory;

        private Button btnRoad, btnSettle, btnCity;
        private GameObject currentPreview;
        private PurchaseType currentPurchaseType;
        private bool isPlacingStructure = false;
        private Plane groundPlane = new Plane(Vector3.up, Vector3.zero); // y = 0 plane

        void OnEnable()
        {
            var root = GetComponent<UIDocument>()?.rootVisualElement;
            if (root == null)
            {
                Debug.LogError("LeftMenuUI: missing UIDocument");
                enabled = false;
                return;
            }

            btnRoad = root.Q<Button>("BuyRoadButton");
            btnSettle = root.Q<Button>("BuySettlementButton");
            btnCity = root.Q<Button>("BuyCityButton");

            if (btnRoad != null) btnRoad.clicked += () => TryPurchase(PurchaseType.Road);
            if (btnSettle != null) btnSettle.clicked += () => TryPurchase(PurchaseType.Settlement);
            if (btnCity != null) btnCity.clicked += () => TryPurchase(PurchaseType.City);
        }

        void TryPurchase(PurchaseType type)
        {
            if (isPlacingStructure) return;

            if (!localInventory.HasEnoughFor(type))
            {
                Debug.Log("Not enough resources.");
                return;
            }

            GameObject prefab = type switch
            {
                PurchaseType.Road => roadPrefab,
                PurchaseType.Settlement => settlementPrefab,
                PurchaseType.City => cityPrefab,
                _ => null
            };

            if (prefab != null)
            {
                currentPurchaseType = type;
                SpawnPreview(prefab);
            }
        }

        void SpawnPreview(GameObject prefab)
        {
            if (currentPreview != null)
                Destroy(currentPreview);

            currentPreview = Instantiate(prefab);
            ApplyHologram(currentPreview);
            isPlacingStructure = true;
        }

        void ApplyHologram(GameObject obj)
        {
            foreach (var rend in obj.GetComponentsInChildren<Renderer>())
                rend.material = hologramMaterial;
        }

        void Update()
        {
            if (!isPlacingStructure || currentPreview == null) return;

            // Update preview position
            currentPreview.transform.position = GetMouseProjectedPosition();

            // Cancel
            if (Input.GetMouseButtonDown(1))
                CancelPreview();

            // Try place on left click
            if (Input.GetMouseButtonDown(0))
            {
                if (TryPlaceStructure())
                {
                    localInventory.SpendResources(currentPurchaseType);
                    Destroy(currentPreview);
                    currentPreview = null;
                    isPlacingStructure = false;
                }
            }
        }

        Vector3 GetMouseProjectedPosition()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (groundPlane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                return hitPoint + Vector3.up * 5f;
            }
            return Vector3.zero;
        }

        bool TryPlaceStructure()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit))
                return false;

            switch (currentPurchaseType)
            {
                case PurchaseType.Road:
                    EdgePoint ep = hit.collider.GetComponent<EdgePoint>();
                    if (ep == null)
                    {
                        Debug.Log("Invalid road target.");
                        return false;
                    }

                    if (StructureManager.Instance.TryPlaceRoad(ep, "debug")) // Replace with real player ID
                    {
                        return true;
                    }
                    return false;

                case PurchaseType.Settlement:
                case PurchaseType.City:
                    VertexPoint vp = hit.collider.GetComponent<VertexPoint>();
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
                    return true;
            }

            return false;
        }

        void CancelPreview()
        {
            if (currentPreview != null)
                Destroy(currentPreview);
            currentPreview = null;
            isPlacingStructure = false;
        }
    }

    [System.Serializable]
    public class PlayerInventory
    {
        public int Brick, Lumber, Wool, Grain, Ore;

        public bool HasEnoughFor(PurchaseType type)
        {
            return type switch
            {
                PurchaseType.Road => Brick >= 1 && Lumber >= 1,
                PurchaseType.Settlement => Brick >= 1 && Lumber >= 1 && Wool >= 1 && Grain >= 1,
                PurchaseType.City => Grain >= 2 && Ore >= 3,
                _ => false
            };
        }

        public void SpendResources(PurchaseType type)
        {
            switch (type)
            {
                case PurchaseType.Road:
                    Brick--; Lumber--;
                    break;
                case PurchaseType.Settlement:
                    Brick--; Lumber--; Wool--; Grain--;
                    break;
                case PurchaseType.City:
                    Grain -= 2; Ore -= 3;
                    break;
            }
        }
    }
}
