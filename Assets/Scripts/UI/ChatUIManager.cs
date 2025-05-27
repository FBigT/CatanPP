// Assets/Scripts/UI/Test/ChatUIManager.cs
using System.Linq;
using Assets.Scripts.GameMode.UI;
using Assets.Scripts.GameMode.Trading.Models;
using Assets.Scripts.User;
using Assets.Scripts.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class ChatUIManager : MonoBehaviour
    {
        [Header("Chat Entry Templates")]
        public VisualTreeAsset chatEntryTemplateUxml;   // existing template
        public VisualTreeAsset tradeEntryTemplateUxml;  // new: trade-offer template

        private ChatMessageController chatController;

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            VisualElement chatContainerRoot = root.Q<VisualElement>("ChatContainer");
            TextField chatInputField = root.Q<TextField>("ChatInput");

            // Subscribe to incoming trade-offers and forward into chat
            WebSocketService.OnTradeOfferReceived += offer =>
            {
                // Build a human-readable summary: "2 wood, 1 sheep"
                string summary = $"{offer.fromUser} offers " +
                                 $"{BuildSummary(offer.offered)} for {BuildSummary(offer.requested)}";

                var chatMsg = new ChatMessage
                {
                    messageType = ChatMessageType.TradeRequest,
                    senderUsername = offer.fromUser,
                    text = summary,
                    timestamp = System.DateTimeOffset.Now.ToString("o"),
                    toUser = offer.toUser,
                    payloadJson = JsonUtility.ToJson(offer)
                };

                // Inject into normal chat stream
                WebSocketService.RaiseChatMessage(chatMsg);
            };

            // Initialize chat controller with both templates
            chatController = new ChatMessageController
            {
                textEntryTemplate = chatEntryTemplateUxml,
                tradeEntryTemplate = tradeEntryTemplateUxml
            };
            chatController.InitializeCharacterList(chatContainerRoot, chatEntryTemplateUxml);

            // Hook up sending regular chat text
            chatInputField.RegisterCallback<KeyUpEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                {
                    string message = chatInputField.value.Trim();
                    if (!string.IsNullOrEmpty(message))
                    {
                        WebSocketService.SendMessage(message);
                        chatInputField.value = string.Empty;
                    }
                }
            });
        }

        private void Update()
        {
            WebSocketService.DispatchMessageQueue();
        }

        /// <summary>
        /// Helper to turn ResourceGroup into comma-separated list
        /// </summary>
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
