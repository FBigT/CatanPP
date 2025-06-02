using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Assets.Scripts.User;
using Assets.Scripts.Utils;
using Assets.Scripts.Dtos.GameMoveResponses;
using Assets.Scripts.GameMode.UI;
using Assets.Scripts.GameMode.Trading.Models;
using System.Linq;



namespace Assets.Scripts.UI
{
    public class TradeRequestUI : MonoBehaviour
    {
        public TMP_Text contentText;
        public Button acceptButton;
        public Button denyButton;

        // The TradeOfferMessage payload:
        private TradeOfferMessage _offer;

        /// <summary>
        /// Call this immediately after Instantiate(prefab) to initialize.
        /// </summary>
        public void Initialize(TradeOfferMessage offer)
        {
            _offer = offer;

            // Build a human‐readable summary:
            string summary = $"{offer.fromUser} offers " +
                             $"{BuildSummary(offer.offered)} for {BuildSummary(offer.requested)}";

            contentText.text = summary;

            // Only show buttons if “I” am the recipient:
            string me = LocalStorageService.GetString("username");
            bool isTarget = (offer.toUser == me);
            acceptButton.gameObject.SetActive(isTarget);
            denyButton.gameObject.SetActive(isTarget);

            acceptButton.onClick.AddListener(() => SendResponse(true));
            denyButton.onClick.AddListener(() => SendResponse(false));

            // If you want auto‐deny, uncomment this line instead of manual click:
            // if (isTarget) _ = DelayAutoResponse(false);
        }

        private string BuildSummary(ResourceGroup g)
        {
            return string.Join(", ",
                g.GetResourceDictionary()
                 .Where(kvp => kvp.Value > 0)
                 .Select(kvp => $"{kvp.Value} {kvp.Key}")
            );
        }

        private async System.Threading.Tasks.Task DelayAutoResponse(bool accepted)
        {
            await System.Threading.Tasks.Task.Delay(100);
            SendResponse(accepted);
        }

        private async void SendResponse(bool accepted)
        {
            // Build the TradeResponseMessage from _offer:
            var resp = new TradeResponseMessage
            {
                fromUser = LocalStorageService.GetString("username"),
                toUser = _offer.fromUser,
                accepted = accepted,
                offered = _offer.offered,
                requested = _offer.requested,
                sessionId = LocalStorageService.GetInt("session-id") ?? 0
            };

            // Send the trade response over WebSocket:
            await WebSocketService.SendTradeResponse(resp);
            Debug.Log($"[TradeRequestUI] Auto-response: {(accepted ? "ACCEPTED" : "DENIED")} trade from {_offer.fromUser}");

            // Disable buttons so user can’t click again:
            acceptButton.interactable = false;
            denyButton.interactable = false;

            // Also send a normal chat message so everyone sees “X accepted/declined the trade.”
            var chatMsg = new ChatMessage
            {
                senderUsername = resp.fromUser,
                toUser = resp.toUser,
                text = $"{resp.fromUser} {(accepted ? "accepted" : "declined")} the trade offer.",
                timestamp = System.DateTime.UtcNow.ToString("o"),
                messageType = ChatMessageType.Text
            };
            await WebSocketService.SendMessage(JsonUtility.ToJson(chatMsg));
        }
    }
}
