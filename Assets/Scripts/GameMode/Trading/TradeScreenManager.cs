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
using Assets.Scripts.User;                    // for LocalStorageService
using Assets.Scripts.Dtos.GameMoveResponses;  // for TradeResponseMessage & TradeExecutedDto

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

        [Header("Trade Popup (Player A only)")]
        [Tooltip("A single popup panel that displays 'Trade Sent', 'Trade Accepted!', or 'Trade Declined!'.")]
        public GameObject tradeSentPanel;

        [Tooltip("The TextMeshProUGUI inside tradeSentPanel where we write the dynamic message.")]
        public TextMeshProUGUI sentMessageText;

        [Header("Incoming-offer Popup (Player B only)")]
        [Tooltip("Shown to Player B when Player A sends a trade.")]
        public GameObject tradeOfferPanel;

        [Tooltip("TextMeshProUGUI inside tradeOfferPanel for 'From: …' and 'Offers: … Wants: …'.")]
        public TextMeshProUGUI contentText;

        [Tooltip("Accept button inside tradeOfferPanel.")]
        public Button acceptButton;

        [Tooltip("Decline button inside tradeOfferPanel.")]
        public Button declineButton;

        // Holds the incoming offer so that RespondToOffer can forward the exact ResourceGroups
        private TradeOfferMessage pendingOffer;

        readonly List<string> selectedRequestedResources = new();
        readonly List<int> selectedRequestedQuantities = new();
        readonly List<string> selectedOfferedResources = new();
        readonly List<int> selectedOfferedQuantities = new();

        private long sessionId;
        private string currentUserName;

        public void ResetAllResourceSelections()
        {
            foreach (var handler in requestResourceButtons)
            {
                handler.ResetQuantity();
            }

            foreach (var handler in offerResourceButtons)
            {
                handler.ResetQuantity();
            }
        }
        private void Awake()
        {
            sessionId = LocalStorageService.GetInt("session-id") ?? 0;
            currentUserName = LocalStorageService.GetString("username") ?? "";
        }

        private void OnEnable()
        {
            WebSocketService.OnTradeResponseReceived += HandleTradeResponse;
            WebSocketService.OnPlayerJoined += HandlePlayerJoined;
            WebSocketService.OnTradeOfferReceived += HandleTradeOffer;
            WebSocketService.OnTradeExecuted += HandleTradeExecuted;
        }

        private void OnDisable()
        {
            WebSocketService.OnTradeResponseReceived -= HandleTradeResponse;
            WebSocketService.OnPlayerJoined -= HandlePlayerJoined;
            WebSocketService.OnTradeOfferReceived -= HandleTradeOffer;
            WebSocketService.OnTradeExecuted -= HandleTradeExecuted;
        }

        private void Start()
        {
            RefreshPlayerDropdown();

            // Connect to WebSocket if not already
            string sessionCode = LocalStorageService.GetString("session-code");
            if (!WebSocketService.Connected)
            {
                Debug.Log("[TradeScreenManager] Connecting to WebSocket…");
                WebSocketService.ConnectToChat(sessionCode).ContinueWith(_ => { });
            }

            // Hide both popups at launch
            tradeSentPanel.SetActive(false);
            tradeOfferPanel.SetActive(false);
        }

        /// <summary>
        /// We must call this on every frame so NativeWebSocket can pump incoming messages.
        /// Without it, only the first STOMP frame arrives; subsequent ones are ignored.
        /// </summary>
        private void Update()
        {
            WebSocketService.DispatchMessageQueue();
        }

        private void RefreshPlayerDropdown()
        {
            TradingManager.Instance.GetSessionPlayers(
                sessionId,
                OnPlayersLoaded,
                err => Debug.LogError($"[TradeScreen] Failed to load players: {err}")
            );
        }

        private void OnPlayersLoaded(List<SessionPlayerDto> players)
        {
            var otherPlayers = players
                .Where(p => !p.username.Equals(currentUserName, StringComparison.OrdinalIgnoreCase))
                .Select(p => p.username)
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var options = new List<string> { "Bank" };
            options.AddRange(otherPlayers);

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
                Debug.Log($"[CaptureSelections] {b.resourceName} → quantity={q}");
                if (q > 0)
                {
                    names.Add(b.resourceName);
                    qty.Add(q);
                }
            }
        }


        #endregion

        #region “Apply Trade” Button (Player A)

        /// <summary>
        /// Called when Player A taps “Apply Trade.”
        /// If “Bank,” do a bank trade; otherwise send a PlayerTradeDto → server,
        /// then dispatch a STOMP frame. Once WebSocket send completes, show “Trade Sent.”
        /// </summary>
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
                // ───── Player-to-player trade ─────
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
                    onSuccess: async () =>
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

                        // Await the STOMP frame so we remain on Unity’s main thread when it finishes:
                        await WebSocketService.SendTradeOffer(offerMsg);
                        Debug.Log("[TradeScreenManager] SendTradeOffer() completed");

                        // 1) Change the single popup’s text to “Trade Sent”
                        sentMessageText.text = $"Trade Sent to {toUser}";

                        // 2) Show that popup for 2 seconds
                        ShowTemporaryPopup(tradeSentPanel);
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

        #region “Incoming Trade Offer” Handler (Player B)

        /// <summary>
        /// Called whenever B’s WebSocket receives a TRADE_OFFER frame.
        /// If “toUser” == currentUserName, show B the popup.
        /// </summary>
        private void HandleTradeOffer(TradeOfferMessage offer)
        {
            if (!offer.toUser.Equals(currentUserName, StringComparison.OrdinalIgnoreCase))
                return;

            // Save the incoming offer so RespondToOffer can forward its ResourceGroups
            pendingOffer = offer;

            // Build the “From: …” line
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

            // 1) Populate the single TextMeshProUGUI inside tradeOfferPanel
            contentText.text = $"{fromLine}\n{detailLines}";

            // 2) Show the “Incoming Trade Offer” popup for Player B
            tradeOfferPanel.SetActive(true);

            // 3) Re-wire Accept and Decline to call the new RespondToOffer(bool)
            acceptButton.onClick.RemoveAllListeners();
            declineButton.onClick.RemoveAllListeners();
            acceptButton.onClick.AddListener(() => RespondToOffer(true));
            declineButton.onClick.AddListener(() => RespondToOffer(false));
        }

        #endregion

        #region “Respond to Incoming Offer” (Player B)

        /// <summary>
        /// Called when Player B taps “Accept” or “Decline” inside tradeOfferPanel.
        /// Uses pendingOffer to forward the exact ResourceGroups that Player A sent.
        /// </summary>
        public void RespondToOffer(bool accepted)
        {
            // Hide B’s incoming-offer popup immediately
            tradeOfferPanel.SetActive(false);

            if (pendingOffer == null)
            {
                Debug.LogError("[TradeScreenManager] No pending offer to respond to!");
                return;
            }

            // Build the TradeResponseMessage
            var response = new TradeResponseMessage
            {
                sessionId = sessionId,
                fromUser = currentUserName,        // Player B
                toUser = pendingOffer.fromUser,  // Player A
                accepted = accepted,
                offered = pendingOffer.offered,
                requested = pendingOffer.requested
            };

            // Tell TradingManager to POST /api/trade/response
            TradingManager.Instance.RespondToTrade(
                response,
                onSuccess: () =>
                {
                    Debug.Log("[TradeScreenManager] /api/trade/response POST succeeded");
                    // Now the server will broadcast:
                    //   1) TRADE_RESPONSE → Player A’s HandleTradeResponse shows “Accepted!” / “Declined!”
                    //   2) (if accepted) TRADE_EXECUTED → both clients’ HandleTradeExecuted(...) runs
                },
                onError: err =>
                {
                    Debug.LogError($"[TradeScreenManager] Failed to POST trade response: {err}");
                }
            );

            // Clear pendingOffer so we don’t re-use it accidentally
            pendingOffer = null;
        }

        #endregion

        #region “Incoming Trade Response” Handler (Player A)

        private void HandleTradeResponse(TradeResponseMessage resp)
        {
            Debug.Log($"[TradeScreenManager] HandleTradeResponse invoked: toUser={resp.toUser}, accepted={resp.accepted}");

            // Only Player A (the original sender) should react
            if (!resp.toUser.Equals(currentUserName, StringComparison.OrdinalIgnoreCase))
                return;

            // If accepted == true, show “Trade Accepted”; otherwise show “Trade Declined”
            if (resp.accepted)
                sentMessageText.text = "Trade Accepted!";
            else
                sentMessageText.text = "Trade Declined!";

            // Immediately show the single popup for 2 seconds
            ShowTemporaryPopup(tradeSentPanel);

            Debug.Log($"[TradeScreenManager] Trade {(resp.accepted ? "ACCEPTED" : "DECLINED")} by {resp.fromUser}");
        }

        #endregion

        #region “Incoming Trade Executed” Handler (both A & B)

        private void HandleTradeExecuted(TradeExecutedDto dto)
        {
            // 1) Was I the “fromUser” (Player A)?
            if (dto.fromUser.Equals(currentUserName, StringComparison.OrdinalIgnoreCase))
            {
                // I offered dto.offered → remove them from my inventory
                // I requested dto.requested → add them to my inventory
                Debug.Log($"[TradeScreenManager] (Player A) Remove: {ResourcesToString(dto.offered)}, Add: {ResourcesToString(dto.requested)}");
                // TODO: replace these logs with calls into your Catan.GameMode.ResourceManager / PlayerState
            }
            // 2) Or was I the “toUser” (Player B)?
            else if (dto.toUser.Equals(currentUserName, StringComparison.OrdinalIgnoreCase))
            {
                // I was the recipient → remove dto.requested, add dto.offered
                Debug.Log($"[TradeScreenManager] (Player B) Remove: {ResourcesToString(dto.requested)}, Add: {ResourcesToString(dto.offered)}");
                // TODO: replace these logs with calls into your Catan.GameMode.ResourceManager / PlayerState
            }

            // 3) Update whatever UI shows our current resources
            UpdateResourceUI();
        }

        private void UpdateResourceUI()
        {
            // Example placeholder: if you have UI texts for each resource, refresh them here.
            // e.g. woodText.text = ResourceManager.Instance.GetCurrentCounts()["wood"].ToString();
            // Implementation depends on your own resource-display system.
        }

        #endregion

        #region Helpers to show/hide popup

        private void ShowTemporaryPopup(GameObject panel)
        {
            panel.SetActive(true);
            CancelInvoke(nameof(HidePopup));
            Invoke(nameof(HidePopup), 2f);
        }

        private void HidePopup()
        {
            tradeSentPanel.SetActive(false);
        }

        #endregion

        // Helper to format a ResourceGroup as a string (for debugging/logging)
        private string ResourcesToString(ResourceGroup group)
        {
            var dict = group.GetResourceDictionary();
            return string.Join(", ", dict
                .Where(kvp => kvp.Value > 0)
                .Select(kvp => $"{kvp.Value}×{kvp.Key}"));
        }
    }
}
