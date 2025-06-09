using System;

namespace Assets.Scripts.Dtos
{
    [Serializable]
    public class EncryptedMessage
    {
        public string encryptedKey;
        public string payload;

        public EncryptedMessage(string encryptedKey, string payload)
        {
            this.encryptedKey = encryptedKey;
            this.payload = payload;
        }
    }
}
