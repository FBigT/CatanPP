// Assets/Scripts/UI/LeftMenu/OnStructureTabEvents.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Catan.Managers;        // PurchaseManager
using Assets.Scripts.Enums;  // PurchaseType
using Catan.UI;             // TopBarUI  (for affordability callback)

namespace Catan.UI.LeftMenu
{
    /// <summary>
    /// Wires the four “Buy …” buttons in the top‑left panel and
    /// forwards the user’s choice to <see cref="PurchaseManager"/>.
    /// It also enables / disables those buttons depending on how
    /// many resources the player currently owns.
    /// </summary>
    public class OnStructureTabEvents : MonoBehaviour
    {
        // ── Inspector ──────────────────────────────────────────────────────
        [Header("Your UIDocument")]
        [SerializeField] private UIDocument uiDocument;

        [Header("Prefabs (assign in Inspector)")]
        [SerializeField] private GameObject roadPrefab;
        [SerializeField] private GameObject settlementPrefab;
        [SerializeField] private GameObject cityPrefab;
        [SerializeField] private GameObject devCardPrefab;

        // ── runtime references ────────────────────────────────────────────
        private Button _buyRoad;
        private Button _buySettlement;
        private Button _buyCity;
        private Button _buyDevCard;
        private Dictionary<PurchaseType, Button> _buttonByType;

        private GameObject _selectedPrefab;

        // simple demo‑costs:        lumber,wool,grain,bricks,ore,gold,silver,obsidian
        private static readonly Dictionary<PurchaseType, int[]> _cost =
            new() {
                { PurchaseType.Road,       new[]{1,1,0,1,0,0,0,0} },
                { PurchaseType.Settlement, new[]{1,1,1,1,0,0,0,0} },
                { PurchaseType.City,       new[]{0,0,2,0,3,0,0,0} },
                { PurchaseType.DevCard,    new[]{0,1,1,0,1,0,0,0} }
            };

        // ── Life‑cycle ─────────────────────────────────────────────────────
        void Awake()
        {
            if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null)
            {
                Debug.LogError("[OnStructureTabEvents] Missing UIDocument!");
                enabled = false;
                return;
            }

            VisualElement root = uiDocument.rootVisualElement;
            List<Button> buttons = root.Query<Button>().Build().ToList();

            // get by name (set in the UXML)
            _buyRoad = buttons.FirstOrDefault(b => b.name == "BuyRoadButton");
            _buySettlement = buttons.FirstOrDefault(b => b.name == "BuySettlementButton");
            _buyCity = buttons.FirstOrDefault(b => b.name == "BuyCityButton");
            _buyDevCard = buttons.FirstOrDefault(b => b.name == "BuyDevCardButton");

            _buttonByType = new()
            {
                { PurchaseType.Road,       _buyRoad },
                { PurchaseType.Settlement, _buySettlement },
                { PurchaseType.City,       _buyCity },
                { PurchaseType.DevCard,    _buyDevCard }
            };

            // log what we found
            Debug.Log($"[OnStructureTabEvents] Found {buttons.Count} Buttons:");
            foreach (var b in buttons)
                Debug.Log($"  • name='{b.name}'  text='{b.text}'");

            WireClicks();
        }

        void OnEnable() => WireClicks();
        void OnDisable() => UnwireClicks();
        void OnDestroy() => UnwireClicks();

        // ── Public API (called by TopBarUI) ───────────────────────────────
        /// <summary>
        /// TopBarUI passes its current numbers so we can grey‑out things
        /// the player cannot afford.
        /// </summary>
        public void UpdateAffordability(int[] amounts)
        {
            foreach (var kv in _buttonByType)
                kv.Value?.SetEnabled(CanAfford(kv.Key, amounts));
        }

        // ── helpers ────────────────────────────────────────────────────────
        bool CanAfford(PurchaseType type, int[] have)
        {
            if (!_cost.TryGetValue(type, out var need)) return true; // no cost table -> always ok
            for (int i = 0; i < need.Length && i < have.Length; i++)
                if (have[i] < need[i]) return false;
            return true;
        }

        void WireClicks()
        {
            _buyRoad?.RegisterCallback<ClickEvent>(_ => Select(roadPrefab, PurchaseType.Road));
            _buySettlement?.RegisterCallback<ClickEvent>(_ => Select(settlementPrefab, PurchaseType.Settlement));
            _buyCity?.RegisterCallback<ClickEvent>(_ => Select(cityPrefab, PurchaseType.City));
            _buyDevCard?.RegisterCallback<ClickEvent>(_ => Select(devCardPrefab, PurchaseType.DevCard));
        }

        void UnwireClicks()
        {
            _buyRoad?.UnregisterCallback<ClickEvent>(_ => Select(roadPrefab, PurchaseType.Road));
            _buySettlement?.UnregisterCallback<ClickEvent>(_ => Select(settlementPrefab, PurchaseType.Settlement));
            _buyCity?.UnregisterCallback<ClickEvent>(_ => Select(cityPrefab, PurchaseType.City));
            _buyDevCard?.UnregisterCallback<ClickEvent>(_ => Select(devCardPrefab, PurchaseType.DevCard));
        }

        void Select(GameObject prefab, PurchaseType type)
        {
            if (prefab == null) { Debug.LogError($"No prefab for {type}!"); return; }

            _selectedPrefab = prefab;
            PurchaseManager.Instance.SetPurchase(type);
            Debug.Log($"[OnStructureTabEvents] Selected {type}");
        }

        /// <summary>Read by <see cref="Catan.Controllers.StructurePlacer"/>.</summary>
        public GameObject GetSelectedStructure() => _selectedPrefab;
    }
}
