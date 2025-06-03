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

        public static event Action<TradeOfferMessage> OnTradeOfferReceived;
        public static event Action<TradeResponseMessage> OnTradeResponseReceived;
        public static event Action  OnPlayerJoined;


        // ✅ ADD THESE NEW EVENTS FOR DEV CARDS:
        public static event Action<BuyCardResponseDto> OnBuyCardResponse;
        public static event Action<PrivateBuyCard> OnPrivateBuyCard;
        public static event Action<PlayCardResponseDto> OnPlayCardResponse;
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
                Debug.Log("[WS] Raw incoming message:\n" + Encoding.UTF8.GetString(bytes));

                var message = Encoding.UTF8.GetString(bytes);

                Debug.Log($"Message received: {message}");

                if (message.StartsWith("CONNECTED"))
                {
                    Connected = true;
                    _ = SendSubscribeFrame();
                }
                else if (message.StartsWith("MESSAGE"))
                {
                    Debug.Log("[WS] Message is a STOMP MESSAGE");
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
                                Debug.Log("[WS] Parsed destination: " + destination);

                                break;
                            }
                        }

                        // Now you can route based on the destination
                        // Handle all of these plz
                        // from Poofy
                        if (destination != null && destination.Contains(WebSocketBrokerDestinations.Chat.Value))
                        {
                            Debug.Log(jsonBody);
                            ChatMessage chatMsg = JsonUtility.FromJson<ChatMessage>(jsonBody);
                            OnChatMessageReceived?.Invoke(chatMsg);
                        } else if (destination != null && destination.Contains(WebSocketBrokerDestinations.Moves.Value))
                        {
                            //GameMoveResponseDto gameMove = JsonConvert.DeserializeObject<GameMoveResponseDto>(jsonBody);
                            //GameMoveType gameMoveType = gameMove.GameMoveType;


                            GameMoveResponseDto gameMove = JsonConvert.DeserializeObject<GameMoveResponseDto>(jsonBody);
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
                                    // ✅ ADD THIS LINE:
                                    OnBuyCardResponse?.Invoke(buyCardResponse);
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
                                    // ✅ ADD THIS LINE:
                                    OnPlayCardResponse?.Invoke(playCardResponseDto);
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
                                            Place2RoadsResponseDto placeRoadsDto = (Place2RoadsResponseDto)playCardResponseDto.moveData;
                                            break;
                                        case Models.DevCardType.YEAR_OF_PLENTY:
                                            TradeOfferMessage yearOfPlentyDto = (TradeOfferMessage)playCardResponseDto.moveData;
                                            break;
                                    }

                                    break;
                                case GameMoveType.VICTORY:
                                    VictoryDto victoryDto = (VictoryDto)gameMove.moveData;
                                    break;
                                case GameMoveType.TURN_ORDER:
                                    TurnOrderResponse turnOrder = (TurnOrderResponse)gameMove.moveData;
                                    break;
                                case GameMoveType.MAP_GEN:
                                    GenerateMapDto generateMapDto = (GenerateMapDto)gameMove.moveData;
                                    break;
                                case GameMoveType.TRADE_OFFER:
                                    {
                                        // 'gameMove.moveData' holds a TradeOfferMessage
                                        var offer = JsonConvert.DeserializeObject<TradeOfferMessage>(
                                            JsonConvert.SerializeObject(gameMove.moveData)
                                        );
                                        Debug.Log($"[WebSocketService] Deserialized TradeOfferMessage: from {offer.fromUser} to {offer.toUser}");
                                        OnTradeOfferReceived?.Invoke(offer);
                                        break;
                                    }

                                case GameMoveType.TRADE_RESPONSE:
                                    {
                                        string respJson = JsonConvert.SerializeObject(gameMove.moveData);
                                        var resp = JsonConvert.DeserializeObject<TradeResponseMessage>(respJson);
                                        Debug.Log($"[WebSocketService] Deserialized TradeResponseMessage: from {resp.fromUser} → {resp.toUser}, accepted={resp.accepted}");
                                        OnTradeResponseReceived?.Invoke(resp);
                                        break;
                                    }


                            };
                        }
                        else if (destination != null && destination.Contains(WebSocketBrokerDestinations.Private.Value))
                        {
                            GameMoveDto gameMove = JsonConvert.DeserializeObject<GameMoveDto>(jsonBody);
                            GameMoveType gameMoveType = gameMove.gameMoveType;

                            if (gameMoveType == GameMoveType.BUY_CARD)
                            {
                                PrivateBuyCard privateBuyCard = (PrivateBuyCard)gameMove.moveData;
                                // ✅ ADD THIS LINE:
                                OnPrivateBuyCard?.Invoke(privateBuyCard);
                            }
                        }
                        else if (destination != null && destination.Contains(WebSocketBrokerDestinations.Players.Value)){
                            JoinSessionNotification joinSessionNotification =
                                JsonConvert.DeserializeObject<JoinSessionNotification>(jsonBody);
                            OnPlayerJoined?.Invoke();

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
            // 1) Subscribe to Chat channel (using your helper)
            await webSocket.SendText(
                WebSocketEndpointsUtils.SubscribeFrame(WebSocketBrokerDestinations.Chat, sessionCode)
            );

            // 2) Subscribe to Players channel (using your helper)
            await webSocket.SendText(
                WebSocketEndpointsUtils.SubscribeFrame(WebSocketBrokerDestinations.Players, sessionCode)
            );

            // 3) Subscribe to Private channel (using your helper)
            await webSocket.SendText(
                WebSocketEndpointsUtils.SubscribeFrame(WebSocketBrokerDestinations.Private, sessionCode)
            );

            // 4) Subscribe to Moves channel (using your helper)
            await webSocket.SendText(
                WebSocketEndpointsUtils.SubscribeFrame(WebSocketBrokerDestinations.Moves, sessionCode)
            );

            //string movesDestination = $"/game/move/{sessionCode}";
            //Debug.Log($"[WebSocketService] Subscribing to Moves at: {movesDestination}");
            //string movesSubscribeFrame =
            //    "SUBSCRIBE\n" +
            //    $"destination:{movesDestination}\n" +
            //    $"id:sub-moves-{sessionCode}\n\n" +
            //    "\0";
            //await webSocket.SendText(movesSubscribeFrame);
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

        public static async Task SendTradeOffer(TradeOfferMessage offer)
        {
            if (!Connected)
            {
                Debug.LogWarning("Cannot send trade offer: WebSocket not connected.");
                return;
            }

            // 1) build the exact same DTO your server expects:

            TradeOfferMessage dto = new TradeOfferMessage
            {
                fromUser = offer.fromUser,
                toUser = offer.toUser,
                offered = offer.offered,
                requested = offer.requested
            };
            GameMoveDto gameMoveDto = new GameMoveDto(dto);


            // 2) send *that* raw dto
            string frame = WebSocketEndpointsUtils.MessageFrame(
              WebSocketApplicationDestinations.Moves,
              sessionCode,
              gameMoveDto
            );
            Debug.Log($"[WebSocketService] >> STOMP SEND (Moves):\n{frame}");
            await webSocket.SendText(frame);
            Debug.Log("[WebSocketService] >> STOMP SEND complete");
        }

        public static async Task SendTradeResponse(TradeResponseMessage resp)
        {
            if (!Connected) { Debug.LogWarning("Not connected"); return; }
            var dto = new GameMoveDto(resp);  // you’ll need a ctor GameMoveDto(TradeResponseMessage)
            string frame = WebSocketEndpointsUtils.MessageFrame(
                WebSocketApplicationDestinations.Moves,
                sessionCode,
                dto
            );
            Debug.Log($"[WebSocketService] >> Sending TRADE_RESPONSE to {resp.toUser}, accepted: {resp.accepted}");

            await webSocket.SendText(frame);
        }
        //public static void RaiseChatMessage(ChatMessage msg)
        //{
        //    OnChatMessageReceived?.Invoke(msg);
        //}

    }
}