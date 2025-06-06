using System;

namespace Assets.Scripts.Dtos
{
    [Serializable]
    public class EncryptedMessage
    {
        public string crypto;

        public EncryptedMessage(string crypto)
        {
            this.crypto = crypto;
        }
    }
}
