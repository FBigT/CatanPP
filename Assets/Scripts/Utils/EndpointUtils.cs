using System;

namespace Assets.Scripts.Utils
{
    public static class EndpointUtils
    {
        public static string BaseUrl { get; } = "http://localhost:8080/api";
        private static readonly string BASE = BaseUrl;

        // USER
        public static string Users => $"{BaseUrl}/users";
        public static string Login => $"{Users}/login";
        public static string Register => $"{Users}/register";
        public static string RegisterGuest => $"{Register}/guest";
        public static string Proflie => $"{Users}/profile";
        public static string Refresh => $"{Users}/refresh";

        public static string GetUserByUsername(string username) => $"{Users}/{username}";
        public static string GetUserById(long id) => $"{Users}/{id}";
        public static string GuestLogin() => $"{Login}/guest";
        public static string DeactivateUser(long id) => $"{Users}/{id}";
        public static string UpdateUser(long id) => $"{Users}/{id}";
        public static string GetPlayerPorfileById(long id) => $"{Proflie}/{id}";
        public static string GetPlayerPorfileByUsername(string username) => $"{Proflie}/{username}";

        // SESSION
        public static string Sessions => $"{BaseUrl}/sessions";
        public static string CloseSession => $"{Sessions}/close";
        public static string SessionSaves => $"{Sessions}/saves";
        public static string Save => $"{Sessions}/save";
        public static string CreateSessions(int numberOfPlayers) => $"{Sessions}/{numberOfPlayers}";
        public static string JoinSession(string code) => $"{Sessions}/join/{code}";
        public static string DeleteSessionSave(long id) => $"{Save}/{id}";
        public static string GetSessionSaves => SessionSaves;

        // DICE
        public static string Dice => $"{BaseUrl}/dice";
        public static string Roll() => $"{Dice}/roll";

        // CITY
        public static string City => $"{BaseUrl}/cities";
        public static string PlaceSettlement => $"{City}/place";
        public static string UpgradeToCity(long id) => $"{City}/{id}/upgrade";

        // TRADING PORT
        public static string TradingPort => $"{BaseUrl}/trading-ports";
        public static string UpdatePortPlaceent(long id) => $"{TradingPort}/{id}/place";

        // GAME
        public static string Game => $"{BaseUrl}/game";
        public static string GetResources => $"{Game}/resources";

        // STRUCTURE PLACEMENT
        public static string PlaceStructure(string owner, int tileId, int cornerIndex)
            => $"{BASE}/place/structure?owner={owner}&tileId={tileId}&cornerIndex={cornerIndex}";

        public static string PlaceRoad(string owner, int tileId, int edgeIndex)
            => $"{BASE}/place/road?owner={owner}&tileId={tileId}&edgeIndex={edgeIndex}";

        public static string UpgradeSettlementToCity(string owner, int tileId, int cornerIndex)
            => $"{BASE}/place/structure/upgrade?owner={owner}&tileId={tileId}&cornerIndex={cornerIndex}";

        // TRADE
        public static string TradeWithBank => $"{BaseUrl}/trade/bank";
        public static string TradeWithPlayer => $"{BaseUrl}/trade/player";

        // SESSION PLAYER
        public static string GetSessionPlayers(long sessionId) => $"{BaseUrl}/session-players/session/{sessionId}";
        public static string GetSessionPlayersByCode(string code) => $"{BaseUrl}/session-players/session/code/{code}";
        public static string GetPlayersByUserId(long userId) => $"{BaseUrl}/session-players/user/{userId}";
        public static string GetActivePlayersByUserId(long userId) => $"{BaseUrl}/session-players/user/{userId}/active";
    }
}
