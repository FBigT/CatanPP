namespace Assets.Scripts.Utils
{
    public class WebSocketApplicationDestinations
    {
        private WebSocketApplicationDestinations(string value) { Value = value; }

        public string Value { get; private set; }

        public static WebSocketApplicationDestinations Chat { get { return new WebSocketApplicationDestinations("/send/chat/"); } }
        public static WebSocketApplicationDestinations Moves { get { return new WebSocketApplicationDestinations("/send/move/"); } }
        public static WebSocketApplicationDestinations Players { get { return new WebSocketApplicationDestinations("/send/players/"); } }
        public override string ToString()
        {
            return Value;
        }

        public static string Construct(WebSocketApplicationDestinations path, string code)
        {
            return path.Value + code;
        }
    }
}
