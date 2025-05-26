using System;

namespace Assets.Scripts.User
{
    [Serializable]
    public class GuestRegisterResponse
    {
        public long guestId;
        public string username;
        public string guestKey;
    }
}
