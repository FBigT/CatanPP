namespace Assets.Scripts.Utils
{
        public static class EndpointUtils
        {
            // ───────────────────────────────────────────────────────────────────────
            //    BASE URL
            // ───────────────────────────────────────────────────────────────────────
            public static string BaseUrl { get; } = "http://localhost:8080/api";

            // Add this alias so your existing PlaceStructure/PlaceRoad code (which uses BASE) compiles:
            private static readonly string BASE = BaseUrl;

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
                => Users + "/" + username;

            public static string GetUserById(long id)
                => Users + "/" + id;

            public static string GuestLogin()
                => Login + "/guest";

            public static string DeactivateUser(long id)
                => Users + "/" + id;

            public static string UpdateUser(long id)
                => Users + "/" + id;

            public static string GetPlayerPorfileById(long id)
                => Proflie + "/" + id;

            public static string GetPlayerPorfileByUsername(string username)
                => Proflie + "/" + username;

            // ----------------------------------------------------------------
            //  SESSION
            // ----------------------------------------------------------------
            public static string Sessions { get; } = BaseUrl + "/sessions";
            public static string CloseSession { get; } = BaseUrl + "/sessions/close";
            public static string SessionSaves { get; } = Sessions + "/saves";
            public static string Save { get; } = Sessions + "/save";

            public static string CreateSessions(int numberOfPlayers)
                => Sessions + "/" + numberOfPlayers;

            public static string JoinSession(string code)
                => Sessions + "/join/" + code;

            public static string DeleteSessionSave(long id)
                => Save + "/" + id;

            // <<< NEW ALIAS for load-game >>> 
            public static string GetSessionSaves => SessionSaves;

            // ----------------------------------------------------------------
            //  DICE
            // ----------------------------------------------------------------
            public static string Dice { get; } = BaseUrl + "/dice";
            public static string Roll()
                => Dice + "/roll";

            // ----------------------------------------------------------------
            //  CITY (Old code for a different approach)
            // ----------------------------------------------------------------
            public static string City { get; } = BaseUrl + "/cities";
            public static string PlaceSettlement { get; } = City + "/place";
            public static string UpgradeToCity(long id)
                => City + "/" + id + "/upgrade";

            // ----------------------------------------------------------------
            //  TRADING PORT
            // ----------------------------------------------------------------
            public static string TradingPort { get; } = BaseUrl + "/trading-ports";
            public static string UpdatePortPlaceent(long id)
                => TradingPort + "/" + id + "/place";

            // ----------------------------------------------------------------
            //  GAME / RESOURCES (New to fetch resources after dice/purchases)
            // ----------------------------------------------------------------
            public static string Game => BaseUrl + "/game";
            public static string GetResources => Game + "/resources";

            // ── NEW: PLACEMENT ─────────────────────────────────────────────────────
            // match your backend’s `/api/place/...` routes
            public static string PlaceStructure(string owner, int tileId, int cornerIndex)
                => $"{BASE}/place/structure?owner={owner}&tileId={tileId}&cornerIndex={cornerIndex}";

            public static string PlaceRoad(string owner, int tileId, int edgeIndex)
                => $"{BASE}/place/road?owner={owner}&tileId={tileId}&edgeIndex={edgeIndex}";

            public static string UpgradeSettlementToCity(string owner, int tileId, int cornerIndex)
                => $"{BASE}/place/structure/upgrade?owner={owner}&tileId={tileId}&cornerIndex={cornerIndex}";
        }

}
