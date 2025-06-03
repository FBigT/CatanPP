using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Assets.Scripts.User;
using Assets.Scripts.Utils;
using Assets.Scripts.Dtos.GameMoveResponses;
using Assets.Scripts.GameMode.Trading.Models;
using System.Linq;

namespace Assets.Scripts.UI
{
    public class TradeRequestUI : MonoBehaviour
    {
        public TMP_Text contentText;
        public Button acceptButton;
        public Button denyButton;

        private TradeOfferMessage _offer;

        public void Initialize(TradeOfferMessage offer)
        {
            _offer = offer;

            string summary = $"{offer.fromUser} offers {BuildSummary(offer.offered)} " +
                             $"for {BuildSummary(offer.requested)}";

            contentText.text = summary;

            string me = LocalStorageService.GetString("username");
            bool isTarget = offer.toUser == me;

            acceptButton.gameObject.SetActive(isTarget);
            denyButton.gameObject.SetActive(isTarget);

            acceptButton.onClick.AddListener(() => SendResponse(true));
            denyButton.onClick.AddListener(() => SendResponse(false));
        }

        private string BuildSummary(ResourceGroup g)
        {
            return string.Join(", ",
                g.GetResourceDictionary()
                 .Where(kvp => kvp.Value > 0)
                 .Select(kvp => $"{kvp.Value} {kvp.Key}")
            );
        }

        private async void SendResponse(bool accepted)
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

            Debug.Log($"[TradeRequestUI] Sent response: {(accepted ? "ACCEPTED" : "DENIED")} to {_offer.fromUser}");

            acceptButton.interactable = false;
            denyButton.interactable = false;

            // (Removed chat‐message send so nothing goes into the chat box)
            // await WebSocketService.SendMessage(JsonUtility.ToJson(chatMsg));

            // Auto‐close after 2 seconds
            await System.Threading.Tasks.Task.Delay(2000);
            Destroy(gameObject);
        }
    }
}
