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
        public static event Action<ChatMessage> OnChatMessageReceived;

        private static WebSocket webSocket;
        public static bool Connected { get; private set; } = false;
        private static string sessionCode;

        public static async Task ConnectToChat(string code)
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
                    Debug.Log(message);
                    int index = message.IndexOf("\n\n");
                    if (index != -1)
                    {
                        string headers = message[..index];
                        string jsonBody = message[(index + 2)..].TrimEnd('\u0000');

                        // Extract destination from headers
                        string destination = null;
                        var lines = headers.Split('\n');
                        foreach (var line in lines)
                        {
                            if (line.StartsWith("destination:"))
                            {
                                destination = line["destination:".Length..];
                                break;
                            }
                        }

                        // Now you can route based on the destination
                        if (destination != null && destination.Contains(WebSocketBrokerDestinations.Chat.Value))
                        {
                            ChatMessage chatMsg = JsonUtility.FromJson<ChatMessage>(jsonBody);
                            OnChatMessageReceived?.Invoke(chatMsg);
                        }
                    }
                }
            };

            await webSocket.Connect();
        }

        private static async Task SendConnectFrame()
        {
            Debug.Log(WebSocketEndpointsUtils.ConnectFrame);
            await webSocket.SendText(WebSocketEndpointsUtils.ConnectFrame);
        }

        private static async Task SendSubscribeFrame()
        {
            await webSocket.SendText(WebSocketEndpointsUtils.SubscribeFrame(WebSocketBrokerDestinations.Chat, sessionCode));
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
        // Add these methods to your existing WebSocketService class
        public static void SendBuyDevCard()
        {
            var gameMove = new Assets.Scripts.Dtos.GameMoveDto(Assets.Scripts.Enums.GameMoveType.BUY_CARD);
            string json = JsonUtility.ToJson(gameMove);
            SendMessage(json);
        }


        public static void SendPlayDevCard(PlayCardDto playCardDto)
        {
            var gameMove = new GameMoveDto(playCardDto);  // This constructor might need to be added
            string json = JsonUtility.ToJson(gameMove);
            SendMessage(json);
        }

        public static void DispatchMessageQueue()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            webSocket?.DispatchMessageQueue();
#endif
        }
    }
}