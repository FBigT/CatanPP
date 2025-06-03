// Assets/Scripts/GameMode/Trading/TradeScreenManager.cs

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Assets.Scripts.Utils;                   // for WebSocketService
using Assets.Scripts.GameMode.Trading.Models;
using Assets.Scripts.GameMode.Trading;
using Assets.Scripts.User;                    // for LocalStorageService
using Assets.Scripts.Dtos.GameMoveResponses;  // for TradeResponseMessage

namespace Assets.Scripts.GameMode.Trading
{
    public class TradeScreenManager : MonoBehaviour
    {
        [Header("Panels (Main UI)")]
        public GameObject mainTradePanel;
        public GameObject requestPanel;
        public GameObject offerPanel;

        [Header("Resource Buttons")]
        public List<ResourceButtonHandler> requestResourceButtons;
        public List<ResourceButtonHandler> offerResourceButtons;

        [Header("Player Dropdown")]
        public Dropdown playerDropdown;

        [Header("Trade Popups")]
        [Tooltip("Drag the 'Trade Sent' panel instance here (a simple popup that says 'Trade Sent').")]
        public GameObject tradeSentPanel;

        [Tooltip("Drag the 'Incoming Trade Offer' panel instance here.")]
        public GameObject tradeOfferPanel;

        [Header("Trade Offer UI Fields (Inside tradeOfferPanel)")]
        [Tooltip("The single Text component inside tradeOfferPanel where we'll write 'From: ...' and 'Offers: ... Wants: ...'.")]
        public TextMeshProUGUI contentText;

        [Tooltip("The Accept button inside tradeOfferPanel.")]
        public Button acceptButton;

        [Tooltip("The Decline button inside tradeOfferPanel.")]
        public Button declineButton;

        readonly List<string> selectedRequestedResources = new();
        readonly List<int> selectedRequestedQuantities = new();
        readonly List<string> selectedOfferedResources = new();
        readonly List<int> selectedOfferedQuantities = new();

        private long sessionId;
        private string currentUserName;

        private void Awake()
        {
            // Grab session‐ID & user‐name early
            sessionId = LocalStorageService.GetInt("session-id") ?? 0;
            currentUserName = LocalStorageService.GetString("username") ?? "";
        }

        private void OnEnable()
        {
            WebSocketService.OnTradeResponseReceived += HandleTradeResponse;
            WebSocketService.OnPlayerJoined += HandlePlayerJoined;
            WebSocketService.OnTradeOfferReceived += HandleTradeOffer;
        }

        private void OnDisable()
        {
            WebSocketService.OnTradeResponseReceived -= HandleTradeResponse;
            WebSocketService.OnPlayerJoined -= HandlePlayerJoined;
            WebSocketService.OnTradeOfferReceived -= HandleTradeOffer;
        }

        async void Start()
        {
            // 1) Populate the dropdown immediately
            RefreshPlayerDropdown();

            // 2) Connect to WebSocket if not already
            string sessionCode = LocalStorageService.GetString("session-code");
            if (!WebSocketService.Connected)
            {
                Debug.Log("[TradeScreenManager] Connecting to WebSocket…");
                await WebSocketService.ConnectToChat(sessionCode);
            }

            // 3) Ensure both popups start hidden
            tradeSentPanel.SetActive(false);
            tradeOfferPanel.SetActive(false);
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
            var otherPlayers = players
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

                        // Send the STOMP frame; once it’s queued, show “Trade Sent” popup.
                        _ = WebSocketService
                                .SendTradeOffer(offerMsg)
                                .ContinueWith(_ =>
                                {
                                    Debug.Log("[TradeScreenManager] SendTradeOffer() completed");
                                    // Make sure UI changes happen on Unity’s main thread:
                                    UnityEngine.WSA.Application.InvokeOnAppThread(
                                        () => tradeSentPanel.SetActive(true),
                                        false
                                    );
                                });
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

        #region “Incoming Trade Offer” Handler

        /// <summary>
        /// Called when someone else (Player A) sends us a TradeOfferMessage via WebSocket.
        /// If the “toUser” field matches our currentUserName, we pop up tradeOfferPanel.
        /// </summary>
        private void HandleTradeOffer(TradeOfferMessage offer)
        {
            // Only show the popup if the offer is addressed to me:
            if (!offer.toUser.Equals(currentUserName, StringComparison.OrdinalIgnoreCase))
                return;

            // 1) Build a single string that includes “From: [user]” and the resource details:
            string fromLine = $"From: {offer.fromUser}";

            // Summarize offered resources
            var offeredList = offer.offered.GetResourceDictionary()
                .Where(kvp => kvp.Value > 0)
                .Select(kvp => $"{kvp.Value} × {kvp.Key}");
            string offeredText = offeredList.Any() ? string.Join(", ", offeredList) : "Nothing";

            // Summarize requested resources
            var requestedList = offer.requested.GetResourceDictionary()
                .Where(kvp => kvp.Value > 0)
                .Select(kvp => $"{kvp.Value} × {kvp.Key}");
            string requestedText = requestedList.Any() ? string.Join(", ", requestedList) : "Nothing";

            string detailLines = $"Offers: {offeredText}\nWants:  {requestedText}";

            // 2) Populate the single Text field
            contentText.text = $"{fromLine}\n{detailLines}";

            // 3) Show the “Incoming Trade Offer” panel
            tradeOfferPanel.SetActive(true);

            // 4) Re‐wire Accept/Decline button callbacks so they pass the real fromUser:
            acceptButton.onClick.RemoveAllListeners();
            declineButton.onClick.RemoveAllListeners();

            acceptButton.onClick.AddListener(() => RespondToOffer(offer.fromUser, true));
            declineButton.onClick.AddListener(() => RespondToOffer(offer.fromUser, false));
        }

        #endregion

        #region “Respond to Incoming Offer” Helper

        /// <summary>
        /// Called by Accept/Decline buttons inside tradeOfferPanel.
        /// </summary>
        public void RespondToOffer(string fromUser, bool accepted)
        {
            // 1) Immediately hide the incoming‐offer panel
            tradeOfferPanel.SetActive(false);

            // 2) Build the TradeResponseMessage and send it
            var response = new TradeResponseMessage
            {
                fromUser = currentUserName,
                toUser = fromUser,
                accepted = accepted
            };

            _ = WebSocketService.SendTradeResponse(response);

            // (Optionally: you can show a small “You accepted” or “You declined” message here)
        }

        #endregion

        #region Trade‐Response Handler (unchanged)

        private void HandleTradeResponse(TradeResponseMessage resp)
        {
            if (!resp.toUser.Equals(currentUserName, StringComparison.OrdinalIgnoreCase))
                return;

            if (resp.accepted)
                Debug.Log($"[TradeScreenManager] Trade ACCEPTED by {resp.fromUser}");
            else
                Debug.Log($"[TradeScreenManager] Trade DENIED by {resp.fromUser}");
        }

        #endregion
    }
}
