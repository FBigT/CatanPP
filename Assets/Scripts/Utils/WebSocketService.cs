using Assets.Scripts.Dtos;
using Assets.Scripts.Dtos.GameMoveResponses;
using Assets.Scripts.Enums;
using Assets.Scripts.User;
using CatanGame.DTOs;
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
        public static event Action<RobberMoveResponse> OnRobberMoved;
        public static event Action<TradeOfferMessage> OnTradeOfferReceived;
        public static event Action<TradeResponseMessage> OnTradeResponseReceived;
        public static event Action OnPlayerJoined;

        public static event Action<TradeExecutedDto> OnTradeExecuted;
        public static event Action<DevCardsListResponseDto> OnDevCardsListReceived;


        
        
        public static event Action<BuyCardResponseDto> OnBuyCardResponse;
        public static event Action<VictoryDto> OnVictoryTriggered;
        public static event Action<PrivateBuyCard> OnPrivateBuyCard;
        public static event Action<PlayCardResponseDto> OnPlayCardResponse;

        public static event Action<DiceResultDto> OnDiceResponse;
        public static event Action<StartGameResponse> OnGameStart;

        public static event Action<GenerateMapDto> OnMapGenerated;

        public static event Action<EndTurnResponse> OnEndTurn;

        public static event Action<PlaceRoadResponse> OnPlaceRoad;
        public static event Action<PlaceStructureResponse> OnPlaceStructure;
        public static event Action<UpgradeStructureResponse> OnUpgradeStructure;

        private static WebSocket webSocket;
        public static bool Connected { get; private set; } = false;
        private static string sessionCode;

        public static async Task ConnectToChat(string code)
        {

            sessionCode = code;

            webSocket = new WebSocket(WebSocketEndpointsUtils.BaseWebSocketUrl);

            webSocket.OnOpen += () =>
            {
                _ = SendConnectFrame();
            };

            webSocket.OnError += (e) =>
            {
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
                Debug.Log($"📡 [WebSocket] === RAW MESSAGE RECEIVED ===");
                Debug.Log($"📡 [WebSocket] Timestamp: {DateTime.Now:HH:mm:ss.fff}");
                Debug.Log($"📡 [WebSocket] Message length: {bytes.Length}");
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
                        }
                        else if (destination != null && destination.Contains(WebSocketBrokerDestinations.Moves.Value))
                        {
                            //GameMoveResponseDto gameMove = JsonConvert.DeserializeObject<GameMoveResponseDto>(jsonBody);
                            //GameMoveType gameMoveType = gameMove.GameMoveType;


                            GameMoveResponseDto gameMove = JsonConvert.DeserializeObject<GameMoveResponseDto>(jsonBody);
                            GameMoveType gameMoveType = gameMove.GameMoveType;

                            switch (gameMoveType)
                            {
                                case GameMoveType.PLACE_ROAD:
                                    {
                                        PlaceRoadResponse placeRoadResponse = (PlaceRoadResponse)gameMove.moveData;
                                        OnPlaceRoad?.Invoke(placeRoadResponse);
                                        break;
                                    }
                                case GameMoveType.PLACE_STRUCTURE:
                                    {
                                        PlaceStructureResponse placeStructureResponse = (PlaceStructureResponse)gameMove.moveData;
                                        OnPlaceStructure?.Invoke(placeStructureResponse);
                                        break;
                                    }
                                case GameMoveType.BUY_CARD:
                                    BuyCardResponseDto buyCardResponse = (BuyCardResponseDto)gameMove.moveData;
                                    OnBuyCardResponse?.Invoke(buyCardResponse);
                                    break;
                                case GameMoveType.UPGRADE_STRUCTURE:
                                    {
                                        UpgradeStructureResponse upgradeStructure = (UpgradeStructureResponse)gameMove.moveData;
                                        OnUpgradeStructure?.Invoke(upgradeStructure);
                                        break;
                                    }
                                case GameMoveType.END_TURN:
                                    {
                                        EndTurnResponse endTurn = (EndTurnResponse)gameMove.moveData;
                                        OnEndTurn?.Invoke((EndTurnResponse)gameMove.moveData);
                                        break;
                                    }
                                case GameMoveType.DICE_ROLL:
                                    {
                                        DiceResultDto diceResult = (DiceResultDto)gameMove.moveData;
                                        OnDiceResponse?.Invoke(diceResult);
                                        break;
                                    }
                                case GameMoveType.ROBBER_MOVE:
                                    var robberResponse = (RobberMoveResponse)gameMove.moveData;
                                    OnRobberMoved?.Invoke(robberResponse);
                                    break;
                                case GameMoveType.PAY_DEBT:
                                    PayDebtResponse payDebt = (PayDebtResponse)gameMove.moveData;
                                    break;
                                case GameMoveType.PLAY_CARD:
                                    PlayCardResponseDto playCardResponseDto = (PlayCardResponseDto)gameMove.moveData;
                                    OnPlayCardResponse?.Invoke(playCardResponseDto);
                                    switch (playCardResponseDto.devCardType)
                                    {
                                        case Models.DevCardType.KNIGHT:
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
                                    OnVictoryTriggered?.Invoke(victoryDto);
                                    break;
                                case GameMoveType.TURN_ORDER:
                                    TurnOrderResponse turnOrder = (TurnOrderResponse)gameMove.moveData;
                                    break;
                                case GameMoveType.START_GAME:
                                    StartGameResponse startGame = (StartGameResponse)gameMove.moveData;
                                    OnGameStart?.Invoke(startGame);
                                    break;
                                case GameMoveType.MAP_GEN:
                                    {
                                        Debug.Log("🗺️ [WebSocketService] Processing MAP_GEN response");
                                        GenerateMapDto generateMapDto = (GenerateMapDto)gameMove.moveData;
                                        Debug.Log($"🗺️ [WebSocketService] Map data contains {generateMapDto.tileDtos?.Count ?? 0} tiles");

                                        // Fire the event for map generation
                                        OnMapGenerated?.Invoke(generateMapDto);
                                        break;
                                    }

                                case GameMoveType.REQUEST_MAP:
                                    {
                                        Debug.Log("🗺️ [WebSocketService] Processing REQUEST_MAP response");
                                        GenerateMapDto requestedMapDto = (GenerateMapDto)gameMove.moveData;
                                        Debug.Log($"🗺️ [WebSocketService] Requested map data contains {requestedMapDto.tileDtos?.Count ?? 0} tiles");

                                        // Fire the same event - the response to a map request is the same as map generation
                                        OnMapGenerated?.Invoke(requestedMapDto);
                                        break;
                                    }
                                case GameMoveType.TRADE_OFFER:
                                    {
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

                                case GameMoveType.TRADE_EXECUTED:
                                    {
                                        var executed = JsonConvert.DeserializeObject<TradeExecutedDto>(
                                            JsonConvert.SerializeObject(gameMove.moveData)
                                        );
                                        Debug.Log($"[WebSocketService] Deserialized TradeExecuted: {executed.fromUser}→{executed.toUser}");
                                        OnTradeExecuted?.Invoke(executed);
                                        break;
                                    }
                                case GameMoveType.REQUEST_DEV_CARDS:
                                    DevCardsListResponseDto devCardsResponse = (DevCardsListResponseDto)gameMove.moveData;
                                    OnDevCardsListReceived?.Invoke(devCardsResponse);
                                    break;
                            }
                            ;
                        }
                        else if (destination != null && destination.Contains(WebSocketBrokerDestinations.Private.Value))
                        {
                            GameMoveDto gameMove = JsonConvert.DeserializeObject<GameMoveDto>(jsonBody);
                            GameMoveType gameMoveType = gameMove.gameMoveType;

                            if (gameMoveType == GameMoveType.BUY_CARD)
                            {
                                PrivateBuyCard privateBuyCard = (PrivateBuyCard)gameMove.moveData;
                                OnPrivateBuyCard?.Invoke(privateBuyCard);
                            }
                        }
                        //hre is player data u dumb fuck
                        else if (destination != null && destination.Contains(WebSocketBrokerDestinations.Players.Value))
                        {
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

        public static async Task SendMapData(GameMoveDto gameMoveDto)
        {
            if (gameMoveDto == null)
            {
                Debug.LogWarning("game move is null.");
                return;
            }

            string messageFrame = WebSocketEndpointsUtils.MessageFrame(WebSocketApplicationDestinations.Moves, sessionCode, gameMoveDto);
            await webSocket.SendText(messageFrame);
        }
        public static async Task SendMapRequest()
        {
            if (!Connected)
            {
                Debug.LogWarning("Cannot send map request: WebSocket not connected.");
                return;
            }

            try
            {
                var gameMove = new GameMoveDto(GameMoveType.REQUEST_MAP);
                Debug.Log("[WebSocketService] Sending map request...");
                await SendGameMove(gameMove);
                Debug.Log("[WebSocketService] Map request sent successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WebSocketService] Failed to send map request: {ex.Message}");
            }
        }

        public static async Task SendDiceRoll()
        {
            if (!Connected)
            {
                Debug.LogWarning("Cannot send dice roll: WebSocket not connected.");
                return;
            }

            try
            {
                var gameMove = new GameMoveDto(GameMoveType.DICE_ROLL);
                Debug.Log("[WebSocketService] Sending DICE_ROLL move...");
                await SendGameMove(gameMove);
                Debug.Log("[WebSocketService] DICE_ROLL move sent successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WebSocketService] Failed to send dice roll: {ex.Message}");
            }
        }

        public static async Task SendStartGame()
        {
            if (!Connected)
            {
                Debug.LogWarning("Cannot send start game: WebSocket not connected.");
                return;
            }

            try
            {
                var gameMove = new GameMoveDto(GameMoveType.START_GAME);
                Debug.Log("[WebSocketService] Sending START_GAME move...");
                await SendGameMove(gameMove);
                Debug.Log("[WebSocketService] START_GAME move sent successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WebSocketService] Failed to send start game: {ex.Message}");
            }
        }

        public static async Task SendEndTurn()
        {
            if (!Connected)
            {
                Debug.LogWarning("end turn problem");
                return;
            }

            try
            {
                var gameMove = new GameMoveDto(GameMoveType.END_TURN);
                Debug.Log("[WebSocketService] Sending end move...");
                await SendGameMove(gameMove);
                Debug.Log("[WebSocketService] end move sent successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WebSocketService] Failed to send end roll: {ex.Message}");
            }
        }

        public static async Task SendStructure()
        {
            if (!Connected)
            {
                Debug.LogWarning("structure problem");
                return;
            }

            try
            {
                var gameMove = new GameMoveDto(GameMoveType.PLACE_STRUCTURE);
                Debug.Log("[WebSocketService] Sending place structure move...");
                await SendGameMove(gameMove);
                Debug.Log("[WebSocketService] place move sent successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WebSocketService] Failed to send end place: {ex.Message}");
            }
        }

        public static async Task SendPlaceRoad(PlaceRoadDto d)
        {
            if (!Connected)
            {
                Debug.LogWarning("road problem");
                return;
            }

            try
            {
                var gameMove = new GameMoveDto(d);
                Debug.Log("[WebSocketService] Sending place road move...");
                await SendGameMove(gameMove);
                Debug.Log("[WebSocketService] road move sent successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WebSocketService] Failed to send end road: {ex.Message}");
            }
        }

        public static async Task SendPlaceStructure(PlaceStructureDto dto)
        {
            if (!Connected)
            {
                Debug.LogWarning("connect prob");
                return;
            }

            if (dto == null)
            {
                Debug.LogWarning("game move is null.");
                return;
            }

            GameMoveDto d = new GameMoveDto(dto);

            string messageFrame = WebSocketEndpointsUtils.MessageFrame(WebSocketApplicationDestinations.Moves, sessionCode, d);
            await webSocket.SendText(messageFrame);
        }
        public static async Task SendPlayCard(DevCardPlayDto playDto)
        {
            if (!Connected) return;

            try
            {
                var moveData = new GameMoveDto
                {
                    gameMoveType = GameMoveType.PLAY_CARD,
                    moveData = playDto
                };

                await SendGameMove(moveData);
                Debug.Log("[WebSocketService] PlayCard sent successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WebSocketService] Failed to send play card: {ex.Message}");
            }
        }
        // Modify SendRobberMove to use direct serialization
        public static async Task SendRobberMove(RobberMoveDto moveData)
        {
            var gameMove = new GameMoveDto(moveData);

            await SendGameMove(gameMove);
        }

        public static async Task SendUpgradeStructure(UpgradeStructureDto dto)
        {
            var gameMove = new GameMoveDto(dto);

            await SendGameMove(gameMove);
        }
    }
}