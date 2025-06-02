using System.Collections.Generic;
using Assets.Scripts.User;
using Assets.Scripts.Utils;
using TMPro;
using UnityEngine;

public class NewChatboxScript : MonoBehaviour
{
    public GameObject chatPanel, textPrefab;
    public TMP_InputField chatInput;

    [SerializeField]
    List<Message> messageList = new List<Message>();

    void Start()
    {
        chatInput.onSubmit.AddListener(text =>
        {
            if (!chatInput.wasCanceled)
            {
                string message = chatInput.text.Trim();
                if (!string.IsNullOrEmpty(message))
                {
                    WebSocketService.SendMessage(message);
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

public class Message
{
    public string text;
    public TMP_Text textObject;
}
