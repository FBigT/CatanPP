using System.Collections.Generic;
using Assets.Scripts.User;
using Assets.Scripts.Utils;
using UnityEngine.UIElements;

namespace Assets.Scripts.GameMode.UI
{
    public class ChatMessageController
    {
        // UXML template for list entries
        VisualTreeAsset m_ListEntryTemplate;

        // UI element references
        ListView m_ChatBox;

        List<ChatMessage> m_AllMessages;

        public void InitializeCharacterList(VisualElement root, VisualTreeAsset listElementTemplate)
        {
            m_AllMessages = new List<ChatMessage>();

            // Store a reference to the template for the list entries
            m_ListEntryTemplate = listElementTemplate;

            // Store a reference to the character list element
            m_ChatBox = root.Q<ListView>("ChatContainer");

            // Set up a make item function for a list entry
            m_ChatBox.makeItem = () =>
            {
                var newListEntry = m_ListEntryTemplate.Instantiate();
                newListEntry.style.flexShrink = 1;
                newListEntry.style.flexGrow = 0;
                var newListEntryLogic = new ChatEntryController();

                newListEntry.userData = newListEntryLogic;

                newListEntryLogic.SetVisualElement(newListEntry);

                return newListEntry;
            };

            // Set up bind function for a specific list entry
            m_ChatBox.bindItem = (item, index) =>
            {
                (item.userData as ChatEntryController)?.SetMessageData(m_AllMessages[index]);
            };

            // Set a fixed item height matching the height of the item provided in makeItem. 
            // For dynamic height, see the virtualizationMethod property.
            m_ChatBox.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;

            // Set the actual item's source list/array
            m_ChatBox.itemsSource = m_AllMessages;

            WebSocketService.OnChatMessageReceived += OnChatMessageReceived;
        }

        public void OnChatMessageReceived(ChatMessage chatMessage) { 
            m_AllMessages.Add(chatMessage);
            m_ChatBox.Rebuild();
            m_ChatBox.ScrollToItem(m_AllMessages.Count - 1);
        }
    }
}
