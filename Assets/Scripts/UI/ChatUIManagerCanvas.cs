using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.GameMode.Trading.Models;
using Assets.Scripts.User;
using Assets.Scripts.Utils;
using Assets.Scripts.GameMode.UI;
using Assets.Scripts.Dtos.GameMoveResponses;
using TMPro;

public class ChatUIManagerCanvas : MonoBehaviour
{
    [Header("Chat Entry Prefabs")]
    public GameObject chatEntryPrefab;
    public GameObject tradeEntryPrefab;

    [Header("UI References")]
    public Transform chatContainer;
    public TMP_InputField chatInputField;

    private void OnEnable()
    {
        WebSocketService.OnTradeOfferReceived += OnTradeOfferReceived;
        WebSocketService.OnTradeResponseReceived += OnTradeResponseReceived;

        chatInputField.onEndEdit.AddListener(OnChatInputSubmitted);
    }

    private void OnDisable()
    {
        WebSocketService.OnTradeOfferReceived -= OnTradeOfferReceived;
        WebSocketService.OnTradeResponseReceived -= OnTradeResponseReceived;
        chatInputField.onEndEdit.RemoveListener(OnChatInputSubmitted);
    }

    private void Update()
    {
        WebSocketService.DispatchMessageQueue();
    }

    private async void OnChatInputSubmitted(string input)
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            string message = chatInputField.text.Trim();
            if (!string.IsNullOrEmpty(message))
            {
                await WebSocketService.SendMessage(message);
                chatInputField.text = string.Empty;
            }
        }
    }

    private void OnTradeOfferReceived(TradeOfferMessage offer)
    {
        Debug.Log($"[ChatUIManagerCanvas] Trade offer from {offer.fromUser}");

        string summary = $"{offer.fromUser} offers {BuildSummary(offer.offered)} for {BuildSummary(offer.requested)}";

        var chatMsg = new ChatMessage
        {
            messageType = ChatMessageType.TradeRequest,
            senderUsername = offer.fromUser,
            text = summary,
            timestamp = DateTimeOffset.Now.ToString("o"),
            toUser = offer.toUser,
            payloadJson = JsonUtility.ToJson(offer)
        };

        //WebSocketService.SendGameMove(chatMsg);

        CreateChatEntry(chatMsg);
    }

    private void OnTradeResponseReceived(TradeResponseMessage resp)
    {
        if (resp.toUser != LocalStorageService.GetString("username")) return;

        string text = resp.accepted
            ? $"{resp.fromUser} accepted your trade."
            : $"{resp.fromUser} declined your trade.";

        var chatMsg = new ChatMessage
        {
            messageType = ChatMessageType.Text,
            senderUsername = resp.fromUser,
            text = text,
            timestamp = DateTimeOffset.Now.ToString("o")
        };

        //WebSocketService.RaiseChatMessage(chatMsg);
        CreateChatEntry(chatMsg);
    }

    private void CreateChatEntry(ChatMessage message)
    {
        GameObject prefab = message.messageType == ChatMessageType.TradeRequest
            ? tradeEntryPrefab
            : chatEntryPrefab;

        GameObject entry = Instantiate(prefab, chatContainer);
        Text textComponent = entry.GetComponentInChildren<Text>();
        if (textComponent != null)
        {
            textComponent.text = $"{message.senderUsername}: {message.text}";
        }
        else
        {
            Debug.LogWarning("Chat entry prefab missing Text component.");
        }
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
