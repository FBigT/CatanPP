using Newtonsoft.Json;
using UnityEngine;

namespace Assets.Scripts.Utils
{
    public static class WebSocketEndpointsUtils
    {
        public static string BaseWebSocketUrl { get; } = "ws://localhost:8080/catan/websocket";

        public static string ConnectFrame { get; } = $"CONNECT\naccept-version:1.2\nhost:localhost\nAuthorization:{LocalStorageService.GetString("token")}\n\n\0";
        public static string DisconnectFrame { get; } = $"DISCONNECT\n\n\u0000";

        public static string SubscribeFrame(WebSocketBrokerDestinations path, string sessionCode) {
            return $"SUBSCRIBE\nid:sub-0\ndestination:{WebSocketBrokerDestinations.Construct(path, sessionCode)}\nAuthorization:{LocalStorageService.GetString("token")}\n\n\0";
        }

        public static string MessageFrame(WebSocketApplicationDestinations path, string sessionCode, object body){
            return $"SEND\ndestination:{WebSocketApplicationDestinations.Construct(path, sessionCode)}\ncontent-type:application/json\nAuthorization:{LocalStorageService.GetString("token")}\n\n{JsonConvert.SerializeObject(body)}\n\0";
        }
    }
}
