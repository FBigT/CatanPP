namespace Assets.Scripts.Utils
{
    public static class EndpointUtils
    {
        public static string BaseUrl { get; } = "http://localhost:8080/api";

        // ----------------------------------------------------------------
        //  USER
        // ----------------------------------------------------------------
        public static string Users { get; } = BaseUrl + "/users";
        public static string Login { get; } = Users + "/login";
        public static string Register { get; } = Users + "/register";
        public static string RegisterGuest { get; } = Register + "/guest";
        public static string Proflie { get; } = Users + "/profile";
        public static string Refresh { get; } = Users + "/refresh";

        public static string GetUserByUsername(string username)
        {
            return Users + "/" + username;
        }

        public static string GetUserById(long id)
        {
            return Users + "/" + id;
        }

        public static string GuestLogin()
        {
            return Login + "/guest";
        }

        public static string DeactivateUser(long id)
        {
            return Users + "/" + id;
        }

        public static string UpdateUser(long id)
        {
            return Users + "/" + id;
        }

        public static string GetPlayerPorfileById(long id)
        {
            return Proflie + "/" + id;
        }

        public static string GetPlayerPorfileByUsername(string username)
        {
            return Proflie + "/" + username;
        }

        // ----------------------------------------------------------------
        //  SESSION
        // ----------------------------------------------------------------
        public static string Sessions { get; } = BaseUrl + "/sessions";
        public static string CloseSession { get; } = BaseUrl + "/sessions/close";
        public static string SessionSaves { get; } = Sessions + "/saves";
        public static string Save { get; } = Sessions + "/save";

        public static string CreateSessions(int numberOfPlayers)
        {
            return Sessions + "/" + numberOfPlayers;
        }

        public static string JoinSession(string code)
        {
            return Sessions + "/join/" + code;
        }

        public static string DeleteSessionSave(long id)
        {
            return Save + "/" + id;
        }

        // ----------------------------------------------------------------
        //  DICE
        // ----------------------------------------------------------------
        public static string Dice { get; } = BaseUrl + "/dice";
        public static string Roll()
        {
            return Dice + "/roll";
        }

        // ----------------------------------------------------------------
        //  CITY (Old code for a different approach)
        // ----------------------------------------------------------------
        public static string City { get; } = BaseUrl + "/cities";
        public static string PlaceSettlement { get; } = City + "/place";
        public static string UpgradeToCity(long id)
        {
            return City + "/" + id + "/upgrade";
        }

        // ----------------------------------------------------------------
        //  TRADING PORT
        // ----------------------------------------------------------------
        public static string TradingPort { get; } = BaseUrl + "/trading-ports";
        public static string UpdatePortPlaceent(long id)
        {
            return TradingPort + "/" + id + "/place";
        }

        // ----------------------------------------------------------------
        //  GAME / RESOURCES (New to fetch resources after dice/purchases)
        // ----------------------------------------------------------------
        public static string Game => BaseUrl + "/game";
        public static string GetResources => Game + "/resources";

        // ----------------------------------------------------------------
        //  NEW: PLACEMENT ENDPOINTS (Matches your /api/place/...)
        //  (Keeping old city endpoints above and just adding these.)
        // ----------------------------------------------------------------
        public static string PlaceStructure(string owner, int tileId, int cornerIndex)
        {
            // => POST /api/place/structure?owner=...&tileId=...&cornerIndex=...
            return $"{BaseUrl}/place/structure?owner={owner}&tileId={tileId}&cornerIndex={cornerIndex}";
        }

        public static string PlaceRoad(string owner, int tileId, int edgeIndex)
        {
            // => POST /api/place/road?owner=...&tileId=...&edgeIndex=...
            return $"{BaseUrl}/place/road?owner={owner}&tileId={tileId}&edgeIndex={edgeIndex}";
        }

        public static string UpgradeSettlementToCity(string owner, int tileId, int cornerIndex)
        {
            // => PUT /api/place/structure/upgrade?owner=...&tileId=...&cornerIndex=...
            return $"{BaseUrl}/place/structure/upgrade?owner={owner}&tileId={tileId}&cornerIndex={cornerIndex}";
        }
    }
}
