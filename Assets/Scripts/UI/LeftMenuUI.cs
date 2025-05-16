using UnityEngine;
using UnityEngine.UIElements;
using Assets.Scripts.Enums;
using Catan.UI;

namespace Catan.UI
{
    public class LeftMenuUI : MonoBehaviour
    {
        [Header("Prefabs")]
        public GameObject roadPrefab, settlementPrefab, cityPrefab, devCardPrefab;

        [Header("Preview Shader")]
        public Material hologramMaterial;

        [Header("Local Resource Inventory")]
        public PlayerInventory localInventory;

        private Button btnRoad, btnSettle, btnCity, btnDevCard;
        private GameObject currentPreview;
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
            btnDevCard = root.Q<Button>("BuyDevCardButton");

            if (btnRoad != null) btnRoad.clicked += () => TryPurchase(PurchaseType.Road);
            if (btnSettle != null) btnSettle.clicked += () => TryPurchase(PurchaseType.Settlement);
            if (btnCity != null) btnCity.clicked += () => TryPurchase(PurchaseType.City);
            if (btnDevCard != null) btnDevCard.clicked += () => TryPurchase(PurchaseType.DevCard);
        }

        void TryPurchase(PurchaseType type)
        {
            if (isPlacingStructure) return;

            if (!localInventory.HasEnoughFor(type))
            {
                Debug.Log("Not enough resources.");
                return;
            }

            localInventory.SpendResources(type);

            GameObject prefab = type switch
            {
                PurchaseType.Road => roadPrefab,
                PurchaseType.Settlement => settlementPrefab,
                PurchaseType.City => cityPrefab,
                PurchaseType.DevCard => devCardPrefab,
                _ => null
            };

            if (prefab != null)
                SpawnPreview(prefab);
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

        void Update()
        {
            if (isPlacingStructure && currentPreview)
            {
                currentPreview.transform.position = GetMouseProjectedPosition();

                if (Input.GetMouseButtonDown(1)) // Right-click cancel
                    CancelPreview();
            }
        }

        void CancelPreview()
        {
            if (currentPreview)
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
                PurchaseType.DevCard => Wool >= 1 && Grain >= 1 && Ore >= 1,
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
                case PurchaseType.DevCard:
                    Wool--; Grain--; Ore--;
                    break;
            }
        }
    }
}
