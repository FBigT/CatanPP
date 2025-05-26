using System;

namespace Assets.Scripts.User
{
    [Serializable]
    public class RegisterForm
    {
        public RegisterForm(string name, string email, string password)
        {
            this.username = name;
            this.email = email;
            this.password = password;
        }

        public string username;
        public string email;
        public string password;
    }
}
