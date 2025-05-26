using System;

namespace Assets.Scripts.User
{
    [Serializable]
    public class LoginForm
    {
        public LoginForm(string username, string password)
        {
            this.username = username;
            this.password = password;
        }

        public string username;
        public string password;
    }
}
