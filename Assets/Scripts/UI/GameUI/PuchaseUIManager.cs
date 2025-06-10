using Assets.Scripts.Enums;
using Catan.Managers;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Catan.UI
{
    public class PuchaseUIManager : MonoBehaviour
    {
        [Header("Purchase Entries")]
        public List<PurchaseEntry> purchaseEntries;

        [Header("Purchase Manager")]
        public PurchaseManagerLocal purchaseManager;

        [Header("Layer Masks")]
        public LayerMask buildingLayerMask = 1;
        public LayerMask roadLayerMask = 1;

        [Header("Cursor Controller")]
        [SerializeField] private CursorController3D cursorController;

        [Header("UI Highligter")]
        [SerializeField] private PurchaseUIHighlighter highlighter;

        private Dictionary<PurchaseType, (Button button, GameObject prefab, KeyCode keyCode)> purchaseDict;
        private PurchaseType currentPurchaseType;
        private bool isPlacingStructure = false;

        public static PuchaseUIManager Instance { get; private set; }

        public delegate void StructureBuiltHandler(PurchaseType type, VertexPoint vp);
        public static event StructureBuiltHandler OnStructureBuilt;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            purchaseDict = new Dictionary<PurchaseType, (Button, GameObject, KeyCode)>();
            foreach (var entry in purchaseEntries)
            {
                if (entry.button == null || entry.prefab == null)
                {
                    Debug.LogWarning($"PurchaseEntry for {entry.type} is missing button or prefab.");
                    continue;
                }

                purchaseDict[entry.type] = (entry.button, entry.prefab, entry.key);
                entry.button.onClick.AddListener(() => TryPurchase(entry.type));
            }

            if (cursorController == null)
                Debug.LogWarning("CursorController3D not assigned to PurchaseUIManager.");
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
                return;

            if (!purchaseDict.TryGetValue(type, out var data))
            {
                Debug.LogError($"No purchase data found for type {type}");
                return;
            }

            currentPurchaseType = type;
            isPlacingStructure = true;

            highlighter.Highlight(currentPurchaseType);

            if (cursorController != null)
                cursorController.SetCursorMode(CursorController3D.CursorMode.Placing);

            EdgePoint.ShowPlacementHighlights = (type == PurchaseType.Road);
        }

        void Update()
        {
            if (!isPlacingStructure)
            {
                foreach (var kvp in purchaseDict)
                {
                    KeyCode key = kvp.Value.keyCode;
                    if (Input.GetKeyDown(key))
                    {
                        TryPurchase(kvp.Key);
                        break;
                    }
                }
                return;
            }

            if (Input.GetMouseButtonDown(1))
                CancelPlacement();

            if (Input.GetMouseButtonDown(0))
            {
                if (TryPlaceStructure())
                {
                    purchaseManager.SpendResources(currentPurchaseType);
                    isPlacingStructure = false;

                    highlighter.ResetAll();

                    if (cursorController != null)
                        cursorController.SetCursorMode(CursorController3D.CursorMode.Idle, true);
                }
            }
        }

        bool TryPlaceStructure()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            switch (currentPurchaseType)
            {
                case PurchaseType.Road:
                    if (!Physics.Raycast(ray, out RaycastHit hitRoad, roadLayerMask)) return false;

                    EdgePoint ep = hitRoad.collider.GetComponent<EdgePoint>();
                    if (ep == null || !StructureManager.Instance.TryPlaceRoad(ep, "debug"))
                    {
                        Debug.Log("Invalid or failed road placement.");
                        return false;
                    }

                    EdgePoint.ShowPlacementHighlights = false;

                    // change to on road built
                    //OnStructureBuilt?.Invoke(currentPurchaseType, purchaseDict[currentPurchaseType].prefab);
                    return true;

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
                    if (!StructureManager.Instance.TryPlaceStructure(vp, targetStructure))
                    {
                        return false;
                    }

                    OnStructureBuilt?.Invoke(currentPurchaseType, vp);
                    return true;
            }

            return false;
        }

        void CancelPlacement()
        {
            highlighter.ResetAll();

            isPlacingStructure = false;
            EdgePoint.ShowPlacementHighlights = false;

            if (cursorController != null)
                cursorController.SetCursorMode(CursorController3D.CursorMode.Idle, false);
        }
    }
}
