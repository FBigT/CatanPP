using UnityEditor;

namespace Assets.Scripts.Utils
{
    public static class EndpointUtils
    {
        public static string BaseUrl { get; } = "http://localhost:8080/api";

        //User
        public static string Users { get; } = BaseUrl + "/users";
        public static string Login { get; } = Users + "/login";
        public static string Register { get; } = Users + "/register";
        public static string RegisterGuest { get; } = Register + "/guest";

        public static string GetUserByUsername(string username) { 
            return Users + "/" + username;
        }

        public static string GetUserById(long id)
        {
            return Users + "/" + id;
        }

        public static string GuestLogin(string key) { 
            return Login + "/guest/" + key;
        }

        public static string DeactivateUser(long id) {
            return Users + "/" + id;
        }

        public static string UpdateUser(long id) {
            return Users + "/" + id;
        }

        public static string GetPlayerPorfileById(long id) { 
            return Users + "/profile/" + id;
        }

        public static string GetPlayerPorfileByUsername(string username)
        {
            return Users + "/profile/" + username;
        }

        //Session
        public static string Sessions {  get; } = BaseUrl + "/sessions";
        public static string CreateSessions(int numberOfPlayers) { 
            return Sessions + "/" + numberOfPlayers;
        }

        public static string JoinSession(string code) { 
            return Sessions + "/join/" + code;
        }

        //Dice
        public static string Dice { get; } = BaseUrl + "/dice";
        public static string Roll() { 
            return Dice + "/roll";
        }

        //City
        public static string City { get; } = BaseUrl + "/cities";

        public static string PlaceSettlement { get; } = City + "/place";

        public static string UpgradeToCity(long id) { 
            return City + "/" + id + "/upgrade";
        }

        //TradingPort
        public static string TradingPort { get; } = BaseUrl + "/trading-ports";

        public static string UpdatePortPlaceent(long id) { 
            return TradingPort + "/" + id + "/place";
        }
    }
}
