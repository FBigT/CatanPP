using Assets.Scripts.Dtos.GameMoveResponses;
using Assets.Scripts.Dtos;
using Assets.Scripts.Enums;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using Assets.Scripts.Dtos.GameMoveResponses; // Make sure this line exists

namespace Assets.Scripts.Utils
{
    public class GameMoveResponseConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(GameMoveResponseDto);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Console.WriteLine("🔍 [GameMoveResponseConverter] === STARTING JSON DESERIALIZATION ===");

            JObject jo;
            try
            {
                jo = JObject.Load(reader);
                Console.WriteLine($"✅ [GameMoveResponseConverter] Successfully loaded JObject: {jo}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [GameMoveResponseConverter] Failed to load JObject: {ex.Message}");
                throw;
            }

            var gameMoveTypeToken = jo["gameMoveType"];
            Console.WriteLine($"🎯 [GameMoveResponseConverter] GameMoveType token: {gameMoveTypeToken}");

            if (gameMoveTypeToken == null)
            {
                Console.WriteLine("❌ [GameMoveResponseConverter] No gameMoveType found in JSON!");
                throw new JsonSerializationException("Missing gameMoveType in JSON");
            }

            GameMoveType? gameMoveType;
            try
            {
                gameMoveType = gameMoveTypeToken.ToObject<GameMoveType>();
                Console.WriteLine($"✅ [GameMoveResponseConverter] Parsed GameMoveType: {gameMoveType}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [GameMoveResponseConverter] Failed to parse GameMoveType: {ex.Message}");
                Console.WriteLine($"Raw gameMoveType value: {gameMoveTypeToken}");
                throw;
            }

            var moveDataToken = jo["moveData"];
            Console.WriteLine($"📦 [GameMoveResponseConverter] MoveData token: {moveDataToken}");

            object moveData;
            try
            {
                Console.WriteLine($"🔄 [GameMoveResponseConverter] Processing moveData for type: {gameMoveType}");

                moveData = gameMoveType switch
                {
                    GameMoveType.PLACE_ROAD => DeserializeWithLogging<PlaceRoadResponse>(moveDataToken, serializer, "PLACE_ROAD"),
                    GameMoveType.BUY_CARD => DeserializeWithLogging<BuyCardResponseDto>(moveDataToken, serializer, "BUY_CARD"),
                    GameMoveType.PRIVATE_BUY_CARD => DeserializeWithLogging<PrivateBuyCard>(moveDataToken, serializer, "PRIVATE_BUY_CARD"),
                    GameMoveType.UPGRADE_STRUCTURE => DeserializeWithLogging<UpgradeStructureResponse>(moveDataToken, serializer, "UPGRADE_STRUCTURE"),
                    GameMoveType.ROBBER_MOVE => DeserializeWithLogging<RobberMoveResponse>(moveDataToken, serializer, "ROBBER_MOVE"),
                    GameMoveType.PLACE_STRUCTURE => DeserializeWithLogging<PlaceStructureResponse>(moveDataToken, serializer, "PLACE_STRUCTURE"),
                    GameMoveType.DICE_ROLL => DeserializeWithLogging<DiceResultDto>(moveDataToken, serializer, "DICE_ROLL"),
                    GameMoveType.END_TURN => DeserializeWithLogging<EndTurnResponse>(moveDataToken, serializer, "END_TURN"),
                    GameMoveType.TURN_ORDER => DeserializeWithLogging<TurnOrderResponse>(moveDataToken, serializer, "TURN_ORDER"),
                    GameMoveType.PLAY_CARD => DeserializeWithLogging<PlayCardResponseDto>(moveDataToken, serializer, "PLAY_CARD"),
                    GameMoveType.TRADE_OFFER => DeserializeWithLogging<TradeOfferMessage>(moveDataToken, serializer, "TRADE_OFFER"),
                    GameMoveType.TRADE_RESPONSE => DeserializeWithLogging<TradeResponseMessage>(moveDataToken, serializer, "TRADE_RESPONSE"),
                    GameMoveType.START_GAME => DeserializeWithLogging<StartGameResponse>(moveDataToken, serializer, "START_GAME"),
                    GameMoveType.REQUEST_DEV_CARDS => DeserializeWithLogging<DevCardsListResponseDto>(moveDataToken, serializer, "REQUEST_DEV_CARDS"), // ← ADD THIS LINE
                    _ => throw new JsonSerializationException($"Unknown GameMoveType: {gameMoveType}"),
                };

                Console.WriteLine($"✅ [GameMoveResponseConverter] Successfully deserialized moveData: {moveData}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [GameMoveResponseConverter] Failed to deserialize moveData: {ex.Message}");
                Console.WriteLine($"Raw moveData: {moveDataToken}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }

            var result = new GameMoveResponseDto
            {
                GameMoveType = gameMoveType.Value,
                moveData = moveData
            };

            Console.WriteLine($"🎉 [GameMoveResponseConverter] Final result: GameMoveType={result.GameMoveType}, moveData={result.moveData}");
            Console.WriteLine("🔍 [GameMoveResponseConverter] === JSON DESERIALIZATION COMPLETE ===");

            return result;
        }

        private T DeserializeWithLogging<T>(JToken token, JsonSerializer serializer, string typeName)
        {
            Console.WriteLine($"🔄 [GameMoveResponseConverter] Deserializing {typeName}...");
            Console.WriteLine($"Raw {typeName} data: {token}");

            try
            {
                if (token == null)
                {
                    Console.WriteLine($"⚠️ [GameMoveResponseConverter] Token is null for {typeName}");
                    return default(T);
                }

                T result = token.ToObject<T>(serializer);
                Console.WriteLine($"✅ [GameMoveResponseConverter] Successfully deserialized {typeName}: {result}");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [GameMoveResponseConverter] Failed to deserialize {typeName}: {ex.Message}");
                Console.WriteLine($"Expected type: {typeof(T).Name}");
                throw;
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var dto = (GameMoveResponseDto)value;
            writer.WriteStartObject();
            writer.WritePropertyName("GameMoveType");
            serializer.Serialize(writer, dto.GameMoveType);
            writer.WritePropertyName("moveData");
            serializer.Serialize(writer, dto.moveData);
            writer.WriteEndObject();
        }
    }
}
