using Assets.Scripts.User;
using NativeWebSocket;
using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Utils
{
    public static class WebSocketService
    {
        private static WebSocket webSocket;
        public static bool Connected { get; private set; } = false;
        private static string sessionCode;

        public static async Task ConnectToChat(string code, Action<ChatMessage> onMessage)
        {
            sessionCode = code;

            webSocket = new WebSocket(WebSocketEndpointsUtils.BaseWebSocketUrl);

            webSocket.OnOpen += () => {
                _ = SendConnectFrame();
            };

            webSocket.OnError += (e) => {
                Debug.LogError($"Error: {e}");
            };

            webSocket.OnClose += (e) =>
            {
                Debug.Log("WebSocket closed!");
                Connected = false;
            };

            webSocket.OnMessage += (bytes) =>
            {
                var message = Encoding.UTF8.GetString(bytes);
                Debug.Log($"Message received: {message}");

                if (message.StartsWith("CONNECTED"))
                {
                    Connected = true;
                    _ = SendSubscribeFrame();
                }
                else if (message.StartsWith("MESSAGE"))
                {
                    int index = message.IndexOf("\n\n");
                    if (index != -1)
                    {
                        string jsonBody = message[(index + 2)..].TrimEnd('\u0000');
                        onMessage?.Invoke(JsonUtility.FromJson<ChatMessage>(jsonBody));
                    }
                }
            };

            await webSocket.Connect();
        }

        private static async Task SendConnectFrame()
        {
            Debug.Log("Sending CONNECT frame...");
            await webSocket.SendText(WebSocketEndpointsUtils.ConnectFrame);
        }

        private static async Task SendSubscribeFrame()
        {
            Debug.Log("Sending chat SUBSCRIBE frame...");
            await webSocket.SendText(WebSocketEndpointsUtils.SubscribeFrame(WebSocketBrokerDestinations.Chat, sessionCode));
            Debug.Log("Sending players SUBSCRIBE frame...");
            await webSocket.SendText(WebSocketEndpointsUtils.SubscribeFrame(WebSocketBrokerDestinations.Players, sessionCode));
        }

        public static async Task SendMessage(string message)
        {
            if (string.IsNullOrEmpty(message) || !Connected)
            {
                Debug.LogWarning("Cannot send message: Not connected or message is empty.");
                return;
            }

            string messageFrame = WebSocketEndpointsUtils.MessageFrame(WebSocketApplicationDestinations.Chat, sessionCode, new ChatMessageOut(message));
            Debug.Log("Sending MESSAGE frame...");
            await webSocket.SendText(messageFrame);
        }

        public static async Task CloseConnection()
        {
            if (webSocket != null)
            {
                await webSocket.Close();
                Connected = false;
            }
        }

        public static void DispatchMessageQueue()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            webSocket?.DispatchMessageQueue();
#endif
        }
    }
}