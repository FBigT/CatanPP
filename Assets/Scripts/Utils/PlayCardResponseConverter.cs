using System;
using Assets.Scripts.Dtos.GameMoveResponses;
using Assets.Scripts.Dtos;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Assets.Scripts.Models;

namespace Assets.Scripts.Utils
{
    public class PlayCardResponseConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(PlayCardResponseDto);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);

            var gameMoveType = jo["DevCardType"]?.ToObject<DevCardType>();
            object moveData = gameMoveType switch
            {
                DevCardType.VICTORY_POINT => jo["moveData"]?.ToObject<PlayerScoreDto>(serializer),
                DevCardType.ROAD_BUILDING => jo["moveData"]?.ToObject<PlaceRoadResponse[]>(serializer),
                DevCardType.YEAR_OF_PLENTY => jo["moveData"]?.ToObject<RobberMoveResponse>(serializer),
                DevCardType.KNIGHT => jo["moveData"]?.ToObject<TradeOfferMessage>(serializer),
                _ => throw new JsonSerializationException($"Unknown GameMoveType: {gameMoveType}"),
            };
            return new PlayCardResponseDto
            {
                devCardType = gameMoveType.Value,
                moveData = moveData
            };
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var dto = (PlayCardResponseDto)value;
            writer.WriteStartObject();
            writer.WritePropertyName("DevCardType");
            serializer.Serialize(writer, dto.devCardType);
            writer.WritePropertyName("moveData");
            serializer.Serialize(writer, dto.moveData);
            writer.WriteEndObject();
        }
    }
}
