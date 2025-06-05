using System.Collections.Generic;
using Assets.Scripts.Dtos.GameMoveResponses;
using Assets.Scripts.UI;
using Assets.Scripts.User;
using Assets.Scripts.Utils;
using TMPro;
using UnityEngine;

public class NewChatboxScript : MonoBehaviour
{
    public GameObject chatPanel, textPrefab;
    public TMP_InputField chatInput;
    public GameObject tradePrefab;

    [SerializeField]
    List<Message> messageList = new List<Message>();

    void Start()
    {
        chatInput.onSubmit.AddListener(async text =>
        {
            if (!chatInput.wasCanceled)
            {
                string message = chatInput.text.Trim();
                if (!string.IsNullOrEmpty(message))
                {
                    await WebSocketService.SendMessage(message);
                    chatInput.text = string.Empty;
                }
            }
        });
        WebSocketService.OnChatMessageReceived += OnMessage;
    }

    void Update()
    {
        WebSocketService.DispatchMessageQueue();   
    }

    private void OnMessage(ChatMessage chatMessage)
    {
        // 1) If this is a trade request, show the trade UI instead of plain text
        if (chatMessage.messageType == ChatMessageType.TradeRequest)
        {
            // Deserialize the payload into a TradeOfferMessage
            var offer = JsonUtility.FromJson<TradeOfferMessage>(chatMessage.payloadJson);

            // Instantiate the tradePrefab under chatPanel
            GameObject go = Instantiate(tradePrefab, chatPanel.transform);

            // Initialize its TradeRequestUI component so it shows the offer text and wires up Accept/Deny buttons
            var ui = go.GetComponent<TradeRequestUI>();
            ui.Initialize(offer);

            // (Optional) keep a reference if you want to remove it later
            messageList.Add(new Message { gameObject = go });
        }
        else
        {
            // 2) Otherwise, treat it as a normal chat line
            Message newMessage = new()
            {
                text = chatMessage.ToString()
            };

            GameObject newText = Instantiate(textPrefab, chatPanel.transform);
            newMessage.textObject = newText.GetComponent<TMP_Text>();
            newMessage.textObject.text = newMessage.text;
            messageList.Add(newMessage);
        }
    }

}

public class Message
{
    public string text;
    public TMP_Text textObject;
    public GameObject gameObject;  

}
