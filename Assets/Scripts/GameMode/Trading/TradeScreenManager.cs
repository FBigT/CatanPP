using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Assets.Scripts.Utils;
using Assets.Scripts.GameMode.Trading.Models;
using Assets.Scripts.GameMode.Trading;
using System;   // ← keep only this TradingManager

namespace Assets.Scripts.GameMode.Trading
{
    public class TradeScreenManager : MonoBehaviour
    {
        [Header("Panels")]
        public GameObject mainTradePanel;
        public GameObject requestPanel;
        public GameObject offerPanel;

        [Header("Resource Buttons")]
        public List<ResourceButtonHandler> requestResourceButtons;
        public List<ResourceButtonHandler> offerResourceButtons;

        [Header("Player Dropdown")]
        public Dropdown playerDropdown;

        readonly List<string> selectedRequestedResources = new();
        readonly List<int> selectedRequestedQuantities = new();
        readonly List<string> selectedOfferedResources = new();
        readonly List<int> selectedOfferedQuantities = new();

        long sessionId;
        string currentUserName;

        void Start()
        {
            sessionId = LocalStorageService.GetInt("session-id") ?? 0;
            currentUserName = LocalStorageService.GetString("username") ?? "";

            playerDropdown.ClearOptions();
            TradingManager.Instance.GetSessionPlayers(
                sessionId,
                OnPlayersLoaded,
                err => Debug.LogError($"[TradeScreen] Load players failed: {err}")
            );
        }

        void OnPlayersLoaded(List<SessionPlayerDto> players)
        {
            var options = new List<string> { "Bank" };

            foreach (var player in players)
            {
                if (!string.Equals(player.username, currentUserName, StringComparison.OrdinalIgnoreCase))
                {
                    options.Add(player.username);
                }
            }

            playerDropdown.ClearOptions();
            playerDropdown.AddOptions(options);
            playerDropdown.value = 0;
            playerDropdown.RefreshShownValue();

            Debug.Log($"[TradeScreen] Players loaded into dropdown: {string.Join(", ", options)}");
        }


        public void OpenRequestPanel() => SwitchPanel(requestPanel);
        public void OpenOfferPanel() => SwitchPanel(offerPanel);
        public void OpenMainTradePanel() => SwitchPanel(mainTradePanel);

        void SwitchPanel(GameObject panel)
        {
            mainTradePanel.SetActive(panel == mainTradePanel);
            requestPanel.SetActive(panel == requestPanel);
            offerPanel.SetActive(panel == offerPanel);
            EventSystem.current.SetSelectedGameObject(null);
        }

        public void ApplyRequestSelection()
        {
            CaptureSelections(requestResourceButtons, selectedRequestedResources, selectedRequestedQuantities);

            Debug.Log("[TradeScreen] Request resources applied: " +
                      string.Join(", ", selectedRequestedResources.Zip(selectedRequestedQuantities, (r, q) => $"{r} x{q}")));

            OpenMainTradePanel(); 
        }


        public void ApplyOfferSelection()
        {
            CaptureSelections(offerResourceButtons, selectedOfferedResources, selectedOfferedQuantities);

            Debug.Log("[TradeScreen] Offer resources applied: " +
                      string.Join(", ", selectedOfferedResources.Zip(selectedOfferedQuantities, (r, q) => $"{r} x{q}")));

            OpenMainTradePanel(); 
        }


        void CaptureSelections(List<ResourceButtonHandler> buttons,
                               List<string> names,
                               List<int> qty)
        {
            names.Clear();
            qty.Clear();

            foreach (var b in buttons)
            {
                int q = b.GetQuantity();
                if (q > 0)
                {
                    names.Add(b.resourceName);
                    qty.Add(q);
                }
            }

            Debug.Log($"[TradeScreen] Captured {names.Count} resources.");
        }


        public void OnApplyTradeClicked()
        {
            string toUser = playerDropdown.options[playerDropdown.value].text;

            if (toUser == "Bank")
            {
                // Process bank trade
                ApplyRequestSelection();
                ApplyOfferSelection();

                var offered = ResourceGroup.FromLists(selectedOfferedResources, selectedOfferedQuantities);
                var requested = ResourceGroup.FromLists(selectedRequestedResources, selectedRequestedQuantities);

                if (!IsValidBankTrade(offered, requested, out string errorMessage))
                {
                    Debug.LogError("[TradeScreen] Invalid bank trade: " + errorMessage);
                    // Optionally show UI error
                    return;
                }

                var dto = new BankTradeDto
                {
                    sessionId = sessionId,
                    fromUser = currentUserName,
                    offered = offered,
                    requested = requested,
                    portType = "Default", // You can extend this later
                    portRatio = 4         // 4:1 is standard unless player has port
                };

                TradingManager.Instance.TradeWithBank(dto,
                    () => Debug.Log("[TradeScreen] Bank trade successful"),
                    err => Debug.LogError("[TradeScreen] Bank trade failed: " + err));
            }
            else
            {
                // Player-to-player trade
                var dto = new PlayerTradeDto
                {
                    sessionId = sessionId,
                    fromUser = currentUserName,
                    toUser = toUser,
                    offered = ResourceGroup.FromLists(selectedOfferedResources, selectedOfferedQuantities),
                    requested = ResourceGroup.FromLists(selectedRequestedResources, selectedRequestedQuantities)
                };

                TradingManager.Instance.TradeWithPlayer(dto,
                    () => Debug.Log("[TradeScreen] Trade successful"),
                    err => Debug.LogError("[TradeScreen] Trade failed: " + err));
            }
        }
        private bool IsValidBankTrade(ResourceGroup offered, ResourceGroup requested, out string error)
        {
            error = "";

            // Sum offered: must offer exactly 4 of *one type*
            var offerPairs = offered.GetResourceDictionary()
                                    .Where(kvp => kvp.Value > 0)
                                    .ToList();

            if (offerPairs.Count != 1)
            {
                error = "Bank trade must offer exactly one type of resource.";
                return false;
            }

            int offerAmount = offerPairs[0].Value;
            if (offerAmount != 4)
            {
                error = "You must offer exactly 4 units of one resource to the bank.";
                return false;
            }

            // Sum requested: must request exactly 1 of one type
            var requestPairs = requested.GetResourceDictionary()
                                        .Where(kvp => kvp.Value > 0)
                                        .ToList();

            if (requestPairs.Count != 1 || requestPairs[0].Value != 1)
            {
                error = "You must request exactly 1 unit of one resource from the bank.";
                return false;
            }

            return true;
        }

    }
}
