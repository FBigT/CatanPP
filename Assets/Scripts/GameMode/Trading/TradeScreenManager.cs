using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Assets.Scripts.Utils;                     // LocalStorageService, WebSocketService
using Assets.Scripts.GameMode.Trading.Models;
using Assets.Scripts.GameMode.Trading;
using Assets.Scripts.User;                      // ChatMessage, ChatMessageType
using Assets.Scripts.Dtos.GameMoveResponses;    // TradeOfferMessage

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
            // 1) Load session & user
            sessionId = LocalStorageService.GetInt("session-id") ?? 0;
            currentUserName = LocalStorageService.GetString("username") ?? "";

            // 2) Populate the dropdown immediately
            playerDropdown.ClearOptions();
            TradingManager.Instance.GetSessionPlayers(
                sessionId,
                OnPlayersLoaded,
                err => Debug.LogError($"[TradeScreen] Load players failed: {err}")
            );

            // 3) Fire-and-forget WebSocket connect (won’t block UI)
            string code = sessionId.ToString();
            if (string.IsNullOrEmpty(code))
            {
                Debug.LogWarning("[TradeScreenManager] No session-code for WebSocket");
            }
            else
            {
                Debug.Log($"[TradeScreenManager] Connecting WebSocket with code={code}");
                var connectTask = WebSocketService.ConnectToChat(code);
                connectTask.ContinueWith(_ =>
                    Debug.Log($"[TradeScreenManager] WebSocket Connected = {WebSocketService.Connected}")
                );
            }
        }

        void OnPlayersLoaded(List<SessionPlayerDto> players)
        {
            var options = new List<string> { "Bank" };
            foreach (var p in players)
            {
                if (!p.username.Equals(currentUserName, System.StringComparison.OrdinalIgnoreCase))
                    options.Add(p.username);
            }

            playerDropdown.ClearOptions();
            playerDropdown.AddOptions(options);
            playerDropdown.value = 0;
            playerDropdown.RefreshShownValue();

            Debug.Log($"[TradeScreenManager] Players loaded: {string.Join(", ", options)}");
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
            CaptureSelections(requestResourceButtons,
                              selectedRequestedResources,
                              selectedRequestedQuantities);
            OpenMainTradePanel();
        }

        public void ApplyOfferSelection()
        {
            CaptureSelections(offerResourceButtons,
                              selectedOfferedResources,
                              selectedOfferedQuantities);
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
        }

        public void OnApplyTradeClicked()
        {
            string toUser = playerDropdown.options[playerDropdown.value].text;

            if (toUser == "Bank")
            {
                // Bank‐trade logic (unchanged)
                ApplyRequestSelection();
                ApplyOfferSelection();

                var offered = ResourceGroup.FromLists(selectedOfferedResources, selectedOfferedQuantities);
                var requested = ResourceGroup.FromLists(selectedRequestedResources, selectedRequestedQuantities);

                if (!IsValidBankTrade(offered, requested, out string err))
                {
                    Debug.LogError("[TradeScreen] Invalid bank trade: " + err);
                    return;
                }

                var bankDto = new BankTradeDto
                {
                    sessionId = sessionId,
                    fromUser = currentUserName,
                    offered = offered,
                    requested = requested,
                    portType = "Default",
                    portRatio = 4
                };

                TradingManager.Instance.TradeWithBank(bankDto,
                    () => Debug.Log("[TradeScreen] Bank trade successful"),
                    e => Debug.LogError("[TradeScreen] Bank trade failed: " + e)
                );
            }
            else
            {
                // Player‐to‐player trade
                ApplyRequestSelection();
                ApplyOfferSelection();

                var dto = new PlayerTradeDto
                {
                    sessionId = sessionId,
                    fromUser = currentUserName,
                    toUser = toUser,
                    offered = ResourceGroup.FromLists(selectedOfferedResources, selectedOfferedQuantities),
                    requested = ResourceGroup.FromLists(selectedRequestedResources, selectedRequestedQuantities)
                };

                TradingManager.Instance.TradeWithPlayer(dto,
                    onSuccess: () =>
                    {
                        Debug.Log("[TradeScreen] Trade request sent to server.");
                        Debug.Log($"[TradeScreenManager] WebSocket Connected = {WebSocketService.Connected}");
                        Debug.Log($"[TradeScreenManager] Sending TradeOfferMessage to {toUser}");

                        var offerMsg = new TradeOfferMessage
                        {
                            fromUser = currentUserName,
                            toUser = toUser,
                            offered = dto.offered,
                            requested = dto.requested
                        };

                        _ = WebSocketService.SendTradeOffer(offerMsg)
                             .ContinueWith(_ =>
                                 Debug.Log("[TradeScreenManager] SendTradeOffer() completed")
                             );
                    },
                    onError: e =>
                    {
                        Debug.LogError($"[TradeScreen] Trade failed: {e}");
                    });
            }
        }

        private bool IsValidBankTrade(ResourceGroup offered, ResourceGroup requested, out string error)
        {
            error = "";
            var offers = offered.GetResourceDictionary().Where(kvp => kvp.Value > 0).ToList();
            if (offers.Count != 1) { error = "Must offer 4 of one resource."; return false; }
            if (offers[0].Value != 4) { error = "Must offer exactly 4 units."; return false; }

            var req = requested.GetResourceDictionary().Where(kvp => kvp.Value > 0).ToList();
            if (req.Count != 1 || req[0].Value != 1)
            {
                error = "Must request exactly 1 unit.";
                return false;
            }
            return true;
        }

        // Helper to build "2 wood, 1 sheep" text
        private string BuildSummary(ResourceGroup g)
        {
            return string.Join(", ",
                g.GetResourceDictionary()
                 .Where(kvp => kvp.Value > 0)
                 .Select(kvp => $"{kvp.Value} {kvp.Key}")
            );
        }
    }
}
