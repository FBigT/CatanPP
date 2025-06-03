// Assets/Scripts/GameMode/Trading/TradeScreenManager.cs

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Assets.Scripts.Utils;                   // ← for WebSocketService
using Assets.Scripts.GameMode.Trading.Models;
using Assets.Scripts.GameMode.Trading;
using Assets.Scripts.User;                    // ← for LocalStorageService
using Assets.Scripts.Dtos.GameMoveResponses;  // ← for TradeResponseMessage

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

        private long sessionId;
        private string currentUserName;

        private void Awake()
        {
            // Grab session‐ID & user‐name as soon as possible
            sessionId = LocalStorageService.GetInt("session-id") ?? 0;
            currentUserName = LocalStorageService.GetString("username") ?? "";
        }

        private void OnEnable()
        {
            WebSocketService.OnTradeResponseReceived += HandleTradeResponse;
            WebSocketService.OnPlayerJoined += HandlePlayerJoined;
        }

        private void OnDisable()
        {
            WebSocketService.OnTradeResponseReceived -= HandleTradeResponse;
            WebSocketService.OnPlayerJoined -= HandlePlayerJoined;
        }

        async void Start()
        {
            // 1) Populate dropdown right away
            RefreshPlayerDropdown();

            // 2) Make sure we’re connected to WebSocket (so we start hearing OnPlayerJoined, etc.)
            string sessionCode = LocalStorageService.GetString("session-code");
            if (!WebSocketService.Connected)
            {
                Debug.Log("[TradeScreenManager] Connecting to WebSocket…");
                await WebSocketService.ConnectToChat(sessionCode);
            }
        }

        /// <summary>
        /// Whenever Start() runs _or_ a new player arrives, call this to re‐fetch + re‐draw the dropdown.
        /// </summary>
        private void RefreshPlayerDropdown()
        {
            TradingManager.Instance.GetSessionPlayers(
                sessionId,
                OnPlayersLoaded,
                err => Debug.LogError($"[TradeScreen] Failed to load players: {err}")
            );
        }

        /// <summary>
        /// Called by TradingManager once it has the up‐to‐date list of everyone in this session.
        /// We sort them, put “Bank” at index 0, then everyone else (alphabetically, skipping self).
        /// </summary>
        private void OnPlayersLoaded(List<SessionPlayerDto> players)
        {
            // 1) Filter out ourselves, then sort the remaining by username
            var otherPlayers =
                players
                    .Where(p => !p.username.Equals(currentUserName, StringComparison.OrdinalIgnoreCase))
                    .Select(p => p.username)
                    .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                    .ToList();

            // 2) Build the dropdown list: always “Bank” first
            var options = new List<string> { "Bank" };
            options.AddRange(otherPlayers);

            // 3) Re‐populate UnityEngine.UI.Dropdown
            playerDropdown.ClearOptions();
            playerDropdown.AddOptions(options);
            playerDropdown.value = 0;
            playerDropdown.RefreshShownValue();

            Debug.Log($"[TradeScreenManager] Dropdown now contains: {string.Join(", ", options)}");
        }

        #region “Someone joined” handler

        // This gets called whenever a new player joins the session (fired by WebSocketService).
        // We simply re‐query the server for the new list of players.
        private void HandlePlayerJoined()
        {
            Debug.Log("[TradeScreenManager] A player joined. Refreshing dropdown…");
            RefreshPlayerDropdown();
        }

        #endregion

        #region Panel Switching (unchanged)

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

        #endregion

        #region Resource Selection (unchanged)

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

        #endregion

        #region “Apply Trade” Button

        public void OnApplyTradeClicked()
        {
            string toUser = playerDropdown.options[playerDropdown.value].text;

            if (toUser == "Bank")
            {
                // ───── Bank trade ─────
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

                TradingManager.Instance.TradeWithBank(
                    bankDto,
                    () => Debug.Log("[TradeScreen] Bank trade successful"),
                    e => Debug.LogError("[TradeScreen] Bank trade failed: " + e)
                );
            }
            else
            {
                // ───── Player‐to‐player trade ─────
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

                TradingManager.Instance.TradeWithPlayer(
                    dto,
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

                        // Send the STOMP frame; player A will see a “Trade Sent” popup via ChatUIManager
                        _ = WebSocketService
                                .SendTradeOffer(offerMsg)
                                .ContinueWith(_ => Debug.Log("[TradeScreenManager] SendTradeOffer() completed"));
                    },
                    onError: e =>
                    {
                        Debug.LogError($"[TradeScreen] Trade failed: {e}");
                    }
                );
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

        #endregion

        #region Trade‐Response Handler (unchanged)

        private void HandleTradeResponse(TradeResponseMessage resp)
        {
            if (resp.toUser != currentUserName) return;

            if (resp.accepted)
                Debug.Log($"[TradeScreenManager] Trade ACCEPTED by {resp.fromUser}");
            else
                Debug.Log($"[TradeScreenManager] Trade DENIED by {resp.fromUser}");
        }

        #endregion
    }
}
