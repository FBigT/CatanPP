using Assets.Scripts.Dtos.GameMoveResponses;
using Assets.Scripts.GameMode.Trading.Models;
using Assets.Scripts.Utils;
using Assets.Scripts.User;
using UnityEngine.UIElements;
using Assets.Scripts.GameMode.Trading;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;

namespace Assets.Scripts.GameMode.UI
{
    public class TradeRequestEntryController
    {
        private readonly Label _contentLabel;
        private readonly Button _acceptBtn;
        private readonly Button _denyBtn;
        private TradeOfferMessage _offer;
        private readonly VisualElement _root;

        public TradeRequestEntryController(VisualElement root)
        {
            _root = root;
            _contentLabel = root.Q<Label>("ChatContent");
            _acceptBtn = root.Q<Button>("AcceptButton");
            _denyBtn = root.Q<Button>("DenyButton");

            _acceptBtn.clicked += () => SendResponse(true);
            _denyBtn.clicked += () => SendResponse(false);
        }

        /// <summary>
        /// Called by ChatMessageController.bindItem
        /// </summary>
        public void Bind(ChatMessage msg)
        {
            _offer = JsonUtility.FromJson<TradeOfferMessage>(msg.payloadJson);

            _contentLabel.text = $"{_offer.fromUser} offers " +
                $"{BuildSummary(_offer.offered)} for {BuildSummary(_offer.requested)}";

            // show buttons *only* to the target player
            string me = LocalStorageService.GetString("username");
            bool isTarget = msg.toUser == me;
            _acceptBtn.visible = _denyBtn.visible = isTarget;

            // === AUTO RESPONSES (UNCOMMENT ONE LINE ONLY) ===

            // Auto-accept the trade offer
            // if (isTarget) _ = DelayAutoResponse(true);

            // Auto-deny the trade offer
            if (isTarget) _ = DelayAutoResponse(false);
        }

        private async Task DelayAutoResponse(bool accepted)
        {
            await Task.Delay(100); // wait 100ms before sending response
            SendResponse(accepted);
        }

        async void SendResponse(bool accepted)
        {
            var resp = new TradeResponseMessage
            {
                fromUser = LocalStorageService.GetString("username"),
                toUser = _offer.fromUser,
                accepted = accepted,
                offered = _offer.offered,
                requested = _offer.requested,
                sessionId = LocalStorageService.GetInt("session-id") ?? 0
            };

            await WebSocketService.SendTradeResponse(resp);

            Debug.Log($"[TradeRequestEntry] Auto-response: {(accepted ? "ACCEPTED" : "DENIED")} trade offer from {_offer.fromUser}.");

            _acceptBtn.SetEnabled(false);
            _denyBtn.SetEnabled(false);

            var msg = new ChatMessage
            {
                senderUsername = resp.fromUser,
                toUser = resp.toUser,
                text = $"{resp.fromUser} {(accepted ? "accepted" : "declined")} the trade offer.",
                timestamp = System.DateTime.UtcNow.ToString("o"),
                messageType = ChatMessageType.Text
            };
            await WebSocketService.SendMessage(JsonUtility.ToJson(msg));
        }

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
