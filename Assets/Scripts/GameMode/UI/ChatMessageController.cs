// Assets/Scripts/GameMode/UI/ChatMessageController.cs
using System.Collections.Generic;
using Assets.Scripts.User;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.GameMode.UI
{
    public class ChatMessageController
    {
        // We only need the text‐entry template now:
        public VisualTreeAsset textEntryTemplate;

        private readonly List<ChatMessage> _allMessages = new();
        private ListView _chatBox;

        /// <summary>
        /// Call this once, passing in the root VisualElement (from UIDocument.rootVisualElement).
        /// It looks up a ListView named "ChatContainer" and hooks it up.
        /// </summary>
        public void InitializeCharacterList(VisualElement root)
        {
            _chatBox = root.Q<ListView>("ChatContainer");
            if (_chatBox == null)
            {
                Debug.LogError("[ChatMessageController] Could not find a ListView named \"ChatContainer\".");
                return;
            }

            // Each item is a VisualElement that we will populate manually:
            _chatBox.makeItem = () => new VisualElement();
            _chatBox.bindItem = BindMessageToItem;
            _chatBox.itemsSource = _allMessages;
            _chatBox.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
        }

        /// <summary>
        /// Invoke this whenever a new chat message (of type Text) arrives.
        /// </summary>
        public void OnChatMessageReceived(ChatMessage chatMessage)
        {
            Debug.Log($"[ChatController] Message received: {chatMessage.text}");
            _allMessages.Add(chatMessage);
            _chatBox.Rebuild();
            _chatBox.ScrollToItem(_allMessages.Count - 1);
        }

        /// <summary>
        /// Called by ListView for each item it needs to display.
        /// We clone a copy of textEntryTemplate and fill in the Label.
        /// </summary>
        private void BindMessageToItem(VisualElement container, int index)
        {
            container.Clear();
            var msg = _allMessages[index];

            // Always treat as a normal text message (no trade messages in chat):
            var ve = textEntryTemplate.CloneTree();
            var ctl = new ChatEntryController();
            ctl.SetVisualElement(ve);
            ctl.SetMessageData(msg);
            container.Add(ve);
        }
    }
}
