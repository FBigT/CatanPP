// Assets/Scripts/UI/LeftMenu/OnStructureTabEvents.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Assets.Scripts.Enums;        // PurchaseType
using Catan.Managers;             // PurchaseManager
using Catan.GameMode;             // CampaignGameMode, GamePhase
using Catan.UI;                   // TopBarUI

namespace Catan.UI.LeftMenu
{
    [RequireComponent(typeof(UIDocument))]
    public class OnStructureTabEvents : MonoBehaviour
    {
        [Header("UI Document & Prefabs")]
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private GameObject roadPrefab;
        [SerializeField] private GameObject settlementPrefab;
        [SerializeField] private GameObject cityPrefab;
        [SerializeField] private GameObject devCardPrefab;

        private readonly Dictionary<PurchaseType, Button> _btns = new();
        private GameObject _selectedPrefab;

        // turn-gate flags
        private bool _itsMyTurn;
        private bool _inSetup;

        void Awake()
        {
            if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null)
            {
                Debug.LogError("[OnStructureTabEvents] no UIDocument!");
                enabled = false;
                return;
            }

            var root = uiDocument.rootVisualElement;
            _btns[PurchaseType.Road] = root.Q<Button>("BuyRoadButton");
            _btns[PurchaseType.Settlement] = root.Q<Button>("BuySettlementButton");
            _btns[PurchaseType.City] = root.Q<Button>("BuyCityButton");
            _btns[PurchaseType.DevCard] = root.Q<Button>("BuyDevCardButton");

            // only valid in Play phase
            _btns[PurchaseType.Road]?.RegisterCallback<ClickEvent>(_ => Select(roadPrefab, PurchaseType.Road));
            _btns[PurchaseType.Settlement]?.RegisterCallback<ClickEvent>(_ => Select(settlementPrefab, PurchaseType.Settlement));
            _btns[PurchaseType.City]?.RegisterCallback<ClickEvent>(_ => Select(cityPrefab, PurchaseType.City));
            _btns[PurchaseType.DevCard]?.RegisterCallback<ClickEvent>(_ => Select(devCardPrefab, PurchaseType.DevCard));
        }

        void OnEnable()
        {
            Debug.Log("[OnStructureTabEvents] OnEnable: subscribing to events");
            PurchaseManager.Instance.OnPurchaseChanged += OnPurchaseChanged;

            if (CampaignGameMode.Instance != null)
            {
                Debug.Log("[OnStructureTabEvents] OnEnable: subscribing to TurnChangedForUI");
                CampaignGameMode.Instance.TurnChangedForUI += OnTurnChanged;
            }
            if (TopBarUI.Instance != null)
            {
                Debug.Log("[OnStructureTabEvents] OnEnable: subscribing to TopBarUI.OnResourcesChanged");
                TopBarUI.Instance.OnResourcesChanged += UpdateAffordability;
            }
        }

        void OnDisable()
        {
            Debug.Log("[OnStructureTabEvents] OnDisable: unsubscribing from events");
            if (PurchaseManager.Instance != null)
                PurchaseManager.Instance.OnPurchaseChanged -= OnPurchaseChanged;

            if (CampaignGameMode.Instance != null)
                CampaignGameMode.Instance.TurnChangedForUI -= OnTurnChanged;

            if (TopBarUI.Instance != null)
                TopBarUI.Instance.OnResourcesChanged -= UpdateAffordability;
        }

        // ——— Turn‐gate callback ———
        private void OnTurnChanged(bool myTurn, bool inSetup)
        {
            Debug.Log($"[OnStructureTabEvents] OnTurnChanged → myTurn={myTurn}, inSetup={inSetup}");
            _itsMyTurn = myTurn;
            _inSetup = inSetup;

            if (!myTurn)
            {
                Debug.Log("[OnStructureTabEvents]   → not my turn: greying out all buttons");
                foreach (var kv in _btns)
                    kv.Value.SetEnabled(false);
                return;
            }

            if (inSetup)
            {
                Debug.Log("[OnStructureTabEvents]   → my turn during setup: leave auto-selected only");
                return;
            }

            // play‐phase & my turn
            Debug.Log("[OnStructureTabEvents]   → play phase & my turn: re-checking affordability now");
            UpdateAffordability(CampaignGameMode.Instance.CurrentPlayer.Resources);
        }

        // ——— Purchase changed (selected or cleared) ———
        private void OnPurchaseChanged(PurchaseType type)
        {
            var gm = CampaignGameMode.Instance;
            bool setup = gm.Phase == GamePhase.Setup;
            Debug.Log($"[OnStructureTabEvents] OnPurchaseChanged → type={type}, setupPhase={setup}");

            if (setup)
            {
                if (type == PurchaseType.None)
                {
                    Debug.Log("[OnStructureTabEvents]   → setup: ignore Clear()");
                    return;
                }

                _selectedPrefab = type switch
                {
                    PurchaseType.Road => roadPrefab,
                    PurchaseType.Settlement => settlementPrefab,
                    PurchaseType.City => cityPrefab,
                    PurchaseType.DevCard => devCardPrefab,
                    _ => null
                };

                Debug.Log($"[OnStructureTabEvents]   → setup: auto-selected {_selectedPrefab.name}");
                foreach (var kv in _btns)
                {
                    bool enabled = (kv.Key == type);
                    Debug.Log($"[OnStructureTabEvents]      button {kv.Key} → SetEnabled({enabled})");
                    kv.Value.SetEnabled(enabled);
                }
                return;
            }

            // play phase: either a new buy selected, or cleared → re-check affordability
            Debug.Log("[OnStructureTabEvents]   → play phase: purchase changed → re-check affordability");
            UpdateAffordability(gm.CurrentPlayer.Resources);
        }

        // ——— Manual selection from UI ———
        private void Select(GameObject prefab, PurchaseType type)
        {
            var gm = CampaignGameMode.Instance;
            Debug.Log($"[OnStructureTabEvents] Select called for {type}, phase={gm.Phase}");
            if (gm.Phase == GamePhase.Setup)
            {
                Debug.Log("[OnStructureTabEvents]   → in setup: ignoring manual select");
                return;
            }

            if (!PurchaseManager.Instance.SetPurchase(type, gm.CurrentPlayer.Resources))
            {
                Debug.LogWarning($"[OnStructureTabEvents]   → cannot afford {type}");
                return;
            }

            _selectedPrefab = prefab;
            Debug.Log($"[OnStructureTabEvents]   → prefab set to {prefab.name}");
        }

        // ——— Called on any resource‐change or turn‐change ———
        public void UpdateAffordability(int[] resources)
        {
            Debug.Log($"[OnStructureTabEvents] UpdateAffordability → resources=[{string.Join(",", resources)}], itsMyTurn={_itsMyTurn}, inSetup={_inSetup}");
            if (!_itsMyTurn)
            {
                Debug.Log("[OnStructureTabEvents]   → not my turn, skipping affordability");
                return;
            }
            if (_inSetup)
            {
                Debug.Log("[OnStructureTabEvents]   → still in setup, skipping affordability");
                return;
            }

            foreach (var kv in _btns)
            {
                bool can = PurchaseManager.Instance.CanAfford(kv.Key, resources);
                Debug.Log($"[OnStructureTabEvents]   → {kv.Key}: CanAfford={can}");
                kv.Value.SetEnabled(can);
            }
        }

        public GameObject GetSelectedStructure() => _selectedPrefab;
    }
}
