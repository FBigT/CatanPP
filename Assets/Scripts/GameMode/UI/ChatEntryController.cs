using Assets.Scripts.User;
using UnityEngine.UIElements;

namespace Assets.Scripts.GameMode.UI
{
    public class ChatEntryController
    {
        Label m_NameLabel;

        // This function retrieves a reference to the 
        // character name label inside the UI element.
        public void SetVisualElement(VisualElement visualElement)
        {
            m_NameLabel = visualElement.Q<Label>("ChatContent");
        }

        public void SetMessageData(ChatMessage chatMessage)
        {
            m_NameLabel.text = $"[{chatMessage.senderUsername}] {chatMessage.text}";
        }

    }
}
