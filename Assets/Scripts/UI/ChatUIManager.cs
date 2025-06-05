// Assets/Scripts/UI/Test/ChatUIManager.cs
using Assets.Scripts.User;
using Assets.Scripts.Utils;
using Assets.Scripts.GameMode.Trading.Models;
using UnityEngine;
using UnityEngine.UIElements;
using Assets.Scripts.Dtos.GameMoveResponses;

namespace Assets.Scripts.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class ChatUIManager : MonoBehaviour
    {
        [Header("Trade-Popups")]
        public GameObject tradeRequestUIPrefab;  // “TradeRequestPanel.prefab”
        public GameObject tradeSentUIPrefab;     // “TradeSentPanel.prefab”
        public Transform uiCanvasRoot;         // Your UI Canvas Transform

        private TextField _chatInputField;

        private void Awake()
        {
            // Cache the chat input field (if you still want normal chat)
            var root = GetComponent<UIDocument>().rootVisualElement;
            _chatInputField = root.Q<TextField>("ChatInput");
        }

        private void OnEnable()
        {
            WebSocketService.OnTradeOfferReceived += HandleTradeOffer;

            if (_chatInputField != null)
            {
                _chatInputField.RegisterCallback<KeyUpEvent>(OnChatInputKeyUp);
            }
        }

        private void OnDisable()
        {
            WebSocketService.OnTradeOfferReceived -= HandleTradeOffer;

            if (_chatInputField != null)
            {
                _chatInputField.UnregisterCallback<KeyUpEvent>(OnChatInputKeyUp);
            }
        }

        private void HandleTradeOffer(TradeOfferMessage offer)
        {
            Debug.Log($"[ChatUIManager] TradeOfferReceived: {offer.fromUser} → {offer.toUser}");
            string me = LocalStorageService.GetString("username");

            if (offer.toUser == me)
            {
                // Recipient sees the “accept/deny” popup
                if (tradeRequestUIPrefab == null || uiCanvasRoot == null)
                {
                    Debug.LogError("[ChatUIManager] Missing tradeRequestUIPrefab or uiCanvasRoot!");
                    return;
                }

                var popupInstance = Instantiate(tradeRequestUIPrefab, uiCanvasRoot);
                var ui = popupInstance.GetComponent<TradeRequestUI>();
                if (ui != null) ui.Initialize(offer);
            }
            else if (offer.fromUser == me)
            {
                // Sender sees the “Trade Sent” confirmation popup
                if (tradeSentUIPrefab == null || uiCanvasRoot == null)
                {
                    Debug.LogError("[ChatUIManager] Missing tradeSentUIPrefab or uiCanvasRoot!");
                    return;
                }

                var sentPopup = Instantiate(tradeSentUIPrefab, uiCanvasRoot);
                var sentUI = sentPopup.GetComponent<TradeSentUI>();
                if (sentUI != null) sentUI.Initialize(offer.toUser);
            }
        }

        private async void OnChatInputKeyUp(KeyUpEvent evt)
        {
            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
            {
                string message = _chatInputField.value.Trim();
                if (!string.IsNullOrEmpty(message))
                {
                    await WebSocketService.SendMessage(message);
                    _chatInputField.value = string.Empty;
                }
            }
        }

        private void Update()
        {
            WebSocketService.DispatchMessageQueue();
        }
    }
}
