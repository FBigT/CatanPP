namespace Assets.Scripts.User
{
    public class LoginResponse
    {
        public string Username { get; set; }
        public string TokenType { get; set; }
        public long UserId { get; set; }
        public string Token { get; set; }
    }
}
