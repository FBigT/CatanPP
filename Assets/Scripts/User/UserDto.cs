using System;

namespace Assets.Scripts.User
{
    [Serializable]
    public class UserDto
    {
        public long id;
        public string username;
        public string email;
        public bool active;
        public bool guest;
        public DateTime createdAt;
    }
}
