using Assets.Scripts.Dtos.GameMoveResponses;
using Assets.Scripts.Dtos;
using Assets.Scripts.Enums;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;

namespace Assets.Scripts.Utils
{
    public class GameMoveResponseConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(GameMoveResponseDto);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);

            var gameMoveType = jo["GameMoveType"]?.ToObject<GameMoveType>();
            object moveData = gameMoveType switch
            {
                GameMoveType.PLACE_ROAD => jo["moveData"]?.ToObject<PlaceRoadResponse>(serializer),
                GameMoveType.BUY_CARD => jo["moveData"]?.ToObject<BuyCardResponseDto>(serializer),
                GameMoveType.PRIVATE_BUY_CARD => jo["moveData"]?.ToObject<PrivateBuyCard>(serializer),
                GameMoveType.UPGRADE_STRUCTURE => jo["moveData"]?.ToObject<UpgradeStructureResponse>(serializer),
                GameMoveType.ROBBER_MOVE => jo["moveData"]?.ToObject<RobberMoveResponse>(serializer),
                GameMoveType.PLACE_STRUCTURE => jo["moveData"]?.ToObject<PlaceStructureResponse>(serializer),
                GameMoveType.DICE_ROLL => jo["moveData"]?.ToObject<DiceResultDto>(serializer),
                GameMoveType.END_TURN => jo["moveData"]?.ToObject<EndTurnResponse>(serializer),
                //Complex type
                GameMoveType.PLAY_CARD => jo["moveData"]?.ToObject<PlayCardResponseDto>(serializer),
                GameMoveType.TRADE_OFFER => jo["moveData"]?.ToObject<TradeOfferMessage>(serializer),
                GameMoveType.TRADE_RESPONSE => jo["moveData"]?.ToObject<TradeResponseMessage>(serializer),
                _ => throw new JsonSerializationException($"Unknown GameMoveType: {gameMoveType}"),


            };
            return new GameMoveResponseDto
            {
                GameMoveType = gameMoveType.Value,
                moveData = moveData
            };
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
