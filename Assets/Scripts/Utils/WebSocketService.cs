using Assets.Scripts.Dtos;
using Assets.Scripts.Dtos.GameMoveResponses;
using Assets.Scripts.Dtos.GameMoves;
using Assets.Scripts.Enums;
using Assets.Scripts.User;
using NativeWebSocket;
using Newtonsoft.Json;
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
                        // Handle all of these plz
                        // from Poofy
                        if (destination != null && destination.Contains(WebSocketBrokerDestinations.Chat.Value))
                        {
                            ChatMessage chatMsg = JsonUtility.FromJson<ChatMessage>(jsonBody);
                            OnChatMessageReceived?.Invoke(chatMsg);
                        } else if (destination != null && destination.Contains(WebSocketBrokerDestinations.Moves.Value))
                        {
                            GameMoveDto gameMove = JsonConvert.DeserializeObject<GameMoveDto>(jsonBody);
                            GameMoveType gameMoveType = gameMove.GameMoveType;

                            switch (gameMoveType)
                            {
                                case GameMoveType.PLACE_ROAD:
                                    PlaceRoadResponse placeRoadResponse = (PlaceRoadResponse)gameMove.moveData;
                                    break;
                                case GameMoveType.PLACE_STRUCTURE:
                                    PlaceStructureResponse placeStructureResponse = (PlaceStructureResponse)gameMove.moveData;
                                    break;
                                case GameMoveType.BUY_CARD:
                                    BuyCardResponseDto buyCardResponse = (BuyCardResponseDto)gameMove.moveData;
                                    break;
                                case GameMoveType.UPGRADE_STRUCTURE:
                                    UpgradeStructureResponse upgradeStructure = (UpgradeStructureResponse)gameMove.moveData;
                                    break;
                                case GameMoveType.END_TURN:
                                    EndTurnResponse endTurn = (EndTurnResponse)gameMove.moveData;
                                    break;
                                case GameMoveType.DICE_ROLL:
                                    DiceResultDto diceResult = (DiceResultDto)gameMove.moveData;
                                    break;
                                case GameMoveType.ROBBER_MOVE:
                                    RobberMoveDto robberMoveDto = (RobberMoveDto)gameMove.moveData;
                                    break;
                                case GameMoveType.PLAY_CARD:
                                    PlayCardResponseDto playCardResponseDto = (PlayCardResponseDto)gameMove.moveData;
                                    switch (playCardResponseDto.devCardType)
                                    {
                                        case Models.DevCardType.KNIGHT:
                                            //For now only robber move data, will maybe add resource stealing later
                                            RobberMoveDto knightMoveDto = (RobberMoveDto)playCardResponseDto.moveData;

                                            break;
                                        case Models.DevCardType.VICTORY_POINT:
                                            PlayerScoreDto playerScoreDto = (PlayerScoreDto)playCardResponseDto.moveData;

                                            break;
                                        case Models.DevCardType.ROAD_BUILDING:
                                            Place2RoadsDto placeRoadsDto = (Place2RoadsDto)playCardResponseDto.moveData;
                                            break;
                                        case Models.DevCardType.YEAR_OF_PLENTY:
                                            TradeOfferMessage yearOfPlentyDto = (TradeOfferMessage)playCardResponseDto.moveData;
                                            break;
                                    }

                                    break;
                                case GameMoveType.VICTORY:
                                    VictoryDto victoryDto = (VictoryDto)gameMove.moveData;
                                    break;
                                case GameMoveType.MAP_GEN:
                                    GenerateMapDto generateMapDto = (GenerateMapDto)gameMove.moveData;
                                    break;
                            }
                            ;
                        }
                        else if (destination != null && destination.Contains(WebSocketBrokerDestinations.Private.Value)) {
                            GameMoveDto gameMove = JsonConvert.DeserializeObject<GameMoveDto>(jsonBody);
                            GameMoveType gameMoveType = gameMove.GameMoveType;

                            PrivateBuyCard privateBuyCard = (PrivateBuyCard)gameMove.moveData;
                        } else if (destination != null && destination.Contains(WebSocketBrokerDestinations.Players.Value)){
                            JoinSessionNotification joinSessionNotification = JsonConvert.DeserializeObject<JoinSessionNotification>(jsonBody);
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
            await webSocket.SendText(WebSocketEndpointsUtils.SubscribeFrame(WebSocketBrokerDestinations.Private, sessionCode));
            await webSocket.SendText(WebSocketEndpointsUtils.SubscribeFrame(WebSocketBrokerDestinations.Moves, sessionCode));
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

        public static async Task SendGameMove(GameMoveDto gameMoveDto)
        {
            if (gameMoveDto == null || !Connected)
            {
                Debug.LogWarning("Cannot send message: Not connected or game move is null.");
                return;
            }

            string messageFrame = WebSocketEndpointsUtils.MessageFrame(WebSocketApplicationDestinations.Moves, sessionCode, gameMoveDto);
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