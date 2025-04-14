using System;

namespace Assets.Scripts.User
{
    [Serializable]
    public class LoginResponse
    {
        public string username;
        public string tokenType;
        public long userId;
        public string token;
    }
}
