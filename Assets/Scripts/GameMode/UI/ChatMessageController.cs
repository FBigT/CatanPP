using System.Collections.Generic;
using Assets.Scripts.User;
using Assets.Scripts.Utils;
using UnityEngine.UIElements;

namespace Assets.Scripts.GameMode.UI
{
    public class ChatMessageController
    {
        public VisualTreeAsset textEntryTemplate;
        public VisualTreeAsset tradeEntryTemplate;
        // UXML template for list entries

        // UI element references
        ListView m_ChatBox;

        List<ChatMessage> m_AllMessages;

        public void InitializeCharacterList(VisualElement root)
        {
            m_AllMessages = new List<ChatMessage>();

            // Store a reference to the template for the list entries
            // Store a reference to the character list element
            m_ChatBox = root.Q<ListView>("ChatContainer");

            // Set up a make item function for a list entry
            m_ChatBox.makeItem = () => {
                var container = new VisualElement();
                container.style.flexShrink = 1;
                container.style.flexGrow = 0;
                return container;
            };

            m_ChatBox.bindItem = (item, idx) => {
                item.Clear();
                var msg = m_AllMessages[idx];
                if (msg.messageType == ChatMessageType.TradeRequest)
                {
                    // trade‐offer UI
                    var tradeVE = tradeEntryTemplate.Instantiate();
                    var tradeCtl = new TradeRequestEntryController(tradeVE);
                    tradeCtl.Bind(msg);
                    item.Add(tradeVE);
                }
                else
                {
                    // normal chat line
                    var textVE = textEntryTemplate.Instantiate();
                    var textCtl = new ChatEntryController();
                    textVE.userData = textCtl;
                    textCtl.SetVisualElement(textVE);
                    textCtl.SetMessageData(msg);
                    item.Add(textVE);
                }
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
