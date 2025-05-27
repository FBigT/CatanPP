using Assets.Scripts.Dtos.GameMoveResponses;
using Assets.Scripts.GameMode.Trading.Models;
using Assets.Scripts.Utils;
using Assets.Scripts.User;
using UnityEngine.UIElements;
using Assets.Scripts.GameMode.Trading;
using System.Linq;
using UnityEngine;

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
        }

        async void SendResponse(bool accepted)
        {
            var resp = new TradeResponseMessage
            {
                fromUser = LocalStorageService.GetString("username"),
                toUser = _offer.fromUser,
                accepted = accepted,
                offered = _offer.offered,
                requested = _offer.requested
            };

            // ** DROP this line ** 
            // TradingManager.Instance.RespondToTrade(_offer, true, ...);

            // 1) Send the response over WebSocket
            await WebSocketService.SendTradeResponse(resp);

            // 2) Disable buttons so you can’t click twice
            _acceptBtn.SetEnabled(false);
            _denyBtn.SetEnabled(false);
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
