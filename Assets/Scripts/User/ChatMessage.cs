using System;

namespace Assets.Scripts.User
{
    public class ChatMessage
    {
        public string senderUsername;
        public string text;
        public string timestamp;
        private DateTimeOffset? parsedTimestamp;

        public DateTimeOffset ParsedTimestamp
        {
            get
            {
                if (parsedTimestamp == null && !string.IsNullOrEmpty(timestamp))
                    parsedTimestamp = DateTimeOffset.Parse(timestamp);
                return parsedTimestamp ?? default;
            }
        }

        public override string ToString() {
            UnityEngine.Debug.Log(timestamp.ToString());
            return $"[{ParsedTimestamp:HH:mm}] {senderUsername} : {text}";
        }
    }
}
