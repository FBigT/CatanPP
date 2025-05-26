namespace Assets.Scripts.Utils
{
    public class WebSocketBrokerDestinations
    {
        private WebSocketBrokerDestinations(string value) { Value = value; }

        public string Value { get; private set; }

        public static WebSocketBrokerDestinations Chat { get { return new WebSocketBrokerDestinations("/game/chat/"); } }
        public static WebSocketBrokerDestinations Moves { get { return new WebSocketBrokerDestinations("/game/moves/"); } }
        public static WebSocketBrokerDestinations Players { get { return new WebSocketBrokerDestinations("/game/players/"); } }
        public override string ToString()
        {
            return Value;
        }

        public static string Construct(WebSocketBrokerDestinations path, string code) { 
            return path.Value + code;
        }
    }
}
