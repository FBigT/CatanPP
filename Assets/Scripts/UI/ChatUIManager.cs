// Assets/Scripts/UI/Test/ChatUIManager.cs
using Assets.Scripts.GameMode.UI;
using Assets.Scripts.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.UI
{
    public class ChatUIManager : MonoBehaviour
    {
        public VisualTreeAsset chatEntryTemplateUxml;

        private ChatMessageController chatController;

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            VisualElement chatContainerRoot = root.Q("ChatContainer");
            TextField chatInputField = root.Q<TextField>("ChatInput");

            chatController = new ChatMessageController();
            chatController.InitializeCharacterList(chatContainerRoot, chatEntryTemplateUxml);

            chatInputField.RegisterCallback<KeyUpEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                {
                    string message = chatInputField.value.Trim();
                    if (!string.IsNullOrEmpty(message))
                    {
                        HandleChatMessage(message);
                        chatInputField.value = string.Empty;
                    }
                }
            });
        }

        private void HandleChatMessage(string message)
        {
            WebSocketService.SendMessage(message);
        }

        private void Update()
        {
            WebSocketService.DispatchMessageQueue();
        }
    }
}
