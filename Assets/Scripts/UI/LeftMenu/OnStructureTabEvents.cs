// Assets/Scripts/UI/LeftMenu/OnStructureTabEvents.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Assets.Scripts.Enums;    // PurchaseType
using Catan.Managers;         // PurchaseManager
using Catan.GameMode;         // CampaignGameMode, GamePhase

namespace Catan.UI.LeftMenu
{
    [RequireComponent(typeof(UIDocument))]
    public class OnStructureTabEvents : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private GameObject roadPrefab;
        [SerializeField] private GameObject settlementPrefab;
        [SerializeField] private GameObject cityPrefab;
        [SerializeField] private GameObject devCardPrefab;

        private readonly Dictionary<PurchaseType, Button> _btns = new();
        private GameObject _selectedPrefab;

        void Awake()
        {
            if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null) { enabled = false; return; }

            var root = uiDocument.rootVisualElement;
            _btns[PurchaseType.Road] = root.Q<Button>("BuyRoadButton");
            _btns[PurchaseType.Settlement] = root.Q<Button>("BuySettlementButton");
            _btns[PurchaseType.City] = root.Q<Button>("BuyCityButton");
            _btns[PurchaseType.DevCard] = root.Q<Button>("BuyDevCardButton");

            // only hook clicks for Play‐phase
            _btns[PurchaseType.Road]?.RegisterCallback<ClickEvent>(_ => Select(roadPrefab, PurchaseType.Road));
            _btns[PurchaseType.Settlement]?.RegisterCallback<ClickEvent>(_ => Select(settlementPrefab, PurchaseType.Settlement));
            _btns[PurchaseType.City]?.RegisterCallback<ClickEvent>(_ => Select(cityPrefab, PurchaseType.City));
            _btns[PurchaseType.DevCard]?.RegisterCallback<ClickEvent>(_ => Select(devCardPrefab, PurchaseType.DevCard));

            PurchaseManager.Instance.OnPurchaseChanged += OnPurchaseChanged;
        }

        private void OnPurchaseChanged(PurchaseType type)
        {
            var gm = CampaignGameMode.Instance;
            bool setup = gm.Phase == GamePhase.Setup;

            if (setup)
            {
                // 1) ignore the Clear() call during setup
                if (type == PurchaseType.None)
                    return;

                // 2) auto-select the one free build
                switch (type)
                {
                    case PurchaseType.Road: _selectedPrefab = roadPrefab; break;
                    case PurchaseType.Settlement: _selectedPrefab = settlementPrefab; break;
                    case PurchaseType.City: _selectedPrefab = cityPrefab; break;
                    case PurchaseType.DevCard: _selectedPrefab = devCardPrefab; break;
                }

                // 3) grey out *all* buttons except the one we’re on
                foreach (var kv in _btns)
                    kv.Value.SetEnabled(kv.Key == type);

                Debug.Log($"[OnStructureTabEvents] Auto-selected {type} for setup.");
                return;
            }

            // ——— PLAY PHASE: normal affordability ———
            int[] have = gm.CurrentPlayer.Resources;
            foreach (var kv in _btns)
                kv.Value.SetEnabled(PurchaseManager.Instance.CanAfford(kv.Key, have));
        }

        private void Select(GameObject prefab, PurchaseType type)
        {
            var gm = CampaignGameMode.Instance;
            if (gm.Phase == GamePhase.Setup)
                return;  // do not allow manual clicks during setup

            if (!PurchaseManager.Instance.SetPurchase(type, gm.CurrentPlayer.Resources))
            {
                Debug.LogWarning($"Cannot afford {type} right now.");
                return;
            }

            _selectedPrefab = prefab;
            Debug.Log($"[OnStructureTabEvents] Selected {type} prefab for placement.");
        }

        public GameObject GetSelectedStructure() => _selectedPrefab;

        public void UpdateAffordability(int[] resources)
        {
            foreach (var kv in _btns)
                kv.Value.SetEnabled(PurchaseManager.Instance.CanAfford(kv.Key, resources));
        }
    }
}
