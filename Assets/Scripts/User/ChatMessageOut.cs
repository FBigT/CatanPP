using System;

namespace Assets.Scripts.User
{
    [Serializable]
    public class ChatMessageOut
    {
        public string text;

        public ChatMessageOut(string text)
        {
            this.text = text;
        }
    }
}
